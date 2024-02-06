using System.Numerics;
using System.Runtime.CompilerServices;

namespace Buddhabrot.Core;

public static class ComplexExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double MagnitudeSquared(this Complex c) => c.Real * c.Real + c.Imaginary * c.Imaginary;
}
