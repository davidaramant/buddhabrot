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
    [ProtoMember(4)] public int MaxX { get; set; }
    [ProtoMember(5)] public int MaxY { get; set; }
    [ProtoMember(6)] public int[] QuadTreeNodes { get; set; } = Array.Empty<int>();

    public void Save(Stream stream) => Serializer.Serialize(stream, this);
    public static Boundaries Load(Stream stream) => Serializer.Deserialize<Boundaries>(stream);
}