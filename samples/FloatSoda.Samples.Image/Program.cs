using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.OVR;
using FloatSoda.Samples.Image;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda(new FloatSodaOptions
{
    AppKey = new AppKey("FloatSoda.Samples.Image"),
});

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(new DashboardWindow
{
    Dpm = new Dpm(1000),
    Title = "Image Test",
    Child = new ImageTestDemo
    {
        ImagePath = Path.Combine(AppContext.BaseDirectory, "assets", "logo-sketch.png")
    }
});

await host.RunAsync();
