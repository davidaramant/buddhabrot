using Buddhabrot.Core;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;
using log4net;
using NOpenCL;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buffer = NOpenCL.Buffer;

namespace Buddhabrot.Points
{
    sealed class IntelHeterogeneousOpenCLPointFinder : PointFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(IntelHeterogeneousOpenCLPointFinder));

        private const int BatchSize = 8192;

        private readonly DisposeStack _disposeStack = new DisposeStack();
        private readonly Device[] _devices;
        private readonly NOpenCL.Program _program;
        private readonly Context _context;

        private readonly float[] _cReals = new float[BatchSize];
        private readonly float[] _cImags = new float[BatchSize];
        private readonly int[] _iterations = new int[BatchSize];

        public IntelHeterogeneousOpenCLPointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            string outputDirectory,
            PointFinderStatistics statistics) :
            base(numberGenerator, iterationRange, outputDirectory, statistics)
        {
            var platform = Platform.GetPlatforms()[0];

            var cpuDevice = platform.GetDevices(DeviceType.Cpu).Single();
            var gpuDevice = platform.GetDevices(DeviceType.Gpu).Single();

            _devices = new[] { cpuDevice, gpuDevice };

            _disposeStack.AddMultiple(_devices);

            _context = _disposeStack.Add(Context.Create(_devices));
            _program = _disposeStack.Add(_context.CreateProgramWithSource(OpenCLKernelSource.Read()));
            _program.Build();
        }

        protected override unsafe void IteratePointBatch(CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            using (var localStack = new DisposeStack())
            {
                foreach (var (c, index) in NumberGenerator.GetPoints(BatchSize).Select((c, i) => (c, i)))
                {
                    _cReals[index] = c.Real;
                    _cImags[index] = c.Imag;
                }

                // TODO: Dynamic batch sizes
                var cpuBatchSize = BatchSize / 2;
                var gpuBatchSize = BatchSize / 2;

                var batchSizes = new[] { cpuBatchSize, gpuBatchSize };

                fixed (float* pCReals = _cReals, pCImags = _cImags)
                fixed (int* pIterations = _iterations)
                {
                    var cRealsBuffer = localStack.Add(CreateBuffer(
                        (IntPtr)pCReals,
                        MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess));
                    var cImagsBuffer = localStack.Add(CreateBuffer(
                        (IntPtr)pCImags,
                        MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess));
                    var iterationsBuffer = localStack.Add(CreateBuffer(
                        (IntPtr)pIterations,
                        MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly));

                    Buffer SplitBuffer(Buffer input, int offset, int size)
                    {
                        return localStack.Add(
                            input.CreateSubBuffer(
                                MemoryFlags.None,
                                new BufferRegion((IntPtr)(offset * 4), (IntPtr)(size * 4))));
                    }

                    var cRealsSubBuffers = new[]
                    {
                        SplitBuffer(cRealsBuffer, 0, cpuBatchSize),
                        SplitBuffer(cRealsBuffer, cpuBatchSize, gpuBatchSize)
                    };
                    var cImagsSubBuffers = new[]
                    {
                        SplitBuffer(cImagsBuffer, 0, cpuBatchSize),
                        SplitBuffer(cImagsBuffer, cpuBatchSize, gpuBatchSize)
                    };
                    var iterationsSubBuffers = new[]
                    {
                        SplitBuffer(iterationsBuffer, 0, cpuBatchSize),
                        SplitBuffer(iterationsBuffer, cpuBatchSize, gpuBatchSize)
                    };

                    var commandQueues = new CommandQueue[_devices.Length];
                    var enqueueEvents = new Event[_devices.Length];
                    for (int deviceIndex = 0; deviceIndex < _devices.Length; deviceIndex++)
                    {
                        var device = _devices[deviceIndex];
                        commandQueues[deviceIndex] = _context.CreateCommandQueue(device);

                        var kernel = localStack.Add(_program.CreateKernel("iterate_points"));

                        kernel.Arguments[0].SetValue(cRealsSubBuffers[deviceIndex]);
                        kernel.Arguments[1].SetValue(cImagsSubBuffers[deviceIndex]);
                        kernel.Arguments[2].SetValue(iterationsSubBuffers[deviceIndex]);

                        enqueueEvents[deviceIndex] = commandQueues[deviceIndex]
                            .EnqueueNDRangeKernel(
                                kernel,
                                globalWorkSize: new[] { (IntPtr)batchSizes[deviceIndex] },
                                localWorkSize: null);
                    }
                    localStack.AddMultiple(commandQueues);
                    localStack.AddMultiple(enqueueEvents);

                    Event.WaitAll(enqueueEvents);

                    for (int deviceIndex = 0; deviceIndex < _devices.Length; deviceIndex++)
                    {
                        using (commandQueues[deviceIndex].EnqueueReadBuffer(
                            iterationsSubBuffers[deviceIndex],
                            blocking: true,
                            offset: 0,
                            size: sizeof(int) * batchSizes[deviceIndex],
                            destination: (IntPtr)(pIterations + deviceIndex * cpuBatchSize)))
                        {
                        }

                        commandQueues[deviceIndex].Finish();
                    }

                    Parallel.ForEach(Partitioner.Create(0, BatchSize), (range, loopState) =>
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            if (IterationRange.IsInside(_iterations[i]))
                            {
                                PointWriter.Save(new FComplex(_cReals[i], _cImags[i]));
                            }
                        }
                    });
                }
            }
            timer.Stop();

            Statistics.AddPointCount(BatchSize);
        }

        private Buffer CreateBuffer(IntPtr ptr, MemoryFlags accessFlags)
        {
            return _context.CreateBuffer(
                MemoryFlags.UseHostPointer | accessFlags,
                sizeof(float) * BatchSize,
                ptr);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposeStack.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
