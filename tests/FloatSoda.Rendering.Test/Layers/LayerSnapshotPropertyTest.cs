using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Rendering.Test.Layers;

public class LayerSnapshotPropertyTest
{
    private static readonly LayerBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    private static PictureLayer MakePicture(SKColor color)
    {
        var rect = SKRect.Create(0, 0, 100, 100);
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(rect);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(rect, paint);
        return new PictureLayer { Picture = recorder.EndRecording() };
    }

    [Fact]
    public void Clone_複製後に元ツリーを更新_Snapshotの描画結果は変化しない()
    {
        // Property: FS-RENDER-SAFE-001
        // frame A のレイヤーツリーをsnapshot化した後、frame B相当の更新を元ツリーへ加える。
        var source = new ContainerLayer();
        source.Children.Add(MakePicture(SKColors.Red));

        var snapshot = source.Clone();

        source.Children.Clear();
        source.Children.Add(MakePicture(SKColors.Blue));

        using var snapshotBitmap = Renderer.Render(snapshot, Size);
        using var sourceBitmap = Renderer.Render(source, Size);

        Assert.Equal(SKColors.Red, snapshotBitmap.GetPixel(50, 50));
        Assert.Equal(SKColors.Blue, sourceBitmap.GetPixel(50, 50));
    }
}
