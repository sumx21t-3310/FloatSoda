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

public class ConstrainedBoxTest
{
    private static (
        RenderPipeline Pipeline,
        RenderView View,
        RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView();
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
    public void CreateRenderObject_追加制約を指定_同じ制約を保持するRenderObjectを返す()
    {
        var constraints = new BoxConstraints(MinWidth: 40, MaxWidth: 120, MinHeight: 30, MaxHeight: 90);
        var widget = new ConstrainedBox { AdditionalConstraints = constraints };

        var renderObject = widget.CreateRenderObject();

        Assert.Equal(constraints, renderObject.AdditionalConstraints);
    }

    [Fact]
    public void WidgetUpdate_追加制約を変更_既存RenderObjectとレイアウト結果を更新する()
    {
        var initial = new ConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(100, 50)
        };
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderObject = Assert.IsType<RenderConstrainedBox>(view.Child);
        Assert.Equal(new SKSize(100, 50), view.Size);

        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = initial with { AdditionalConstraints = BoxConstraints.Tight(160, 90) }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();
        pipeline.FlushLayout();

        Assert.Same(renderObject, view.Child);
        Assert.Equal(BoxConstraints.Tight(160, 90), renderObject.AdditionalConstraints);
        Assert.Equal(new SKSize(160, 90), view.Size);
    }

    [Fact]
    public void PerformLayout_親制約と追加制約を指定_交差範囲を子へ伝播する()
    {
        var child = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(300, 250)
        };
        var renderObject = new RenderConstrainedBox
        {
            AdditionalConstraints = new BoxConstraints(
                MinWidth: 100,
                MaxWidth: 200,
                MinHeight: 80,
                MaxHeight: 180),
            Child = child
        };

        renderObject.Layout(new BoxConstraints(MaxWidth: 150, MaxHeight: 120));

        Assert.Equal(
            new BoxConstraints(MinWidth: 100, MaxWidth: 150, MinHeight: 80, MaxHeight: 120),
            child.Constraints);
        Assert.Equal(new SKSize(150, 120), renderObject.Size);
    }

    [Fact]
    public void PerformLayout_最小値が最大値を超える追加制約_ArgumentExceptionを投げる()
    {
        var renderObject = new RenderConstrainedBox
        {
            AdditionalConstraints = new BoxConstraints(MinWidth: 100, MaxWidth: 50)
        };

        Assert.Throws<ArgumentException>(() => renderObject.Layout(BoxConstraints.Unbounded));
    }

    [Fact]
    public void PublicApi_ConstrainedBoxの公開表面_規約どおりである()
    {
        var type = typeof(ConstrainedBox);
        var property = Assert.Single(type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

        Assert.Equal(nameof(ConstrainedBox.AdditionalConstraints), property.Name);
        Assert.Equal(typeof(BoxConstraints), property.PropertyType);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
        Assert.True(Attribute.IsDefined(property, typeof(RequiredMemberAttribute)));
        Assert.True(typeof(SingleChildRenderObjectWidget<RenderConstrainedBox>).IsAssignableFrom(type));
        Assert.DoesNotContain(
            type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
            member => member switch
            {
                PropertyInfo candidate => candidate.PropertyType.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true,
                MethodInfo candidate => candidate.ReturnType.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true
                    || candidate.GetParameters().Any(parameter => parameter.ParameterType.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true),
                _ => false
            });
    }
}
