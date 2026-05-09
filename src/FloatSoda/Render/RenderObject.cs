using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;

namespace FloatSoda.Render;

public interface IParentData;

public record BoxParentData(Offset Offset) : IParentData;

public abstract class RenderObject
{
    public IParentData? ParentData { get; set; }

    public abstract Size Size { get; }

    public abstract void Layout(BoxConstraints boxConstraints);
    public abstract void Paint(PaintingContext context, Offset offset);
}

public abstract class RenderBox : RenderObject
{
    public override Size Size { get; } = Size.Zero;
}

public abstract class RenderProxyBox(RenderBox? child = null) : RenderBox
{
    public override void Layout(BoxConstraints boxConstraints)
    {
        if (child != null)
        {
        }
    }
}

public class RenderConstrainedBox(BoxConstraints additionalConstraints, RenderBox? child) : RenderBox
{
    public override void Layout(BoxConstraints boxConstraints)
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        throw new NotImplementedException();
    }
}

public class RenderView(float width, float height, ContainerLayer layer) : RenderObject
{
    public override Size Size => new(width, height);

    public RenderBox? Child { get; set; }
    public ContainerLayer Layer { get; } = new TransformLayer();


    public void PerformLayout() => Layout(BoxConstraints.Tight(Size));
    public override void Layout(BoxConstraints boxConstraints) => Child?.Layout(boxConstraints);

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}