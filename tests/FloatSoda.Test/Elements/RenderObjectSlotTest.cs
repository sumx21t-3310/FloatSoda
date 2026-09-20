using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Test.Elements;

public class RenderObjectSlotTest
{
    private sealed class Holder
    {
        public bool ShowColoredBox = true;
        public SwitcherState? State;
    }

    /// <summary>Build結果のRenderObjectの型を、ColoredBoxとSizedBoxで切り替える。</summary>
    private sealed record Switcher : StatefulWidget<Switcher>
    {
        public required Holder Holder { get; init; }

        public override State<Switcher> CreateState() => new SwitcherState();
    }

    private sealed class SwitcherState : State<Switcher>
    {
        public override void InitState() => Widget!.Holder.State = this;

        public void Toggle() => SetState(() => Widget!.Holder.ShowColoredBox = !Widget!.Holder.ShowColoredBox);

        public override Widget Build(IBuildContext context) => Widget!.Holder.ShowColoredBox
            ? new ColoredBox { Color = new Color(0, 0, 255) }
            : new SizedBox { Width = 1, Height = 1 };
    }

    private static readonly Widget Marker = new Opacity { Value = 0.5 };

    [Fact]
    public void Rebuild_Stackの先頭の子のRenderObjectが別の型へ差し替わる_子の順序を保つ()
    {
        var holder = new Holder();
        var (owner, renderView) = Mount(new Stack
        {
            Children = { new Switcher { Holder = holder }, Marker }
        });
        Assert.Equal([typeof(RenderColoredBox), typeof(RenderOpacity)], ChildTypes<RenderStack>(renderView));

        holder.State!.Toggle();
        owner.BuildScope();

        Assert.Equal([typeof(RenderConstrainedBox), typeof(RenderOpacity)], ChildTypes<RenderStack>(renderView));
    }

    [Fact]
    public void Rebuild_Rowの中央の子のRenderObjectが別の型へ差し替わる_子の順序を保つ()
    {
        var holder = new Holder();
        var (owner, renderView) = Mount(new Row
        {
            Children = { Marker, new Switcher { Holder = holder }, new Padding { Spacing = EdgeInsets.All(1) } }
        });

        holder.State!.Toggle();
        owner.BuildScope();

        Assert.Equal(
            [typeof(RenderOpacity), typeof(RenderConstrainedBox), typeof(RenderPadding)],
            ChildTypes<RenderFlex>(renderView));
    }

    [Fact]
    public void Update_Rowの途中へ子を追加_追加した位置へRenderObjectを挿入する()
    {
        var (owner, renderView, root) = MountRoot(new Row
        {
            Children = { Marker, new Padding { Spacing = EdgeInsets.All(1) } }
        });

        Update(owner, renderView, root, new Row
        {
            Children = { Marker, new SizedBox { Width = 1, Height = 1 }, new Padding { Spacing = EdgeInsets.All(1) } }
        });

        Assert.Equal(
            [typeof(RenderOpacity), typeof(RenderConstrainedBox), typeof(RenderPadding)],
            ChildTypes<RenderFlex>(renderView));
    }

    [Fact]
    public void Update_Keyつきの子を並べ替え_RenderObjectも同じ順序へ移動する()
    {
        var first = new SizedBox { Key = new ValueKey<string>("a"), Width = 1, Height = 1 };
        var second = new Opacity { Key = new ValueKey<string>("b"), Value = 0.5 };
        var third = new Padding { Key = new ValueKey<string>("c"), Spacing = EdgeInsets.All(1) };
        var (owner, renderView, root) = MountRoot(new Row { Children = { first, second, third } });
        var before = Children<RenderFlex>(renderView);

        Update(owner, renderView, root, new Row { Children = { third, first, second } });

        // 並べ替えではRenderObjectを作り直さず、同じインスタンスを移動する。
        // 失敗時の表示を読みやすくするため、RenderObjectそのものではなく元の位置で比べる。
        var after = Children<RenderFlex>(renderView).Select(child => before.IndexOf(child)).ToArray();
        Assert.Equal([2, 0, 1], after);
    }

    [Fact]
    public void Rebuild_直前の兄弟がStatefulWidget_兄弟の子孫のRenderObjectの次へ挿入する()
    {
        // 直前の兄弟がRenderObjectを持たないElementでも、その子孫のRenderObjectを挿入位置の基準にする。
        var first = new Holder();
        var second = new Holder();
        var (owner, renderView) = Mount(new Row
        {
            Children = { new Switcher { Holder = first }, new Switcher { Holder = second }, Marker }
        });

        second.State!.Toggle();
        owner.BuildScope();

        Assert.Equal(
            [typeof(RenderColoredBox), typeof(RenderConstrainedBox), typeof(RenderOpacity)],
            ChildTypes<RenderFlex>(renderView));
    }

    [Fact]
    public void Rebuild_ParentDataWidgetの下でRenderObjectが差し替わる_子の順序とParentDataを保つ()
    {
        var holder = new Holder();
        var (owner, renderView) = Mount(new Stack
        {
            Children =
            {
                new Positioned { Left = 3, Child = new Switcher { Holder = holder } },
                Marker
            }
        });

        holder.State!.Toggle();
        owner.BuildScope();

        var children = Children<RenderStack>(renderView);
        Assert.Equal([typeof(RenderConstrainedBox), typeof(RenderOpacity)], children.Select(child => child.GetType()));
        Assert.Equal(3, ((StackParentData)children[0].ParentData!).Left);
    }

