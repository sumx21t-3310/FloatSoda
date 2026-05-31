using System.Numerics;
using FloatSoda;
using FloatSoda.Engine;
using FloatSoda.Render;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OVRSharp;

var builder = new FloatSodaAppBuilder();

builder.Services.AddSingleton(LoggerFactory.Create(builder => builder.AddConsole()));
builder.Services.AddScoped<IFrameLimiter, OpenVRFrameLimiter>();

using var app = builder.Build();

var thumbnail = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "thumbnail.png");


var floatSodaAbsolute = new RenderPipeline
{
    RenderView = new RenderView(1000, 1000)
};

var floatSodaDashboard = new RenderPipeline
{
    RenderView = new RenderView(1000, 1000)
};

var floatSodaLeftHand = new RenderPipeline
{
    RenderView = new RenderView(1000, 1000)
};

app.CreateOverlayWindow("FloatSoda Absolute", floatSodaAbsolute, position: new Vector3(0, 1.2f, -1f));
app.CreateOverlayWindow("FloatSoda Dashboard", floatSodaDashboard, isDashboard: true, thumbnailPath: thumbnail);
app.CreateOverlayWindow("FloatSoda Left Hand", floatSodaLeftHand, position: new Vector3(0, 0, -1),
    trackedDevice: Overlay.TrackedDeviceRole.LeftHand);

app.Run();