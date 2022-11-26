﻿namespace Buddhabrot.Core.Boundary;

/// <summary>
/// Dimensions for a quad in region coordinates (+Y is up)
/// </summary>
public readonly record struct QuadDimensions(
    int X,
    int Y,
    int Height)
{
    public int SideLength => 1 << (Height - 1);
    public int QuadrantLength => 1 << (Height - 2);

    public bool Contains(RegionId id) =>
        id.X >= X &&
        id.Y >= Y &&
        id.X < (X + SideLength) &&
        id.Y < (Y + SideLength);

    public QuadDimensions Expand() => this with {Height = Height + 1};
    public QuadDimensions LL => this with {Height = Height - 1};
    public QuadDimensions LR => this with {X = X + QuadrantLength, Height = Height - 1};
    public QuadDimensions UL => this with {Y = Y + QuadrantLength, Height = Height - 1};
    public QuadDimensions UR => new(X: X + QuadrantLength, Y: Y + QuadrantLength, Height: Height - 1);

    public Quadrant DetermineQuadrant(RegionId id)
    {
        var xComponent = (id.X >= X + QuadrantLength) ? 1 : 0;
        var yComponent = (id.Y >= Y + QuadrantLength) ? 2 : 0;

        return (Quadrant) (xComponent + yComponent);
    }

    public QuadDimensions GetQuadrant(Quadrant quadrant)
    {
        var isLeft = (int) quadrant % 2;
        var isLower = (int) quadrant / 2;

        return new(
            X: X + isLeft * QuadrantLength,
            Y: Y + isLower * QuadrantLength,
            Height: Height - 1);
    }

    public override string ToString() => $"{{X: {X}, Y:{Y}, Height: {Height}}}";
}