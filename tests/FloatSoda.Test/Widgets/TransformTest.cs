using System.Numerics;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
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
}
