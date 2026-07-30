using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

Widget root = new Align
{
    Alignment = Alignment.Center,
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox
        {
            Color = SKColors.CornflowerBlue
        }
    }
};

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(100),
    Title = "MyDashboard", Child = root
});
await host.RunAsync();
