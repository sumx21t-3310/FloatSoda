using System.Numerics;
using FloatSoda.Engine;
using FloatSoda.Engine.OVR;
using FloatSoda.Geometry;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Samples.RenderPath;

/// <summary>
/// 段3: 段2と同じ Layer ツリーを、png ではなく GL テクスチャとしてオーバーレイに出す。
/// この最小のコードは、GL のリソースの後始末を省いている。
/// </summary>
static class Step3
{
    public static void Run()
    {
        var size = new SKSizeI(400, 300);

        using var app = new OVRApplication(new OVRAppInfo(new AppKey("com.example.render_path")));

        var identity = new OverlayIdentity(
            $"com.example.render_path.step3.{Guid.NewGuid()}",
            "RenderPath step 3"
        );
        using var overlay = new DeviceTrackedOverlay(identity);
        overlay.Transform.Position = new Vector3(0, 0, -1);

        using var cts = new CancellationTokenSource();
        var renderThread = new RenderPostTaskRunner("RenderThread", new FramePacer(60));
        renderThread.Start(cts.Token);

        // GLView と Renderer は描画スレッドで作る
        renderThread.PostTask(() =>
        {
            var renderer = new Renderer { GLView = new GLView(size) };
            var window = new OverlayWindow(overlay, renderer, new Dpm(1000));

            window.Resize(size); // 画像の幅(400 px / 1000 dpm = 0.4 m)をオーバーレイに反映する
            renderThread.PostRender(window, CreateLayerTree(size)); // Present は描画スレッドで走る
        });

        overlay.Visibility.Show();

        Console.WriteLine("HMD の中でオーバーレイを探してください。Enter キーで終了します。");
        Console.ReadLine();

        renderThread.Stop();
    }

    private static ILayer CreateLayerTree(SKSizeI size)
    {
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(new SKRect(0, 0, size.Width, size.Height));
        canvas.Clear(SKColors.WhiteSmoke);
        using var paint = new SKPaint { Color = SKColors.DarkSeaGreen };
        canvas.DrawRect(new SKRect(100, 75, 300, 225), paint);

        return new ContainerLayer
        {
            Children = { new PictureLayer { Picture = recorder.EndRecording() } },
        };
    }
}
