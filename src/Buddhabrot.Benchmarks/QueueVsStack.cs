using BenchmarkDotNet.Attributes;

namespace Buddhabrot.Benchmarks;

public class QueueVsStackBenchmark
{
    [Params(10_000, 100_000)] public int CutOff { get; set; }

    [Benchmark]
    public void Queue()
    {
        var queue = new Queue<int>();
        queue.Enqueue(1);
        int iterationsToAdd = CutOff;

        while (queue.Any())
        {
            queue.Dequeue();

            if (iterationsToAdd-- > 0)
            {
                for (int adds = 0; adds < 8; adds++)
                {
                    queue.Enqueue(iterationsToAdd);
                }
            }
        }
    }

    [Benchmark]
    public void Stack()
    {
        var stack = new Stack<int>();
        stack.Push(1);
        int iterationsToAdd = CutOff;

        while (stack.Any())
        {
            stack.Pop();

            if (iterationsToAdd-- > 0)
            {
                for (int adds = 0; adds < 8; adds++)
                {
                    stack.Push(iterationsToAdd);
                }
            }
        }
    }
}