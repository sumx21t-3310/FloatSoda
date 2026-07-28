using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.OVR;
using FloatSoda.Samples.PointerRegionSample;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("FloatSoda.Samples.PointerRegion"),
});

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Pointer Region",
    Child = new PointerRegionDemo(),
});

await host.RunAsync();