    [Fact]
    public void Update_先頭の子を取り除く_残りの子の順序を保つ()
    {
        var (owner, renderView, root) = MountRoot(new Row
        {
            Children = { new SizedBox { Width = 1, Height = 1 }, Marker, new Padding { Spacing = EdgeInsets.All(1) } }
        });

        Update(owner, renderView, root, new Row
        {
            Children = { Marker, new Padding { Spacing = EdgeInsets.All(1) } }
        });

        Assert.Equal([typeof(RenderOpacity), typeof(RenderPadding)], ChildTypes<RenderFlex>(renderView));
    }

    [Fact]
    public void Update_子の並びが変わらない_RenderObjectを移動せずレイアウトを要求しない()
    {
        var (owner, renderView, root) = MountRoot(new Row
        {
            Children = { Marker, new SizedBox { Width = 1, Height = 1 } }
        });
        renderView.PrepareInitialFrame();
        renderView.Owner!.FlushLayout();
        var flex = (RenderFlex)Children<RenderView>(renderView)[0];
        Assert.False(flex.NeedsLayout);

        // 別インスタンスだが等しいWidgetで更新する。slotが変わらないので移動は起きない。
        Update(owner, renderView, root, new Row
        {
            Children = { Marker, new SizedBox { Width = 1, Height = 1 } }
        });

        Assert.False(flex.NeedsLayout);
    }

    [Fact]
    public void Slot_複数の子を持つ親の子_indexと直前の兄弟を保持する()
    {
        var (_, _, root) = MountRoot(new Row { Children = { Marker, new SizedBox { Width = 1, Height = 1 } } });
        var elements = new List<Element>();
        FindElement<MultiChildRenderObjectElement<RenderFlex>>(root).VisitChildren(elements.Add);

        Assert.Equal(new IndexedSlot(0, null), elements[0].Slot);
        Assert.Equal(new IndexedSlot(1, elements[0]), elements[1].Slot);
    }

    [Fact]
    public void Slot_StatefulWidgetの子_親のslotを引き継ぐ()
    {
        var holder = new Holder();
        var (_, _, root) = MountRoot(new Row { Children = { Marker, new Switcher { Holder = holder } } });
        var elements = new List<Element>();
        FindElement<MultiChildRenderObjectElement<RenderFlex>>(root).VisitChildren(elements.Add);
        Element? built = null;
        elements[1].VisitChildren(child => built = child);

        Assert.NotNull(built);
        Assert.Equal(elements[1].Slot, built.Slot);
    }

    [Fact]
    public void Move_既に指定した位置にある_レイアウトを要求しない()
    {
        var (first, second, collection, owner) = CreateCollection();
        owner.NeedsLayout = false;

        collection.Move(second, first);

        Assert.Equal([first, second], collection);
        Assert.False(owner.NeedsLayout);
    }

    [Fact]
    public void Move_afterがnull_先頭へ移動する()
    {
        var (first, second, collection, _) = CreateCollection();

        collection.Move(second, null);

        Assert.Equal([second, first], collection);
    }

    [Fact]
    public void Move_保持していない子_ArgumentExceptionを投げる()
    {
        var (first, _, collection, _) = CreateCollection();

        Assert.Throws<ArgumentException>(() => collection.Move(new RenderConstrainedBox(), first));
    }

    [Fact]
    public void Move_自分自身の次へ移動_ArgumentExceptionを投げ子を失わない()
    {
        var (first, second, collection, _) = CreateCollection();

        Assert.Throws<ArgumentException>(() => collection.Move(first, first));
        Assert.Equal([first, second], collection);
    }

    [Fact]
    public void Insert_afterが保持していない子_ArgumentExceptionを投げ子を追加しない()
    {
        var (first, second, collection, _) = CreateCollection();

        Assert.Throws<ArgumentException>(
            () => collection.Insert(new RenderConstrainedBox(), new RenderConstrainedBox()));
        Assert.Equal([first, second], collection);
    }

    private static (BuildOwner Owner, RenderView RenderView) Mount(Widget widget)
    {
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(() => { });
        new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, null);

        return (owner, renderView);
    }

    private static (BuildOwner Owner, RenderView RenderView, RenderObjectToWidgetElement<RenderView> Root) MountRoot(
        Widget widget)
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

        return (owner, renderView, root);
    }

    private static void Update(
        BuildOwner owner,
        RenderView renderView,
        RenderObjectToWidgetElement<RenderView> root,
        Widget widget)
    {
        new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = widget
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();
    }

    private static (RenderBox First, RenderBox Second, MultiChildrenCollection<RenderBox> Collection, RenderFlex Owner)
        CreateCollection()
    {
        var owner = new RenderFlex();
        RenderBox first = new RenderConstrainedBox();
        RenderBox second = new RenderConstrainedBox();
        owner.Children.Add(first);
        owner.Children.Add(second);
        return (first, second, owner.Children, owner);
    }

    private static T FindElement<T>(Element root) where T : Element
    {
        T? found = null;
        Visit(root);
        return found ?? throw new InvalidOperationException($"{typeof(T).Name} が見つかりません。");

        void Visit(Element element)
        {
            if (found is not null) return;
            if (element is T match)
            {
                found = match;
                return;
            }

            element.VisitChildren(Visit);
        }
    }

    private static Type[] ChildTypes<T>(RenderObject root) where T : RenderObject =>
        [.. Children<T>(root).Select(child => child.GetType())];

    private static List<RenderObject> Children<T>(RenderObject root) where T : RenderObject
    {
        T? parent = null;
        Find(root);
        Assert.NotNull(parent);

        var children = new List<RenderObject>();
        parent.VisitChildren(children.Add);
        return children;

        void Find(RenderObject node)
        {
            if (parent is not null) return;
            if (node is T match)
            {
                parent = match;
                return;
            }

            node.VisitChildren(Find);
        }
    }
}
