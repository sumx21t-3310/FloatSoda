using FloatSoda;
using FloatSoda.Core;
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
        Child = new ColoredBox
        {
            Color = SKColors.Tomato
        }
    }
};


app.CreateDashboardOverlay("FloatSodaDashboard", floatSodaDashboard, 1000, 1000);

app.Run();