using System.Runtime.InteropServices;
using FloatSoda.OVR.Exceptions;

namespace FloatSoda.OVR.Input;

/// <summary>
/// アクションマニフェストをSteamVRへ登録し、毎フレーム <c>UpdateActionState</c> で
/// 全 <see cref="InputAction"/> の状態を更新します。<see cref="VREventDispatcher"/> と同様に
/// メインループから1フレーム1回 <see cref="Update"/> を呼び出して使います。
/// </summary>
public sealed class VRInputUpdater
{
    private static readonly uint ActiveActionSetSize = (uint)Marshal.SizeOf<VRActiveActionSet_t>();

    private readonly IReadOnlyList<InputActionMap> _maps;
    private VRActiveActionSet_t[] _activeSets = [];
    private int _activeSetsVersion = -1;

    /// <summary>
    /// マニフェストを登録し、全アクションセット/アクションのハンドルを解決します。
    /// </summary>
    /// <param name="actionManifestPath">アクションマニフェストの絶対パス。</param>
    /// <param name="maps">更新対象のアクションマップの一覧。</param>
    /// <exception cref="VRInputException">マニフェスト登録またはハンドル解決に失敗した場合。</exception>
    public VRInputUpdater(string actionManifestPath, IReadOnlyList<InputActionMap> maps)
    {
        _maps = maps;

        VRInputException.ThrowIfError(OpenVR.Input.SetActionManifestPath(actionManifestPath));

        foreach (var map in maps)
        {
            ulong setHandle = 0;
            VRInputException.ThrowIfError(
                OpenVR.Input.GetActionSetHandle(InputPaths.ActionSetPath(map.Name), ref setHandle));
            map.Handle = setHandle;

            foreach (var action in map.Actions)
            {
                ulong actionHandle = 0;
                VRInputException.ThrowIfError(
                    OpenVR.Input.GetActionHandle(InputPaths.ActionPath(map.Name, action.Name), ref actionHandle));
                action.Handle = actionHandle;
            }
        }
    }

    /// <summary>
    /// 有効なアクションセットの状態を更新し、各アクションの値・イベントへ反映します。
    /// フレーム内の一時的な取得エラーは無視します(次フレームで回復するため)。
    /// </summary>
    public void Update()
    {
        RefreshActiveSets();
        if (_activeSets.Length == 0) return;

        var error = OpenVR.Input.UpdateActionState(_activeSets, ActiveActionSetSize);
        if (error != EVRInputError.None) return;

        foreach (var map in _maps)
        {
            if (!map.Enabled) continue;
            foreach (var action in map.Actions)
            {
                action.Update();
            }
        }
    }

    /// <summary>Enabledの組み合わせが変わったフレームだけアクティブセット配列を作り直します。</summary>
    private void RefreshActiveSets()
    {
        var version = 0;
        for (var i = 0; i < _maps.Count; i++)
        {
            if (_maps[i].Enabled) version |= 1 << i;
        }

        if (version == _activeSetsVersion) return;
        _activeSetsVersion = version;

        _activeSets = _maps
            .Where(static map => map.Enabled)
            .Select(static map => new VRActiveActionSet_t
            {
                ulActionSet = map.Handle,
                ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle,
            })
            .ToArray();
    }
}
