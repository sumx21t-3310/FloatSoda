using System.Numerics;
using FloatSoda;
using FloatSoda.Render;
using OVRSharp;


using var app = new FloatSodaApp();


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