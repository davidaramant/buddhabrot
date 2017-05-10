using System;
using System.Numerics;
using System.Threading;
using Buddhabrot.Utility;
using NOpenCL;

namespace Buddhabrot.IterationKernels
{
    public sealed class OpenCLKernel : IKernel
    {
        private const int BatchSize = 8192;
        private readonly Batch _pointBatch;
        private readonly DisposeStack _resources = new DisposeStack();

        public OpenCLKernel(Device device)
        {
            _resources.Add(device);

            var context = _resources.Add(Context.Create(device));
            var program = _resources.Add(context.CreateProgramWithSource(OpenCLKernelSource.Read()));
            program.Build();

            _pointBatch = new Batch(program, context, device, BatchSize);
        }

        public void Dispose() => _resources.Dispose();

        public IPointBatch GetBatch() => _pointBatch.Reset();

        private sealed class Batch : IPointBatch, IPointBatchResults
        {
            private readonly NOpenCL.Program _program;
            private readonly Context _context;
            private readonly Device _device;
            private readonly double[] _cReals;
            private readonly double[] _cImags;
            // There's no benefit to using longs for iterations in this kernel
            private readonly int[] _iterations;

            public int Capacity { get; }
            public int Count { get; private set; }

            public Batch(NOpenCL.Program program, Context context, Device device, int batchSize)
            {
                _program = program;
                _context = context;
                _device = device;
                Capacity = batchSize;

                _cReals = new double[batchSize];
                _cImags = new double[batchSize];
                _iterations = new int[batchSize];
            }

            public Batch Reset()
            {
                Count = 0;
                return this;
            }

            public Complex GetPoint(int index) => new Complex(_cReals[index], _cImags[index]);

            public long GetIteration(int index) => _iterations[index];

            public int AddPoint(Complex c)
            {
                var index = Count;
                Count++;

                _cReals[index] = c.Real;
                _cImags[index] = c.Imaginary;
                return index;
            }

            public unsafe IPointBatchResults ComputeIterations(CancellationToken token, long maxIterations)
            {
                // TODO: Look into not duplicating all this stuff every time.
                using (var localStack = new DisposeStack())
                {
                    fixed (double* pCReals = _cReals, pCImags = _cImags)
                    fixed (int* pIterations = _iterations)
                    {
                        using (var cRealsBuffer = CreateBuffer(
                            (IntPtr)pCReals,
                            sizeof(double),
                            MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess))
                        using (var cImagsBuffer = CreateBuffer(
                            (IntPtr)pCImags,
                            sizeof(double),
                            MemoryFlags.ReadOnly | MemoryFlags.HostNoAccess))
                        using (var iterationsBuffer = CreateBuffer(
                            (IntPtr)pIterations,
                            sizeof(int),
                            MemoryFlags.WriteOnly | MemoryFlags.HostReadOnly))
                        using (var commandQueue = _context.CreateCommandQueue(_device))
                        using (var kernel = localStack.Add(_program.CreateKernel("iterate_points")))
                        {
                            kernel.Arguments[0].SetValue(cRealsBuffer);
                            kernel.Arguments[1].SetValue(cImagsBuffer);
                            kernel.Arguments[2].SetValue(iterationsBuffer);
                            kernel.Arguments[3].SetValue((int)maxIterations);

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
                    }
                }

                return this;
            }

            private NOpenCL.Buffer CreateBuffer(IntPtr ptr, int unitSize, MemoryFlags accessFlags)
            {
                var memoryAccess = _device.HostUnifiedMemory ? MemoryFlags.UseHostPointer : MemoryFlags.CopyHostPointer;

                return _context.CreateBuffer(
                    memoryAccess | accessFlags,
                    unitSize * BatchSize,
                    ptr);
            }
        }
    }
}
