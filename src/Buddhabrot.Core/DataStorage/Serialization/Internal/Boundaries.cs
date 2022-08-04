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
    [ProtoMember(1)] public int VerticalDivisions { get; set; }
    [ProtoMember(2)] public int MaximumIterations { get; set; }
    
    // Legacy property from when I was capping X/Y to be shorts
    [ProtoMember(3)] public int[] EncodedRegions { get; set; } = Array.Empty<int>();
    [ProtoMember(4)] public RegionLocation[] Regions { get; set; } = Array.Empty<RegionLocation>();


    public void Save(Stream stream) => Serializer.Serialize(stream, this);
    public static Boundaries Load(Stream stream) => Serializer.Deserialize<Boundaries>(stream);
}