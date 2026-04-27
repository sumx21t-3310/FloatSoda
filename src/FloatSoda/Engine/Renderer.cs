using FloatSoda.Exceptions;
using FloatSoda.Render;
using SkiaSharp;
using Valve.VR;

namespace FloatSoda.Engine;

public class Renderer(string overlayName, string overlayKey, FrameTimer frameTimer, SKSurface surface, ITextureHandle textureHandle)
{
    private ulong _overlayHandle;
    private SKSurface _surface;


    public void Initialize(int width = 1024, int height = 1024)
    {
        OpenVR.Overlay.CreateOverlay(overlayKey, overlayName, ref _overlayHandle).ThrowIfError();
        OpenVR.Overlay.SetOverlayWidthInMeters(_overlayHandle, 1.0f);
        OpenVR.Overlay.ShowOverlay(_overlayHandle);
    }


    public async Task Render(Element root)
    {
        OnRender(root);
        await frameTimer.WaitForNextFrame();
    }

    private void OnRender(Element root)
    {
        var renderContext = RenderContext.Create(_surface);

        root.Draw(renderContext);

        _surface.Canvas.Flush();

        var textureT = new Texture_t
        {
            handle = textureHandle.GetHandle(),
            eType = ETextureType.OpenGL,
            eColorSpace = EColorSpace.Auto
        };

        OpenVR.Overlay.SetOverlayTexture(_overlayHandle, ref textureT).ThrowIfError();
    }
}

public interface ITextureHandle
{
    public IntPtr GetHandle();
    public void Flush();
}