using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.OVR;
using FloatSoda.Samples.DecoratedBox;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// --desktop を渡すと、SteamVRダッシュボードの代わりにデスクトップウィンドウへ表示する。
// 目視確認をモニタ上で完結させるためのもの(SteamVRの起動自体は必要)。
// Hostの構成バインダーは値を伴わない引数を解釈できないため、渡す前に取り除く。
var useDesktop = args.Contains("--desktop");
var hostArgs = args.Where(argument => argument != "--desktop").ToArray();

var builder = Host.CreateApplicationBuilder(hostArgs);
builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("FloatSoda.Samples.DecoratedBox"),
});

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

var demo = new DecoratedBoxDemo();

app.CreateWindow(useDesktop
    ? new DesktopWindow { Title = "DecoratedBox", Child = demo }
    : new DashboardWindow { Dpm = new Dpm(1000), Title = "DecoratedBox", Child = demo });

await host.RunAsync();
