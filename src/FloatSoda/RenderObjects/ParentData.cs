using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.RenderObjects;

public interface IParentData;

public class BoxParentData(Offset offset = default) : IParentData
{
    public Offset Offset { get; set; } = offset;
}