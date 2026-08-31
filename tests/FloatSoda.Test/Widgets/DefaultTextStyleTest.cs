using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class DefaultTextStyleTest
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

    /// <summary>ルートWidgetを差し替えて再ビルドまで実行する。</summary>
    private static void ReattachRoot(
        (RenderPipeline Pipeline, RenderView View, RenderObjectToWidgetElement<RenderView> Root) tree,
        Widget widget)
    {
        new RenderObjectToWidgetAdapter
        {
            Container = tree.View,
            Child = widget
        }.AttachToRenderTree(tree.Root.Owner!, tree.Root);

        tree.Root.Owner!.BuildScope();
    }

    /// <summary>Build時にDefaultTextStyle.Ofの解決結果を記録するプローブ。</summary>
    private record Probe : StatelessWidget
    {
        public required Action<IBuildContext> OnBuild { get; init; }

        public override Widget Build(IBuildContext context)
        {
            OnBuild(context);
            return new SizedBox { Width = 10, Height = 10 };
        }
    }

    [Fact]
    public void Of_祖先にDefaultTextStyleが無い_全プロパティ未指定のフォールバックを返す()
    {
        DefaultTextStyle? found = null;

        Mount(new Probe { OnBuild = ctx => found = DefaultTextStyle.Of(ctx) });

        Assert.NotNull(found);
        Assert.Equal(new TextStyle(), found!.Style);
    }

    [Fact]
    public void Of_祖先が入れ子_最も近い祖先を返す()
    {
        DefaultTextStyle? found = null;

        Mount(new DefaultTextStyle
        {
            Style = new TextStyle { FontSize = 48 },
            Child = new DefaultTextStyle
            {
                Style = new TextStyle { FontSize = 12 },
                Child = new Probe { OnBuild = ctx => found = DefaultTextStyle.Of(ctx) }
            }
        });

        Assert.NotNull(found);
        Assert.Equal(12, found!.Style.FontSize);
    }

    [Fact]
    public void Of_フォールバックをツリーへ配置_ビルドで例外を投げる()
    {
        DefaultTextStyle? fallback = null;
        Mount(new Probe { OnBuild = ctx => fallback = DefaultTextStyle.Of(ctx) });

        Assert.Throws<InvalidOperationException>(() => Mount(fallback!));
    }

    [Fact]
    public void Build_配下のTextがStyle未指定_書式を継承する()
    {
        var style = new TextStyle { FontSize = 48, Color = new Color(255, 0, 0) };

        var (_, view, _) = Mount(new DefaultTextStyle
        {
            Style = style,
            Child = new Text("FloatSoda")
        });

        var renderParagraph = Assert.IsType<RenderParagraph>(view.Child);
        Assert.Equal(style, renderParagraph.Text.Style);
    }

    [Fact]
    public void Build_配下のTextが一部プロパティを明示_明示値を優先し残りを継承する()
    {
        var (_, view, _) = Mount(new DefaultTextStyle
        {
            Style = new TextStyle { FontSize = 48, Color = new Color(255, 0, 0) },
            Child = new Text("FloatSoda")
            {
                Style = new TextStyle { Color = new Color(0, 0, 255) }
            }
        });

        var renderParagraph = Assert.IsType<RenderParagraph>(view.Child);
        Assert.Equal(48, renderParagraph.Text.Style!.FontSize);
        Assert.Equal(new Color(0, 0, 255), renderParagraph.Text.Style.Color);
    }

    [Fact]
    public void Build_TextのStyleのInheritがfalse_DefaultTextStyleを継承しない()
    {
        var standalone = new TextStyle { Inherit = false, Color = new Color(0, 0, 255) };

        var (_, view, _) = Mount(new DefaultTextStyle
        {
            Style = new TextStyle { FontSize = 48 },
            Child = new Text("FloatSoda") { Style = standalone }
        });

        var renderParagraph = Assert.IsType<RenderParagraph>(view.Child);
        Assert.Same(standalone, renderParagraph.Text.Style);
        Assert.Null(renderParagraph.Text.Style!.FontSize);
    }

    [Fact]
    public void Build_Styleを変更して再アタッチ_依存するTextが再ビルドされる()
    {
        Text CreateText() => new("FloatSoda");

        var tree = Mount(new DefaultTextStyle
        {
            Style = new TextStyle { FontSize = 48 },
            Child = CreateText()
        });
        var renderParagraph = Assert.IsType<RenderParagraph>(tree.View.Child);
        Assert.Equal(48, renderParagraph.Text.Style!.FontSize);

        // Text自体はrecord等価なwidgetのまま、DefaultTextStyleのStyleだけを変える。
        // 依存追跡による再ビルドが働かなければ、TextのRichTextは古い書式のまま残る。
        ReattachRoot(tree, new DefaultTextStyle
        {
            Style = new TextStyle { FontSize = 96 },
            Child = CreateText()
        });

        Assert.Same(renderParagraph, tree.View.Child);
        Assert.Equal(96, renderParagraph.Text.Style!.FontSize);
    }

    [Fact]
    public void UpdateShouldNotify_Styleが同値_falseを返す()
    {
        var widget = new DefaultTextStyle { Style = new TextStyle { FontSize = 48 }, Child = new SizedBox() };
        var old = new DefaultTextStyle { Style = new TextStyle { FontSize = 48 }, Child = new SizedBox() };

        Assert.False(widget.UpdateShouldNotify(old));
    }

    [Fact]
    public void UpdateShouldNotify_Styleが変更_trueを返す()
    {
        var widget = new DefaultTextStyle { Style = new TextStyle { FontSize = 96 }, Child = new SizedBox() };
        var old = new DefaultTextStyle { Style = new TextStyle { FontSize = 48 }, Child = new SizedBox() };

        Assert.True(widget.UpdateShouldNotify(old));
    }
}
