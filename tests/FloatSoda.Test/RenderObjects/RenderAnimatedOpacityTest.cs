using FloatSoda.Animation;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Testing;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Animation;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Test.Animation;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderAnimatedOpacityTest
{
    private static readonly SKSizeI Size = new(100, 100);

    private static AnimationController CreateController(FakeFrameScheduler scheduler) => new()
    {
        Vsync = new TickerProvider { ResolveScheduler = () => scheduler },
        Duration = TimeSpan.FromSeconds(1)
    };

    /// <summary>RenderAnimatedOpacityをパイプラインへAttachし、初回フレームをフラッシュ済みの状態で返す。</summary>
    private static (RenderAnimatedOpacity opacityBox, RenderPipeline pipeline) CreateAttachedTree(
        IAnimation<double> animation)
    {
        var opacityBox = new RenderAnimatedOpacity
        {
            Opacity = animation,
            Child = new RenderColoredBox { Color = SKColors.Red }
        };

        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = new RenderView(Size.Width, Size.Height)
            {
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(Size.Width, Size.Height),
                    Child = opacityBox
                }
            }
        };

        pipeline.RenderView.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();

        return (opacityBox, pipeline);
    }

    [Fact]
    public void AnimationChange_MarksNeedsPaint()
    {
        var scheduler = new FakeFrameScheduler();
        var controller = CreateController(scheduler);
        var (opacityBox, _) = CreateAttachedTree(controller);

        Assert.False(opacityBox.NeedsPaint);

        controller.Forward();
        scheduler.Pump(TimeSpan.Zero);
        scheduler.Pump(TimeSpan.FromSeconds(0.5));

        Assert.True(opacityBox.NeedsPaint);
    }

    [Fact]
    public void UnchangedValue_DoesNotMarkNeedsPaint()
    {
        var scheduler = new FakeFrameScheduler();
        var controller = CreateController(scheduler);
        var (opacityBox, pipeline) = CreateAttachedTree(controller);

        controller.Forward();
        scheduler.Pump(TimeSpan.Zero);
        scheduler.Pump(TimeSpan.FromSeconds(0.5));
        pipeline.FlushPaint();

        Assert.False(opacityBox.NeedsPaint);

        // 同じタイムスタンプ → 値が変わらないフレームでは再ペイントしない
        scheduler.Pump(TimeSpan.FromSeconds(0.5));

        Assert.False(opacityBox.NeedsPaint);
    }

    [Fact]
    public void Detach_UnsubscribesFromAnimation()
    {
        var scheduler = new FakeFrameScheduler();
        var controller = CreateController(scheduler);
        var (opacityBox, _) = CreateAttachedTree(controller);

        opacityBox.Detach();

        controller.Forward();
        scheduler.Pump(TimeSpan.Zero);
        scheduler.Pump(TimeSpan.FromSeconds(0.5));

        Assert.False(opacityBox.NeedsPaint);
    }

    [Fact]
    public void OpacitySwap_ResubscribesToNewAnimation()
    {
        var scheduler1 = new FakeFrameScheduler();
        var scheduler2 = new FakeFrameScheduler();
        var controller1 = CreateController(scheduler1);
        var controller2 = CreateController(scheduler2);
        var (opacityBox, pipeline) = CreateAttachedTree(controller1);

        opacityBox.Opacity = controller2;
        pipeline.FlushPaint();
        Assert.False(opacityBox.NeedsPaint);

        // 旧アニメーションはもう反映されない
        controller1.Forward();
        scheduler1.Pump(TimeSpan.Zero);
        scheduler1.Pump(TimeSpan.FromSeconds(0.5));
        Assert.False(opacityBox.NeedsPaint);

        // 新アニメーションには追従する
        controller2.Forward();
        scheduler2.Pump(TimeSpan.Zero);
        scheduler2.Pump(TimeSpan.FromSeconds(0.5));
        Assert.True(opacityBox.NeedsPaint);
    }

    [Fact]
    public void RendersChildWithAnimatedAlpha()
    {
        var scheduler = new FakeFrameScheduler();
        var controller = CreateController(scheduler);
        controller.Forward(from: 0.5); // Value = 0.5 のまま(Pumpしない)

        var tree = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(Size.Width, Size.Height),
            Child = new RenderAnimatedOpacity
            {
                Opacity = controller,
                Child = new RenderColoredBox { Color = SKColors.Red }
            }
        };

        using var bitmap = new RenderObjectBitmapRenderer().Render(tree, Size);

        var pixel = bitmap.GetPixel(50, 50);
        Assert.Equal(255, pixel.Red);
        Assert.InRange(pixel.Alpha, 120, 136); // 0.5 * 255 ≒ 128
    }

    [Fact]
    public void ZeroAlpha_SkipsChildPainting()
    {
        var scheduler = new FakeFrameScheduler();
        var controller = CreateController(scheduler); // Value = 0.0(初期値)

        var tree = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(Size.Width, Size.Height),
            Child = new RenderAnimatedOpacity
            {
                Opacity = controller,
                Child = new RenderColoredBox { Color = SKColors.Red }
            }
        };

        using var bitmap = new RenderObjectBitmapRenderer().Render(tree, Size);

        Assert.Equal(0, bitmap.GetPixel(50, 50).Alpha);
    }
}
