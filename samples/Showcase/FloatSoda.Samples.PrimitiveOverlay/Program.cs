using System.Numerics;
using FloatSoda.OVR;
using FloatSoda.OVR.Exceptions;
using FloatSoda.OVR.Overlay;

// 表示する png のパスは第1引数で指定する。省略すると、デスクトップの layer_tree_output.png を使う。
var imagePath =
    args.Length > 0
        ? Path.GetFullPath(args[0])
        : Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            "layer_tree_output.png"
        );

using var app = new OVRApplication(new OVRAppInfo(new("com.sumx21t.floatsoda.overlay_primitive")));

try
{
    app.Identify();
}
catch (VRApplicationException e)
{
    // マニフェスト未登録の AppKey でも動かせるように、識別の失敗は続行する(AutoLaunch などが働かない)
    Console.WriteLine($"アプリケーションの識別に失敗しました: {e.Message}");
}

var eventDispatcher = new VRSystemEventDispatcher();
bool isRunning = true;

eventDispatcher.Register(EVREventType.VREvent_Quit, (in _) => isRunning = false);

var overlayName = $"{app.Info.Key}.overlay_sample";
var overlayKey = $"{overlayName}.{Guid.NewGuid()}";
var identity = new OverlayIdentity(overlayKey, overlayName);
using var overlay = new DeviceTrackedOverlay(identity);

overlay.Transform.Position = new Vector3(0, 0, -1);

overlay.Texture.FromFile(imagePath);
overlay.Visibility.Show();

var vibration = new VibrationClip(Duration: TimeSpan.FromMilliseconds(25));

overlay.Input.Method = VROverlayInputMethod.Mouse;
overlay.State[VROverlayFlags.MakeOverlaysInteractiveIfVisible] = true;

overlay.EventDispatcher.Register(
    EVREventType.VREvent_FocusEnter,
    (in _) =>
    {
        overlay.State[VROverlayFlags.MakeOverlaysInteractiveIfVisible] = true;
        overlay.Vibration.Trigger(vibration);
    }
);

overlay.EventDispatcher.Register(
    EVREventType.VREvent_FocusLeave,
    (in _) =>
    {
        overlay.State[VROverlayFlags.MakeOverlaysInteractiveIfVisible] = false;
        overlay.Vibration.Trigger(vibration with { Amplitude = 0.1f });
    }
);

overlay.EventDispatcher.Register(
    EVREventType.VREvent_MouseButtonDown,
    (in e) =>
    {
        float x = e.data.mouse.x;
        float y = e.data.mouse.y;
        var btn = (EVRMouseButton)e.data.mouse.button;
        Console.WriteLine($"クリック: ({x:F2}, {y:F2}) ボタン: {btn}");
        overlay.Vibration.Trigger(TimeSpan.FromMilliseconds(25), 1500f, .5f);
    }
);

overlay.EventDispatcher.Register(
    EVREventType.VREvent_MouseButtonUp,
    (in e) => Console.WriteLine("クリック離した")
);

overlay.EventDispatcher.Register(
    EVREventType.VREvent_MouseMove,
    (in e) => Console.WriteLine($"移動: ({e.data.mouse.x:F2}, {e.data.mouse.y:F2})")
);

var wasHovered = false;
while (isRunning)
{
    // ホバー状態が変わったときだけ出力する
    var isHovered = overlay.Input.IsHovered;
    if (isHovered != wasHovered)
    {
        Console.WriteLine(isHovered ? "hovered" : "unhovered");
        wasHovered = isHovered;
    }

    eventDispatcher.PollEvents();
    overlay.EventDispatcher.PollEvents();

    Thread.Sleep(1000 / 72);
}
