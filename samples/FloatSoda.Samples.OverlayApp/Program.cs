using System.Numerics;
using FloatSoda;
using OVRSharp;


using var app = new FloatSodaApp();


var thumbnail = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "thumbnail.png");

app.CreateOverlayWindow("FloatSoda Absolute", position: new Vector3(0, 1.2f, -1f));
app.CreateOverlayWindow("FloatSoda Dashboard", isDashboard: true, thumbnailPath: thumbnail);
app.CreateOverlayWindow("FloatSoda Left Hand", position: new Vector3(0, 0, -1),
    trackedDevice: Overlay.TrackedDeviceRole.LeftHand);

app.Run();