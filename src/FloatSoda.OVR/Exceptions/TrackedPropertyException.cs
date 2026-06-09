namespace FloatSoda.OVR.Exceptions;

public class TrackedPropertyException(string message, ETrackedPropertyError errorCode)
    : OpenVRSystemException<ETrackedPropertyError>(message, errorCode)
{
    private static string GetMessage(ETrackedPropertyError error) => error switch
    {
        ETrackedPropertyError.TrackedProp_Success => "トラッキングプロパティ操作は正常に完了しました。",
        ETrackedPropertyError.TrackedProp_WrongDataType => "要求されたプロパティのデータ型が正しくありません。",
        ETrackedPropertyError.TrackedProp_WrongDeviceClass => "このデバイスクラスでは利用できないプロパティです。",
        ETrackedPropertyError.TrackedProp_BufferTooSmall => "プロパティを格納するバッファサイズが不足しています。",
        ETrackedPropertyError.TrackedProp_UnknownProperty => "指定されたプロパティは存在しません。",
        ETrackedPropertyError.TrackedProp_InvalidDevice => "指定されたデバイスは無効です。",
        ETrackedPropertyError.TrackedProp_CouldNotContactServer => "OpenVR サーバーへ接続できませんでした。",
        ETrackedPropertyError.TrackedProp_ValueNotProvidedByDevice => "デバイスからこのプロパティ値は提供されていません。",
        ETrackedPropertyError.TrackedProp_StringExceedsMaximumLength => "文字列が許可されている最大長を超えています。",
        ETrackedPropertyError.TrackedProp_NotYetAvailable => "プロパティ値はまだ利用可能ではありません。",
        ETrackedPropertyError.TrackedProp_PermissionDenied => "プロパティへのアクセス権限がありません。",
        ETrackedPropertyError.TrackedProp_InvalidOperation => "無効なプロパティ操作が実行されました。",
        ETrackedPropertyError.TrackedProp_CannotWriteToWildcards => "ワイルドカードプロパティへ書き込むことはできません。",
        ETrackedPropertyError.TrackedProp_IPCReadFailure => "IPC 読み取り中にエラーが発生しました。",
        ETrackedPropertyError.TrackedProp_OutOfMemory => "メモリ不足によりプロパティ操作に失敗しました。",
        ETrackedPropertyError.TrackedProp_InvalidContainer => "指定されたプロパティコンテナは無効です。",
        _ => throw new ArgumentOutOfRangeException(nameof(error), error, null)
    };

    public static void ThrowIfError(ETrackedPropertyError error)
    {
        if (error == ETrackedPropertyError.TrackedProp_Success) return;

        throw new TrackedPropertyException(GetMessage(error), error);
    }
}