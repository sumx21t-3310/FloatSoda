using System.Numerics;
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.Render;
using OVRSharp;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();

using var app = builder.Build();

var thumbnail = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "thumbnail.png");


var floatSodaAbsolute = new RenderPositionedBox
{
    Child = new RenderFlex
    {
        MainAxisAlignment = MainAxisAlignment.SpaceBetween,
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Direction = Axis.Vertical,
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
                Child = new RenderColoredBox() { Color = SKColors.Blue }
            },
            new RenderConstrainedBox
            {
                AdditionalConstraints = BoxConstraints.Tight(300, 300),
                Child = new RenderColoredBox() { Color = SKColors.LimeGreen }
            }
        ]
    }
};


var floatSodaDashboard = new RenderPositionedBox
{
    Child = new RenderFlex
    {
        MainAxisAlignment = MainAxisAlignment.SpaceBetween,
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Direction = Axis.Vertical,
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
        MainAxisAlignment = MainAxisAlignment.SpaceBetween,
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Direction = Axis.Vertical,
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

app.CreateOverlayWindow("FloatSoda Absolute", floatSodaAbsolute, new SKSize(1000, 1000),
    position: new Vector3(0, 1.2f, -1f));
app.CreateOverlayWindow("FloatSoda Dashboard", floatSodaDashboard, new SKSize(1000, 1000), isDashboard: true,
    thumbnailPath: thumbnail);
app.CreateOverlayWindow("FloatSoda Left Hand", floatSodaLeftHand, new SKSize(1000, 1000),
    position: new Vector3(0, 0, -1), trackedDevice: Overlay.TrackedDeviceRole.LeftHand);

app.Run();