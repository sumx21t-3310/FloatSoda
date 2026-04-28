using System.Diagnostics;
using System.Numerics;
using FloatSoda;
using FloatSoda.Engine;
using FloatSoda.Exceptions;
using FloatSoda.OVR;
using FloatSoda.Render;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using Valve.VR;
using Vector2 = FloatSoda.Vector2;


using var initializer = new OVRInitializer();
initializer.Initialize();

int width = 1000, height = 1000;

unsafe
{
    GLFW.Init();
    GLFW.WindowHint(WindowHintBool.Visible, false);

    var window = GLFW.CreateWindow(width, width, "", null, null);
    GLFW.MakeContextCurrent(window);

    GL.LoadBindings(new GLFWBindingsContext());
}





var interfaceNative = GRGlInterface.Create();
var grContext = GRContext.CreateGl(interfaceNative);


var textureId = GL.GenTexture();

GL.BindTexture(TextureTarget.Texture2D, textureId);
GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

var backendTexture = new GRBackendTexture(width, height, false, new GRGlTextureInfo
{
    Id = (uint)textureId,
    Target = (uint)TextureTarget.Texture2D,
    Format = (uint)InternalFormat.Rgba8
});

var surface = SKSurface.Create(grContext, backendTexture, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888);


using var overlay = new Overlay("FloatSoda", "FloatSoda");

overlay.Create();
overlay.Show();
overlay.SetOverlayWidthInMeters(0.5f);
overlay.SetAbsolutePosition(new Transform
{
    Position = new Vector3(0, 1.7f, -1.0f),
    Rotation = Quaternion.Identity,
});

using var renderContext = new RenderContext(new SKPaint(), surface.Canvas);

var limiter = new FrameLimiter(120);
while (true)
{
    Element rootElement = new BoxElement
    {
        Size = new Size(1000, 1000),
        Color = SKColors.AliceBlue,
        Child = new BoxElement
        {
            Size = new Size(100, 100),
            Position = new Vector2(X: Random.Shared.Next(0, 100), Y: 0),
            Color = SKColors.CadetBlue,
        },
    };

    rootElement.Draw(renderContext);

    // 3. SkiaSharpのコマンドをGPU（OpenGL）に送る
    surface.Canvas.Flush();
    grContext.Flush();

    // 4. OpenGLの実行完了を待機
    GL.Flush();

    var texture = new Texture_t
    {
        handle = textureId,
        eType = ETextureType.OpenGL,
        eColorSpace = EColorSpace.Auto,
    };

    // 5. VRオーバーレイにテクスチャをセット
    overlay.SetTexture(ref texture);

    // 6. フレーム制限
    limiter.Wait();
}

public class FrameLimiter(int targetFrameRate = 30)
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    // 1フレームあたりの目標時間を「ティック単位」で計算
    private readonly double _targetTicksPerFrame = Stopwatch.Frequency / (double)targetFrameRate;

    public void Wait()
    {
        // 1. 前回のSyncから経過した時間を取得
        double elapsedTicks = _stopwatch.ElapsedTicks;

        // 2. 待機が必要か判定
        double ticksToWait = _targetTicksPerFrame - elapsedTicks;

        if (ticksToWait > 0)
        {
            // 残り時間が長い場合は、CPU負荷を下げるために Sleep を使う
            // ただし、Sleepの誤差を考慮して「1ミリ秒前」には切り上げる
            int msToSleep = (int)((ticksToWait / Stopwatch.Frequency) * 1000) - 1;

            if (msToSleep > 0)
            {
                Thread.Sleep(msToSleep);
            }

            // 3. 【重要】最後の微調整。目標時間に達するまでループで待機（高精度）
            while (_stopwatch.ElapsedTicks < _targetTicksPerFrame)
            {
                // Thread.Yield() は他のスレッドに処理権を譲りつつ、即座に戻ってくる
                Thread.Yield();
            }
        }

        // 4. ストップウォッチをリセットして再スタート
        _stopwatch.Restart();
    }
}

public class OVRInitializer : IDisposable
{
    public void Initialize()
    {
        var error = EVRInitError.None;
        OpenVR.Init(ref error, EVRApplicationType.VRApplication_Overlay);

        error.ThrowIfError();
    }

    public void Shutdown()
    {
        if (OpenVR.System == null) return;
        OpenVR.Shutdown();
    }

    public void Dispose()
    {
        Shutdown();
    }
}

public class Overlay(string key, string name) : IDisposable
{
    private ulong _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;

    public void Create() => OpenVR.Overlay.CreateOverlay(key, name, ref _overlayHandle).ThrowIfError();

    public void Show() => OpenVR.Overlay.ShowOverlay(_overlayHandle).ThrowIfError();

    public void SetImage(string filePath) => OpenVR.Overlay.SetOverlayFromFile(_overlayHandle, filePath).ThrowIfError();

    public void SetTexture(ref Texture_t texture) => OpenVR.Overlay.SetOverlayTexture(_overlayHandle, ref texture).ThrowIfError();

    public void SetOverlayWidthInMeters(float width) => OpenVR.Overlay.SetOverlayWidthInMeters(_overlayHandle, width).ThrowIfError();

    public void SetAbsolutePosition(Transform transform)
    {
        var matrix = transform.ToHmdMatrix34_t();
        OpenVR.Overlay.SetOverlayTransformAbsolute(_overlayHandle, ETrackingUniverseOrigin.TrackingUniverseStanding, ref matrix).ThrowIfError();
    }


    public void Dispose()
    {
        if (_overlayHandle == OpenVR.k_ulOverlayHandleInvalid) return;

        OpenVR.Overlay.DestroyOverlay(_overlayHandle).ThrowIfError();
        _overlayHandle = OpenVR.k_ulOverlayHandleInvalid;
    }
}