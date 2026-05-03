using System.Numerics;
using FloatSoda;
using FloatSoda.Engine;


using var app = new FloatSodaApp();


app.CreateFloatingWindow("Pepsi", position: new Vector3(0, 1.2f, -1f));
app.CreateFloatingWindow("CocaCora", trackingTarget: TrackingTarget.LeftController);

app.Run();