namespace FloatSoda.OVR.Input;

/// <summary>アクションマニフェスト上のパス命名を一元管理します。</summary>
internal static class InputPaths
{
    /// <summary>アクションセットのフルパス(<c>/actions/{map}</c>)を返します。</summary>
    public static string ActionSetPath(string mapName) => $"/actions/{mapName}";

    /// <summary>入力アクションのフルパス(<c>/actions/{map}/in/{action}</c>)を返します。</summary>
    public static string ActionPath(string mapName, string actionName) => $"/actions/{mapName}/in/{actionName}";

    /// <summary>SteamVRのバインディングJSONが期待するコントローラー種別名を返します。</summary>
    public static string ControllerName(ControllerType controller) => controller switch
    {
        ControllerType.Index => "knuckles",
        ControllerType.ViveWand => "vive_controller",
        ControllerType.OculusTouch => "oculus_touch",
        _ => throw new ArgumentOutOfRangeException(nameof(controller), controller, null),
    };
}
