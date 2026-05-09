using System.Numerics;
using FloatSoda;
using FloatSoda.OVR;


using var app = new FloatSodaApp();

app.CreateOverlayWindow("Pepsi", position: new Vector3(0, 1.2f, -1f));
app.CreateOverlayWindow("CocaCora", trackingTarget: TrackingTarget.LeftController);

app.Run();