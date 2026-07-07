using FloatSoda;
using FloatSoda.OVR.Overlay;
using FloatSoda.Samples.OverlayApp;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();


Widget floatSodaDashboard = new Align
{
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox()
        {
            Color = SKColors.Tomato
        }
    }
};


app.CreateDashboardOverlay("FloatSodaDashboard", floatSodaDashboard, 1000, 1000);
// app.CreateTrackingOverlay("watch", new WatchWidget(), 1000, 1000, TrackedDevice.LeftController);

app.Run();