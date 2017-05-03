﻿using System;
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
    public sealed class EdgeAreas
    {
        private static readonly ILog Log = LogManager.GetLogger(nameof(EdgeLocator));

        private const string HeaderText = "Float Areas V1.0";
        // A sixteen byte magic header value
        private static readonly byte[] MagicBytes = Encoding.ASCII.GetBytes(HeaderText);

        public Size GridResolution { get; }
        public ComplexArea ViewPort { get; }
        private readonly List<EdgeArea> _areas;
        public int AreaCount => _areas.Count;

        private EdgeAreas(
            Size gridResolution,
            ComplexArea viewPort,
            List<EdgeArea> areas)
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

        public EdgeAreas Compress()
        {
            var compressedAreas = new List<EdgeArea>();

            foreach (var col in _areas.GroupBy(ea => ea.GridLocation.X).OrderBy(col => col.Key))
            {
                EdgeArea start = null;
                EdgeArea end = null;
                int height = 0;

                foreach (var edge in col.OrderBy(ea => ea.GridLocation.Y))
                {
                    if (start == null)
                    {
                        start = edge;
                        end = edge;
                        height = 1;
                    }
                    else if (start.GridLocation.Y + height == edge.GridLocation.Y)
                    {
                        end = edge;
                        height++;
                    }
                    else
                    {
                        compressedAreas.Add(new EdgeArea(
                            start.GridLocation,
                            new Size(1, height)));

                        start = edge;
                        end = edge;
                        height = 1;
                    }
                }

                if (start != null && end != null)
                {
                    compressedAreas.Add(new EdgeArea(
                        start.GridLocation,
                        new Size(1, height)));
                }
            }

            return new EdgeAreas(GridResolution, ViewPort, compressedAreas);
        }

        public void Write(string filePath)
        {
            Write(filePath, GridResolution, ViewPort, _areas);
        }

        public static void Write(
            string filePath,
            Size gridResolution,
            ComplexArea viewPort,
            IEnumerable<EdgeArea> areas)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            File.WriteAllBytes(filePath, MagicBytes);

            using (var stream = new FileStream(filePath, FileMode.Append))
            using (var writer = new BinaryWriter(stream))
            {
                writer.WriteSize(gridResolution);
                writer.WriteComplexArea(viewPort);

                int count = 0;
                foreach (var area in areas)
                {
                    count++;
                    writer.WriteEdgeArea(area);
                }
                Log.Info($"Wrote {count} edge areas.");
            }
        }

        public static EdgeAreas Load(string filePath)
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

                    var edgeAreas = new List<EdgeArea>();

                    while (stream.Position < stream.Length)
                    {
                        edgeAreas.Add(reader.ReadEdgeArea());
                    }

                    Log.Info($"Loaded edges with resolution ({size.Width:N0}x{size.Height:N0}), view port {viewPort}, and {edgeAreas.Count} edge areas.");

                    return new EdgeAreas(
                        size,
                        viewPort,
                        edgeAreas);
                }
            }
        }
    }
}
