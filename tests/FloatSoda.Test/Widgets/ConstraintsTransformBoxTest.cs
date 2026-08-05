using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ConstraintsTransformBoxTest
{
    private static readonly WidgetBitmapRenderer BitmapRenderer = new();

    public static TheoryData<BoxConstraintsTransform, BoxConstraints> StandardTransforms => new()
    {
        { ConstraintsTransformBox.Unmodified, new BoxConstraints(10, 100, 20, 200) },
        { ConstraintsTransformBox.Unconstrained, BoxConstraints.Unbounded },
        { ConstraintsTransformBox.WidthUnconstrained, new BoxConstraints(0, double.PositiveInfinity, 20, 200) },
        { ConstraintsTransformBox.HeightUnconstrained, new BoxConstraints(10, 100, 0, double.PositiveInfinity) },
        { ConstraintsTransformBox.MaxWidthUnconstrained, new BoxConstraints(10, double.PositiveInfinity, 20, 200) },
        { ConstraintsTransformBox.MaxHeightUnconstrained, new BoxConstraints(10, 100, 20, double.PositiveInfinity) },
        { ConstraintsTransformBox.MaxUnconstrained, new BoxConstraints(10, double.PositiveInfinity, 20, double.PositiveInfinity) },
    };

    public static TheoryData<BoxConstraints> InvalidConstraints => new()
    {
        new BoxConstraints(MinWidth: double.NaN),
        new BoxConstraints(MinWidth: -1),
        new BoxConstraints(MinWidth: double.PositiveInfinity, MaxWidth: double.PositiveInfinity),
        new BoxConstraints(MaxWidth: double.NaN),
        new BoxConstraints(MaxWidth: double.NegativeInfinity),
        new BoxConstraints(MinWidth: 20, MaxWidth: 10),
        new BoxConstraints(MinHeight: double.NaN),
        new BoxConstraints(MinHeight: -1),
        new BoxConstraints(MinHeight: double.PositiveInfinity, MaxHeight: double.PositiveInfinity),
        new BoxConstraints(MaxHeight: double.NaN),
        new BoxConstraints(MaxHeight: double.NegativeInfinity),
        new BoxConstraints(MinHeight: 20, MaxHeight: 10),
    };

    private static (
        RenderPipeline Pipeline,
        RenderView View,
        RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget, int width = 40, int height = 40)
    {
        var view = new RenderView(width, height) { FixedSize = new SKSize(width, height) };
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();

        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = widget
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        return (pipeline, view, root);
    }

    [Fact]
    public void PerformLayout_任意変換を指定_変換後の制約を子へ渡す()
    {
        var parentConstraints = new BoxConstraints(10, 100, 20, 80);
        var transformedConstraints = new BoxConstraints(30, 150, 40, 120);
        BoxConstraints? received = null;
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = constraints =>
            {
                received = constraints;
                return transformedConstraints;
            },
            Child = child
        };

        renderObject.Layout(parentConstraints);

        Assert.Equal(parentConstraints, received);
        Assert.Equal(transformedConstraints, child.Constraints);
        Assert.Equal(new SKSize(30, 40), renderObject.Size);
    }

    [Theory]
    [MemberData(nameof(StandardTransforms))]
    public void StandardTransform_親制約を指定_期待する境界だけを外す(
        BoxConstraintsTransform transform,
        BoxConstraints expected)
    {
        var constraints = new BoxConstraints(10, 100, 20, 200);

        Assert.Equal(expected, transform(constraints));
    }

    [Theory]
    [InlineData(null, 0, double.PositiveInfinity, 0, double.PositiveInfinity)]
    [InlineData(Axis.Horizontal, 10, 100, 0, double.PositiveInfinity)]
    [InlineData(Axis.Vertical, 0, double.PositiveInfinity, 20, 200)]
    public void Build_制約を維持する軸を指定_対応する軸だけを維持する(
        Axis? axis,
        double minWidth,
        double maxWidth,
        double minHeight,
        double maxHeight)
    {
        var widget = new UnconstrainedBox { ConstrainedAxis = axis };

        var composed = Assert.IsType<ConstraintsTransformBox>(widget.Build(null!));
        var transformed = composed.ConstraintsTransform(new BoxConstraints(10, 100, 20, 200));

        Assert.Equal(new BoxConstraints(minWidth, maxWidth, minHeight, maxHeight), transformed);
    }

    [Theory]
    [InlineData(null, 80, 20)]
    [InlineData(Axis.Horizontal, 40, 20)]
    [InlineData(Axis.Vertical, 80, 40)]
    public void UnconstrainedBox_両軸または指定軸を維持_子の自然サイズを対応する軸へ反映する(
        Axis? axis,
        float expectedWidth,
        float expectedHeight)
    {
        var widget = new UnconstrainedBox
        {
            ConstrainedAxis = axis,
            Child = new SizedBox { Width = 80, Height = 20 }
        };
        var (pipeline, view, _) = Mount(widget);

        pipeline.FlushLayout();

        var renderObject = Assert.IsType<RenderConstraintsTransformBox>(view.Child);
        Assert.Equal(new SKSize(40, 40), renderObject.Size);
        Assert.Equal(new SKSize(expectedWidth, expectedHeight), renderObject.Child!.Size);
    }

    [Theory]
    [InlineData(-1, -1, 0, 0)]
    [InlineData(0, 0, -25, 25)]
    [InlineData(1, 1, -50, 50)]
    public void PerformLayout_子が親より大きい_Alignmentに応じたオフセットを設定する(
        float alignmentX,
        float alignmentY,
        double expectedX,
        double expectedY)
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(150, 50) };
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unconstrained,
            Alignment = new Alignment(alignmentX, alignmentY),
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(100, 100));

        Assert.Equal(new SKSize(100, 100), renderObject.Size);
        Assert.Equal(new Offset(expectedX, expectedY), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void PerformLayout_子が親より小さい_Alignmentに応じたオフセットを設定する()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) };
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unconstrained,
            Alignment = Alignment.BottomRight,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(100, 100));

        Assert.Equal(new Offset(60, 80), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void PerformLayout_子がnull_親制約で許される最小サイズを返す()
    {
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unconstrained
        };

        renderObject.Layout(new BoxConstraints(10, 100, 20, 200));

        Assert.Equal(new SKSize(10, 20), renderObject.Size);
    }

    [Fact]
    public void Paint_ClipNoneで横方向へoverflow_親領域外も描画する()
    {
        using var bitmap = BitmapRenderer.Render(CreateOverflowWidget(Clip.None), new SKSizeI(100, 100));

        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 50));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 50));
    }

    [Theory]
    [InlineData(Clip.HardEdge)]
    [InlineData(Clip.Antialias)]
    public void Paint_Clip有効で横方向へoverflow_自身の矩形境界で切り抜く(Clip clipBehavior)
    {
        using var bitmap = BitmapRenderer.Render(CreateOverflowWidget(clipBehavior), new SKSizeI(100, 100));

        Assert.Equal(0, bitmap.GetPixel(20, 50).Alpha);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 50));
        Assert.Equal(0, bitmap.GetPixel(80, 50).Alpha);
    }

    [Fact]
    public void HitTest_子が横方向へoverflow_親領域内だけAlignmentオフセットを反映する()
    {
        var child = new RenderColoredBox
        {
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 20) }
        };
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unconstrained,
            Alignment = Alignment.Center,
            Child = child
        };
        renderObject.Layout(BoxConstraints.Tight(40, 40));

        Assert.True(renderObject.HitTest(new HitTestResult(), new Offset(5, 15)));
        Assert.True(renderObject.HitTest(new HitTestResult(), new Offset(35, 15)));
        Assert.False(renderObject.HitTest(new HitTestResult(), new Offset(-5, 15)));
        Assert.False(renderObject.HitTest(new HitTestResult(), new Offset(45, 15)));
    }

    [Fact]
    public void WidgetUpdate_TransformAlignmentClipAxisを変更_既存RenderObjectへ反映する()
    {
        var initial = new UnconstrainedBox
        {
            ConstrainedAxis = Axis.Horizontal,
            Alignment = Alignment.TopLeft,
            ClipBehavior = Clip.None,
            Child = new SizedBox { Width = 80, Height = 20 }
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        pipeline.FlushPaint();
        var renderObject = Assert.IsType<RenderConstraintsTransformBox>(view.Child);

        var updated = initial with
        {
            ConstrainedAxis = Axis.Vertical,
            Alignment = Alignment.BottomRight,
            ClipBehavior = Clip.HardEdge
        };
        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = updated
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();
        pipeline.FlushLayout();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.Equal(Clip.HardEdge, renderObject.ClipBehavior);
        Assert.Equal(new BoxConstraints(0, double.PositiveInfinity, 40, 40), renderObject.Child!.Constraints);
    }

    [Fact]
    public void WidgetUpdate_Clipだけ変更_LayoutDirtyにせずPaintDirtyにする()
    {
        var initial = new ConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unconstrained,
            ClipBehavior = Clip.None,
            Child = new SizedBox { Width = 80, Height = 20 }
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        pipeline.FlushPaint();
        var renderObject = Assert.IsType<RenderConstraintsTransformBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { ClipBehavior = Clip.HardEdge }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.False(renderObject.NeedsLayout);
        Assert.True(renderObject.NeedsPaint);
    }

    [Theory]
    [MemberData(nameof(InvalidConstraints))]
    public void PerformLayout_変換結果がBoxConstraints契約外_ArgumentExceptionを投げる(BoxConstraints invalid)
    {
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = _ => invalid,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded }
        };

        Assert.Throws<ArgumentException>(() => renderObject.Layout(BoxConstraints.Unbounded));
    }

    [Fact]
    public void PerformLayout_変換結果の最大値だけが正の無限大_制約として受理する()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(30, 20) };
        var renderObject = new RenderConstraintsTransformBox
        {
            ConstraintsTransform = _ => BoxConstraints.Unbounded,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(10, 10));

        Assert.Equal(BoxConstraints.Unbounded, child.Constraints);
    }

    [Fact]
    public void Constructor_Transformがnull_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new ConstraintsTransformBox { ConstraintsTransform = null! });
        Assert.Throws<ArgumentNullException>(() => new RenderConstraintsTransformBox { ConstraintsTransform = null! });
    }

    [Fact]
    public void Constructor_AlignmentClipAxisが不正_対応する例外を投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unmodified,
            Alignment = new Alignment(float.NaN, 0)
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => new ConstraintsTransformBox
        {
            ConstraintsTransform = ConstraintsTransformBox.Unmodified,
            ClipBehavior = (Clip)99
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => new UnconstrainedBox { ConstrainedAxis = (Axis)99 });
    }

    [Fact]
    public void PublicApi_新規型の公開表面_規約どおりで外部描画型を公開しない()
    {
        var transformProperty = typeof(ConstraintsTransformBox).GetProperty(nameof(ConstraintsTransformBox.ConstraintsTransform))!;
        Assert.Equal(typeof(BoxConstraintsTransform), transformProperty.PropertyType);
        Assert.Contains(typeof(IsExternalInit), transformProperty.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        Assert.True(Attribute.IsDefined(transformProperty, typeof(RequiredMemberAttribute)));
        Assert.True(typeof(MulticastDelegate).IsAssignableFrom(typeof(BoxConstraintsTransform).BaseType));
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderConstraintsTransformBox>).IsAssignableFrom(typeof(ConstraintsTransformBox)));
        Assert.True(typeof(StatelessWidget).IsAssignableFrom(typeof(UnconstrainedBox)));

        foreach (var type in new[] { typeof(BoxConstraintsTransform), typeof(ConstraintsTransformBox), typeof(UnconstrainedBox), typeof(RenderConstraintsTransformBox) })
        {
            Assert.DoesNotContain(
                type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly),
                member => member switch
                {
                    PropertyInfo property => IsExternalDrawingType(property.PropertyType),
                    MethodInfo method => IsExternalDrawingType(method.ReturnType)
                        || method.GetParameters().Any(parameter => IsExternalDrawingType(parameter.ParameterType)),
                    _ => false
                });
        }
    }

    private static Widget CreateOverflowWidget(Clip clipBehavior) => new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new Align
        {
            Alignment = Alignment.Center,
            Child = new ConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(40, 40),
                Child = new ConstraintsTransformBox
                {
                    ConstraintsTransform = ConstraintsTransformBox.Unconstrained,
                    Alignment = Alignment.Center,
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

    private static bool IsExternalDrawingType(Type type) =>
        type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true;
}
