namespace Buddhabrot.Core.Boundary;

public enum VisitedRegionType
{
	/// <summary>
	/// A region that was not visited. Unknown what lurks there.
	/// </summary>
	Unknown,
	Border,
	Filament,
	Rejected,
}
