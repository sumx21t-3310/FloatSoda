using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Samples.RenderPath;

/// <summary>段2: 自分で組んだ Layer ツリーを png にする。段1のオーバーレイに出す絵はこの出力を使える。</summary>
static class Step2
{
    public static void Run(string outputPath)
    {
        var size = new SKSizeI(400, 300);

        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));
        canvas.Clear(SKColors.WhiteSmoke);
        using var paint = new SKPaint { Color = SKColors.DarkSeaGreen };
        canvas.DrawRect(new SKRect(100, 75, 300, 225), paint);

        var root = new ContainerLayer
        {
            Children = { new PictureLayer { Picture = recorder.EndRecording() } },
        };

        using var bitmap = new LayerBitmapRenderer().Render(root, size);
        bitmap.Save(outputPath);

        Console.WriteLine($"保存しました: {outputPath}");
    }
}
