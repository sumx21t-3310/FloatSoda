using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloatSoda;

public class OverlayAppBuilder(string[] args)
{
    public static OverlayAppBuilder CreateDefault(string key, string name)
    {
        return new OverlayAppBuilder(Environment.GetCommandLineArgs())
            .ConfigureOverlay(key, name)
            .AddRoot(new DebugElement())
            .AddEventDispatcher()
            .AddLogging();
    }

    private readonly HostApplicationBuilder _builder = Host.CreateApplicationBuilder(args);

    public OverlayAppBuilder ConfigureOverlay(string key, string name)
    {
        _builder.Services.AddSingleton<FrameTimer>();

        _builder.Services.AddSingleton<Renderer>(sp =>
        {
            var timer = sp.GetRequiredService<FrameTimer>();
            return new Renderer(name, key, timer);
        });

        return this;
    }

    public OverlayAppBuilder AddRoot(Element root)
    {
        _builder.Services.AddSingleton(root);
        return this;
    }

    public OverlayAppBuilder AddEventDispatcher()
    {
        _builder.Services.AddSingleton<EventDispatcher>();
        return this;
    }

    public OverlayAppBuilder AddLogging()
    {
        _builder.Services.AddLogging(config => { config.AddConsole(); });

        return this;
    }

    public IHost Build()
    {
        _builder.Services.AddHostedService<OverlayBackgroundService>();
        return _builder.Build();
    }
}