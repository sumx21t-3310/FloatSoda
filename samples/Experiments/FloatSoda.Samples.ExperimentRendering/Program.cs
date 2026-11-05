using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

// png の出力先は第1引数で指定する。省略すると、カレントディレクトリに出す。
var outputDirectory = Path.GetFullPath(args.Length > 0 ? args[0] : ".");
Directory.CreateDirectory(outputDirectory);

var size = new SKSizeI(400, 300);
var renderer = new LayerBitmapRenderer();

Save("plain", picture => picture);
Save("opacity", picture => new OpacityLayer { Alpha = 128, Children = { picture } });
Save("clip", picture => new ClipRectLayer(new SKRect(0, 0, 200, 150)) { Children = { picture } });
Save(
    "transform",
    picture => new TransformLayer
    {
        Transform = SKMatrix.CreateRotationDegrees(15, 200, 150),
        Children = { picture },
    }
);

Console.WriteLine($"保存先: {outputDirectory}");

void Save(string name, Func<ILayer, ILayer> wrap)
{
    // PictureLayer は SKPicture の所有権を持たないので、描画と保存が終わるまで呼び出し側が保持する
    using var picture = CreatePicture();
    var root = new ContainerLayer { Children = { wrap(new PictureLayer { Picture = picture }) } };

    using var bitmap = renderer.Render(root, size);
    bitmap.Save(Path.Combine(outputDirectory, $"{name}.png"));

    Console.WriteLine(
        $"{name}.png: 青(50,50)={bitmap.GetPixel(50, 50)} "
            + $"橙(300,230)={bitmap.GetPixel(300, 230)} 重なり(150,120)={bitmap.GetPixel(150, 120)}"
    );
}

SKPicture CreatePicture()
{
    using var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));

    using var blue = new SKPaint { Color = SKColors.CornflowerBlue };
    using var orange = new SKPaint { Color = SKColors.Orange };
    canvas.DrawRect(new SKRect(20, 20, 220, 160), blue);
    canvas.DrawRect(new SKRect(120, 100, 360, 260), orange);

    return recorder.EndRecording();
}
