using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace FloatSoda.Engine.Render;

public class GLView : IDisposable
{
    public GRContext GrContext { get; }
    public SKSurface Surface { get; }
    public int TextureHandle { get; }

    private readonly unsafe Window* _window;
    private bool _disposed;

    public GLView(int width = 1000, int height = 1000)
    {
        unsafe
        {
            GLFW.WindowHint(WindowHintBool.Visible, false);

            _window = GLFW.CreateWindow(width, height, "", null, null);
            if (_window == null) throw new InvalidOperationException("GLFWウィンドウの作成に失敗しました。");

            GLFW.MakeContextCurrent(_window);

            GL.LoadBindings(new GLFWBindingsContext());
        }

        GrContext = GRContext.CreateGl(GRGlInterface.Create()) ?? throw new InvalidOperationException("GRGContextの作成に失敗しました");


        TextureHandle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

        var backendTexture = new GRBackendTexture(width, height, false, new GRGlTextureInfo
        {
            Id = (uint)TextureHandle,
            Target = (uint)TextureTarget.Texture2D,
            Format = (uint)InternalFormat.Rgba8
        });

        Surface = SKSurface.Create(GrContext, backendTexture, GRSurfaceOrigin.BottomLeft, SKColorType.Rgba8888) ?? throw new InvalidOperationException("SKSurfaceの作成に失敗しました。");
    }

    public void Clear()
    {
        GrContext.ResetContext();

        Surface.Canvas.Clear(SKColors.Transparent);
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