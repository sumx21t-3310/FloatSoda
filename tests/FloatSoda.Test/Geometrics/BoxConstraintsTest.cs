using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Test.Geometrics;

public class BoxConstraintsTest
{
    // ── ファクトリメソッド ──────────────────────────────────────────

    [Fact]
    public void Tight_SetsMinMaxEqual()
    {
        var c = BoxConstraints.Tight(200, 150);

        Assert.Equal(200, c.MinWidth);
        Assert.Equal(200, c.MaxWidth);
        Assert.Equal(150, c.MinHeight);
        Assert.Equal(150, c.MaxHeight);
    }

    [Fact]
    public void Tight_SKSize_SetsMinMaxEqual()
    {
        var c = BoxConstraints.Tight(new SKSize(300, 400));

        Assert.Equal(300, c.MinWidth);
        Assert.Equal(300, c.MaxWidth);
        Assert.Equal(400, c.MinHeight);
        Assert.Equal(400, c.MaxHeight);
    }

    [Fact]
    public void TightFor_BothAxes_SetsMinMaxEqual()
    {
        var c = BoxConstraints.TightFor(width: 100, height: 80);

        Assert.Equal(100, c.MinWidth);
        Assert.Equal(100, c.MaxWidth);
        Assert.Equal(80, c.MinHeight);
        Assert.Equal(80, c.MaxHeight);
    }

    [Fact]
    public void TightFor_WidthOnly_HeightIsUnconstrained()
    {
        var c = BoxConstraints.TightFor(width: 100);

        Assert.Equal(100, c.MinWidth);
        Assert.Equal(100, c.MaxWidth);
        Assert.Equal(0, c.MinHeight);
        Assert.Equal(double.PositiveInfinity, c.MaxHeight);
    }

    [Fact]
    public void TightFor_HeightOnly_WidthIsUnconstrained()
    {
        var c = BoxConstraints.TightFor(height: 80);

        Assert.Equal(0, c.MinWidth);
        Assert.Equal(double.PositiveInfinity, c.MaxWidth);
        Assert.Equal(80, c.MinHeight);
        Assert.Equal(80, c.MaxHeight);
    }

    [Fact]
    public void TightFor_NoArgs_FullyUnconstrained()
    {
        var c = BoxConstraints.TightFor();

        Assert.Equal(0, c.MinWidth);
        Assert.Equal(double.PositiveInfinity, c.MaxWidth);
        Assert.Equal(0, c.MinHeight);
        Assert.Equal(double.PositiveInfinity, c.MaxHeight);
    }

    [Fact]
    public void Loose_SetsMinToZero()
    {
        var c = BoxConstraints.Loose(500, 300);

        Assert.Equal(0, c.MinWidth);
        Assert.Equal(500, c.MaxWidth);
        Assert.Equal(0, c.MinHeight);
        Assert.Equal(300, c.MaxHeight);
    }

    [Fact]
    public void Loose_SKSize_SetsMinToZero()
    {
        var c = BoxConstraints.Loose(new SKSize(500, 300));

        Assert.Equal(0, c.MinWidth);
        Assert.Equal(500, c.MaxWidth);
        Assert.Equal(0, c.MinHeight);
        Assert.Equal(300, c.MaxHeight);
    }

    [Fact]
    public void Default_StructDefault_IsAllZero()
    {
        // record struct のデフォルトコンストラクタはすべてのフィールドを 0 に初期化する
        var c = new BoxConstraints();

        Assert.Equal(0, c.MinWidth);
        Assert.Equal(0, c.MaxWidth);
        Assert.Equal(0, c.MinHeight);
        Assert.Equal(0, c.MaxHeight);
    }

    // ── Constrain ─────────────────────────────────────────────────

    [Fact]
    public void Constrain_SizeWithinBounds_Unchanged()
    {
        var c = new BoxConstraints(MinWidth: 50, MaxWidth: 200, MinHeight: 50, MaxHeight: 200);
        var result = c.Constrain(100, 100);

        Assert.Equal(100f, result.Width);
        Assert.Equal(100f, result.Height);
    }

    [Fact]
    public void Constrain_SizeBelowMinimum_ClampedToMin()
    {
        var c = new BoxConstraints(MinWidth: 100, MaxWidth: 300, MinHeight: 80, MaxHeight: 300);
        var result = c.Constrain(10, 20);

        Assert.Equal(100f, result.Width);
        Assert.Equal(80f, result.Height);
    }

