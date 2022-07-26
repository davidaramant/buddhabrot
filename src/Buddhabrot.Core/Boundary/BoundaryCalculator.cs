using System.Collections.Concurrent;
using System.Numerics;
using Buddhabrot.Core.IterationKernels;

namespace Buddhabrot.Core.Boundary;

public static class BoundaryCalculator
{
    readonly record struct LatticePoint(int EncodedPosition)
    {
        public int X => ushort.MaxValue & EncodedPosition;
        public int Y => EncodedPosition >> 16;

        public LatticePoint(int x, int y) : this((y << 16) + x)
        {
        }
    }

    record AreaCorners(
        bool LowerLeft,
        bool LowerRight,
        bool UpperRight,
        bool UpperLeft)
    {
        public bool IsUpperEdge => UpperLeft != UpperRight;
        public bool IsLowerEdge => LowerLeft != LowerRight;
        public bool IsLeftEdge => LowerLeft != UpperLeft;
        public bool IsRightEdge => LowerRight != UpperRight;

        public bool ContainsBorder => IsUpperEdge || IsLowerEdge || IsLeftEdge || IsRightEdge;
    }

    public static async Task<IReadOnlyList<AreaId>> FindBoundaryAreas(
        AreaSizeInfo areaSizeInfo,
        IProgress<AreaId> progress,
        CancellationToken cancelToken = default)
    {
        ConcurrentDictionary<LatticePoint, bool> isPointInSet = new();
        Dictionary<AreaId, bool> doesAreaContainBorder = new();
        Queue<AreaId> idsToCheck = new();
        idsToCheck.Enqueue(new AreaId(0, 0));

        while (idsToCheck.Any())
        {
            var currentId = idsToCheck.Dequeue();

            var corners = await GetAreaCorners(currentId);
            
            doesAreaContainBorder.Add(currentId, corners.ContainsBorder);

            if (corners.IsUpperEdge)
            {
                AddIdToCheck(currentId.X, currentId.Y + 1);
            }

            if (corners.IsLowerEdge)
            {
                AddIdToCheck(currentId.X, currentId.Y - 1);
            }

            if (corners.IsLeftEdge)
            {
                AddIdToCheck(currentId.X - 1, currentId.Y);
            }

            if (corners.IsRightEdge)
            {
                AddIdToCheck(currentId.X + 1, currentId.Y);
            }
            
            progress.Report(currentId);
        }

        return doesAreaContainBorder.Where(pair => pair.Value).Select(pair => pair.Key).ToList();

        void AddIdToCheck(int x, int y)
        {
            if (x < 0 || y < 0 || x >= areaSizeInfo.VerticalDivisions * 2 || y >= areaSizeInfo.VerticalDivisions)
                return;

            var id = new AreaId(x, y);
            
            if (doesAreaContainBorder.ContainsKey(id))
                return;
            
            idsToCheck.Enqueue(id);
        }

        async Task<AreaCorners> GetAreaCorners(AreaId id)
        {
            var lowerLeftTask = IsPointInSet(new LatticePoint(id.X, id.Y));
            var lowerRightTask = IsPointInSet(new LatticePoint(id.X + 1, id.Y));
            var upperRightTask = IsPointInSet(new LatticePoint(id.X + 1, id.Y + 1));
            var upperLeftTask = IsPointInSet(new LatticePoint(id.X, id.Y + 1));

            await Task.WhenAll(lowerLeftTask, lowerRightTask, upperRightTask, upperLeftTask);

            return new AreaCorners(
                lowerLeftTask.Result,
                lowerRightTask.Result,
                upperRightTask.Result,
                upperLeftTask.Result);
        }

        async Task<bool> IsPointInSet(LatticePoint point)
        {
            if (isPointInSet.TryGetValue(point, out var inSet))
            {
                return inSet;
            }

            Complex c = new Complex(
                real: point.X * areaSizeInfo.SideLength - 2,
                imaginary: point.Y * areaSizeInfo.SideLength);
            inSet = await Task.Run(() => ScalarDoubleKernel.IsInSet(c), cancelToken);
            isPointInSet.TryAdd(point, inSet);
            return inSet;
        }
    }
}