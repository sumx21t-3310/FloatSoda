using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Render.Painting;

public abstract class CustomClipper<T>
{
    public abstract T GetClip(SKSize size);

    public abstract bool ShouldReclip(CustomClipper<T> oldClipper);
}

public abstract class RenderCustomClip<T> : RenderProxyBox
{
    public CustomClipper<T>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Common.Layer.Clip.Antialias;

    protected abstract T DefaultClip { get; }
    protected T Clip => Clipper != null ? Clipper.GetClip(Size) : DefaultClip;
}

public class RenderClipRect : RenderCustomClip<SKRect>
{
    protected override SKRect DefaultClip => Size.And(Offset.Zero);

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            
        }
    }
}

public class RenderClipRoundRect : RenderCustomClip<SKRoundRect>
{
    protected override SKRoundRect DefaultClip { get; }
}

public class RenderClipPath : RenderCustomClip<SKPath>
{
    protected override SKPath DefaultClip { get; }
}