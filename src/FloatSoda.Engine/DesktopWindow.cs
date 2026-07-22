using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace FloatSoda.Engine;

public class DesktopWindow : IEngineWindow
{
    private readonly Renderer renderer;
    private int _blitFbo;

    public DesktopWindow(Renderer renderer)
    {
        this.renderer = renderer;

        unsafe
        {
            PointerSource = new GLFWRawPointerSource(renderer.GLView.WindowHandle);
        }
    }

    public void Present(ILayer layer)
    {
        renderer.Render(layer);

        if (_blitFbo == 0) _blitFbo = GL.GenFramebuffer();

        var size = renderer.GLView.Size;

        int fbWidth, fbHeight;
        unsafe
        {
            GLFW.GetFramebufferSize(renderer.GLView.WindowHandle, out fbWidth, out fbHeight);
        }

        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _blitFbo);
        GL.FramebufferTexture2D(FramebufferTarget.ReadFramebuffer, FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D, renderer.GLView.TextureHandle, 0);
        GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
        GL.BlitFramebuffer(0, 0, size.Width, size.Height, 0, 0, fbWidth, fbHeight,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);

        renderer.GLView.SwapBuffers();
    }

    public void Resize(SKSizeI size)
    {
        renderer.Resize(size.Width, size.Height);

        unsafe
        {
            GLFW.SetWindowSize(renderer.GLView.WindowHandle, size.Width, size.Height);
        }
    }

    public IRawPointerSource? PointerSource { get; }

    public void Dispose()
    {
        PointerSource?.Dispose();

        if (_blitFbo != 0)
        {
            GL.DeleteFramebuffer(_blitFbo);
            _blitFbo = 0;
        }
    }
}
