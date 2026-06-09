namespace FloatSoda.OVR.Exceptions;

public class VRNotificationException(string message, EVRNotificationError errorCode)
    : OpenVRSystemException<EVRNotificationError>(message, errorCode)
{
    public VRNotificationException(EVRNotificationError errorCode)
        : this(GetMessage(errorCode), errorCode)
    {
    }

    private static string GetMessage(EVRNotificationError error) => error switch
    {
        EVRNotificationError.InvalidNotificationId => "無効な通知IDです。通知が存在しないか、すでに削除されています。",
        EVRNotificationError.NotificationQueueFull => "通知キューがいっぱいです。これ以上通知を追加できません。",
        EVRNotificationError.InvalidOverlayHandle => "無効なオーバーレイハンドルです。",
        EVRNotificationError.SystemWithUserValueAlreadyExists => "同じ UserValue を持つ通知がすでに存在しています。",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    public static void ThrowIfError(EVRNotificationError error)
    {
        if (error == EVRNotificationError.OK) return;

        throw new VRNotificationException(error);
    }
}