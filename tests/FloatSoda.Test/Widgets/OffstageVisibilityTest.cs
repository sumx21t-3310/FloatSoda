using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class OffstageVisibilityTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(40, 40);

    [Fact]
    public void PerformLayout_Offstageでも子をレイアウトする()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(20, 10) };
        var offstage = new RenderOffstage { Offstage = true, Child = child };

        offstage.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(new SKSize(20, 10), child.Size);
        Assert.Equal(SKSize.Empty, offstage.Size);
    }

    [Fact]
    public void Offstage_IsOffstageがtrue_描画せずヒットテストしない()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Offstage
            {
                IsOffstage = true,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);
        Assert.Equal(0, bitmap.GetPixel(20, 20).Alpha);

        var renderChild = new RenderColoredBox();
        var renderOffstage = new RenderOffstage { Offstage = true, Child = renderChild };
        renderOffstage.Layout(BoxConstraints.Tight(40, 40));
        Assert.Equal(new SKSize(40, 40), renderChild.Size);
        Assert.False(renderOffstage.HitTest(new HitTestResult(), new Offset(20, 20)));
    }

    [Fact]
    public void Offstage_IsOffstageがfalse_描画してヒットテストする()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Offstage
            {
                IsOffstage = false,
                Child = new ColoredBox { Color = new Color(255, 0, 0) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 20));

        var renderOffstage = new RenderOffstage { Offstage = false, Child = new RenderColoredBox() };
        renderOffstage.Layout(BoxConstraints.Tight(40, 40));
        Assert.True(renderOffstage.HitTest(new HitTestResult(), new Offset(20, 20)));
    }

    [Theory]
    [InlineData(true, 255, 0, 0)]
    [InlineData(false, 0, 0, 255)]
    public void Visibility_Visibleに応じてChildまたはReplacementを描画する(bool visible, byte red, byte green, byte blue)
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Visibility
            {
                Visible = visible,
                Child = new ColoredBox { Color = new Color(255, 0, 0) },
                Replacement = new ColoredBox { Color = new Color(0, 0, 255) }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(new SKColor(red, green, blue), bitmap.GetPixel(20, 20));
    }

    [Fact]
    public void Visibility_非表示時_ChildのListenerがヒットテスト経路へ漏れない()
    {
        var hiddenView = BuildView(new Visibility
        {
            Visible = false,
            Child = ListenerBox(),
            Replacement = new SizedBox { Width = 40, Height = 40 }
        });
        var hiddenResult = new HitTestResult();
        hiddenView.HitTest(hiddenResult, new Offset(20, 20));
        Assert.DoesNotContain(hiddenResult.Path, entry => entry.Target is RenderPointerListener);

        var visibleView = BuildView(new Visibility
        {
            Visible = true,
            Child = ListenerBox(),
            Replacement = new SizedBox { Width = 40, Height = 40 }
        });
        var visibleResult = new HitTestResult();
        visibleView.HitTest(visibleResult, new Offset(20, 20));
        Assert.Contains(visibleResult.Path, entry => entry.Target is RenderPointerListener);
    }

    [Fact]
    public void Build_ChildまたはReplacementがnull_ArgumentNullExceptionを投げる()
    {
        var nullChild = new Visibility { Child = null! };
        var nullReplacement = new Visibility { Child = new SizedBox(), Replacement = null! };

        Assert.Throws<ArgumentNullException>(() => nullChild.Build(null!));
        Assert.Throws<ArgumentNullException>(() => nullReplacement.Build(null!));
    }

    [Fact]
    public void Child_Visibilityの必須公開プロパティ_RequiredMemberAttributeを持つ()
    {
        var child = typeof(Visibility).GetProperty(nameof(Visibility.Child));
        Assert.NotNull(child);
        Assert.NotNull(child!.GetCustomAttributes(typeof(RequiredMemberAttribute), inherit: false).SingleOrDefault());
    }

    private static Widget ListenerBox() => new Listener
    {
        Child = new ColoredBox
        {
            Color = new Color(255, 0, 0),
            Child = new SizedBox { Width = 40, Height = 40 }
        }
    };

    private static RenderView BuildView(Widget widget)
    {
        var view = new RenderView(Size.Width, Size.Height);
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        var owner = new BuildOwner(() => { });
        _ = new RenderObjectToWidgetAdapter { Container = view, Child = widget }.AttachToRenderTree(owner, null);
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }
}