    [Fact]
    public void Constrain_SizeAboveMaximum_ClampedToMax()
    {
        var c = new BoxConstraints(MinWidth: 0, MaxWidth: 200, MinHeight: 0, MaxHeight: 150);
        var result = c.Constrain(500, 400);

        Assert.Equal(200f, result.Width);
        Assert.Equal(150f, result.Height);
    }

    [Fact]
    public void Constrain_WithTightConstraints_AlwaysReturnsTightSize()
    {
        var c = BoxConstraints.Tight(120, 90);
        var result = c.Constrain(999, 999);

        Assert.Equal(120f, result.Width);
        Assert.Equal(90f, result.Height);
    }

    [Fact]
    public void Constrain_SKSize_SameAsScalarOverload()
    {
        var c = new BoxConstraints(MinWidth: 50, MaxWidth: 200, MinHeight: 50, MaxHeight: 200);
        var expected = c.Constrain(100, 100);
        var result = c.Constrain(new SKSize(100, 100));

        Assert.Equal(expected, result);
    }

    // ── ConstrainWidth / ConstrainHeight ──────────────────────────

    [Theory]
    [InlineData(50, 100, 200, 100)]   // within bounds
    [InlineData(10, 100, 200, 100)]   // below min → clamped to min
    [InlineData(999, 100, 200, 200)]  // above max → clamped to max
    public void ConstrainWidth_Clamps(double input, double min, double max, double expected)
    {
        var c = new BoxConstraints(MinWidth: min, MaxWidth: max, MinHeight: 0, MaxHeight: double.PositiveInfinity);
        Assert.Equal(expected, c.ConstrainWidth(input));
    }

    [Theory]
    [InlineData(80, 50, 150, 80)]    // within bounds
    [InlineData(10, 50, 150, 50)]    // below min → clamped to min
    [InlineData(999, 50, 150, 150)]  // above max → clamped to max
    public void ConstrainHeight_Clamps(double input, double min, double max, double expected)
    {
        var c = new BoxConstraints(MinWidth: 0, MaxWidth: double.PositiveInfinity, MinHeight: min, MaxHeight: max);
        Assert.Equal(expected, c.ConstrainHeight(input));
    }

    // ── Loosen ────────────────────────────────────────────────────

    [Fact]
    public void Loosen_ResetsMinToZero_KeepsMax()
    {
        var c = new BoxConstraints(MinWidth: 100, MaxWidth: 400, MinHeight: 80, MaxHeight: 300);
        var loosened = c.Loosen;

        Assert.Equal(0, loosened.MinWidth);
        Assert.Equal(400, loosened.MaxWidth);
        Assert.Equal(0, loosened.MinHeight);
        Assert.Equal(300, loosened.MaxHeight);
    }

    // ── Smallest ──────────────────────────────────────────────────

    [Fact]
    public void Smallest_ReturnsConstrainedZero()
    {
        var c = new BoxConstraints(MinWidth: 60, MaxWidth: 200, MinHeight: 40, MaxHeight: 200);
        var smallest = c.Smallest;

        Assert.Equal(60f, smallest.Width);
        Assert.Equal(40f, smallest.Height);
    }

    // ── Enforce ───────────────────────────────────────────────────

    [Fact]
    public void Enforce_ClampsToParentBounds()
    {
        var child = new BoxConstraints(MinWidth: 0, MaxWidth: 500, MinHeight: 0, MaxHeight: 500);
        var parent = new BoxConstraints(MinWidth: 50, MaxWidth: 200, MinHeight: 30, MaxHeight: 150);

        var enforced = child.Enforce(parent);

        Assert.Equal(50, enforced.MinWidth);
        Assert.Equal(200, enforced.MaxWidth);
        Assert.Equal(30, enforced.MinHeight);
        Assert.Equal(150, enforced.MaxHeight);
    }

    [Fact]
    public void Enforce_TightChild_ClampedToParentMax()
    {
        var child = BoxConstraints.Tight(999, 999);
        var parent = new BoxConstraints(MinWidth: 0, MaxWidth: 300, MinHeight: 0, MaxHeight: 200);

        var enforced = child.Enforce(parent);

        Assert.Equal(300, enforced.MinWidth);
        Assert.Equal(300, enforced.MaxWidth);
        Assert.Equal(200, enforced.MinHeight);
        Assert.Equal(200, enforced.MaxHeight);
    }
}
