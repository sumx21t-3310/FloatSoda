using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Test.Geometrics;

public class BorderRadiusTest
{
    // ── Radius ─────────────────────────────────────────────────────

    [Fact]
    public void Radius_Constructor_SetsXY()
    {
        var r = new Radius(10, 20);

        Assert.Equal(10f, r.X);
        Assert.Equal(20f, r.Y);
    }

    [Fact]
    public void Radius_Zero_IsBothZero()
    {
        Assert.Equal(0f, Radius.Zero.X);
        Assert.Equal(0f, Radius.Zero.Y);
    }

    [Fact]
    public void Radius_Circular_SetsBothAxesEqual()
    {
        var r = Radius.Circular(15);

        Assert.Equal(15f, r.X);
        Assert.Equal(15f, r.Y);
    }

    [Fact]
    public void Radius_ImplicitToSKPoint()
    {
        SKPoint point = new Radius(8, 16);

        Assert.Equal(8f, point.X);
        Assert.Equal(16f, point.Y);
    }

    // ── BorderRadius ファクトリ ────────────────────────────────────

    [Fact]
    public void BorderRadius_All_SetsAllCornersEqual()
    {
        var radius = new Radius(12, 6);
        var br = BorderRadius.All(radius);

        Assert.Equal(radius, br.TopLeft);
        Assert.Equal(radius, br.TopRight);
        Assert.Equal(radius, br.BottomRight);
        Assert.Equal(radius, br.BottomLeft);
    }

    [Fact]
    public void BorderRadius_Zero_IsAllZeroCorners()
    {
        var br = BorderRadius.Zero;

        Assert.Equal(Radius.Zero, br.TopLeft);
        Assert.Equal(Radius.Zero, br.TopRight);
        Assert.Equal(Radius.Zero, br.BottomRight);
        Assert.Equal(Radius.Zero, br.BottomLeft);
    }

    [Fact]
    public void BorderRadius_Circular_SetsAllCornersToCircularRadius()
    {
        var br = BorderRadius.Circular(20);

        var expected = Radius.Circular(20);
        Assert.Equal(expected, br.TopLeft);
        Assert.Equal(expected, br.TopRight);
        Assert.Equal(expected, br.BottomRight);
        Assert.Equal(expected, br.BottomLeft);
    }

    [Fact]
    public void BorderRadius_Constructor_SetsEachCornerIndependently()
    {
        var tl = new Radius(10, 10);
        var tr = new Radius(20, 20);
        var br = new Radius(30, 30);
        var bl = new Radius(40, 40);

        var borderRadius = new BorderRadius(tl, tr, br, bl);

        Assert.Equal(tl, borderRadius.TopLeft);
        Assert.Equal(tr, borderRadius.TopRight);
        Assert.Equal(br, borderRadius.BottomRight);
        Assert.Equal(bl, borderRadius.BottomLeft);
    }

    [Fact]
    public void BorderRadius_Default_IsAllZeroCorners()
    {
        var br = new BorderRadius();

        Assert.Equal(default(Radius), br.TopLeft);
        Assert.Equal(default(Radius), br.TopRight);
        Assert.Equal(default(Radius), br.BottomRight);
        Assert.Equal(default(Radius), br.BottomLeft);
    }

    // ── ToRoundRect ────────────────────────────────────────────────

    [Fact]
    public void ToRoundRect_PreservesRect()
    {
        var rect = new SKRect(10, 20, 210, 120);
        var roundRect = BorderRadius.Circular(8).ToRoundRect(rect);

        Assert.Equal(rect, roundRect.Rect);
    }

    [Fact]
    public void ToRoundRect_Circular_AllCornersHaveSameRadius()
    {
        var rect = SKRect.Create(0, 0, 200, 100);
        var roundRect = BorderRadius.Circular(10).ToRoundRect(rect);

        var tl = roundRect.GetRadii(SKRoundRectCorner.UpperLeft);
        var tr = roundRect.GetRadii(SKRoundRectCorner.UpperRight);
        var br = roundRect.GetRadii(SKRoundRectCorner.LowerRight);
        var bl = roundRect.GetRadii(SKRoundRectCorner.LowerLeft);

        Assert.Equal(new SKPoint(10, 10), tl);
        Assert.Equal(new SKPoint(10, 10), tr);
        Assert.Equal(new SKPoint(10, 10), br);
        Assert.Equal(new SKPoint(10, 10), bl);
    }

    [Fact]
    public void ToRoundRect_IndependentCorners_MapsCorrectly()
    {
        var rect = SKRect.Create(0, 0, 200, 100);
        var borderRadius = new BorderRadius(
            TopLeft:     new Radius(5, 5),
            TopRight:    new Radius(10, 10),
            BottomRight: new Radius(15, 15),
            BottomLeft:  new Radius(20, 20)
        );

        var roundRect = borderRadius.ToRoundRect(rect);

        Assert.Equal(new SKPoint(5,  5),  roundRect.GetRadii(SKRoundRectCorner.UpperLeft));
        Assert.Equal(new SKPoint(10, 10), roundRect.GetRadii(SKRoundRectCorner.UpperRight));
        Assert.Equal(new SKPoint(15, 15), roundRect.GetRadii(SKRoundRectCorner.LowerRight));
        Assert.Equal(new SKPoint(20, 20), roundRect.GetRadii(SKRoundRectCorner.LowerLeft));
    }

    [Fact]
    public void ToRoundRect_EllipticalRadius_MapsXYCorrectly()
    {
        var rect = SKRect.Create(0, 0, 200, 100);
        var borderRadius = BorderRadius.All(new Radius(30, 15));

        var roundRect = borderRadius.ToRoundRect(rect);

        var tl = roundRect.GetRadii(SKRoundRectCorner.UpperLeft);
        Assert.Equal(30f, tl.X);
        Assert.Equal(15f, tl.Y);
    }

    [Fact]
    public void ToRoundRect_ZeroRadius_ProducesSharpCorners()
    {
        var rect = SKRect.Create(0, 0, 100, 100);
        var roundRect = BorderRadius.Zero.ToRoundRect(rect);

        Assert.Equal(new SKPoint(0, 0), roundRect.GetRadii(SKRoundRectCorner.UpperLeft));
        Assert.Equal(new SKPoint(0, 0), roundRect.GetRadii(SKRoundRectCorner.UpperRight));
        Assert.Equal(new SKPoint(0, 0), roundRect.GetRadii(SKRoundRectCorner.LowerRight));
        Assert.Equal(new SKPoint(0, 0), roundRect.GetRadii(SKRoundRectCorner.LowerLeft));
    }
}
