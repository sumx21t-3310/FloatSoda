using FloatSoda.Abstractions.Engine;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;

namespace FloatSoda.Engine;

/// <summary>
/// GLFW の可視ウィンドウへレイヤーツリーを表示するデスクトップ向けエンジンウィンドウです。
/// 描画結果を既定のフレームバッファへ転送し、ウィンドウ固有のポインター入力を提供します。
/// </summary>
public class DesktopWindow : IEngineWindow
{
    private readonly Renderer renderer;
    private int _blitFbo;

    /// <summary>
    /// 指定したレンダラーの GLFW ウィンドウを表示先として使用するインスタンスを初期化します。
    /// </summary>
    /// <param name="renderer">描画と GLFW ウィンドウの管理に使用するレンダラー。</param>
    public DesktopWindow(Renderer renderer)
    {
        this.renderer = renderer;

        unsafe
        {
            PointerSource = new GLFWRawPointerSource(renderer.GLView.WindowHandle);
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void Resize(SKSizeI size)
    {
        renderer.Resize(size.Width, size.Height);

        unsafe
        {
            GLFW.SetWindowSize(renderer.GLView.WindowHandle, size.Width, size.Height);
        }
    }

    /// <inheritdoc />
    public IRawPointerSource? PointerSource { get; }

    /// <summary>
    /// ポインター入力と、表示に使用した OpenGL フレームバッファを解放します。
    /// </summary>
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
