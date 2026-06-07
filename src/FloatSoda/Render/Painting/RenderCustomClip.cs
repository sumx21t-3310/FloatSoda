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

    protected T Clip => Clipper != null ? Clipper.GetClip(Size) : DefaultClip;
    protected abstract T DefaultClip { get; }
}

public class RenderClipRect : RenderCustomClip<SKRect>
{
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            context.PushClipRect(offset, Clip, (c, o) => base.Paint(c, o), ClipBehavior);
        }
    }
}

public class RenderClipRoundRect : RenderCustomClip<SKRoundRect>
{
    public required BorderRadius BorderRadius { get; init; }

    protected override SKRoundRect DefaultClip => BorderRadius.ToRoundRect(SKRect.Create(Offset.Zero, Size));

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            context.PushClipRoundRect(offset, Clip.Rect, Clip, (c, o) => base.Paint(c, o), ClipBehavior);
        }
    }
}

public class RenderClipPath : RenderCustomClip<SKPath>
{
    protected override SKPath DefaultClip
    {
        get
        {
            var path = new SKPath();
            path.AddRect(SKRect.Create(Offset.Zero, Size));
            return path;
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            context.PushClipPath(
                offset,
                SKRect.Create(Offset.Zero, Size),
                Clip,
                (c, o) => base.Paint(c, o),
                ClipBehavior
            );
        }
    }
}

public class RenderClipOval : RenderCustomClip<SKRect>
{
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    public override void Paint(PaintingContext context, Offset offset)
    {
        var path = new SKPath();
        path.AddOval(Clip);
        context.PushClipPath(offset, Clip, path, (c, o) => base.Paint(c, o), ClipBehavior);
    }
}