using FloatSoda.Common.Geometries;

namespace FloatSoda.Render;

public interface IParentData;

public class BoxParentData(Offset offset = default) : IParentData
{
    public Offset Offset { get; set; } = offset;
}