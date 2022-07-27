using SkiaSharp;

namespace Buddhabrot.Core.Images;

public sealed class RasterImage : IDisposable
{
    private readonly int _scale;
    private readonly SKBitmap _bitmap;

    public int Width { get; }
    public int Height { get; }
    public int PixelCount => Width * Height;

    public RasterImage(System.Drawing.Size resolution, int scale = 1) : this(resolution.Width, resolution.Height, scale)
    {
    }

    public RasterImage(int width, int height, int scale = 1)
    {
        _scale = scale;
        Width = width;
        Height = height;
        _bitmap = new SKBitmap(width, height);
    }

    private static SKColor ToSKColor(System.Drawing.Color color) =>
        new SKColor(red: color.R, green: color.G, blue: color.B);
    
    public void Fill(System.Drawing.Color color)
    {
        using var canvas = new SKCanvas(_bitmap);
        canvas.Clear(ToSKColor(color));
    }

    public void SetPixel(int x, int y, System.Drawing.Color color)
    {
        _bitmap.SetPixel(x, y, ToSKColor(color));
    }

    public void SetPixel(int pixelIndex, System.Drawing.Color color)
    {
        var x = pixelIndex % Width;
        var y = pixelIndex / Width;

        SetPixel(x, y, color);
    }

    /// <summary>
    /// Saves the image to the specified file path.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    public void Save(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Create);

        if (_scale != 1)
        {
            var resizedWidth = _scale * Width;
            var resizedHeight = _scale * Height;

            using var surface = SKSurface.Create(
                new SKImageInfo
                {
                    Width = resizedWidth,
                    Height = resizedHeight,
                    ColorType = SKImageInfo.PlatformColorType,
                    AlphaType = SKAlphaType.Premul
                });
            using var paint = new SKPaint { IsAntialias = false, FilterQuality = SKFilterQuality.None };

            using var img = SKImage.FromBitmap(_bitmap);

            surface.Canvas.DrawImage(
                img,
                new SKRectI(0, 0, resizedWidth, resizedHeight),
                paint);
            surface.Canvas.Flush();

            using var newImg = surface.Snapshot();
            using var data = Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" => newImg.Encode(SKEncodedImageFormat.Jpeg, quality: 85),
                ".png" => newImg.Encode(SKEncodedImageFormat.Png, quality: 100),
                _ => throw new ArgumentException("Unsupported file format.")
            };

            data.SaveTo(stream);
        }
        else
        {
            switch (Path.GetExtension(filePath))
            {
                case ".jpg":
                    _bitmap.Encode(stream, SKEncodedImageFormat.Jpeg, quality: 85);
                    break;
                case ".png":
                    _bitmap.Encode(stream, SKEncodedImageFormat.Png, quality: 100);
                    break;
                default:
                    throw new ArgumentException("Unsupported file format.");
            }
        }
    }

    public void Dispose() => _bitmap.Dispose();
}