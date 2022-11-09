using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buddhabrot.Core.Utilities;

public sealed class RingBuffer<T> : IEnumerable<T>
{
    private readonly T[] _buffer;
    private int _start;
    public int Capacity => _buffer.Length;
    public int Count { get; private set; }

    public RingBuffer(int capacity)
    {
        _buffer = new T[capacity];
    }

    public void Add(T item)
    {
        var nextIndex = (_start + Count + 1) % Capacity;
        _buffer[nextIndex] = item;

        Count = Math.Min(Count + 1, Capacity);
    }

    public IEnumerator<T> GetEnumerator()
    {
        for(int i = 0; i < Count; i++)
        {
            var index = (i + _start) % Capacity;
            yield return _buffer[index];    
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
