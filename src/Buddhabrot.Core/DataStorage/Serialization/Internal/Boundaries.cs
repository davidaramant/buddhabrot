using ProtoBuf;

namespace Buddhabrot.Core.DataStorage.Serialization.Internal;

[ProtoContract]
public sealed class Boundaries
{
    [ProtoMember(1)]
    public int VerticalDivisions { get; set; }
    [ProtoMember(2)]
    public int MaximumIterations { get; set; }
    [ProtoMember(3)] 
    public int[] Regions { get; set; } = Array.Empty<int>();

    public void Save(Stream stream) => Serializer.Serialize(stream, this);
    public static Boundaries Load(Stream stream) => Serializer.Deserialize<Boundaries>(stream);
}