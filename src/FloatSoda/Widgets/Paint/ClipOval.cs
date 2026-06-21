using FloatSoda.Common.Layer;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

public record ClipOval : SingleChildRenderObjectWidget
{
    public CustomClipper<SKRect>? CustomClipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;

    public override RenderObject CreateRenderObject()
    {
        return new RenderClipOval
        {
            Clipper = CustomClipper,
            ClipBehavior = ClipBehavior
        };
    }
}

public record ClipRect : SingleChildRenderObjectWidget
{
    public CustomClipper<SKRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderObject CreateRenderObject()
    {
        return new RenderClipRect
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }
}

public record ClipRoundRect : SingleChildRenderObjectWidget
{
    public CustomClipper<SKRoundRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderObject CreateRenderObject()
    {
        return new RenderClipRoundRect
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }
}

public record ClipCustomPath : SingleChildRenderObjectWidget
{
    public CustomClipper<SKPath>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderObject CreateRenderObject()
    {
        return new RenderClipPath()
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
    }
}