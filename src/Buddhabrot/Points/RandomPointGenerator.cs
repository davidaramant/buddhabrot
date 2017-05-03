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

                    point = new FComplex(
                        GetRandomInRange(area.RealRange),
                        GetRandomInRange(area.ImagRange));
                } while (MandelbulbChecker.IsInsideBulbs(point));
                yield return point;
            }
        }
    }
}
