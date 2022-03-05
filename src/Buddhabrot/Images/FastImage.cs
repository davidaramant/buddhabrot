using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Buddhabrot.Images;

public sealed class FastImage
{
    public const int JpgQuality = 85;

    const PixelFormat Format = PixelFormat.Format24bppRgb;
    readonly int _pixelSizeInBytes = Image.GetPixelFormatSize(Format) / 8;
    readonly int _stride;
    readonly byte[] _pixelBuffer;

    public int Width { get; }
    public int Height { get; }
    public int PixelCount => Width * Height;

    public FastImage(int tileSize) : this(tileSize, tileSize)
    {
    }

    public FastImage(Size resolution) : this(resolution.Width, resolution.Height)
    {
    }

    public FastImage(int width, int height)
    {
        Width = width;
        Height = height;
        _stride = width * _pixelSizeInBytes;
        _pixelBuffer = new byte[_stride * height];
    }

    public void Fill(Color color)
    {
        Parallel.For(
            0,
            PixelCount,
            pixelIndex => SetPixel(pixelIndex, color));
    }

    public void SetPixel(Point p, Color color) => SetPixel(p.X, p.Y, color);

    public void SetPixel(int x, int y, Color color)
    {
        var index = y * _stride + x * _pixelSizeInBytes;
        SetPixelFromIndex(index, color);
    }

    public void SetPixel(int pixelIndex, Color color)
    {
        var index = pixelIndex * _pixelSizeInBytes;
        SetPixelFromIndex(index, color);
    }

    private void SetPixelFromIndex(int index, Color color)
    {
        _pixelBuffer[index] = color.B;
        _pixelBuffer[index + 1] = color.G;
        _pixelBuffer[index + 2] = color.R;
    }

    /// <summary>
    /// Saves the image to the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public void Save(string filePath)
    {
        using (var bmp = new Bitmap(Width, Height, Format))
        {
            var bmpData = bmp.LockBits(
                new Rectangle(0, 0, Width, Height),
                ImageLockMode.WriteOnly,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Copy the RGB values back to the bitmap
            Marshal.Copy(_pixelBuffer, 0, ptr, _pixelBuffer.Length);

            bmp.UnlockBits(bmpData);

            switch (Path.GetExtension(filePath))
            {
                case ".jpg":
                case ".jpeg":
                    bmp.Save(filePath, JgpEncoder, QualitySetting);
                    break;
                case ".png":
                    bmp.Save(filePath);
                    break;
                default:
                    throw new ArgumentException("Unsupported file format.");
            }
        }
    }

    private static readonly ImageCodecInfo JgpEncoder = GetEncoder(ImageFormat.Jpeg);
    private static readonly EncoderParameters QualitySetting = CreateQualityParameter();

    private static ImageCodecInfo GetEncoder(ImageFormat format) =>
        ImageCodecInfo.GetImageDecoders().FirstOrDefault(codec => codec.FormatID == format.Guid);

    private static EncoderParameters CreateQualityParameter()
    {
        var encoderParams = new EncoderParameters(1);
        var encoderParam = new EncoderParameter(Encoder.Quality, (long)JpgQuality);
        encoderParams.Param[0] = encoderParam;

        return encoderParams;
    }
}