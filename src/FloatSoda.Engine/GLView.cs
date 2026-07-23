using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace FloatSoda.Engine;

/// <summary>
/// GLFW の OpenGL コンテキストと、描画先となる Skia サーフェスおよび OpenGL テクスチャを管理します。
/// OpenGL リソースを所有するレンダースレッド上で作成、使用、破棄してください。
/// </summary>
public class GLView : IDisposable
{
    /// <summary>
    /// OpenGL バックエンドに接続された Skia の GPU コンテキストを取得します。
    /// </summary>
    public GRContext GrContext { get; }

    /// <summary>
    /// 現在の OpenGL テクスチャを描画先とする Skia サーフェスを取得します。
    /// リサイズすると既存のサーフェスは破棄され、新しいインスタンスに置き換わります。
    /// </summary>
    public SKSurface Surface { get; private set; }

    /// <summary>
    /// 描画結果を保持する OpenGL 2D テクスチャのハンドルを取得します。
    /// リサイズすると既存のテクスチャは削除され、値が更新されます。
    /// </summary>
    public int TextureHandle { get; private set; }

    /// <summary>
    /// 描画先のピクセルサイズを取得します。
    /// </summary>
    public SKSizeI Size { get; private set; }

    /// <summary>
    /// このGLFWウィンドウがOS上に可視ウィンドウとして表示されるかどうか。
    /// コンストラクタの <c>visible</c> 引数で確定し、以降は変更できません。
    /// </summary>
    public bool Visible { get; }

    internal unsafe Window* WindowHandle => _window;
    private readonly unsafe Window* _window;
    private bool _disposed;

    /// <summary>
    /// GLFW ウィンドウと、その OpenGL コンテキストへ接続された Skia 描画先を作成します。
    /// </summary>
    /// <param name="initialSize">描画先の初期ピクセルサイズ。null の場合は1000×1000です。</param>
    /// <param name="visible">OS 上に GLFW ウィンドウを表示する場合は <see langword="true"/>。</param>
    /// <param name="title">可視ウィンドウのタイトル。</param>
    /// <exception cref="InvalidOperationException">
    /// GLFW ウィンドウ、Skia GPU コンテキスト、または Skia サーフェスを作成できない場合にスローされます。
    /// </exception>
    public GLView(SKSizeI? initialSize = null, bool visible = false, string title = "")
    {
        Size = initialSize ?? new SKSizeI(1000, 1000);

        Visible = visible;

        unsafe
        {
            GLFW.WindowHint(WindowHintBool.Visible, visible);

            _window = GLFW.CreateWindow(Size.Width, Size.Height, title, null, null);
            if (_window is null) throw new InvalidOperationException("GLFWウィンドウの作成に失敗しました。");

            GLFW.MakeContextCurrent(_window);

            GL.LoadBindings(new GLFWBindingsContext());
        }

        GrContext = GRContext.CreateGl(GRGlInterface.Create()) ??
                    throw new InvalidOperationException("GRGContextの作成に失敗しました");

        (TextureHandle, Surface) = CreateTextureAndSurface(Size);
    }

    private (int textureHandle, SKSurface surface) CreateTextureAndSurface(SKSizeI size)
    {
        var textureHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, textureHandle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, size.Width, size.Height, 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte, IntPtr.Zero);

        using var backendTexture = new GRBackendTexture(size.Width, size.Height, false, new GRGlTextureInfo
        {
            Id = (uint)textureHandle,
            Target = (uint)TextureTarget.Texture2D,
            Format = (uint)InternalFormat.Rgba8
        });

        var surface = SKSurface.Create(GrContext, backendTexture, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888) ??
                      throw new InvalidOperationException("SKSurfaceの作成に失敗しました。");

        return (textureHandle, surface);
    }

    /// <summary>
    /// 描画先のサイズを変更します。既存のテクスチャとSurfaceは破棄され、再作成されます。
    /// </summary>
    /// <param name="width">新しい描画先の幅（ピクセル）。</param>
    /// <param name="height">新しい描画先の高さ（ピクセル）。</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="width"/> または <paramref name="height"/> が0以下の場合にスローされます。
    /// </exception>
    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "幅・高さは正の値である必要があります。");

        if (Size.Width == width && Size.Height == height)
            return; // サイズが変わらないなら何もしない

        unsafe
        {
            GLFW.MakeContextCurrent(_window);
        }

        GrContext.ResetContext();

        // 古いリソースを破棄
        Surface.Dispose();
        if (TextureHandle != 0)
        {
            GL.DeleteTexture(TextureHandle);
        }

        Size = new SKSizeI(width, height);
        (var newTextureHandle, var newSurface) = CreateTextureAndSurface(Size);

        TextureHandle = newTextureHandle;
        Surface = newSurface;
    }

    /// <summary>
    /// 描画先を指定したピクセルサイズへ変更します。
    /// </summary>
    /// <param name="size">新しい描画先のピクセルサイズ。</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="size"/> の幅または高さが0以下の場合にスローされます。
    /// </exception>
    public void Resize(SKSizeI size) => Resize(size.Width, size.Height);

    /// <summary>
    /// OpenGL コンテキストを現在のスレッドへ関連付け、描画先を透明色でクリアします。
    /// </summary>
    public void Clear()
    {
        unsafe
        {
            GLFW.MakeContextCurrent(_window);
        }

        GrContext.ResetContext();

        Surface.Canvas.Clear(SKColors.Transparent);
    }

    /// <summary>
    /// GLFW ウィンドウのフロントバッファとバックバッファを交換します。
    /// </summary>
    public void SwapBuffers()
    {
        unsafe
        {
            GLFW.SwapBuffers(_window);
        }
    }

    /// <summary>
    /// Skia と OpenGL に保留されている描画コマンドを送信します。
    /// </summary>
    public void Flush()
    {
        Surface.Flush();
        GrContext.Flush();

        GL.Flush();
    }

    /// <summary>
    /// Skia と OpenGL の描画リソースを解放し、GLFW ウィンドウを破棄します。
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        GrContext.Flush();
        Surface.Dispose();
        GrContext.Dispose();

        if (TextureHandle != 0)
        {
            GL.DeleteTexture(TextureHandle);
        }

        unsafe
        {
            if (_window != null)
            {
                GLFW.DestroyWindow(_window);
            }
        }
    }
}
