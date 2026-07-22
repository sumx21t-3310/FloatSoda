using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace FloatSoda.Engine;

public class GLView : IDisposable
{
    public GRContext GrContext { get; }
    public SKSurface Surface { get; private set; }
    public int TextureHandle { get; private set; }
    public SKSizeI Size { get; private set; }

    /// <summary>
    /// このGLFWウィンドウがOS上に可視ウィンドウとして表示されるかどうか。
    /// コンストラクタの <c>visible</c> 引数で確定し、以降は変更できません。
    /// </summary>
    public bool Visible { get; }

    internal unsafe Window* WindowHandle => _window;
    private readonly unsafe Window* _window;
    private bool _disposed;

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

    public void Resize(SKSizeI size) => Resize(size.Width, size.Height);

    public void Clear()
    {
        unsafe
        {
            GLFW.MakeContextCurrent(_window);
        }

        GrContext.ResetContext();

        Surface.Canvas.Clear(SKColors.Transparent);
    }

    public void SwapBuffers()
    {
        unsafe
        {
            GLFW.SwapBuffers(_window);
        }
    }

    public void Flush()
    {
        Surface.Flush();
        GrContext.Flush();

        GL.Flush();
    }

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
