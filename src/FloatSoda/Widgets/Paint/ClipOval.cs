using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

public record ClipOval : SingleChildRenderObjectWidget<RenderClipOval>
{
    public CustomClipper<SKRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;

    public override RenderClipOval CreateRenderObject()
    {
        return new RenderClipOval
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    public override void UpdateRenderObject(RenderClipOval renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

public record ClipRect : SingleChildRenderObjectWidget<RenderClipRect>
{
    public CustomClipper<SKRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderClipRect CreateRenderObject()
    {
        return new RenderClipRect
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    public override void UpdateRenderObject(RenderClipRect renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

public record ClipRoundRect : SingleChildRenderObjectWidget<RenderClipRoundRect>
{
    public BorderRadius BorderRadius;
    public CustomClipper<SKRoundRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderClipRoundRect CreateRenderObject()
    {
        return new RenderClipRoundRect
        {
            BorderRadius = BorderRadius,
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    public override void UpdateRenderObject(RenderClipRoundRect renderObject)
    {
        renderObject.BorderRadius = BorderRadius;
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}

public record ClipCustomPath : SingleChildRenderObjectWidget<RenderClipPath>
{
    public CustomClipper<SKPath>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderClipPath CreateRenderObject()
    {
        return new RenderClipPath()
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }

    public override void UpdateRenderObject(RenderClipPath renderObject)
    {
        renderObject.Clipper = Clipper;
        renderObject.ClipBehavior = ClipBehavior;
    }
}