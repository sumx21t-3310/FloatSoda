using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class IntrinsicWidgetTest
{
    [Fact]
    public void WidgetUpdate_StepWidthを変更_既存RenderObjectとlayout結果を更新する()
    {
        var initial = new IntrinsicWidth
        {
            StepWidth = 20,
            Child = new SizedBox { Width = 53, Height = 17 }
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderIntrinsicWidth>(view.Child);
        Assert.Equal(new SKSize(60, 17), view.Size);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { StepWidth = 50 }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();
        pipeline.FlushLayout();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(50, renderObject.StepWidth);
        Assert.Equal(new SKSize(100, 17), view.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Step_0以下または非有限値_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new IntrinsicWidth { StepWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new IntrinsicHeight { StepHeight = value });
    }

    [Fact]
    public void PublicApi_IntrinsicWidget_期待するinitプロパティのみを公開し外部描画型を露出しない()
    {
        AssertWidgetApi(typeof(IntrinsicWidth), nameof(IntrinsicWidth.StepWidth), typeof(RenderIntrinsicWidth));
        AssertWidgetApi(typeof(IntrinsicHeight), nameof(IntrinsicHeight.StepHeight), typeof(RenderIntrinsicHeight));
    }

    private static void AssertWidgetApi(Type type, string propertyName, Type renderObjectType)
    {
        var property = Assert.Single(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        Assert.Equal(propertyName, property.Name);
        Assert.Equal(typeof(double?), property.PropertyType);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        Assert.True(typeof(SingleChildRenderObjectWidget<>).MakeGenericType(renderObjectType).IsAssignableFrom(type));
        Assert.DoesNotContain(
            type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            member => member switch
            {
                PropertyInfo candidate => IsExternalRenderingType(candidate.PropertyType),
                MethodInfo candidate => IsExternalRenderingType(candidate.ReturnType)
                    || candidate.GetParameters().Any(parameter => IsExternalRenderingType(parameter.ParameterType)),
                _ => false
            });
    }

    private static bool IsExternalRenderingType(Type type) =>
        type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Topten.RichTextKit", StringComparison.Ordinal) == true;

    private static (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        var root = new RenderObjectToWidgetAdapter { Container = view, Child = widget }
            .AttachToRenderTree(new BuildOwner(() => { }), null);
        return (pipeline, view, root);
    }
}
