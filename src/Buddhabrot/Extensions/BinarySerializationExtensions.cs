using System.Drawing;
using System.IO;
using System.Numerics;
using Buddhabrot.Core;

namespace Buddhabrot.Extensions
{
    public static class BinarySerializationExtensions
    {
        public static void WriteRange(this BinaryWriter writer, DoubleRange range)
        {
            writer.Write(range.InclusiveMin);
            writer.Write(range.ExclusiveMax);
        }

        public static DoubleRange ReadRange(this BinaryReader reader) => new DoubleRange(reader.ReadDouble(), reader.ReadDouble());

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

        public static void WriteViewPort(this BinaryWriter writer, ViewPort viewPort)
        {
            writer.WriteComplexArea(viewPort.Area);
            writer.WriteSize(viewPort.Resolution);
        }

        public static ViewPort ReadViewPort(this BinaryReader reader) => new ViewPort(reader.ReadComplexArea(), reader.ReadSize());

        public static void WritePoint(this BinaryWriter writer, Point p)
        {
            writer.Write(p.X);
            writer.Write(p.Y);
        }

        public static Point ReadPoint(this BinaryReader reader) => new Point(reader.ReadInt32(), reader.ReadInt32());

        public static void WriteComplex(this BinaryWriter writer, Complex complex)
        {
            writer.Write(complex.Real);
            writer.Write(complex.Imaginary);
        }

        public static Complex ReadComplex(this BinaryReader reader) => new Complex(reader.ReadDouble(), reader.ReadDouble());
    }
}
