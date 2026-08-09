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
using LayoutSize = FloatSoda.Geometrics.Size;

namespace FloatSoda.Test.Widgets;

public class SizedOverflowBoxTest
{
    public static TheoryData<Alignment, float, float> Alignments => new()
    {
        { Alignment.TopLeft, 0, 0 },
        { Alignment.TopCenter, -20, 0 },
        { Alignment.TopRight, -40, 0 },
        { Alignment.CenterLeft, 0, -15 },
        { Alignment.Center, -20, -15 },
        { Alignment.CenterRight, -40, -15 },
        { Alignment.BottomLeft, 0, -30 },
        { Alignment.BottomCenter, -20, -30 },
        { Alignment.BottomRight, -40, -30 },
    };

    [Fact]
    public void PerformLayout_指定サイズと子のサイズ_独立に決まる()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 60) };
        var renderObject = new RenderSizedOverflowBox
        {
            RequestedSize = new LayoutSize(40, 30),
            Child = child
        };
        var constraints = BoxConstraints.Loose(100, 100);

        renderObject.Layout(constraints);

        Assert.Equal(new SKSize(40, 30), renderObject.Size);
        Assert.Equal(new SKSize(80, 60), child.Size);
        Assert.Equal(constraints, child.Constraints);
    }

    [Theory]
    [MemberData(nameof(Alignments))]
    public void PerformLayout_Alignment各値_子のオフセットを設定する(
        Alignment alignment,
        float expectedX,
        float expectedY)
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(80, 60) };
        var renderObject = new RenderSizedOverflowBox
        {
            RequestedSize = new LayoutSize(40, 30),
            Alignment = alignment,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(new Offset(expectedX, expectedY), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void WidgetUpdate_SizeとAlignmentを変更_既存RenderObjectをLayoutDirtyにする()
    {
        var initial = new SizedOverflowBox
        {
            Size = new LayoutSize(40, 30),
            Alignment = Alignment.TopLeft
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderSizedOverflowBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with
            {
                Size = new LayoutSize(20, 10),
                Alignment = Alignment.BottomRight
            }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(new LayoutSize(20, 10), renderObject.RequestedSize);
        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.True(renderObject.NeedsLayout);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    [InlineData(double.NegativeInfinity, 0)]
    [InlineData(0, -1)]
    [InlineData(0, double.NaN)]
    [InlineData(0, double.PositiveInfinity)]
    [InlineData(0, double.NegativeInfinity)]
    public void Constructor_Sizeが不正_ArgumentOutOfRangeExceptionを投げる(double width, double height)
    {
        var size = new LayoutSize(width, height);
        Assert.Throws<ArgumentOutOfRangeException>(() => new SizedOverflowBox { Size = size });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderSizedOverflowBox { RequestedSize = size });
    }

    [Fact]
    public void Constructor_Alignmentが非有限_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SizedOverflowBox
        {
            Size = new LayoutSize(1, 1),
            Alignment = new Alignment(float.NaN, 0)
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderSizedOverflowBox
        {
            RequestedSize = new LayoutSize(1, 1),
            Alignment = new Alignment(0, float.PositiveInfinity)
        });
    }

    [Fact]
    public void PublicApi_公開表面_InitOnlyでSkia型を公開しない()
    {
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderSizedOverflowBox>)
            .IsAssignableFrom(typeof(SizedOverflowBox)));
        Assert.Equal(typeof(LayoutSize), typeof(SizedOverflowBox).GetProperty(nameof(SizedOverflowBox.Size))!.PropertyType);

        foreach (var type in new[] { typeof(LayoutSize), typeof(SizedOverflowBox), typeof(RenderSizedOverflowBox) })
        {
            AssertNoSkiaDeclaredMembers(type);
        }

        foreach (var property in typeof(SizedOverflowBox)
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }
    }

    private static (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView(100, 100) { FixedSize = new SKSize(100, 100) };
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
}
