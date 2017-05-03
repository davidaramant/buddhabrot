using System.Drawing;
using System.IO;
using Buddhabrot.Core;
using Buddhabrot.Edges;

namespace Buddhabrot.Extensions
{
    public static class BinarySerializationExtensions
    {
        public static void WriteRange(this BinaryWriter writer, Range range)
        {
            writer.Write(range.InclusiveMin);
            writer.Write(range.ExclusiveMax);
        }

        public static Range ReadRange(this BinaryReader reader) => new Range(reader.ReadSingle(), reader.ReadSingle());

        public static void WriteComplexArea(this BinaryWriter writer, ComplexArea area)
        {
            writer.WriteRange(area.RealRange);
            writer.WriteRange(area.ImagRange);
        }

        public static ComplexArea ReadComplexArea(this BinaryReader reader) => new ComplexArea(reader.ReadRange(), reader.ReadRange());

        public static void WriteSize(this BinaryWriter writer, Size size)
        {
            writer.Write(size.Width);
            writer.Write(size.Height);
        }
        public static Size ReadSize(this BinaryReader reader) => new Size(reader.ReadInt32(), reader.ReadInt32());

        public static void WritePoint(this BinaryWriter writer, Point p)
        {
            writer.Write(p.X);
            writer.Write(p.Y);
        }

        public static Point ReadPoint(this BinaryReader reader) => new Point(reader.ReadInt32(), reader.ReadInt32());

        public static void WriteEdgeArea(this BinaryWriter writer, EdgeArea edgeArea)
        {
            writer.WritePoint(edgeArea.GridLocation);
            writer.WriteSize(edgeArea.Dimensions);
        }

        public static EdgeArea ReadEdgeArea(this BinaryReader reader) => new EdgeArea(reader.ReadPoint(), reader.ReadSize());
    }
}
