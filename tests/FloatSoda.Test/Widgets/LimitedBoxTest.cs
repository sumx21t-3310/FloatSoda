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

public class LimitedBoxTest
{
    [Fact]
    public void PerformLayout_両軸が制約されていない_指定した最大寸法を子へ適用する()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(200, 150) };
        var renderObject = new RenderLimitedBox { MaxWidth = 80, MaxHeight = 60, Child = child };

        renderObject.Layout(BoxConstraints.Unbounded);

        Assert.Equal(new BoxConstraints(0, 80, 0, 60), child.Constraints);
        Assert.Equal(new SKSize(80, 60), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_親に有限上限がある_指定した最大寸法を適用しない()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(200, 150) };
        var renderObject = new RenderLimitedBox { MaxWidth = 80, MaxHeight = 60, Child = child };

        renderObject.Layout(BoxConstraints.Loose(300, 300));

        Assert.Equal(BoxConstraints.Loose(300, 300), child.Constraints);
        Assert.Equal(new SKSize(200, 150), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_幅だけ制約されていない_最大幅だけを適用する()
    {
        var child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(200, 150) };
        var renderObject = new RenderLimitedBox { MaxWidth = 80, MaxHeight = 60, Child = child };

        renderObject.Layout(new BoxConstraints(MaxHeight: 200));

        Assert.Equal(new BoxConstraints(0, 80, 0, 200), child.Constraints);
        Assert.Equal(new SKSize(80, 150), renderObject.Size);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_最大寸法が契約外_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LimitedBox { MaxWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new LimitedBox { MaxHeight = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderLimitedBox { MaxWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderLimitedBox { MaxHeight = value });
    }

    [Fact]
    public void Constructor_最大寸法が正の無限大_既定値として受理する()
    {
        var widget = new LimitedBox();
        var renderObject = widget.CreateRenderObject();

        Assert.Equal(double.PositiveInfinity, widget.MaxWidth);
        Assert.Equal(double.PositiveInfinity, widget.MaxHeight);
        Assert.Equal(double.PositiveInfinity, renderObject.MaxWidth);
        Assert.Equal(double.PositiveInfinity, renderObject.MaxHeight);
    }

    [Fact]
    public void WidgetUpdate_最大寸法を変更_既存RenderObjectをLayoutDirtyにする()
    {
        var initial = new LimitedBox { MaxWidth = 80, MaxHeight = 60 };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderLimitedBox>(view.Child);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { MaxWidth = 40, MaxHeight = 30 }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(40, renderObject.MaxWidth);
        Assert.Equal(30, renderObject.MaxHeight);
        Assert.True(renderObject.NeedsLayout);
    }

    [Fact]
    public void PublicApi_LimitedBoxの公開表面_規約どおりで外部描画型を公開しない()
    {
        foreach (var property in typeof(LimitedBox).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
        {
            Assert.Equal(typeof(double), property.PropertyType);
            Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        }

        Assert.True(typeof(SingleChildRenderObjectWidget<RenderLimitedBox>).IsAssignableFrom(typeof(LimitedBox)));
        AssertNoSkiaMembers(typeof(LimitedBox));
        AssertNoSkiaMembers(typeof(RenderLimitedBox));
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
