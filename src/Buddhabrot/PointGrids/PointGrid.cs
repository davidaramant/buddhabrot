using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using Buddhabrot.IterationKernels;
using Buddhabrot.Utility;
using log4net;

namespace Buddhabrot.PointGrids
{
    public sealed class PointGrid : IDisposable, IEnumerable<PointRow>
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(PointGrid));
        //                                 0123456789abcdef
        private const string HeaderText = "Point Grid V2.00";

        public Size PointResolution { get; }
        public ComplexArea ViewPort { get; }
        public ComputationType ComputationType { get; }

        private readonly Stream _pointStream;
        private readonly long _pointsPosition;

        public static PointGrid Load(string filePath)
        {
            FileStream stream = null;

            try
            {
                stream = File.OpenRead(filePath);

                using (var reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true))
                {
                    var readHeader = reader.ReadString(); ;
                    if (readHeader != HeaderText)
                        throw new InvalidOperationException($"Unsupported edge span file format: {filePath}");

                    var size = reader.ReadSize();
                    var viewPort = reader.ReadComplexArea();
                    var computationType = (ComputationType)reader.ReadInt32();
                    Log.Info($"Loaded point grid with resolution ({size.Width:N0}x{size.Height:N0}), " +
                             $"view port {viewPort}.");
                    return new PointGrid(size, viewPort, computationType, stream);
                }
            }
            catch (Exception)
            {
                stream?.Dispose();
                throw;
            }
        }

        private PointGrid(
            Size pointResolution,
            ComplexArea viewPort,
            ComputationType computationType,
            Stream pointStream)
        {
            PointResolution = pointResolution;
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

                    if (totalLength == PointResolution.Width)
                    {
                        yield return new PointRow(PointResolution.Width, y, rowSegments);
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
            Size pointResolution,
            ComplexArea viewPort,
            ComputationType computationType,
            IEnumerable<bool> pointsInSet)
        {
            using (var stream = new FileStream(filePath, FileMode.Create))
            using (var writer = new BinaryWriter(stream, Encoding.ASCII))
            {
                writer.Write(HeaderText);
                writer.WriteSize(pointResolution);
                writer.WriteComplexArea(viewPort);
                writer.Write((int)computationType);

                int rowsWritten = 0;
                foreach (var row in pointsInSet.Batch(pointResolution.Width))
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

                if (rowsWritten != pointResolution.Height)
                {
                    throw new ArgumentException($"Expected {pointResolution.Height} rows but only got {rowsWritten}.");
                }
            }
        }

        public static void Compute(
            string filePath,
            Size pointResolution,
            ComplexArea viewPort,
            ComputationType computationType,
            CancellationToken token)
        {
            Log.Info($"Outputting to: {filePath}");
            Log.Info($"Resolution: {pointResolution.Width:N0}x{pointResolution.Height:N0}");
            Log.Info($"View port: {viewPort}");
            Log.Info($"Computation type: {computationType}");

            Func<Complex, bool> GetComputationMethod()
            {
                switch (computationType)
                {
                    case ComputationType.ScalarDouble:
                        return c => ScalarDoubleKernel.FindEscapeTime(c).IsInfinite;
                    case ComputationType.ScalarFloat:
                        return c => ScalarFloatKernel.FindEscapeTime(c).IsInfinite;
                    default:
                        throw new ArgumentException("Unsupported computation type for this operation: " + computationType);
                }
            }

            Func<Complex, bool> isInSet = GetComputationMethod();


            IEnumerable<bool> GetPointsInSet()
            {
                var pointCalculator = new PositionCalculator(pointResolution, viewPort);

                var pointsInSet = new bool[pointResolution.Width];
                using (var progress = TimedOperation.Start("Computing point grid", totalWork: pointResolution.Area()))
                {
                    for (int row = 0; row < pointResolution.Height; row++)
                    {
                        Parallel.For(
                            0,
                            pointResolution.Width,
                            col => pointsInSet[col] = isInSet(pointCalculator.GetPoint(col, row)));

                        for (int x = 0; x < pointResolution.Width; x++)
                        {
                            yield return pointsInSet[x];
                        }

                        progress.AddWorkDone(pointResolution.Width);
                    }
                }
            }

            Write(filePath, pointResolution, viewPort, computationType, GetPointsInSet());
        }
    }
}
