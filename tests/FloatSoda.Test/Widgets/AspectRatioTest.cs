using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class AspectRatioTest
{
    [Fact]
    public void PerformLayout_幅上限から比率を適用_幅100高さ50になる()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Unbounded };
        var renderObject = new RenderAspectRatio { AspectRatio = 2, Child = child };

        renderObject.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(new SKSize(100, 50), renderObject.Size);
        Assert.Equal(BoxConstraints.Tight(100, 50), child.Constraints);
    }

    [Fact]
    public void PerformLayout_高さ上限で幅を再計算_幅50高さ100になる()
    {
        var renderObject = new RenderAspectRatio { AspectRatio = 0.5 };

        renderObject.Layout(BoxConstraints.Loose(100, 100));

        Assert.Equal(new SKSize(50, 100), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_両軸が固定_比率より親制約を優先する()
    {
        var renderObject = new RenderAspectRatio { AspectRatio = 2 };

        renderObject.Layout(BoxConstraints.Tight(80, 80));

        Assert.Equal(new SKSize(80, 80), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_両軸が制約されていない_InvalidOperationExceptionを投げる()
    {
        var renderObject = new RenderAspectRatio { AspectRatio = 2 };

        Assert.Throws<InvalidOperationException>(() => renderObject.Layout(BoxConstraints.Unbounded));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_Ratioが正の有限値ではない_ArgumentOutOfRangeExceptionを投げる(double ratio)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new AspectRatio { Ratio = ratio });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderAspectRatio { AspectRatio = ratio });
    }

    [Fact]
    public void WidgetUpdate_Ratioを変更_既存RenderObjectをLayoutDirtyにして再利用する()
    {
        var initial = new AspectRatio { Ratio = 2 };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderAspectRatio>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { Ratio = 1 }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.True(renderObject.NeedsLayout);
        pipeline.FlushLayout();
        Assert.Equal(new SKSize(100, 100), renderObject.Size);
    }

    [Fact]
    public void PublicApi_AspectRatioの公開表面_規約どおりで外部描画型を公開しない()
    {
        var property = typeof(AspectRatio).GetProperty(nameof(AspectRatio.Ratio))!;

        Assert.Equal(typeof(double), property.PropertyType);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        Assert.True(Attribute.IsDefined(property, typeof(RequiredMemberAttribute)));
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderAspectRatio>).IsAssignableFrom(typeof(AspectRatio)));
        AssertNoSkiaMembers(typeof(AspectRatio));
        AssertNoSkiaMembers(typeof(RenderAspectRatio));
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
