using ProtoBuf;

namespace Buddhabrot.Core.DataStorage.Serialization.Internal;

[ProtoContract]
public sealed class RegionLocation
{
	[ProtoMember(1)]
	public int X { get; init; }

	[ProtoMember(2)]
	public int Y { get; init; }
}

[ProtoContract]
public sealed class Boundaries
{
	[ProtoMember(1)]
	public int VerticalPower { get; init; }

	[ProtoMember(2)]
	public int MaximumIterations { get; init; }

	[ProtoMember(3)]
	public RegionLocation[] Regions { get; init; } = [];

	public void Save(Stream stream) => Serializer.Serialize(stream, this);

	public static Boundaries Load(Stream stream) => Serializer.Deserialize<Boundaries>(stream);
}

[ProtoContract]
public sealed class PersistedQuadtree
{
	[ProtoMember(1)]
	public int Height { get; init; }

	[ProtoMember(2)]
	public uint[] Nodes { get; init; } = [];

	public void Save(Stream stream) => Serializer.Serialize(stream, this);

	public static PersistedQuadtree Load(Stream stream) => Serializer.Deserialize<PersistedQuadtree>(stream);
}
