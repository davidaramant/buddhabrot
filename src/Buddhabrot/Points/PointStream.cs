using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using Buddhabrot.Core;
using Buddhabrot.Extensions;

namespace Buddhabrot.Points;

public sealed class PointStream : IDisposable, IEnumerable<Complex>
{
    private static readonly ILog Log = Logger.Create<PointStream>();
    //                                 0123456789abcdef
    private const string HeaderText = "Point List V1.00";

    public ViewPort ViewPort { get; }
    public int Count { get; }

    private readonly Stream _pointStream;
    private readonly long _pointsPosition;

    public static PointStream Load(string filePath)
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

                var viewPort = reader.ReadViewPort();
                var count = reader.ReadInt32();
                Log.Info($"Points with viewport ({viewPort.Resolution.Width:N0}x{viewPort.Resolution.Height:N0}), " +
                         $"Count {count}.");
                return new PointStream(viewPort, count, stream);
            }
        }
        catch (Exception)
        {
            stream?.Dispose();
            throw;
        }
    }

    private PointStream(
        ViewPort viewPort,
        int count,
        Stream pointStream)
    {
        ViewPort = viewPort;
        Count = count;
        _pointStream = pointStream;
        _pointsPosition = pointStream.Position;
    }

    public void Dispose() => _pointStream.Dispose();

    public IEnumerator<Complex> GetEnumerator()
    {
        _pointStream.Position = _pointsPosition;
        using (var reader = new BinaryReader(_pointStream, Encoding.ASCII, leaveOpen: true))
        {
            while (_pointStream.Position < _pointStream.Length)
            {
                yield return reader.ReadComplex();
            }
        }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

    public static void Write(
        string filePath,
        ViewPort viewPort, 
        IEnumerable<Complex> points)
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        using (var writer = new BinaryWriter(stream, Encoding.ASCII))
        {
            writer.Write(HeaderText);
            writer.WriteViewPort(viewPort);

            long countPosition = stream.Position;
            stream.Position += sizeof(int);

            int count = 0;
            foreach (var point in points)
            {
                count++;
                writer.WriteComplex(point);
            }

            Log.Info($"Wrote {count:N0} points.");

            stream.Position = countPosition;
            writer.Write(count);
        }
    }
}