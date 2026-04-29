using System.Numerics;
using FloatSoda;
using FloatSoda.Engine;
using FloatSoda.Engine.Painting;
using SkiaSharp;


using var app = new FloatSodaApp();

var centerWindowLayer = new ContainerLayer
{
    Children =
    {
        new PaintLayer { Color = SKColors.Tomato, Size = new Size(1000, 1000) },
        new PaintLayer { Color = SKColors.AliceBlue, Size = new Size(1000, 700) },
        new PaintLayer { Color = SKColors.CornflowerBlue, Size = new Size(1000, 300) }
    }
};

var leftHandWindowLayer = new ContainerLayer
{
    Children =
    {
        new PaintLayer { Color = SKColors.Tomato, Size = new Size(1000, 1000) },
    }
};

app.CreateFloatingWindow("Pepsi", centerWindowLayer, position: new Vector3(0, 1.2f, -1f));
app.CreateFloatingWindow("CocaCora", leftHandWindowLayer, trackingTarget: TrackingTarget.LeftController);

app.Run();