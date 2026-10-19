using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class ClipTest
{
    [Fact]
    public void HitTest_ClipOvalで楕円の外の角を押す_子が反応しない()
    {
        var log = new List<string>();

        Tap(new ClipOval { Child = Tappable(log) }, 5, 5);

        Assert.Empty(log);
    }

    [Fact]
    public void HitTest_ClipOvalの中心を押す_子が反応する()
    {
        var log = new List<string>();

        Tap(new ClipOval { Child = Tappable(log) }, 50, 50);

        Assert.Equal(["tap"], log);
    }

    [Fact]
    public void HitTest_ClipOvalとClipperで切り出した楕円の外を押す_子が反応しない()
    {
        var log = new List<string>();

        // 左半分 (0,0)-(50,100) に内接する楕円の中心は (25,50)。(60,50) は楕円の外
        Tap(new ClipOval { Clipper = new LeftHalf(), Child = Tappable(log) }, 60, 50);

        Assert.Empty(log);
    }

    [Fact]
    public void HitTest_ClipRectとClipperで切り落とした右半分を押す_子が反応しない()
    {
        var log = new List<string>();

        Tap(new ClipRect { Clipper = new LeftHalf(), Child = Tappable(log) }, 75, 50);

        Assert.Empty(log);
    }

    [Fact]
    public void HitTest_ClipRectとClipperで残した左半分を押す_子が反応する()
    {
        var log = new List<string>();

        Tap(new ClipRect { Clipper = new LeftHalf(), Child = Tappable(log) }, 25, 50);

        Assert.Equal(["tap"], log);
    }

    [Fact]
    public void HitTest_ClipCustomPathとClipperで切り落とした右半分を押す_子が反応しない()
    {
        var log = new List<string>();

        Tap(new ClipCustomPath { Clipper = new LeftHalfPath(), Child = Tappable(log) }, 75, 50);

        Assert.Empty(log);
    }

    [Fact]
    public void HitTest_ClipRoundRectとClipperで角の外を押す_子が反応しない()
    {
        var log = new List<string>();

        Tap(new ClipRoundRect { Clipper = new RoundedFull(), Child = Tappable(log) }, 2, 2);

        Assert.Empty(log);
    }

    [Fact]
    public void HitTest_ClipRoundRectでClipperを省略し角の外を押す_子が反応する()
    {
        var log = new List<string>();

        // Flutter の RenderClipRRect と同じく、Clipper がないときは角丸の形を見ない
        Tap(new ClipRoundRect { BorderRadius = BorderRadius.Circular(40), Child = Tappable(log) }, 2, 2);

        Assert.Equal(["tap"], log);
    }

    [Fact]
    public void HitTest_ClipRoundRectとClipperで角の円の内側を押す_子が反応する()
    {
        var log = new List<string>();

        // 半径 40 の左上の角の円の中心は (40,40)。(15,15) までの距離は約 35 なので円の内側
        Tap(new ClipRoundRect { Clipper = new RoundedFull(), Child = Tappable(log) }, 15, 15);

        Assert.Equal(["tap"], log);
    }

    [Fact]
    public void Clip_ポインターを何度も動かす_ClipperのGetClipを呼び直さない()
    {
        var clipper = new CountingLeftHalf();
        var binding = new WidgetBinding(new RenderView());
        binding.AttachRootWidget(new ClipRect { Clipper = clipper, Child = Tappable([]) });
        binding.FlushBuildAndLayout();

        binding.HandlePointerEvent(new PointerEvent(1, PointerEventPhase.Add, new Offset(10, 10)));
        for (var x = 10; x < 20; x++)
        {
            binding.HandlePointerEvent(new PointerEvent(1, PointerEventPhase.Move, new Offset(x, 10)));
        }

        Assert.Equal(1, clipper.GetClipCount);
    }

    private sealed class CountingLeftHalf : CustomClipper<SKRect>
    {
        public int GetClipCount { get; private set; }

        public override SKRect GetClip(SKSize size)
        {
            GetClipCount++;
            return new SKRect(0, 0, size.Width / 2, size.Height);
        }

        public override bool ShouldReclip(CustomClipper<SKRect> oldClipper) => false;
    }

    private sealed class LeftHalf : CustomClipper<SKRect>
    {
        public override SKRect GetClip(SKSize size) => new(0, 0, size.Width / 2, size.Height);

        public override bool ShouldReclip(CustomClipper<SKRect> oldClipper) => false;
    }

    private sealed class LeftHalfPath : CustomClipper<SKPath>
    {
        public override SKPath GetClip(SKSize size)
        {
            var path = new SKPath();
            path.AddRect(new SKRect(0, 0, size.Width / 2, size.Height));
            return path;
        }

        public override bool ShouldReclip(CustomClipper<SKPath> oldClipper) => false;
    }

    private sealed class RoundedFull : CustomClipper<SKRoundRect>
    {
        public override SKRoundRect GetClip(SKSize size) => new(new SKRect(0, 0, size.Width, size.Height), 40);

        public override bool ShouldReclip(CustomClipper<SKRoundRect> oldClipper) => false;
    }

    private static Widget Tappable(List<string> log) => new GestureDetector
    {
        Behaviour = HitTestBehaviour.Opaque,
        OnTap = () => log.Add("tap"),
        Child = new SizedBox { Width = 100, Height = 100 },
    };

    private static void Tap(Widget root, double x, double y)
    {
        var binding = new WidgetBinding(new RenderView());
        binding.AttachRootWidget(root);
        binding.FlushBuildAndLayout();
        foreach (var phase in new[] { PointerEventPhase.Add, PointerEventPhase.Down, PointerEventPhase.Up })
        {
            binding.HandlePointerEvent(new PointerEvent(1, phase, new Offset(x, y)));
        }
    }
}
