using System.Numerics;
using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.OVR;
using FloatSoda.OVR.Input;
using FloatSoda.OVR.Overlay;
using FloatSoda.Samples.OverlayApp;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// アクション入力のデモ。トリガーを引くとコンソールへ出力される。
// デフォルトバインドはSteamVRのコントローラーバインディングUIからユーザーが変更できる。
var grab = new InputAction<bool>
{
    Name = "grab",
    SuggestedPath = "/user/hand/right/input/trigger/click",
};

builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("FloatSoda.Samples.OverlayApp"),
    InputActionMaps = [new InputActionMap { Name = "main", Actions = [grab] }],
});

grab.OnPerformed += _ => Console.WriteLine("grab: pressed");
grab.OnReleased += () => Console.WriteLine("grab: released");

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

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

// ドラッグのデモ。GestureDetector.OnPanUpdate でボックスを掴んで動かせる。
app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Drag",
    Child = new DragBoxWidget()
});

// Position 省略時はプレイエリア中央から前方1m・高さ1mに表示される。
app.CreateWindow(new WorldSpaceWindow
{
    // Dpm = new Dpm(1000),
    Title = "WorldSpace Watch",
    Child = new WatchWidget()
});

await host.RunAsync();
