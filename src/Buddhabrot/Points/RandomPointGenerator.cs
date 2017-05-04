using System;
using System.Collections.Generic;
using Buddhabrot.Core;

namespace Buddhabrot.Points
{
    public sealed class RandomPointGenerator
    {
        private readonly ComplexArea[] _areas;
        private readonly Random _random;

        public RandomPointGenerator(ComplexArea[] areas, int? seed = null)
        {
            _areas = areas;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public IEnumerable<FComplex> GetPoints(int batchSize)
        {
            for (int i = 0; i < batchSize; i++)
            {
                FComplex point;
                do
                {
                    var areaIndex = _random.Next(_areas.Length);
                    var area = _areas[areaIndex];

                    float GetRandomInRange(Range range) =>
                        range.Magnitude * (float)_random.NextDouble() + range.InclusiveMin;

                    var imag = GetRandomInRange(area.ImagRange);

                    // The areas are only on the positive side of the real axis, so we have to randomly move them down
                    if (_random.Next(2) % 2 == 0)
                    {
                        imag = -imag;
                    }

                    point = new FComplex(
                        GetRandomInRange(area.RealRange),
                        imag);
                } while (MandelbulbChecker.IsInsideBulbs(point));
                yield return point;
            }
        }
    }
}
