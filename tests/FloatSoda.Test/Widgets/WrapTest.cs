using System.Reflection;
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

public class WrapTest
{
    [Fact]
    public void WidgetMount_複数の子を指定_RenderWrapへ接続して折り返す()
    {
        var view = new RenderView(70, 100) { FixedSize = new SKSize(70, 100) };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Wrap
            {
                Spacing = 5,
                RunSpacing = 7,
                Children =
                [
                    new SizedBox { Width = 30, Height = 10 },
                    new SizedBox { Width = 30, Height = 10 },
                    new SizedBox { Width = 30, Height = 10 },
                ]
            }
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        pipeline.FlushLayout();

        var wrap = Assert.IsType<RenderWrap>(view.Child);
        Assert.Equal(3, wrap.Children.Count);
        Assert.Equal(new Offset(0, 17), Assert.IsType<WrapParentData>(wrap.Children.Last().ParentData).Offset);
    }

    [Fact]
    public void UpdateRenderObject_全レイアウト設定を変更_RenderObjectへ反映してLayoutDirtyにする()
    {
        var renderObject = new Wrap().CreateRenderObject();
        renderObject.Layout(BoxConstraints.Tight(100, 100));
        Assert.False(renderObject.NeedsLayout);

        new Wrap
        {
            Direction = Axis.Vertical,
            Alignment = WrapAlignment.SpaceEvenly,
            Spacing = 3,
            RunAlignment = WrapAlignment.SpaceAround,
            RunSpacing = 4,
            CrossAxisAlignment = WrapCrossAlignment.Center,
            VerticalDirection = VerticalDirection.Up,
        }.UpdateRenderObject(renderObject);

        Assert.Equal(Axis.Vertical, renderObject.Direction);
        Assert.Equal(WrapAlignment.SpaceEvenly, renderObject.Alignment);
        Assert.Equal(3, renderObject.Spacing);
        Assert.Equal(WrapAlignment.SpaceAround, renderObject.RunAlignment);
        Assert.Equal(4, renderObject.RunSpacing);
        Assert.Equal(WrapCrossAlignment.Center, renderObject.CrossAxisAlignment);
        Assert.Equal(VerticalDirection.Up, renderObject.VerticalDirection);
        Assert.True(renderObject.NeedsLayout);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_SpacingまたはRunSpacingが不正_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { Spacing = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { RunSpacing = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderWrap { Spacing = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderWrap { RunSpacing = value });
    }

    [Fact]
    public void Constructor_列挙値が不正_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { Direction = (Axis)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { Alignment = (WrapAlignment)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { RunAlignment = (WrapAlignment)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { CrossAxisAlignment = (WrapCrossAlignment)99 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Wrap { VerticalDirection = (VerticalDirection)99 });
    }

    [Fact]
    public void PublicApi_Wrapの公開表面_専用列挙型を使用してSkiaSharp型を公開しない()
    {
        Assert.True(typeof(MultiChildRenderObjectWidget<RenderWrap>).IsAssignableFrom(typeof(Wrap)));
        Assert.Equal(typeof(WrapAlignment), typeof(Wrap).GetProperty(nameof(Wrap.Alignment))!.PropertyType);
        Assert.Equal(typeof(WrapAlignment), typeof(Wrap).GetProperty(nameof(Wrap.RunAlignment))!.PropertyType);
        Assert.Equal(typeof(WrapCrossAlignment), typeof(Wrap).GetProperty(nameof(Wrap.CrossAxisAlignment))!.PropertyType);

        var exposedTypes = typeof(Wrap)
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .SelectMany(member => member switch
            {
                PropertyInfo property => [property.PropertyType],
                MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
                _ => []
            });

        Assert.DoesNotContain(exposedTypes, type => type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true);
    }
}
