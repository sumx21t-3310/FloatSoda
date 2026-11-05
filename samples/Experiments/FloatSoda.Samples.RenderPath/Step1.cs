using System.Numerics;
using FloatSoda.OVR;
using FloatSoda.OVR.Overlay;

namespace FloatSoda.Samples.RenderPath;

/// <summary>段1: FloatSoda.OVR だけで、png の画像をオーバーレイとして HMD の前に出す。</summary>
static class Step1
{
    public static void Run(string imagePath)
    {
        using var app = new OVRApplication(new OVRAppInfo(new AppKey("com.example.render_path")));

        var identity = new OverlayIdentity(
            $"com.example.render_path.step1.{Guid.NewGuid()}",
            "RenderPath step 1"
        );
        using var overlay = new DeviceTrackedOverlay(identity);

        overlay.Transform.Position = new Vector3(0, 0, -1); // 追従先(既定は HMD)の 1 m 前
        overlay.Texture.FromFile(imagePath);
        overlay.Visibility.Show();

        Console.WriteLine("HMD の中でオーバーレイを探してください。Enter キーで終了します。");
        Console.ReadLine();
    }
}
