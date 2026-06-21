using FloatSoda.Common.Layer;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

namespace FloatSoda.Widgets.Paint;

public record ClipOval : SingleChildRenderObjectWidget<RenderClipOval>
{
    public CustomClipper<SKRect>? CustomClipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;

    public override RenderClipOval CreateRenderObject()
    {
        return new RenderClipOval
        {
            Clipper = CustomClipper,
            ClipBehavior = ClipBehavior
        };
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
}

public record ClipRoundRect : SingleChildRenderObjectWidget<RenderClipRoundRect>
{
    public CustomClipper<SKRoundRect>? Clipper { get; init; } = null;
    public Clip ClipBehavior { get; init; } = Clip.Antialias;


    public override RenderClipRoundRect CreateRenderObject()
    {
        return new RenderClipRoundRect
        {
            Clipper = Clipper,
            ClipBehavior = ClipBehavior
        };
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
}