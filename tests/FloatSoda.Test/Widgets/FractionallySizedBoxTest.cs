using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class FractionallySizedBoxTest
{
    public static TheoryData<Alignment, float, float> Alignments => new()
    {
        { Alignment.TopLeft, 0, 0 },
        { Alignment.TopCenter, 50, 0 },
        { Alignment.TopRight, 100, 0 },
        { Alignment.CenterLeft, 0, 37.5f },
        { Alignment.Center, 50, 37.5f },
        { Alignment.CenterRight, 100, 37.5f },
        { Alignment.BottomLeft, 0, 75 },
        { Alignment.BottomCenter, 50, 75 },
        { Alignment.BottomRight, 100, 75 },
    };

    [Fact]
    public void PerformLayout_親がBounded_Factorを子のTight制約へ反映する()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            WidthFactor = 0.5,
            HeightFactor = 0.25,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(BoxConstraints.Tight(100, 25), child.Constraints);
        Assert.Equal(new SKSize(200, 100), renderObject.Size);
        Assert.Equal(new SKSize(100, 25), child.Size);
    }

    [Fact]
    public void PerformLayout_FactorがNullの軸_親制約をそのまま子へ渡す()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            HeightFactor = 0.5,
            Child = child
        };
        var constraints = new BoxConstraints(10, 100, 20, 200);

        renderObject.Layout(constraints);

        Assert.Equal(new BoxConstraints(10, 100, 100, 100), child.Constraints);
    }

    [Fact]
    public void PerformLayout_Factor指定軸がUnbounded_InvalidOperationExceptionを投げる()
    {
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            WidthFactor = 0.5,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded }
        };

        Assert.Throws<InvalidOperationException>(() => renderObject.Layout(BoxConstraints.Unbounded));
    }

    [Fact]
    public void GetMaxIntrinsicWidth_高さ無制約かつHeightFactorが0_子へ高さ0を渡す()
    {
        var child = new IntrinsicProbeBox();
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            HeightFactor = 0,
            Child = child
        };

        Assert.Equal(40, renderObject.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(0, child.LastMaxIntrinsicWidthHeight);
    }

    [Fact]
    public void GetMaxIntrinsicHeight_幅無制約かつWidthFactorが0_子へ幅0を渡す()
    {
        var child = new IntrinsicProbeBox();
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            WidthFactor = 0,
            Child = child
        };

        Assert.Equal(20, renderObject.GetMaxIntrinsicHeight(double.PositiveInfinity));
        Assert.Equal(0, child.LastMaxIntrinsicHeightWidth);
    }

    [Theory]
    [MemberData(nameof(Alignments))]
    public void PerformLayout_Alignment各値_子のオフセットを設定する(
        Alignment alignment,
        float expectedX,
        float expectedY)
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderFractionallySizedOverflowBox
        {
            WidthFactor = 0.5,
            HeightFactor = 0.25,
            Alignment = alignment,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new Offset(expectedX, expectedY), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void WidgetUpdate_FactorとAlignmentを変更_既存RenderObjectをLayoutDirtyにする()
    {
        var initial = new FractionallySizedBox
        {
            WidthFactor = 0.5,
            HeightFactor = 0.5,
            Alignment = Alignment.TopLeft
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderFractionallySizedOverflowBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with
            {
                WidthFactor = 0.25,
                HeightFactor = null,
                Alignment = Alignment.BottomRight
            }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(0.25, renderObject.WidthFactor);
        Assert.Null(renderObject.HeightFactor);
        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.True(renderObject.NeedsLayout);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_Factorが不正_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FractionallySizedBox { WidthFactor = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new FractionallySizedBox { HeightFactor = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFractionallySizedOverflowBox { WidthFactor = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFractionallySizedOverflowBox { HeightFactor = value });
    }

    [Fact]
    public void Constructor_Alignmentが非有限_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FractionallySizedBox
        {
            Alignment = new Alignment(float.NaN, 0)
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderFractionallySizedOverflowBox
        {
            Alignment = new Alignment(0, float.PositiveInfinity)
        });
    }

    [Fact]
    public void PublicApi_公開表面_InitOnlyでSkia型を公開しない()
    {
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderFractionallySizedOverflowBox>)
            .IsAssignableFrom(typeof(FractionallySizedBox)));
        AssertNoSkiaDeclaredMembers(typeof(FractionallySizedBox));

        foreach (var property in typeof(FractionallySizedBox)
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }
    }

    private static (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView(200, 100) { FixedSize = new SKSize(200, 100) };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        var root = new RenderObjectToWidgetAdapter { Container = view, Child = widget }
            .AttachToRenderTree(new BuildOwner(() => { }), null);
        return (pipeline, view, root);
    }

    private static void AssertNoSkiaDeclaredMembers(Type type) => Assert.DoesNotContain(
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

    private sealed class IntrinsicProbeBox : RenderBox
    {
        public double LastMaxIntrinsicWidthHeight { get; private set; } = double.NaN;

        public double LastMaxIntrinsicHeightWidth { get; private set; } = double.NaN;

        public override void PerformLayout() => Size = Constraints.Smallest;

        protected override double ComputeMaxIntrinsicWidth(double height)
        {
            LastMaxIntrinsicWidthHeight = height;
            return 40;
        }

        protected override double ComputeMaxIntrinsicHeight(double width)
        {
            LastMaxIntrinsicHeightWidth = width;
            return 20;
        }

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}
