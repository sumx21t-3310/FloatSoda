using FloatSoda.Common.Geometries;
using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects.Painting;

public abstract class CustomClipper<T>
{
    public abstract T GetClip(SKSize size);

    public abstract bool ShouldReclip(CustomClipper<T> oldClipper);
}

public abstract class RenderCustomClip<T> : RenderProxyBox
{
    public CustomClipper<T>? Clipper
    {
        get;
        set
        {
            if (field == value) return;

            var oldClipper = value;
            field = value;

            if (value == null || oldClipper == null || value.GetType() != oldClipper.GetType() ||
                value.ShouldReclip(oldClipper))
            {
                MarkNeedsLayout();
            }
        }
    }


    public Clip ClipBehavior
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = Common.Layer.Clip.Antialias;

    protected T? Clip
    {
        get => Clipper != null ? Clipper.GetClip(Size) : DefaultClip;
        set => throw new NotImplementedException();
    }

    protected abstract T DefaultClip { get; }

    protected void MarkNeedsClip()
    {
        Clip = default;
        MarkNeedsPaint();
    }

    public override void PerformLayout()
    {
        var oldSize = Size;

        base.PerformLayout();

        if (oldSize != Size) Clip = default;
    }

    protected void UpdateClip()
    {
        if (Clipper != null) Clip ??= Clipper.GetClip(Size) ?? DefaultClip;
    }
}

public class RenderClipRect : RenderCustomClip<SKRect>
{
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            UpdateClip();
            Layer = context.PushClipRect(
                offset,
                Clip,
                (c, o) => base.Paint(c, o), ClipBehavior,
                Layer as ClipRectLayer);
        }
        else
        {
            Layer = null;
        }
    }
}

public class RenderClipRoundRect : RenderCustomClip<SKRoundRect>
{
    public BorderRadius BorderRadius { get; set; }

    protected override SKRoundRect DefaultClip => BorderRadius.ToRoundRect(SKRect.Create(Offset.Zero, Size));

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null)
        {
            UpdateClip();
            Layer = context.PushClipRoundRect(
                offset,
                Clip.Rect,
                Clip,
                (c, o) => base.Paint(c, o),
                ClipBehavior,
                Layer as ClipRoundRectLayer
            );
        }
        else
        {
            Layer = null;
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
        if (Child == null)
        {
            UpdateClip();
            Layer = context.PushClipPath(
                offset,
                SKRect.Create(Offset.Zero, Size),
                Clip,
                (c, o) => base.Paint(c, o),
                ClipBehavior,
                Layer as ClipPathLayer
            );
        }
        else
        {
            Layer = null;
        }
    }
}

public class RenderClipOval : RenderCustomClip<SKRect>
{
    protected override SKRect DefaultClip => SKRect.Create(Offset.Zero, Size);

    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child == null) return;

        UpdateClip();

        var path = new SKPath();
        path.AddOval(Clip);

        Layer = context.PushClipPath(
            offset,
            Clip,
            path,
            (c, o) => base.Paint(c, o),
            ClipBehavior,
            Layer as ClipPathLayer
        );
    }
}