using System.Diagnostics;
using Humanizer;

namespace Buddhabrot.Benchmarks;

public static class SimpleBenchmarker
{
	public static void Run<T>(Func<T> method, string name, int trials = 3)
	{
		Console.WriteLine($"{new string('-', 20)}\n{name}\nResult: {method()}");
		GC.Collect();
		var timer = Stopwatch.StartNew();

		for (int i = 0; i < trials; i++)
		{
			GC.KeepAlive(method());
		}

		timer.Stop();
		var avg = timer.Elapsed / 3.0;
		Console.WriteLine($"Took: {avg.Humanize(2)}");
	}
}
