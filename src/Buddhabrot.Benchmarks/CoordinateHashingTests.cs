using Buddhabrot.ManualVisualizations;
using Humanizer;
using Tedd;

namespace Buddhabrot.Benchmarks;

public static class CoordinateHashingTests
{
	public static void ComputeHistograms()
	{
		var sd = VisitedRegionsDataSet.Load();

		var dataPath = Path.Combine(
			DataLocation.CreateDirectory("Benchmarks", nameof(CoordinateHashingTests).Humanize()).FullName
		);

		var hashcode64 = new int[64];
		var bitpacking64 = new int[64];
		var morton64 = new int[64];

		var hashcode256 = new int[256];
		var bitpacking256 = new int[256];
		var morton256 = new int[256];

		for (int i = 0; i < sd.Count; i++)
		{
			if (sd.GetCommandType(i) == VisitedRegionsDataSet.CommandType.Add)
			{
				var x = sd.X[i];
				var y = sd.Y[i];

				var h64 = ((uint)HashCode.Combine(x, y)) % 64;
				hashcode64[h64]++;

				var bp64 = (y & 0b111) << 3 | (x & 0b111);
				bitpacking64[bp64]++;

				var m64 = MortonEncoding.Encode((uint)x, (uint)y) % 64;
				morton64[m64]++;

				var h256 = ((uint)HashCode.Combine(x, y)) % 256;
				hashcode256[h256]++;

				var bp256 = (y & 0b1111) << 4 | (x & 0b1111);
				bitpacking256[bp256]++;

				var m256 = MortonEncoding.Encode((uint)x, (uint)y) % 256;
				morton256[m256]++;
			}
		}

		using (var h64File = File.Open(Path.Combine(dataPath, "h64.csv"), FileMode.Create))
		{
			using var writer = new StreamWriter(h64File);

			writer.WriteLine("Method," + string.Join(',', Enumerable.Range(0, 64)));
			writer.WriteLine("HashCode.Combine," + string.Join(',', hashcode64));
			writer.WriteLine("Bit packing," + string.Join(',', bitpacking64));
			writer.WriteLine("Morton Code," + string.Join(',', morton64));
		}

		using (var h256File = File.Open(Path.Combine(dataPath, "h256.csv"), FileMode.Create))
		{
			using var writer = new StreamWriter(h256File);

			writer.WriteLine("Method," + string.Join(',', Enumerable.Range(0, 256)));
			writer.WriteLine("HashCode.Combine," + string.Join(',', hashcode256));
			writer.WriteLine("Bit packing," + string.Join(',', bitpacking256));
			writer.WriteLine("Morton Code," + string.Join(',', morton256));
		}
	}
}
