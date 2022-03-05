using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;

namespace Buddhabrot.PointGrids;

public sealed class PointGrid : IDisposable, IEnumerable<PointRow>
{
    private static readonly ILog Log = Logger.Create<PointGrid>();
    //                                 0123456789abcdef
    private const string HeaderText = "Point Grid V3.00";

    public ViewPort ViewPort { get; }
    public ComputationType ComputationType { get; }

    private readonly Stream _pointStream;
    private readonly long _pointsPosition;

    public static PointGrid Load(string filePath)
    {
        FileStream stream = null;

        try
        {
            stream = File.OpenRead(filePath);

            using var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
            var readHeader = reader.ReadString(); ;
            if (readHeader != HeaderText)
                throw new InvalidOperationException($"Unsupported edge span file format: {filePath}");

            var viewPort = reader.ReadViewPort();
            var computationType = (ComputationType)reader.ReadInt32();
            Log.Info($"Loaded point grid with resolution ({viewPort.Resolution.Width:N0}x{viewPort.Resolution.Height:N0}), " +
                     $"Area {viewPort.Area}.");
            return new PointGrid(viewPort, computationType, stream);
        }
        catch (Exception)
        {
            stream?.Dispose();
            throw;
        }
    }

    private PointGrid(
        ViewPort viewPort,
        ComputationType computationType,
        Stream pointStream)
    {
        ViewPort = viewPort;
        ComputationType = computationType;
        _pointStream = pointStream;
        _pointsPosition = pointStream.Position;
    }

    public void Dispose() => _pointStream.Dispose();

    public IEnumerator<PointRow> GetEnumerator()
    {
        _pointStream.Position = _pointsPosition;
        using (var reader = new BinaryReader(_pointStream, Encoding.ASCII, leaveOpen: true))
        {
            var rowSegments = new List<(bool inSet, int length)>();
            int totalLength = 0;
            int y = 0;
            while (_pointStream.Position < _pointStream.Length)
            {
                var packedLength = reader.ReadInt32();
                var inSet = packedLength > 0;

                var length = inSet ? packedLength : -packedLength;

                var segment = (inSet, length);
                rowSegments.Add(segment);
                totalLength += length;

                if (totalLength == ViewPort.Resolution.Width)
                {
                    yield return new PointRow(ViewPort.Resolution.Width, y, rowSegments);
                    rowSegments.Clear();
                    totalLength = 0;
                    y++;
                }
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static void Write(
        string filePath,
        ViewPort viewPort,
        ComputationType computationType,
        IEnumerable<bool> pointsInSet)
    {
        using var stream = new FileStream(filePath, FileMode.Create);
        using var writer = new BinaryWriter(stream, Encoding.ASCII);
        writer.Write(HeaderText);
        writer.WriteViewPort(viewPort);
        writer.Write((int)computationType);

        int rowsWritten = 0;
        foreach (var row in pointsInSet.Batch(viewPort.Resolution.Width))
        {
            bool inSet = false;
            int length = 0;
            foreach (var point in row)
            {
                if (length == 0)
                {
                    inSet = point;
                    length = 1;
                }
                else if (point == inSet)
                {
                    length++;
                }
                else
                {
                    writer.Write(inSet ? length : -length);

                    inSet = point;
                    length = 1;
                }
            }

            writer.Write(inSet ? length : -length);
            rowsWritten++;
        }

        if (rowsWritten != viewPort.Resolution.Height)
        {
            throw new ArgumentException($"Expected {viewPort.Resolution.Height} rows but only got {rowsWritten}.");
        }
    }

    public static void Compute(
        string filePath,
        ViewPort viewPort,
        ComputationType computationType,
        CancellationToken token)
    {
        Log.Info($"Outputting to: {filePath}");
        Log.Info($"Resolution: {viewPort.Resolution.Width:N0}x{viewPort.Resolution.Height:N0}");
        Log.Info($"Area: {viewPort.Area}");
        Log.Info($"Computation type: {computationType}");

        IEnumerable<bool> GetPointsInSetScalar()
        {
            var kernel = KernelBuilder.BuildScalarKernel(computationType);

            var rowPointsInSet = new bool[viewPort.Resolution.Width];
            using var progress = TimedOperation.Start("points", totalWork: viewPort.Resolution.Area());
            for (int row = 0; row < viewPort.Resolution.Height; row++)
            {
                Parallel.For(
                    0,
                    viewPort.Resolution.Width,
                    col => rowPointsInSet[col] = kernel.FindEscapeTime(viewPort.GetComplex(col, row), Constant.IterationRange.Max).IsInfinite);

                for (int x = 0; x < viewPort.Resolution.Width; x++)
                {
                    yield return rowPointsInSet[x];
                }

                progress.AddWorkDone(viewPort.Resolution.Width);
            }
        }

        IEnumerable<bool> GetPointsInSetVectorDoubles()
        {
            using var progress = TimedOperation.Start("points", totalWork: viewPort.Resolution.Area());
            var vWidth = VectorDoubleKernel.Capacity;

            var vectorBatches = viewPort.Resolution.Width / vWidth;
            var remainder = viewPort.Resolution.Width % vWidth;
            if (remainder != 0)
            {
                vectorBatches++;
            }
            var lastIndex = vectorBatches - 1;

            var rowPointsInSet = new bool[viewPort.Resolution.Width];
            // TODO: Why is the Parallel.For inside a loop?
            for (int row = 0; row < viewPort.Resolution.Height; row++)
            {
                Parallel.For(
                    0,
                    vectorBatches,
                    batchIndex =>
                    {
                        var realBatch = new double[vWidth];
                        var imagBatch = new double[vWidth];
                        var times = new EscapeTime[vWidth];

                        var batchSize = (batchIndex == lastIndex) ? remainder : vWidth;

                        for (int i = 0; i < batchSize; i++)
                        {
                            var c = viewPort.GetComplex(batchIndex * vWidth + i, row);
                            realBatch[i] = c.Real;
                            imagBatch[i] = c.Imaginary;
                        }

                        VectorDoubleKernel.FindEscapeTimes(
                            realBatch, imagBatch, Constant.IterationRange.Max, times);

                        for (int i = 0; i < batchSize; i++)
                        {
                            rowPointsInSet[batchIndex * vWidth + i] = times[i].Iterations == Constant.IterationRange.Max;
                        }
                    });

                for (int x = 0; x < viewPort.Resolution.Width; x++)
                {
                    yield return rowPointsInSet[x];
                }

                progress.AddWorkDone(viewPort.Resolution.Width);
            }
        }

        IEnumerable<bool> ChooseEnumerator() =>
            computationType switch
            {
                ComputationType.ScalarDouble => GetPointsInSetScalar(),
                ComputationType.ScalarFloat => GetPointsInSetScalar(),
                ComputationType.VectorDouble => GetPointsInSetVectorDoubles(),
                _ => throw new ArgumentException("Unsupported computation type: " + computationType)
            };

        Write(filePath, viewPort, computationType, ChooseEnumerator());
    }
}