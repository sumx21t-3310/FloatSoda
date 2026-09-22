using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class FlexParityFindingsTest
{
    [Fact]
    public void CreateRenderObject_配置を省略_子を主軸の開始位置へ配置する()
    {
        var view = Mount(new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new Flex
            {
                Direction = Axis.Horizontal,
                Children = { new SizedBox { Width = 20, Height = 20 } }
            }
        });

        var flex = Find<RenderFlex>(view);
        var child = Assert.Single(flex.Children);

        Assert.Equal(new Offset(0, 40), Assert.IsType<FlexParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void Build_Rowの交差軸配置を省略_子を中央へ配置する()
    {
        var view = Mount(new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new Row
            {
                Children = { new SizedBox { Width = 20, Height = 20 } }
            }
        });

        var flex = Find<RenderFlex>(view);
        var child = Assert.Single(flex.Children);

        Assert.Equal(new Offset(0, 40), Assert.IsType<FlexParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void Build_Columnの交差軸配置を省略_子を中央へ配置する()
    {
        var view = Mount(new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new Column
            {
                Children = { new SizedBox { Width = 20, Height = 20 } }
            }
        });

        var flex = Find<RenderFlex>(view);
        var child = Assert.Single(flex.Children);

        Assert.Equal(new Offset(40, 0), Assert.IsType<FlexParentData>(child.ParentData).Offset);
    }

    private static RenderView Mount(Widget widget)
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        _ = new RenderObjectToWidgetAdapter { Container = view, Child = widget }
            .AttachToRenderTree(new BuildOwner(() => { }), null);
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }

    private static T Find<T>(RenderObject root) where T : RenderObject
    {
        T? found = null;
        Visit(root);
        return found ?? throw new InvalidOperationException($"{typeof(T).Name} が見つかりません。");

        void Visit(RenderObject node)
        {
            if (found is not null) return;
            if (node is T match)
            {
                found = match;
                return;
            }

            node.VisitChildren(Visit);
        }
    }
}

public class LayoutInputParityFindingsTest
{
    [Fact]
    public void Layout_制約不変でRelayoutBoundaryだけ変更_PerformLayoutを繰り返さない()
    {
        var child = new LayoutCountingBox();
        var parent = new RenderConstrainedBox { Child = child };
        parent.RelayoutBoundary = parent;
        var constraints = new BoxConstraints(MaxWidth: 100, MaxHeight: 100);
        child.Layout(constraints);
        Assert.Equal(1, child.LayoutCount);

        child.Layout(constraints, parentUseSize: true);

        Assert.Same(parent, child.RelayoutBoundary);
        Assert.Equal(1, child.LayoutCount);
    }

    [Fact]
    public void WidthFactor_負数を指定_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Align { WidthFactor = -1 });
    }

    [Fact]
    public void StepWidth_0を指定_丸めを無効にする()
    {
        var widget = new IntrinsicWidth { StepWidth = 0 };

        Assert.Null(widget.CreateRenderObject().StepWidth);
    }

    private sealed class LayoutCountingBox : RenderBox
    {
        public int LayoutCount { get; private set; }

        public override void PerformLayout()
        {
            LayoutCount++;
            Size = Constraints.Smallest;
        }

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}

