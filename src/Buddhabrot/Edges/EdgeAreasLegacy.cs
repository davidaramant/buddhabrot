using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;
using log4net;

namespace Buddhabrot.Edges
{
    public sealed class EdgeAreasLegacy
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeAreasLegacy));

        private const string HeaderText = "Float Areas V1.0";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        private readonly List<EdgeAreaLegacy> _areas;
        public int AreaCount => _areas.Count;

        public static EdgeAreasLegacy CreateCompressed(
            Size gridResolution,
            ComplexArea viewPort,
            IEnumerable<EdgeAreaLegacy> areas)
        {
            var compressedAreas = CompressAreas(areas.ToList());
            return new EdgeAreasLegacy(gridResolution, viewPort, compressedAreas);
        }

        private EdgeAreasLegacy(
            Size gridResolution,
            ComplexArea viewPort,
            List<EdgeAreaLegacy> areas)
        {
            GridResolution = gridResolution;
            ViewPort = viewPort;
            _areas = areas;
        }

        public IEnumerable<Point> GetAreaLocations()
        {
            foreach (var area in _areas)
            {
                for (int yDelta = 0; yDelta < area.Dimensions.Height; yDelta++)
                {
                    for (int xDelta = 0; xDelta < area.Dimensions.Width; xDelta++)
                    {
                        yield return area.GridLocation.OffsetBy(xDelta, yDelta);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the complex areas, repeated by how large they are.
        /// </summary>
        /// <returns>An array of complex areas.  References to areas that are larger than a single square are repeated.</returns>
        public ComplexArea[] GetDistributedComplexAreas()
        {
            float realIncrement = ViewPort.RealRange.Magnitude / (GridResolution.Width - 1);
            float imagIncrement = ViewPort.ImagRange.Magnitude / (GridResolution.Height - 1);

            float GetRealValue(int x) => ViewPort.RealRange.InclusiveMin + x * realIncrement;
            float GetImagValue(int y) => ViewPort.ImagRange.InclusiveMin + y * imagIncrement;

            return _areas.SelectMany(ea =>
                {
                    var area = new ComplexArea(
                        realRange: new FloatRange(GetRealValue(ea.GridLocation.X), GetRealValue(ea.GridLocation.X + ea.Dimensions.Width)),
                        imagRange: new FloatRange(GetImagValue(ea.GridLocation.Y), GetImagValue(ea.GridLocation.Y + ea.Dimensions.Height)));

                    return Enumerable.Repeat(area, ea.Dimensions.Area());
                })
                .ToArray();
        }

        public EdgeAreasLegacy CreateCompressedVersion()
        {
            // Guard against re-compressing the areas by expanding them.
            // In case a fancier compression algorithm gets put in, this can be used to re-compress an old file.
            return new EdgeAreasLegacy(
                GridResolution,
                ViewPort,
                CompressAreas(GetAreaLocations().Select(location => new EdgeAreaLegacy(location)).ToList()));
        }

        private static List<EdgeAreaLegacy> CompressAreas(List<EdgeAreaLegacy> areas)
        {
            Log.Info($"Compressing {areas.Count:N0} areas...");

            List<EdgeAreaLegacy> CompressVertically(List<EdgeAreaLegacy> inputAreas)
            {
                return CompressAreas(
                    inputAreas,
                    compressionDimension: ea => ea.GridLocation.Y,
                    secondaryDimension: ea => ea.GridLocation.X,
                    sizeCreator: height => new Size(1, height));
            }
            List<EdgeAreaLegacy> CompressHorizontally(List<EdgeAreaLegacy> inputAreas)
            {
                return CompressAreas(
                    inputAreas,
                    compressionDimension: ea => ea.GridLocation.X,
                    secondaryDimension: ea => ea.GridLocation.Y,
                    sizeCreator: width => new Size(width, 1));
            }

            var vhCompressed = CompressHorizontally(CompressVertically(areas));
            var hvCompressed = CompressVertically(CompressHorizontally(areas));

            Log.Info($"Compressed vertically, then horizontally: {vhCompressed.Count:N0} areas");
            Log.Info($"Compressed hoizontally, then vertically: {hvCompressed.Count:N0} areas");

            return hvCompressed.Count < vhCompressed.Count
                ? hvCompressed
                : vhCompressed;
        }

        /// <summary>
        /// Compresses the areas using run-length encoding.
        /// </summary>
        /// <param name="areas">The areas.</param>
        /// <param name="compressionDimension">Which dimension to perform RLE on.</param>
        /// <param name="secondaryDimension">The secondary dimension.</param>
        /// <param name="sizeCreator">How to turn the scalar length into a size.</param>
        /// <returns>
        /// Compressed areas.
        /// </returns>
        private static List<EdgeAreaLegacy> CompressAreas(
            List<EdgeAreaLegacy> areas,
            Func<EdgeAreaLegacy, int> compressionDimension,
            Func<EdgeAreaLegacy, int> secondaryDimension,
            Func<int, Size> sizeCreator)
        {
            var compressedAreas = areas.Where(ea => ea.Dimensions.Area() > 1).ToList();

            foreach (var slice in areas.Where(ea => ea.Dimensions.Area() == 1).GroupBy(secondaryDimension).OrderBy(group => group.Key))
            {
                EdgeAreaLegacy start = null;
                EdgeAreaLegacy end = null;
                int length = 0;

                foreach (var edge in slice.OrderBy(compressionDimension))
                {
                    if (start == null)
                    {
                        start = edge;
                        end = edge;
                        length = 1;
                    }
                    else if (compressionDimension(start) + length == compressionDimension(edge))
                    {
                        end = edge;
                        length++;
                    }
                    else
                    {
                        compressedAreas.Add(new EdgeAreaLegacy(
                            start.GridLocation,
                            sizeCreator(length)));

                        start = edge;
                        end = edge;
                        length = 1;
                    }
                }

                if (start != null && end != null)
                {
                    compressedAreas.Add(new EdgeAreaLegacy(
                        start.GridLocation,
                        sizeCreator(length)));
                }
            }
            return compressedAreas;
        }

        public void Write(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, MagicBytes);

            using (var stream = new FileStream(filePath, FileMode.Append))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteSize(GridResolution);
                writer.WriteComplexArea(ViewPort);

                int count = 0;
                foreach (var area in _areas)
                {
                    count++;
                    writer.WriteEdgeArea(area);
                }
                Log.Info($"Wrote {count:N0} edge areas.");
            }
        }

        public static EdgeAreasLegacy Load(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var header = new byte[MagicBytes.Length];
                var bytesRead = stream.Read(header, 0, header.Length);
                if (bytesRead != header.Length || Encoding.ASCII.GetString(header) != HeaderText)
                    throw new InvalidOperationException($"Edges file was corrupt: {filePath}");

                using (var reader = new BinaryReader(stream))
                {
                    var size = reader.ReadSize();
                    var viewPort = reader.ReadComplexArea();

                    var edgeAreas = new List<EdgeAreaLegacy>();

                    while (stream.Position < stream.Length)
                    {
                        edgeAreas.Add(reader.ReadEdgeAreaLegacy());
                    }

                    Log.Info($"Loaded edges with resolution ({size.Width:N0}x{size.Height:N0}), view port {viewPort}, and {edgeAreas.Count:N0} edge areas.");

                    return new EdgeAreasLegacy(
                        size,
                        viewPort,
                        edgeAreas);
                }
            }
        }
    }
}
