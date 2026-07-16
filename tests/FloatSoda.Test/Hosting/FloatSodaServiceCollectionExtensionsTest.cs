using FloatSoda.OVR;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FloatSoda.Test.Hosting;

public class FloatSodaServiceCollectionExtensionsTest
{
    [Fact]
    public void AddFloatSodaRegistersRuntimeWithGenericHost()
    {
        var builder = Host.CreateApplicationBuilder();
        var options = new FloatSodaOptions
        {
            AppKey = new AppKey("test.overlay"),
            TargetFrameRate = 90
        };

        builder.Services.AddFloatSoda(options);

        using var host = builder.Build();
        var app = host.Services.GetRequiredService<FloatSodaApp>();

        Assert.Same(options, host.Services.GetRequiredService<FloatSodaOptions>());
        Assert.Equal(options.AppKey, host.Services.GetRequiredService<OVRAppInfo>().Key);
        Assert.Single(host.Services.GetServices<IHostedService>());

        app.CreateWindow(new DashboardWindow
        {
            Title = "TestWindow",
            Child = new SizedBox { Width = 1, Height = 1 }
        });
    }

    [Fact]
    public void AddFloatSodaRejectsNonPositiveFrameRate()
    {
        var services = new ServiceCollection();
        var options = new FloatSodaOptions { TargetFrameRate = 0 };

        Assert.Throws<ArgumentOutOfRangeException>(() => services.AddFloatSoda(options));
    }
}
