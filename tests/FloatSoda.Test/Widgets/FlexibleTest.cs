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

public class FlexibleTest
{
    [Fact]
    public void WidgetUpdate_Flex値変更をParentDataへ適用して再レイアウトする()
    {
        var owner = new BuildOwner(() => { });
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();

        var root = Attach(1, view, owner, null);
        pipeline.FlushLayout();
        var renderFlex = GetFlex(view);
        var children = renderFlex.Children.ToArray();
        Assert.Equal(new SKSize(100, 40), children[0].Size);
        Assert.Equal(new SKSize(100, 40), children[1].Size);
        Assert.False(renderFlex.NeedsLayout);

        root = Attach(3, view, owner, root);
        owner.BuildScope();

        Assert.True(renderFlex.NeedsLayout);
        pipeline.FlushLayout();
        children = renderFlex.Children.ToArray();
        Assert.Equal(new SKSize(150, 40), children[0].Size);
        Assert.Equal(new SKSize(50, 40), children[1].Size);
        Assert.Equal(3, Assert.IsType<FlexParentData>(children[0].ParentData).Flex);
    }

    [Fact]
    public void FlexibleとExpanded_既定のFitをParentDataへ適用する()
    {
        var looseChild = Box();
        var tightChild = Box();
        var renderFlex = new RenderFlex { Children = { looseChild, tightChild } };

        new Flexible { Child = new SizedBox() }.ApplyParentData(looseChild);
        new Expanded { Child = new SizedBox() }.ApplyParentData(tightChild);

        Assert.Equal(FlexFit.Loose, Assert.IsType<FlexParentData>(looseChild.ParentData).Fit);
        Assert.Equal(FlexFit.Tight, Assert.IsType<FlexParentData>(tightChild.ParentData).Fit);
    }

    [Fact]
    public void ApplyParentData_FlexとFitの変更時に親をLayoutDirtyにして結果を更新する()
    {
        var child = Box();
        var renderFlex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            Children = { child }
        };
        var initial = new Flexible { Flex = 1, Fit = FlexFit.Loose, Child = new SizedBox() };
        initial.ApplyParentData(child);
        renderFlex.Layout(BoxConstraints.Tight(100, 20));
        Assert.Equal(new SKSize(10, 10), child.Size);
        Assert.False(renderFlex.NeedsLayout);

        var updated = initial with { Flex = 2, Fit = FlexFit.Tight };
        updated.ApplyParentData(child);

        Assert.True(renderFlex.NeedsLayout);
        renderFlex.Layout(BoxConstraints.Tight(100, 20));
        Assert.Equal(new SKSize(100, 10), child.Size);
        var parentData = Assert.IsType<FlexParentData>(child.ParentData);
        Assert.Equal(2, parentData.Flex);
        Assert.Equal(FlexFit.Tight, parentData.Fit);
    }

    [Fact]
    public void Spacer_指定したFlex比率の空領域をRowへ挿入する()
    {
        var owner = new BuildOwner(() => { });
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        _ = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new SizedBox
            {
                Width = 180,
                Height = 30,
                Child = new Row
                {
                    CrossAxisAlignment = CrossAxisAlignment.Stretch,
                    Children =
                    [
                        new Spacer { Flex = 2 },
                        new Expanded { Child = new SizedBox() }
                    ]
                }
            }
        }.AttachToRenderTree(owner, null);

        pipeline.FlushLayout();

        var renderFlex = GetFlex(view);
        var children = renderFlex.Children.ToArray();
        Assert.Equal(new SKSize(120, 30), children[0].Size);
        Assert.Equal(new SKSize(60, 30), children[1].Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Flex_0以下を指定するとArgumentOutOfRangeExceptionを投げる(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Flexible { Flex = value, Child = new SizedBox() });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Expanded { Flex = value, Child = new SizedBox() });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Spacer { Flex = value });
    }

    [Fact]
    public void Fit_未定義値を指定するとArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Flexible
        {
            Fit = (FlexFit)999,
            Child = new SizedBox()
        });
    }

    [Fact]
    public void PublicApi_FlexibleExpandedSpacerの公開表面が規約どおりで描画型を露出しない()
    {
        AssertInitProperty(typeof(Flexible), nameof(Flexible.Flex), typeof(int));
        AssertInitProperty(typeof(Flexible), nameof(Flexible.Fit), typeof(FlexFit));
        AssertInitProperty(typeof(Expanded), nameof(Expanded.Flex), typeof(int));
        AssertInitProperty(typeof(Spacer), nameof(Spacer.Flex), typeof(int));
        Assert.True(typeof(ParentDataWidget<FlexParentData>).IsAssignableFrom(typeof(Flexible)));
        Assert.True(typeof(ParentDataWidget<FlexParentData>).IsAssignableFrom(typeof(Expanded)));
        Assert.True(typeof(StatelessWidget).IsAssignableFrom(typeof(Spacer)));
        Assert.Equal([FlexFit.Tight, FlexFit.Loose], Enum.GetValues<FlexFit>());

        Type[] types = [typeof(Flexible), typeof(Expanded), typeof(Spacer), typeof(FlexParentData), typeof(FlexFit)];
        var exposedTypes = types
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .SelectMany(GetSignatureTypes);
        Assert.DoesNotContain(exposedTypes, type => type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true);
    }

    private static RenderObjectToWidgetElement<RenderView> Attach(
        int firstFlex,
        RenderView view,
        BuildOwner owner,
        RenderObjectToWidgetElement<RenderView>? element)
        => new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new SizedBox
            {
                Width = 200,
                Height = 40,
                Child = new Row
                {
                    CrossAxisAlignment = CrossAxisAlignment.Stretch,
                    Children =
                    [
                        new Expanded { Flex = firstFlex, Child = new SizedBox() },
                        new Expanded { Child = new SizedBox() }
                    ]
                }
            }
        }.AttachToRenderTree(owner, element);

    private static RenderFlex GetFlex(RenderView view)
    {
        var constrainedBox = Assert.IsType<RenderConstrainedBox>(view.Child);
        return Assert.IsType<RenderFlex>(constrainedBox.Child);
    }

    private static RenderConstrainedBox Box() => new()
    {
        AdditionalConstraints = BoxConstraints.Tight(10, 10)
    };

    private static void AssertInitProperty(Type type, string name, Type propertyType)
    {
        var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(property);
        Assert.Equal(propertyType, property.PropertyType);
        Assert.Contains(typeof(IsExternalInit), property.SetMethod!.ReturnParameter.GetRequiredCustomModifiers());
    }

    private static IEnumerable<Type> GetSignatureTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
        _ => [],
    };
}
