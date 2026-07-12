using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.UI.Cream;

/// <summary>
/// Cream デザインシステムのボタン。振る舞いは <see cref="ButtonBase"/> に委譲し、
/// このクラスは <see cref="InteractionState"/> から見た目へのマッピングのみを行う。
/// </summary>
public record Button : StatelessWidget
{
    /// <summary>ボタンの中身。</summary>
    public required Widget Child { get; init; }

    /// <summary>ボタンが押されたときに呼び出されるハンドラ。</summary>
    public Action? OnPressed { get; init; }

    /// <summary>操作を無効化するかどうか。</summary>
    public bool IsDisabled { get; init; }

    /// <summary>見た目のスタイル。未指定なら祖先の <see cref="CreamTheme"/>、それも無ければ既定値。</summary>
    public ButtonStyle? Style { get; init; }

    public override Widget Build(IBuildContext context)
    {
        return new ButtonBase
        {
            OnPressed = OnPressed,
            IsDisabled = IsDisabled,
            Builder = (ctx, state) =>
            {
                var style = Style ?? CreamTheme.Of(ctx)?.ButtonStyle ?? ButtonStyle.Default;
                var background = state switch
                {
                    { IsDisabled: true } => style.DisabledBackgroundColor,
                    { IsPressed: true } => style.PressedBackgroundColor,
                    _ => style.BackgroundColor
                };
                return new ColoredBox
                {
                    Color = background,
                    Child = new Align
                    {
                        WidthFactor = 1.5,
                        HeightFactor = 1.5,
                        Child = Child
                    }
                };
            }
        };
    }
}
