using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class FittedBoxTest
{
    private static readonly WidgetBitmapRenderer BitmapRenderer = new();

    [Fact]
    public void PerformLayout_Containで固定領域を指定_子を自然サイズで自身を親サイズにする()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 20) };
        var renderObject = new RenderFittedBox { Fit = BoxFit.Contain, Child = child };

        renderObject.Layout(BoxConstraints.Tight(40, 40));

        Assert.Equal(BoxConstraints.Unbounded, child.Constraints);
        Assert.Equal(new SKSize(80, 20), child.Size);
        Assert.Equal(new SKSize(40, 40), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_ScaleDownとContainを最小制約下で比較_ScaleDownだけ最小サイズへ拡大しない()
    {
        var constraints = new BoxConstraints(80, 100, 80, 100);
        var contain = new RenderFittedBox
        {
            Fit = BoxFit.Contain,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) }
        };
        var scaleDown = new RenderFittedBox
        {
            Fit = BoxFit.ScaleDown,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) }
        };

        contain.Layout(constraints);
        scaleDown.Layout(constraints);

        Assert.Equal(new SKSize(100, 80), contain.Size);
        Assert.Equal(new SKSize(80, 80), scaleDown.Size);
    }

    [Fact]
    public void Paint_Contain中央配置_子を幅40高さ10で中央へ描画する()
    {
        using var bitmap = BitmapRenderer.Render(new SizedBox
        {
            Width = 40,
            Height = 40,
            Child = new FittedBox
            {
                Fit = BoxFit.Contain,
                Child = new SizedBox
                {
                    Width = 80,
                    Height = 20,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }, new SKSizeI(40, 40));

        Assert.Equal(0, bitmap.GetPixel(20, 10).Alpha);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 20));
        Assert.Equal(0, bitmap.GetPixel(20, 30).Alpha);
    }

    [Fact]
    public void Paint_Contain下端配置_子を下端へ揃えて描画する()
    {
        using var bitmap = BitmapRenderer.Render(new SizedBox
        {
            Width = 40,
            Height = 40,
            Child = new FittedBox
            {
                Fit = BoxFit.Contain,
                Alignment = Alignment.BottomCenter,
                Child = new SizedBox
                {
                    Width = 80,
                    Height = 20,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }, new SKSizeI(40, 40));

        Assert.Equal(0, bitmap.GetPixel(20, 20).Alpha);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 35));
    }

    [Theory]
    [InlineData(BoxFit.Fill, true)]
    [InlineData(BoxFit.Contain, false)]
    [InlineData(BoxFit.Cover, true)]
    [InlineData(BoxFit.FitWidth, false)]
    [InlineData(BoxFit.FitHeight, true)]
    [InlineData(BoxFit.None, false)]
    [InlineData(BoxFit.ScaleDown, false)]
    public void Paint_BoxFitを指定_各方式の拡大縮小結果を描画する(BoxFit fit, bool paintsTopEdge)
    {
        using var bitmap = BitmapRenderer.Render(new SizedBox
        {
            Width = 40,
            Height = 40,
            Child = new FittedBox
            {
                Fit = fit,
                Child = new SizedBox
                {
                    Width = 80,
                    Height = 20,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }, new SKSizeI(40, 40));

        Assert.Equal(paintsTopEdge, bitmap.GetPixel(20, 0).Alpha > 0);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 20));
    }

    [Fact]
    public void Paint_CoverでClipNone_自身の領域外も描画する()
    {
        using var bitmap = BitmapRenderer.Render(CreateCoverOverflowWidget(Clip.None), new SKSizeI(100, 100));

        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 50));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 50));
    }

    [Theory]
    [InlineData(Clip.HardEdge)]
    [InlineData(Clip.Antialias)]
    public void Paint_CoverでClip有効_自身の矩形境界で切り抜く(Clip clipBehavior)
    {
        using var bitmap = BitmapRenderer.Render(CreateCoverOverflowWidget(clipBehavior), new SKSizeI(100, 100));

        Assert.Equal(0, bitmap.GetPixel(20, 50).Alpha);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 50));
        Assert.Equal(0, bitmap.GetPixel(80, 50).Alpha);
    }

    [Fact]
    public void HitTestChildren_Containで縮小_逆変換した子の描画領域だけがヒットする()
    {
        var renderObject = new RenderFittedBox
        {
            Fit = BoxFit.Contain,
            Child = new RenderColoredBox
            {
                Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 20) }
            }
        };
        renderObject.Layout(BoxConstraints.Tight(40, 40));

        Assert.True(renderObject.HitTest(new HitTestResult(), new Offset(20, 20)));
        Assert.False(renderObject.HitTest(new HitTestResult(), new Offset(20, 5)));
    }

    [Fact]
    public void HitTestChildren_Containで縮小_逆変換をPointerEventへ保持する()
    {
        PointerEvent? received = null;
        var listener = new RenderPointerListener
        {
            Behaviour = HitTestBehaviour.Opaque,
            OnPointerDown = pointerEvent => received = pointerEvent,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 20) }
        };
        var renderObject = new RenderFittedBox { Fit = BoxFit.Contain, Child = listener };
        renderObject.Layout(BoxConstraints.Tight(40, 40));

        var result = new HitTestResult();
        Assert.True(renderObject.HitTest(result, new Offset(20, 20)));
        var entry = Assert.Single(result.Path, item => ReferenceEquals(item.Target, listener));

        entry.Target.HandleEvent(
            new PointerEvent(1, PointerEventPhase.Down, new Offset(20, 20), entry.Transform),
            entry);

        Assert.True(received.HasValue);
        Assert.True(received.Value.Transform.HasValue);
        var localPosition = Vector2.Transform(
            new Vector2((float)received.Value.Position.X, (float)received.Value.Position.Y),
            received.Value.Transform.Value);
        Assert.Equal(new Vector2(40, 10), localPosition);
    }

    [Fact]
    public void WidgetUpdate_FitAlignmentClipを変更_既存RenderObjectへ反映する()
    {
        var initial = new FittedBox
        {
            Fit = BoxFit.Contain,
            Alignment = Alignment.Center,
            ClipBehavior = Clip.None,
            Child = new SizedBox { Width = 80, Height = 20 }
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        pipeline.FlushPaint();
        var renderObject = Assert.IsType<RenderFittedBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with
            {
                Fit = BoxFit.ScaleDown,
                Alignment = Alignment.BottomRight,
                ClipBehavior = Clip.HardEdge
            }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(BoxFit.ScaleDown, renderObject.Fit);
        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.Equal(Clip.HardEdge, renderObject.ClipBehavior);
        Assert.True(renderObject.NeedsLayout);
    }

    [Fact]
    public void Fit_ContainからCoverへ変更_LayoutDirtyにせずPaintDirtyにする()
    {
        var renderObject = new RenderFittedBox
        {
            Fit = BoxFit.Contain,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 20) }
        };
        renderObject.Layout(BoxConstraints.Tight(40, 40));
        renderObject.NeedsPaint = false;

        renderObject.Fit = BoxFit.Cover;

        Assert.False(renderObject.NeedsLayout);
        Assert.True(renderObject.NeedsPaint);
    }

    [Fact]
    public void Constructor_FitAlignmentClipが不正_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FittedBox { Fit = (BoxFit)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new FittedBox { Alignment = new Alignment(float.NaN, 0) });
        Assert.Throws<ArgumentOutOfRangeException>(() => new FittedBox { ClipBehavior = (Clip)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFittedBox { Fit = (BoxFit)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFittedBox { Alignment = new Alignment(0, float.PositiveInfinity) });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFittedBox { ClipBehavior = (Clip)99 });
    }

    [Fact]
    public void PublicApi_FittedBoxとBoxFitの公開表面_規約どおりで外部描画型を公開しない()
    {
        Assert.Equal(
            new[] { "Fill", "Contain", "Cover", "FitWidth", "FitHeight", "None", "ScaleDown" },
            Enum.GetNames<BoxFit>());

        foreach (var property in typeof(FittedBox).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }

        Assert.True(typeof(SingleChildRenderObjectWidget<RenderFittedBox>).IsAssignableFrom(typeof(FittedBox)));
        AssertNoSkiaMembers(typeof(BoxFit));
        AssertNoSkiaMembers(typeof(FittedBox));
        AssertNoSkiaMembers(typeof(RenderFittedBox));
    }

    private static Widget CreateCoverOverflowWidget(Clip clipBehavior) => new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new Align
        {
            Alignment = Alignment.Center,
            Child = new ConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(40, 40),
                Child = new FittedBox
                {
                    Fit = BoxFit.Cover,
                    ClipBehavior = clipBehavior,
                    Child = new SizedBox
                    {
                        Width = 80,
                        Height = 20,
                        Child = new ColoredBox { Color = new Color(255, 0, 0) }
                    }
                }
            }
        }
    };

    private static (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView(100, 100) { FixedSize = new SKSize(100, 100) };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        var root = new RenderObjectToWidgetAdapter { Container = view, Child = widget }
            .AttachToRenderTree(new BuildOwner(() => { }), null);
        return (pipeline, view, root);
    }

    private static void AssertNoSkiaMembers(Type type) => Assert.DoesNotContain(
        type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly),
        member => member switch
        {
            PropertyInfo property => IsSkiaType(property.PropertyType),
            MethodInfo method => IsSkiaType(method.ReturnType)
                                 || method.GetParameters().Any(parameter => IsSkiaType(parameter.ParameterType)),
            _ => false
        });

    private static bool IsSkiaType(Type type) =>
        type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true;
}
