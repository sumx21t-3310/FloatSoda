using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using SkiaSharp;

namespace FloatSoda.Render;

public interface IParentData;

public record BoxParentData(Offset Offset) : IParentData;

public abstract class RenderObject
{
    public IParentData? ParentData { get; set; }

    public abstract SKSize Size { get; protected set; }

    public abstract void Layout(BoxConstraints constraints);
    public abstract void Paint(PaintingContext context, Offset offset);
}

public abstract class RenderBox : RenderObject
{
    public override SKSize Size { get; protected set; } = SKSize.Empty;
}

public abstract class RenderProxyBox(RenderBox? child = null) : RenderBox
{
    public RenderBox? Child { get; set; } = child;

    public override void Layout(BoxConstraints constraints)
    {
        if (Child != null)
        {
            Child.Layout(constraints);
            Size = Child.Size;
        }
        else
        {
            Size = constraints.Smallest;
        }
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderConstrainedBox(BoxConstraints additionalConstraints, RenderBox? child) : RenderProxyBox(child)
{
    public override void Layout(BoxConstraints constraints)
    {
        var enforcedConstraints = additionalConstraints.Enforce(constraints);
        Child?.Layout(enforcedConstraints);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderColoredBox : RenderProxyBox
{
    public SKColor Color { get; set; } = SKColors.Black;

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (!Size.IsEmpty)
        {
            context.Canvas.DrawRect(Size.And(offset), new SKPaint { Color = Color });
        }

        Child?.Paint(context, offset);
    }
}

public class RenderView(float width, float height) : RenderObject
{
    public override SKSize Size { get; protected set; } = new(width, height);

    public RenderBox? Child { get; set; }
    public ContainerLayer Layer { get; } = new TransformLayer();


    public void PerformLayout() => Layout(BoxConstraints.Tight(Size));
    public override void Layout(BoxConstraints constraints) => Child?.Layout(constraints);

    public override void Paint(PaintingContext context, Offset offset) => Child?.Paint(context, offset);
}

public class RenderPositionedBox : RenderBox
{
    public override void Layout(BoxConstraints constraints)
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        throw new NotImplementedException();
    }
}

public class RenderCustomClip : RenderBox
{
    public override void Layout(BoxConstraints constraints)
    {
        throw new NotImplementedException();
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        throw new NotImplementedException();
    }
}