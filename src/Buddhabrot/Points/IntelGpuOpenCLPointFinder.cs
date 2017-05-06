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
    sealed class IntelGpuOpenCLPointFinder : PointFinder
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(IntelGpuOpenCLPointFinder));

        private const int BatchSize = 8192;

        private readonly DisposeStack _disposeStack = new DisposeStack();
        private readonly Device _gpuDevice;
        private readonly NOpenCL.Program _program;
        private readonly Context _context;

        private readonly float[] _cReals = new float[BatchSize];
        private readonly float[] _cImags = new float[BatchSize];
        private readonly int[] _iterations = new int[BatchSize];

        public IntelGpuOpenCLPointFinder(
            RandomPointGenerator numberGenerator,
            IntRange iterationRange,
            string outputDirectory,
            PointFinderStatistics statistics) :
            base(numberGenerator, iterationRange, outputDirectory, statistics)
        {
            var platform = Platform.GetPlatforms()[0];

            _gpuDevice = _disposeStack.Add(platform.GetDevices(DeviceType.Gpu).Single());

            _context = _disposeStack.Add(Context.Create(_gpuDevice));
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

                fixed (float* pCReals = _cReals, pCImags = _cImags)
                fixed (int* pIterations = _iterations)
                {
                    using (var cRealsBuffer = CreateBuffer((IntPtr)pCReals, MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess))
                    using (var cImagsBuffer = CreateBuffer((IntPtr)pCImags, MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess))
                    using (var iterationsBuffer = CreateBuffer((IntPtr)pIterations, MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly))
                    using (var commandQueue = _context.CreateCommandQueue(_gpuDevice))
                    using (var kernel = localStack.Add(_program.CreateKernel("iterate_points")))
                    {
                        kernel.Arguments[0].SetValue(cRealsBuffer);
                        kernel.Arguments[1].SetValue(cImagsBuffer);
                        kernel.Arguments[2].SetValue(iterationsBuffer);

                        using (var eventQueue = commandQueue.EnqueueNDRangeKernel(
                            kernel,
                            globalWorkSize: new[] { (IntPtr)BatchSize },
                            localWorkSize: null))
                        {
                            Event.WaitAll(eventQueue);
                        }

                        using (commandQueue.EnqueueReadBuffer(
                            iterationsBuffer,
                            blocking: true,
                            offset: 0,
                            size: sizeof(int) * BatchSize,
                            destination: (IntPtr)pIterations))
                        {
                        }

                        commandQueue.Finish();
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
