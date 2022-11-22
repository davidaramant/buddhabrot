using ProtoBuf;

namespace Buddhabrot.Core.DataStorage.Serialization.Internal;

[ProtoContract]
public sealed class RegionLocation
{
    [ProtoMember(1)] public int X { get; set; }
    [ProtoMember(2)] public int Y { get; set; }
}

[ProtoContract]
public sealed class Boundaries
{
    [ProtoMember(1)] public int VerticalPower { get; set; }
    [ProtoMember(2)] public int MaximumIterations { get; set; }
    [ProtoMember(3)] public RegionLocation[] Regions { get; set; } = Array.Empty<RegionLocation>();
    
    public void Save(Stream stream) => Serializer.Serialize(stream, this);
    public static Boundaries Load(Stream stream) => Serializer.Deserialize<Boundaries>(stream);
}

[ProtoContract]
public sealed class PersistedQuadTree
{
    [ProtoMember(1)] public int Height { get; set; }
    [ProtoMember(2)] public uint[] Nodes { get; set; } = Array.Empty<uint>();
    
    public void Save(Stream stream) => Serializer.Serialize(stream, this);
    public static PersistedQuadTree Load(Stream stream) => Serializer.Deserialize<PersistedQuadTree>(stream);
}