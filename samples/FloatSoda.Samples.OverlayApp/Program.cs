using FloatSoda;
using FloatSoda.OVR.Overlay;
using FloatSoda.Samples.OverlayApp;
using FloatSoda.Widgets;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();

// Size 未指定のウィンドウは Child のレイアウト結果にサイズが追従する。
app.CreateWindow(new DashboardWindow
{
    Title = "FloatSodaDashboard",
    Child = new StackWidget()
});

app.CreateWindow(new DashboardWindow
{
    Title = "WatchDashBoard",
    Child = new WatchWidget()
});

app.CreateWindow(new DeviceTrackedWindow
{
    Title = "Left Hand",
    Child = new WatchWidget(),
    Target = TrackedDevice.LeftController
});

// Position 省略時はプレイエリア中央から前方1m・高さ1mに表示される。
app.CreateWindow(new WorldSpaceWindow
{
    Title = "WorldSpace Watch",
    Child = new WatchWidget()
});

app.Run();