using System.Diagnostics.CodeAnalysis;

namespace Buddhabrot.Core.Images;

public static class ImageSizeDescription
{
	[SuppressMessage("ReSharper", "IdentifierTypo")]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public static string ToBase2(long pixels)
	{
		const long kibipixel = 1_024;
		const long mebipixel = kibipixel * kibipixel;
		const long gibipixel = kibipixel * mebipixel;
		const long tebipixel = kibipixel * gibipixel;

		return pixels switch
		{
			< kibipixel => $"{pixels} pixels",
			< mebipixel => $"{pixels / kibipixel} kibipixels",
			< gibipixel => $"{pixels / mebipixel} mebipixels",
			< tebipixel => $"{pixels / gibipixel} gibipixels",
			_ => $"{pixels / tebipixel:N0} tebipixels",
		};
	}

	[SuppressMessage("ReSharper", "IdentifierTypo")]
	[SuppressMessage("ReSharper", "StringLiteralTypo")]
	public static string ToMetric(long pixels)
	{
		const long kilopixel = 1_000;
		const long megapixel = kilopixel * kilopixel;
		const long gigapixel = kilopixel * megapixel;
		const long terapixel = kilopixel * gigapixel;

		return pixels switch
		{
			< kilopixel => $"{pixels} pixels",
			< megapixel => $"{(double)pixels / kilopixel:N1} kilopixels",
			< gigapixel => $"{(double)pixels / megapixel:N1} megapixels",
			< terapixel => $"{(double)pixels / gigapixel:N1} gigapixels",
			_ => $"{(double)pixels / terapixel:N1} terapixels",
		};
	}
}
