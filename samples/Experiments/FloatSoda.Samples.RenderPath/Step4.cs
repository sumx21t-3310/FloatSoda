using FloatSoda.Geometrics;
using FloatSoda.Geometry;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

namespace FloatSoda.Samples.RenderPath;

/// <summary>段4: 段3で手書きした処理をフレームワークに任せ、ウィジェットで書いた色の四角をダッシュボードに出す。</summary>
static class Step4
{
    public static async Task RunAsync(string[] hostArgs)
    {
        var builder = Host.CreateApplicationBuilder(hostArgs);
        builder.Services.AddFloatSoda();

        using var host = builder.Build();
        var app = host.Services.GetRequiredService<FloatSodaApp>();

        app.CreateWindow(
            new DashboardWindow
            {
                Dpm = new Dpm(100),
                Title = "RenderPath",
                Child = new Align
                {
                    Alignment = Alignment.Center,
                    Child = new SizedBox
                    {
                        Width = 100,
                        Height = 100,
                        Child = new ColoredBox { Color = SKColors.CornflowerBlue },
                    },
                },
            }
        );

        await host.RunAsync();
    }
}
