using System.Numerics;
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.OVR.Overlay;
using FloatSoda.Render.Layout;
using FloatSoda.Render.Painting;
using FloatSoda.Samples.OverlayApp;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();


var floatSodaWorldSpace = new RenderPositionedBox
{
    Child = new RenderFlex
    {
        MainAxisAlignment = MainAxisAlignment.Center,
        Direction = Axis.Vertical,
        Children =
        [
            new RenderClipPath
            {
                Clipper = new ArcClipper(),
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(300, 300),
                    Child = new RenderColoredBox() { Color = SKColors.Tomato }
                },
            },
            new RenderClipRoundRect
            {
                BorderRadius = BorderRadius.Circular(20),
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(300, 300),
                    Child = new RenderColoredBox()
                    {
                        Color = SKColors.Yellow
                    }
                },
            },
            new RenderClipOval
            {
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(300, 300),
                    Child = new RenderColoredBox() { Color = SKColors.LimeGreen }
                }
            },
        ]
    }
};


var floatSodaDashboard = new RenderPositionedBox
{
    Child = new RenderFlex
    {
        MainAxisAlignment = MainAxisAlignment.Start,
        CrossAxisAlignment = CrossAxisAlignment.End,
        Direction = Axis.Horizontal,
        Children =
        [
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(550, 300),
                Child = new RenderColoredBox() { Color = SKColor.FromHsv(250, 255, 100) }
            },
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.Bisque }
            },
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.LimeGreen }
            }
        ]
    }
};

var floatSodaLeftHand = new RenderPositionedBox
{
    Child = new RenderFlex
    {
        MainAxisSize = MainAxisSize.Min,
        Children =
        [
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.Tomato }
            },
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.Yellow }
            },
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.BlueViolet }
            }
        ]
    }
};


app.CreateWorldSpaceOverlay("FloatSodaWorldSpace", floatSodaWorldSpace, 1000, 1000, new Vector3(0, 1, -1));
app.CreateDashboardOverlay("FloatSodaDashboard", floatSodaDashboard, 1000, 1000);
app.CreateTrackingOverlay("FloatSodaTracking", floatSodaLeftHand, 1000, 1000, TrackedDevice.LeftController);

app.Run();