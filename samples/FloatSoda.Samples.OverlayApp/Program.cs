using FloatSoda;
using FloatSoda.OVR.Overlay;
using FloatSoda.Samples.OverlayApp;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();

var size = new SKSizeI(1000, 1000);


app.CreateDashboardOverlay("FloatSodaDashboard", new StackWidget { Width = size.Width, Height = size.Height },
    size.Width, size.Width);

app.CreateDashboardOverlay("WatchDashBoard", new WatchWidget(), size.Width, size.Height);
app.CreateTrackingOverlay("Left Hand", new WatchWidget(), size.Width, size.Height, TrackedDevice.LeftController);

app.Run();