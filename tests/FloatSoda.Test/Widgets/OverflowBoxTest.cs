using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class OverflowBoxTest
{
    private static readonly WidgetBitmapRenderer BitmapRenderer = new();

    public static TheoryData<Alignment, float, float> Alignments => new()
    {
        { Alignment.TopLeft, 0, 0 },
        { Alignment.TopCenter, -25, 0 },
        { Alignment.TopRight, -50, 0 },
        { Alignment.CenterLeft, 0, 25 },
        { Alignment.Center, -25, 25 },
        { Alignment.CenterRight, -50, 25 },
        { Alignment.BottomLeft, 0, 50 },
        { Alignment.BottomCenter, -25, 50 },
        { Alignment.BottomRight, -50, 50 },
    };

    [Fact]
    public void PerformLayout_上書き制約を指定_子が自身より大きくレイアウトされる()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderConstrainedOverflowBox
        {
            MinWidth = 150,
            MaxWidth = 150,
            MinHeight = 50,
            MaxHeight = 50,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(100, 100));

        Assert.Equal(BoxConstraints.Tight(150, 50), child.Constraints);
        Assert.Equal(new SKSize(100, 100), renderObject.Size);
        Assert.Equal(new SKSize(150, 50), child.Size);
    }

    [Theory]
    [InlineData(OverflowBoxFit.Max, 100, 80)]
    [InlineData(OverflowBoxFit.DeferToChild, 40, 20)]
    public void PerformLayout_Fitを指定_自身のサイズを決める(
        OverflowBoxFit fit,
        float expectedWidth,
        float expectedHeight)
    {
        var renderObject = new RenderConstrainedOverflowBox
        {
            Fit = fit,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(40, 20) }
        };

        renderObject.Layout(BoxConstraints.Loose(100, 80));

        Assert.Equal(new SKSize(expectedWidth, expectedHeight), renderObject.Size);
    }

    [Theory]
    [MemberData(nameof(Alignments))]
    public void PerformLayout_Alignment各値_子のオフセットを設定する(
        Alignment alignment,
        float expectedX,
        float expectedY)
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderConstrainedOverflowBox
        {
            MinWidth = 150,
            MaxWidth = 150,
            MinHeight = 50,
            MaxHeight = 50,
            Alignment = alignment,
            Child = child
        };

        renderObject.Layout(BoxConstraints.Tight(100, 100));

        Assert.Equal(new Offset(expectedX, expectedY), Assert.IsType<BoxParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void Paint_子が横方向へOverflow_自身の領域外も描画する()
    {
        using var bitmap = BitmapRenderer.Render(CreateOverflowWidget(), new SKSizeI(100, 100));

        Assert.Equal(SKColors.Red, bitmap.GetPixel(20, 50));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(35, 50));
        Assert.Equal(SKColors.Red, bitmap.GetPixel(80, 50));
    }

    [Fact]
    public void WidgetUpdate_制約FitAlignmentを変更_既存RenderObjectをLayoutDirtyにする()
    {
        var initial = new OverflowBox
        {
            MinWidth = 0,
            MaxWidth = 80,
            Fit = OverflowBoxFit.Max,
            Alignment = Alignment.TopLeft
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderConstrainedOverflowBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with
            {
                MinWidth = 20,
                MaxWidth = 40,
                Fit = OverflowBoxFit.DeferToChild,
                Alignment = Alignment.BottomRight
            }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(20, renderObject.MinWidth);
        Assert.Equal(40, renderObject.MaxWidth);
        Assert.Equal(OverflowBoxFit.DeferToChild, renderObject.Fit);
        Assert.Equal(Alignment.BottomRight, renderObject.Alignment);
        Assert.True(renderObject.NeedsLayout);
    }

    [Fact]
    public void Constructor_MinがMaxを超える_ArgumentExceptionを投げる()
    {
        Assert.Throws<ArgumentException>(() => new OverflowBox { MinWidth = 20, MaxWidth = 10 });
        Assert.Throws<ArgumentException>(() => new OverflowBox { MaxWidth = 10, MinWidth = 20 });

        var renderObject = new RenderConstrainedOverflowBox { MinHeight = 20, MaxHeight = 10 };
        Assert.Throws<ArgumentException>(() => renderObject.Layout(BoxConstraints.Loose(100, 100)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_Minが不正_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OverflowBox { MinWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderConstrainedOverflowBox { MinHeight = value });
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_Maxが不正_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OverflowBox { MaxWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderConstrainedOverflowBox { MaxHeight = value });
    }

    [Fact]
    public void Constructor_FitとAlignmentが不正_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OverflowBox { Fit = (OverflowBoxFit)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new OverflowBox
        {
            Alignment = new Alignment(float.NaN, 0)
        });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderConstrainedOverflowBox { Fit = (OverflowBoxFit)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderConstrainedOverflowBox
        {
            Alignment = new Alignment(0, float.NegativeInfinity)
        });
    }

    [Fact]
    public void PublicApi_公開表面_InitOnlyでSkia型を公開しない()
    {
        Assert.Equal(new[] { "Max", "DeferToChild" }, Enum.GetNames<OverflowBoxFit>());
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderConstrainedOverflowBox>)
            .IsAssignableFrom(typeof(OverflowBox)));
        Assert.Null(typeof(OverflowBox).GetProperty("ClipBehavior"));

        foreach (var type in new[] { typeof(OverflowBoxFit), typeof(OverflowBox), typeof(RenderConstrainedOverflowBox) })
        {
            AssertNoSkiaDeclaredMembers(type);
        }

        foreach (var property in typeof(OverflowBox)
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }
    }

    private static Widget CreateOverflowWidget() => new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new Align
        {
            Child = new ConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(40, 40),
                Child = new OverflowBox
                {
                    MinWidth = 80,
                    MaxWidth = 80,
                    MinHeight = 20,
                    MaxHeight = 20,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }
    };

    private static (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView(100, 80) { FixedSize = new SKSize(100, 80) };
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
