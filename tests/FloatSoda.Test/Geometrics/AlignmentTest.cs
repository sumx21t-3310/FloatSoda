using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.Test.Geometrics;

public class AlignmentTest
{
    // ── 静的プリセット ─────────────────────────────────────────────

    public static IEnumerable<object[]> PresetData =>
    [
        [Alignment.TopLeft,      -1f, -1f],
        [Alignment.TopCenter,     0f, -1f],
        [Alignment.TopRight,      1f, -1f],
        [Alignment.CenterLeft,   -1f,  0f],
        [Alignment.Center,        0f,  0f],
        [Alignment.CenterRight,   1f,  0f],
        [Alignment.BottomLeft,   -1f,  1f],
        [Alignment.BottomCenter,  0f,  1f],
        [Alignment.BottomRight,   1f,  1f],
    ];

    [Theory]
    [MemberData(nameof(PresetData))]
    public void Preset_HasCorrectXY(Alignment alignment, float expectedX, float expectedY)
    {
        Assert.Equal(expectedX, alignment.X);
        Assert.Equal(expectedY, alignment.Y);
    }

    // ── Pivot ──────────────────────────────────────────────────────
    // Pivot(size) = (size.W/2 * X, size.H/2 * Y)
    // size=200x100 → centerX=100, centerY=50

    public static IEnumerable<object[]> PivotData =>
    [
        [Alignment.TopLeft,      -100.0, -50.0],
        [Alignment.TopCenter,       0.0, -50.0],
        [Alignment.TopRight,      100.0, -50.0],
        [Alignment.CenterLeft,   -100.0,   0.0],
        [Alignment.Center,          0.0,   0.0],
        [Alignment.CenterRight,   100.0,   0.0],
        [Alignment.BottomLeft,   -100.0,  50.0],
        [Alignment.BottomCenter,    0.0,  50.0],
        [Alignment.BottomRight,   100.0,  50.0],
    ];

    [Theory]
    [MemberData(nameof(PivotData))]
    public void Pivot_ReturnsCorrectOffset(Alignment alignment, double expectedX, double expectedY)
    {
        var pivot = alignment.Pivot(new SKSize(200, 100));

        Assert.Equal(expectedX, pivot.X);
        Assert.Equal(expectedY, pivot.Y);
    }

    [Fact]
    public void Pivot_ZeroSize_ReturnsZeroOffset()
    {
        var pivot = Alignment.BottomRight.Pivot(SKSize.Empty);

        Assert.Equal(0.0, pivot.X);
        Assert.Equal(0.0, pivot.Y);
    }

    // ── ComputeOffset ──────────────────────────────────────────────
    // parent=1000x600, child=100x100
    // remaining=(900,500), centerSpace=(450,250)
    // result = (450*(1+X), 250*(1+Y))

    public static IEnumerable<object[]> ComputeOffsetData =>
    [
        [Alignment.TopLeft,        0.0,   0.0],
        [Alignment.TopCenter,    450.0,   0.0],
        [Alignment.TopRight,     900.0,   0.0],
        [Alignment.CenterLeft,     0.0, 250.0],
        [Alignment.Center,       450.0, 250.0],
        [Alignment.CenterRight,  900.0, 250.0],
        [Alignment.BottomLeft,     0.0, 500.0],
        [Alignment.BottomCenter, 450.0, 500.0],
        [Alignment.BottomRight,  900.0, 500.0],
    ];

    [Theory]
    [MemberData(nameof(ComputeOffsetData))]
    public void ComputeOffset_PlacesChildCorrectly(Alignment alignment, double expectedX, double expectedY)
    {
        var parent = new SKSize(1000, 600);
        var child = new SKSize(100, 100);

        var offset = alignment.ComputeOffset(parent, child);

        Assert.Equal(expectedX, offset.X);
        Assert.Equal(expectedY, offset.Y);
    }

    [Fact]
    public void ComputeOffset_SameSize_ReturnsZero()
    {
        // 親と子が同サイズ → remaining が (0,0) → すべて (0,0)
        var size = new SKSize(300, 200);
        var offset = Alignment.BottomRight.ComputeOffset(size, size);

        Assert.Equal(0.0, offset.X);
        Assert.Equal(0.0, offset.Y);
    }

    [Fact]
    public void ComputeOffset_Custom_HalfAlignment()
    {
        // X=0.5, Y=-0.5 → result = (450*(1+0.5), 250*(1-0.5)) = (675, 125)
        var alignment = new Alignment(0.5f, -0.5f);
        var offset = alignment.ComputeOffset(new SKSize(1000, 600), new SKSize(100, 100));

        Assert.Equal(675.0, offset.X);
        Assert.Equal(125.0, offset.Y);
    }

    // ── Flip ───────────────────────────────────────────────────────

    [Fact]
    public void FlipAll_NegatesBothAxes()
    {
        var result = Alignment.TopLeft.FlipAll();

        Assert.Equal(Alignment.BottomRight, result);
    }

    [Fact]
    public void FlipAll_Center_RemainsCenter()
    {
        var result = Alignment.Center.FlipAll();

        Assert.Equal(Alignment.Center, result);
    }

    [Fact]
    public void FlipX_NegatesXOnly()
    {
        var result = Alignment.CenterLeft.FlipX();

        Assert.Equal(Alignment.CenterRight, result);
    }

    [Fact]
    public void FlipY_NegatesYOnly()
    {
        var result = Alignment.TopCenter.FlipY();

        Assert.Equal(Alignment.BottomCenter, result);
    }

    [Fact]
    public void FlipX_ThenFlipX_RestoresOriginal()
    {
        var original = Alignment.TopRight;
        var result = original.FlipX().FlipX();

        Assert.Equal(original, result);
    }

    [Fact]
    public void FlipAll_ThenFlipAll_RestoresOriginal()
    {
        var original = Alignment.BottomLeft;
        var result = original.FlipAll().FlipAll();

        Assert.Equal(original, result);
    }
}
