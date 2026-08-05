using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class IndexedStackTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(40, 40);

    private sealed class LayoutCounterBox(double width, double height) : RenderBox
    {
        public int LayoutCount { get; private set; }

        public override void PerformLayout()
        {
            LayoutCount++;
            Size = Constraints.Constrain(width, height);
        }

        public override void Paint(PaintingContext context, Offset offset) { }
    }

    [Fact]
    public void PerformLayout_選択されていない子も含めて全子をレイアウトする()
    {
        var first = new LayoutCounterBox(10, 20);
        var second = new LayoutCounterBox(30, 15);
        var stack = new RenderIndexedStack { Index = 1, Children = { first, second } };

        stack.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(1, first.LayoutCount);
        Assert.Equal(1, second.LayoutCount);
        Assert.Equal(new SKSize(30, 20), stack.Size);
    }

    [Theory]
    [InlineData(0, 255, 0, 0)]
    [InlineData(1, 0, 0, 255)]
    public void IndexedStack_Indexで選択した子だけを描画する(int index, byte red, byte green, byte blue)
    {
        using var bitmap = Renderer.Render(BuildWidget(index), Size);

        Assert.Equal(new SKColor(red, green, blue), bitmap.GetPixel(20, 20));
    }

    [Fact]
    public void HitTestChildren_Indexで選択した子だけがヒットする()
    {
        var first = ListenerRenderBox();
        var second = ListenerRenderBox();
        var stack = new RenderIndexedStack { Index = 1, Children = { first, second } };
        stack.Layout(BoxConstraints.Tight(40, 40));

        var result = new HitTestResult();
        Assert.True(stack.HitTest(result, new Offset(20, 20)));
        Assert.DoesNotContain(result.Path, entry => ReferenceEquals(entry.Target, first));
        Assert.Contains(result.Path, entry => ReferenceEquals(entry.Target, second));
    }

    [Fact]
    public void Index_変更時_レイアウトせず描画とヒット対象だけを更新する()
    {
        var first = ListenerRenderBox();
        var second = ListenerRenderBox();
        var stack = new RenderIndexedStack { Index = 0, Children = { first, second } };
        stack.Layout(BoxConstraints.Tight(40, 40));
        stack.NeedsPaint = false;

        stack.Index = 1;

        Assert.False(stack.NeedsLayout);
        Assert.True(stack.NeedsPaint);
        var result = new HitTestResult();
        Assert.True(stack.HitTest(result, new Offset(20, 20)));
        Assert.Contains(result.Path, entry => ReferenceEquals(entry.Target, second));
    }

    [Fact]
    public void Index_null_全子を描画せずヒットテストしない()
    {
        using var bitmap = Renderer.Render(BuildWidget(null), Size);
        Assert.Equal(0, bitmap.GetPixel(20, 20).Alpha);

        var stack = new RenderIndexedStack { Index = null, Children = { ListenerRenderBox() } };
        stack.Layout(BoxConstraints.Tight(40, 40));
        Assert.False(stack.HitTest(new HitTestResult(), new Offset(20, 20)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(2)]
    public void CreateRenderObject_IndexがChildren範囲外_ArgumentOutOfRangeExceptionを投げる(int index)
    {
        var widget = BuildWidget(index);
        Assert.Throws<ArgumentOutOfRangeException>(widget.CreateRenderObject);
    }

    private static IndexedStack BuildWidget(int? index) => new()
    {
        Index = index,
        Children =
        [
            new SizedBox
            {
                Width = 40,
                Height = 40,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            },
            new SizedBox
            {
                Width = 40,
                Height = 40,
                Child = new ColoredBox { Color = new Color(0, 0, 255) }
            }
        ]
    };

    private static RenderPointerListener ListenerRenderBox() => new()
    {
        Child = new RenderColoredBox
        {
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 40) }
        }
    };
}
