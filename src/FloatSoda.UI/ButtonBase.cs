using FloatSoda.Elements;
using FloatSoda.Widgets;

namespace FloatSoda.UI;

/// <summary>
/// ボタンの振る舞いのみを提供するヘッドレスウィジェット。
/// 見た目は <see cref="Builder"/> に完全に委譲され、このクラス自身は
/// デザインシステム層の InheritedWidget が無くても動作する。
/// </summary>
public record ButtonBase : StatefulWidget<ButtonBase>
{
    /// <summary>現在の <see cref="InteractionState"/> から見た目を構築するデリゲート。</summary>
    public required Func<IBuildContext, InteractionState, Widget> Builder { get; init; }

    /// <summary>ボタンが押されたときに呼び出されるハンドラ。</summary>
    public Action? OnPressed { get; init; }

    /// <summary>操作を無効化するかどうか。</summary>
    public bool IsDisabled { get; init; }

    /// <inheritdoc/>
    public override State<ButtonBase> CreateState() => new ButtonBaseState();
}

/// <summary>
/// <see cref="ButtonBase"/>の押下・無効化状態を管理し、<see cref="InteractionState"/>を
/// <see cref="ButtonBase.Builder"/>へ渡して見た目を構築する状態です。
/// </summary>
public class ButtonBaseState : State<ButtonBase>
{
    /// <inheritdoc/>
    public override Widget Build(IBuildContext context)
    {
        // TODO: ジェスチャ/ヒットテスト実装後に GestureDetector を配線する
        // (押下・ホバー・フォーカスの状態機械は必ずこのクラスに置き、DS層には持ち出さない)
        var state = new InteractionState
        {
            IsDisabled = Widget!.IsDisabled
        };
        return Widget.Builder(context, state);
    }
}
