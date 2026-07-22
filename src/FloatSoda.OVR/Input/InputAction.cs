using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FloatSoda.OVR.Input;

/// <summary>
/// アクション入力の非ジェネリック基底です。<see cref="InputAction{T}"/> を型消去して
/// <see cref="InputActionMap"/> に収集するために使います。直接継承はできません。
/// </summary>
public abstract class InputAction
{
    private protected InputAction()
    {
    }

    /// <summary>
    /// アクション名を取得します(例: <c>grab</c>)。
    /// マニフェスト上のフルパスは <c>/actions/{map}/in/{name}</c> になります。
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// SteamVRのバインディングUIに表示される名前を取得します。省略時は <see cref="Name"/> が使われます。
    /// </summary>
    public string? LocalizedName { get; init; }

    /// <summary>
    /// コントローラー種別ごとの初期割り当ての宣言を取得します。
    /// 空の場合は <see cref="SuggestedPath"/> が全コントローラー種別へ複製されます。
    /// </summary>
    public IReadOnlyList<DefaultBinding> DefaultBindings { get; init; } = [];

    /// <summary>
    /// 全コントローラー種別に共通で提案する入力パスを取得します
    /// (例: <c>/user/hand/right/input/trigger/click</c>)。
    /// コントローラーごとに割り当てを変えたい場合は <see cref="DefaultBindings"/> を使います。
    /// </summary>
    public string? SuggestedPath { get; init; }

    /// <summary>
    /// このアクションが現在いずれかの物理入力にバインドされているかを取得します。
    /// </summary>
    public bool IsActive { get; private protected set; }

    /// <summary>IVRInputのアクションハンドル。<see cref="VRInputUpdater"/> が解決します。</summary>
    internal ulong Handle { get; set; }

    /// <summary>アクションマニフェストの type フィールド値(boolean / vector1 / vector2)。</summary>
    internal abstract string ManifestType { get; }

    /// <summary>IVRInputから最新の状態を取り込み、必要ならイベントを発火します。</summary>
    internal abstract void Update();

    /// <summary>指定コントローラー向けの初期割り当てパスを列挙します(SuggestedPath複製込み)。</summary>
    internal IEnumerable<string> BindingPathsFor(ControllerType controller)
    {
        foreach (var binding in DefaultBindings)
        {
            if (binding.Controller == controller) yield return binding.Path;
        }

        if (DefaultBindings.Count == 0 && SuggestedPath is not null)
        {
            yield return SuggestedPath;
        }
    }
}

/// <summary>
/// 値の型付きアクション入力です。<typeparamref name="T"/> は
/// <see cref="bool"/>(ボタン)、<see cref="float"/>(トリガー引き量など)、
/// <see cref="Vector2"/>(スティック/トラックパッド)のいずれかです。
/// 宣言(初期化プロパティ)とランタイム状態(<see cref="Value"/>・イベント)を1つのインスタンスが担います。
/// </summary>
/// <typeparam name="T">アクション値の型(bool / float / Vector2)。</typeparam>
public sealed class InputAction<T> : InputAction where T : struct
{
    private static readonly uint DigitalSize = (uint)Marshal.SizeOf<InputDigitalActionData_t>();
    private static readonly uint AnalogSize = (uint)Marshal.SizeOf<InputAnalogActionData_t>();

    static InputAction()
    {
        if (typeof(T) != typeof(bool) && typeof(T) != typeof(float) && typeof(T) != typeof(Vector2))
        {
            throw new NotSupportedException(
                $"InputAction<{typeof(T).Name}> は未対応です。bool / float / Vector2 を使用してください。");
        }
    }

    /// <summary>
    /// 最新のアクション値を取得します。<see cref="VRInputUpdater"/> により毎フレーム更新されます。
    /// </summary>
    public T Value { get; private set; }

    /// <summary>
    /// アクションが「実行」されたときに発火します。
    /// <see cref="bool"/> では押下エッジ、<see cref="float"/> / <see cref="Vector2"/> では値が変化したフレームです。
    /// </summary>
    public event Action<T>? OnPerformed;

    /// <summary>
    /// <see cref="bool"/> アクションが離されたときに発火します。アナログアクションでは発火しません。
    /// </summary>
    public event Action? OnReleased;

    internal override string ManifestType =>
        typeof(T) == typeof(bool) ? "boolean" :
        typeof(T) == typeof(float) ? "vector1" : "vector2";

    internal override void Update()
    {
        if (Handle == OpenVR.k_ulInvalidActionHandle) return;

        if (typeof(T) == typeof(bool))
        {
            var data = default(InputDigitalActionData_t);
            var error = OpenVR.Input.GetDigitalActionData(
                Handle, ref data, DigitalSize, OpenVR.k_ulInvalidInputValueHandle);
            if (error != EVRInputError.None) return;

            IsActive = data.bActive;
            var state = data.bState;
            Value = Unsafe.As<bool, T>(ref state);

            if (!data.bChanged) return;
            if (data.bState) OnPerformed?.Invoke(Value);
            else OnReleased?.Invoke();
        }
        else
        {
            var data = default(InputAnalogActionData_t);
            var error = OpenVR.Input.GetAnalogActionData(
                Handle, ref data, AnalogSize, OpenVR.k_ulInvalidInputValueHandle);
            if (error != EVRInputError.None) return;

            IsActive = data.bActive;

            if (typeof(T) == typeof(float))
            {
                var x = data.x;
                Value = Unsafe.As<float, T>(ref x);
                if (data.deltaX != 0f) OnPerformed?.Invoke(Value);
            }
            else
            {
                var xy = new Vector2(data.x, data.y);
                Value = Unsafe.As<Vector2, T>(ref xy);
                if (data.deltaX != 0f || data.deltaY != 0f) OnPerformed?.Invoke(Value);
            }
        }
    }
}
