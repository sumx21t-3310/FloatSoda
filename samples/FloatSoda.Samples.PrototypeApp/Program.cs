using System.Numerics;
using FloatSoda;
using FloatSoda.OVR;
using FloatSoda.Render;
using SkiaSharp;


string manifestPath = Path.GetFullPath("FloatSoda.vrmanifest");

using var app = new FloatSodaApp();

Element rootElement = new BoxElement
{
    Size = new Size(1000, 1000),
    Color = SKColors.AliceBlue,
    Child = new BoxElement
    {
        Size = new Size(100, 100),
        Color = SKColors.CadetBlue,
    }
};

app.CreateFloatingWindow("FloatSoda", "FloadSoda", rootElement, 0.5f, new Vector3(0, 1.2f, -1), TrackingTarget.World);

app.Run();