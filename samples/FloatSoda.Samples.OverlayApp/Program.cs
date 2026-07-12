using System.Numerics;
using FloatSoda;
using FloatSoda.Common.Geometries;
using FloatSoda.OVR.Overlay;
using FloatSoda.Samples.OverlayApp;
using FloatSoda.Widgets;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();

// Size 未指定のウィンドウは Child のレイアウト結果にサイズが追従する。
app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "FloatSodaDashboard",
    Child = new StackWidget()
});

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "WatchDashBoard",
    Child = new WatchWidget()
});

app.CreateWindow(new DeviceTrackedWindow
{
    Rotation = Quaternion.CreateFromYawPitchRoll(
        Angle.FromDegrees(100).Radians,
        Angle.FromDegrees(180).Radians,
        Angle.FromDegrees(0).Radians),
    Offset = new Vector3(-0.1f, 0, 0.2f),
    Title = "Left Hand",
    Child = new WatchWidget(),
    Target = TrackedDevice.LeftController
});

// AnimationControllerのデモ。約1.5秒周期でフェードイン/アウトを繰り返す。
app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Pulse",
    Child = new PulseWidget()
});

// カウンターアプリのデモ。StatefulWidget + SetState() の例。
app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Counter",
    Child = new CounterWidget()
});

// Position 省略時はプレイエリア中央から前方1m・高さ1mに表示される。
app.CreateWindow(new WorldSpaceWindow
{
    Dpm = new Dpm(1000),
    Title = "WorldSpace Watch",
    Child = new WatchWidget()
});

app.Run();