using System.Numerics;
using FloatSoda.Engine;
using FloatSoda.OVR.Overlay;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;

namespace FloatSoda;

public static class FloatSodaAppExtensions
{
    /// <summary>
    /// フレームレートを指定します。デフォルトは 30fps です。
    /// </summary>
    public static FloatSodaAppBuilder WithTargetFrameRate(this FloatSodaAppBuilder builder, int fps)
    {
        builder.Services.AddScoped<IFrameLimiter>(_ => new FrameLimiter(fps));
        return builder;
    }

    /// <summary>
    /// OpenVR Compositor の WaitGetPoses でフレームタイミングを制御します。
    /// SteamVR が起動していない場合は初期化時に例外がスローされます。
    /// </summary>
    public static FloatSodaAppBuilder WithOpenVRFrameLimiter(this FloatSodaAppBuilder builder)
    {
        builder.Services.AddScoped<IFrameLimiter, OpenVRFrameLimiter>();
        return builder;
    }

    /// <summary>
    /// SteamVR ダッシュボードに表示されるオーバーレイウィンドウを作成します。
    /// </summary>
    public static void CreateDashboardOverlay(
        this FloatSodaApp app,
        string windowName,
        Widget root,
        int width, int height)
    {
        app.CreateOverlayWindow(
            windowName,
            root,
            width, height,
            name => new DashboardOverlay(new DashboardOverlayIdentity($"{app.AppName.ToLower()}.{windowName.ToLower()}", windowName)));
    }

    /// <summary>
    /// ワールド空間の指定座標に固定されたオーバーレイウィンドウを作成します。
    /// </summary>
    public static void CreateWorldSpaceOverlay(
        this FloatSodaApp app,
        string windowName,
        Widget root,
        int width, int height,
        Vector3 position,
        Quaternion? rotation = null)
    {
        app.CreateOverlayWindow(
            windowName,
            root,
            width, height,
            name =>
            {
                var overlay = new WorldSpaceOverlay(new OverlayIdentity($"{app.AppName.ToLower()}.{name.ToLower()}", name));
                overlay.Transform.Position = position;
                overlay.Transform.Rotation = rotation ?? Quaternion.Identity;
                overlay.Visibility.Show();
                return overlay;
            });
    }

    /// <summary>
    /// 指定したトラッキングデバイスに追従するオーバーレイウィンドウを作成します。
    /// </summary>
    public static void CreateTrackingOverlay(
        this FloatSodaApp app,
        string windowName,
        Widget root,
        int width, int height,
        TrackedDevice target,
        Vector3? offset = null,
        Quaternion? rotation = null)
    {
        app.CreateOverlayWindow(
            windowName,
            root,
            width, height,
            name =>
            {
                var overlay = new DeviceTrackedOverlay(new OverlayIdentity($"{app.AppName.ToLower()}.{name.ToLower()}", name));
                overlay.Transform.Target = target;
                overlay.Transform.Position = offset ?? Vector3.Zero;
                overlay.Transform.Rotation = rotation ?? Quaternion.Identity;
                overlay.Visibility.Show();
                return overlay;
            });
    }
}