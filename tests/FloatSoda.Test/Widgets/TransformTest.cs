using System.Numerics;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Gesture;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class TransformTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    [Fact]
    public void Transform_平行移動行列を指定_レイアウトを変えずに描画位置だけ移動する()
    {
        var widget = new SizedBox
        {
            Width = Size.Width,
            Height = Size.Height,
            Child = new Align
            {
                Alignment = Alignment.Center,
                Child = new Transform
                {
                    Matrix = Matrix3x2.CreateTranslation(30, 20),
                    Child = new SizedBox
                    {
                        Width = 20,
                        Height = 20,
                        Child = new ColoredBox { Color = new Color(255, 0, 0) }
                    }
                }
            }
        };

        using var bitmap = Renderer.Render(widget, Size);

        Assert.Equal(0, bitmap.GetPixel(45, 45).Alpha);
        Assert.Equal(SKColors.Red, bitmap.GetPixel(75, 65));
    }

    [Fact]
    public void Transform_Matrixが非有限値_ArgumentOutOfRangeExceptionを投げる()
    {
        var matrix = Matrix3x2.Identity;
        matrix.M11 = float.NaN;

        Assert.Throws<ArgumentOutOfRangeException>(() => new Transform { Matrix = matrix });
    }

    [Fact]
    public void HitTest_TransformHitTestsが有効_逆変換した座標で判定する()
    {
        var renderTransform = new RenderTransform
        {
            Transform = Matrix3x2.CreateScale(0.5f),
            Child = new RenderColoredBox()
        };
        renderTransform.Layout(BoxConstraints.Tight(20, 20));

        Assert.False(renderTransform.HitTest(new HitTestResult(), new FloatSoda.Abstractions.Geometries.Offset(15, 15)));

        renderTransform.TransformHitTests = false;

        Assert.True(renderTransform.HitTest(new HitTestResult(), new FloatSoda.Abstractions.Geometries.Offset(15, 15)));
    }

    [Fact]
    public void HitTest_二倍拡大で見た目の内側かつレイアウトの外を押す_子が反応する()
    {
        // 50x50 の箱を中心基準で2倍にした見た目は (150,150)-(250,250)。レイアウト上は (175,175)-(225,225)
        var (binding, events) = MountTransformed(Matrix3x2.CreateScale(2));

        Send(binding, PointerEventPhase.Add, 160, 160);
        Send(binding, PointerEventPhase.Down, 160, 160);

        Assert.Single(events);
    }

    [Fact]
    public void HitTest_四十五度回転で見た目の頂点の近くを押す_子が反応する()
    {
        // 見た目の右の頂点は (235.36, 200)。(230,200) は見た目の内側で、レイアウト上の箱の外
        var (binding, events) = MountTransformed(Matrix3x2.CreateRotation(MathF.PI / 4));

        Send(binding, PointerEventPhase.Add, 230, 200);
        Send(binding, PointerEventPhase.Down, 230, 200);

        Assert.Single(events);
    }

    [Fact]
    public void HitTest_二倍拡大の見た目の外を押す_子が反応しない()
    {
        var (binding, events) = MountTransformed(Matrix3x2.CreateScale(2));

        Send(binding, PointerEventPhase.Add, 140, 140);
        Send(binding, PointerEventPhase.Down, 140, 140);

        Assert.Empty(events);
    }

    [Fact]
    public void HitTestChildren_二倍拡大_イベントのTransformに拡大が入る()
    {
        var (binding, events) = MountTransformed(Matrix3x2.CreateScale(2));

        Send(binding, PointerEventPhase.Add, 210, 210);
        Send(binding, PointerEventPhase.Down, 210, 210);

        // 見た目の (210,210) は、子のローカル座標で 25 + (210 - 200) / 2 = 30
        var local = Vector2.Transform(new Vector2(210, 210), Assert.Single(events).Transform!.Value);
        Assert.Equal(30f, local.X, 3);
        Assert.Equal(30f, local.Y, 3);
    }

    private static (WidgetBinding Binding, List<PointerEvent> Events) MountTransformed(Matrix3x2 matrix)
    {
        var events = new List<PointerEvent>();
        var binding = new WidgetBinding(new RenderView { FixedSize = new SKSize(400, 400) });
        binding.AttachRootWidget(new Center
        {
            Child = new Transform
            {
                Matrix = matrix,
                Alignment = Alignment.Center,
                Child = new Listener
                {
                    Behaviour = HitTestBehaviour.Opaque,
                    OnPointerDown = events.Add,
                    Child = new SizedBox { Width = 50, Height = 50 },
                },
            },
        });
        binding.FlushBuildAndLayout();
        return (binding, events);
    }

    private static void Send(WidgetBinding binding, PointerEventPhase phase, double x, double y)
        => binding.HandlePointerEvent(new PointerEvent(1, phase, new FloatSoda.Abstractions.Geometries.Offset(x, y)));
}