public class ClipParityFindingsTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();

    [Fact]
    public void ClipBehavior_ClipRectで指定を省略_HardEdgeを使用する()
    {
        var renderObject = new ClipRect().CreateRenderObject();

        Assert.Equal(Clip.HardEdge, renderObject.ClipBehavior);
    }

    [Fact]
    public void ClipBehavior_Noneを指定_子を切り抜かず描画する()
    {
        using var bitmap = Renderer.Render(
            new SizedBox
            {
                Width = 20,
                Height = 20,
                Child = new ClipRect
                {
                    ClipBehavior = Clip.None,
                    Clipper = new TenPixelClipper(),
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            },
            new SKSizeI(20, 20));

        Assert.Equal(SKColors.Red, bitmap.GetPixel(15, 15));
    }

    [Fact]
    public void UpdateRenderObject_描画後にBorderRadiusを変更_PaintDirtyにする()
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        _ = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new SizedBox
            {
                Width = 20,
                Height = 20,
                Child = new ClipRoundRect
                {
                    BorderRadius = BorderRadius.Zero,
                    Child = new ColoredBox { Color = new Color(255, 0, 0) }
                }
            }
        }.AttachToRenderTree(new BuildOwner(() => { }), null);
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();
        var renderObject = Find<RenderClipRoundRect>(view);
        Assert.False(renderObject.NeedsPaint);

        new ClipRoundRect { BorderRadius = BorderRadius.Circular(8) }.UpdateRenderObject(renderObject);

        Assert.True(renderObject.NeedsPaint);
    }

    [Fact]
    public void HitTest_楕円の外側かつ外接矩形の内側_ヒット対象にしない()
    {
        var listener = new RenderPointerListener
        {
            Behaviour = HitTestBehaviour.Opaque,
            Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(100, 100) }
        };
        var clip = new RenderClipOval { Child = listener };
        var view = Mount(clip);
        var result = new HitTestResult();

        view.HitTest(result, new Offset(1, 1));

        Assert.DoesNotContain(result.Path, entry => ReferenceEquals(entry.Target, listener));
    }

    private static RenderView Mount(RenderBox child)
    {
        var view = new RenderView { Child = child };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        return view;
    }

    private static T Find<T>(RenderObject root) where T : RenderObject
    {
        T? found = null;
        Visit(root);
        return found ?? throw new InvalidOperationException($"{typeof(T).Name} が見つかりません。");

        void Visit(RenderObject node)
        {
            if (found is not null) return;
            if (node is T match)
            {
                found = match;
                return;
            }

            node.VisitChildren(Visit);
        }
    }

    private sealed class TenPixelClipper : CustomClipper<SKRect>
    {
        public override SKRect GetClip(SKSize size) => SKRect.Create(0, 0, 10, 10);

        public override bool ShouldReclip(CustomClipper<SKRect> oldClipper) => false;
    }
}

public class PointerParityFindingsTest
{
    [Fact]
    public void Ignoring_指定を省略_ポインター入力を無視する()
    {
        var renderObject = new IgnorePointer().CreateRenderObject();

        Assert.True(renderObject.Ignoring);
    }

    [Fact]
    public void HitTest_画像の領域内_自身をヒット対象にする()
    {
        using var bitmap = new SKBitmap(8, 8);
        using var image = SKImage.FromBitmap(bitmap);
        var renderImage = new RenderImage { Image = image };
        renderImage.Layout(BoxConstraints.Tight(8, 8));
        var result = new HitTestResult();

        var hit = renderImage.HitTest(result, new Offset(4, 4));

        Assert.True(hit);
        Assert.Contains(result.Path, entry => ReferenceEquals(entry.Target, renderImage));
    }

    [Fact]
    public void Paint_Fitを省略_画像を原寸より拡大しない()
    {
        using var sourceBitmap = new SKBitmap(8, 4);
        sourceBitmap.Erase(SKColors.Blue);
        using var image = SKImage.FromBitmap(sourceBitmap);
        var renderImage = new RenderImage { Image = image };
        using var rendered = new RenderObjectBitmapRenderer().Render(
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(40, 40),
                Child = renderImage
            },
            new SKSizeI(40, 40));

        Assert.Equal(default, rendered.GetPixel(2, 15));
    }
}

public class PaintingApiParityFindingsTest
{
    [Fact]
    public void Color_指定を省略_必須プロパティとして拒否する()
    {
        var property = typeof(ColoredBox).GetProperty(nameof(ColoredBox.Color));

        Assert.NotNull(property);
        Assert.True(property.IsDefined(typeof(RequiredMemberAttribute)));
    }
}

public class WidgetUpdateParityFindingsTest
{
    [Fact]
    public void Build_値が等しい新しいBuilderへ更新_もう一度呼び出す()
    {
        var buildCount = 0;
        Widget Build(IBuildContext context)
        {
            buildCount++;
            return new SizedBox();
        }

        var owner = new BuildOwner(() => { });
        var view = new RenderView();
        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Builder { ChildBuilder = Build }
        }.AttachToRenderTree(owner, null);
        Assert.Equal(1, buildCount);

        _ = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Builder { ChildBuilder = Build }
        }.AttachToRenderTree(owner, root);
        owner.BuildScope();

        Assert.Equal(2, buildCount);
    }
}
