using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Elements;

public class InheritedWidgetTest
{
    private record ScopeA : InheritedWidget
    {
        public required int Value { get; init; }

        public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
            oldWidget is ScopeA old && old.Value != Value;
    }

    private record ScopeB : InheritedWidget
    {
        public required string Name { get; init; }

        public override bool UpdateShouldNotify(InheritedWidget oldWidget) =>
            oldWidget is ScopeB old && old.Name != Name;
    }

    /// <summary>Build時に祖先のInheritedWidgetを探索し、見つかった値を記録するプローブ。</summary>
    private record Probe : StatelessWidget
    {
        public required Action<IBuildContext> OnBuild { get; init; }

        public override Widget Build(IBuildContext context)
        {
            OnBuild(context);
            return new SizedBox { Width = 10, Height = 10 };
        }
    }

    private static (RenderObjectToWidgetElement<RenderView> Root, BuildOwner Owner) MountTree(Widget widget)
    {
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };

        var owner = new BuildOwner(() => { });

        var root = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        return (root, owner);
    }

    [Fact]
    public void SingleScope_DescendantCanDependOnIt()
    {
        ScopeA? found = null;

        MountTree(new ScopeA
        {
            Value = 1,
            Child = new Probe
            {
                OnBuild = ctx => found = ctx.DependOnInheritedWidgetOfExactType<ScopeA>()
            }
        });

        Assert.NotNull(found);
        Assert.Equal(1, found!.Value);
    }

    [Fact]
    public void NestedDifferentScopes_DescendantFindsBoth()
    {
        ScopeA? foundA = null;
        ScopeB? foundB = null;

        MountTree(new ScopeA
        {
            Value = 1,
            Child = new ScopeB
            {
                Name = "inner",
                Child = new Probe
                {
                    OnBuild = ctx =>
                    {
                        foundA = ctx.DependOnInheritedWidgetOfExactType<ScopeA>();
                        foundB = ctx.DependOnInheritedWidgetOfExactType<ScopeB>();
                    }
                }
            }
        });

        // 外側のScopeAはコピーされたマップ経由で見つかる
        Assert.NotNull(foundA);
        Assert.Equal(1, foundA!.Value);

        // 内側のScopeBは自分自身をマップに登録していないと見つからない
        Assert.NotNull(foundB);
        Assert.Equal("inner", foundB!.Name);
    }

    [Fact]
    public void NestedSameType_InnerScopeShadowsOuter()
    {
        ScopeA? found = null;

        MountTree(new ScopeA
        {
            Value = 1,
            Child = new ScopeA
            {
                Value = 2,
                Child = new Probe
                {
                    OnBuild = ctx => found = ctx.DependOnInheritedWidgetOfExactType<ScopeA>()
                }
            }
        });

        // Flutter同様、最も近い祖先が優先される
        Assert.NotNull(found);
        Assert.Equal(2, found!.Value);
    }

    [Fact]
    public void DependOn_ReturnsNull_WhenNoAncestorExists()
    {
        ScopeA? found = new ScopeA { Value = -1, Child = new SizedBox() };

        MountTree(new Probe
        {
            OnBuild = ctx => found = ctx.DependOnInheritedWidgetOfExactType<ScopeA>()
        });

        Assert.Null(found);
    }

    [Fact]
    public void GetElementFor_ReturnsNull_WhenTypeNotRegistered()
    {
        var buildRan = false;
        InheritedElement? found = null;
        Exception? thrown = null;

        MountTree(new ScopeA
        {
            Value = 1,
            Child = new Probe
            {
                OnBuild = ctx =>
                {
                    buildRan = true;
                    try
                    {
                        found = ctx.GetElementForInheritedWidgetOfExactType<ScopeB>();
                    }
                    catch (Exception e)
                    {
                        thrown = e;
                    }
                }
            }
        });

        // 未登録の型はnullを返すべき（KeyNotFoundExceptionを投げない）
        Assert.True(buildRan);
        Assert.Null(thrown);
        Assert.Null(found);
    }
}
