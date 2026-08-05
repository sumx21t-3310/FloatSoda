using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

/// <summary>表示状態に応じて子または置換ウィジェットのどちらか一方を構築します。</summary>
/// <remarks>非表示にした子の状態を保持する機能は提供しません。</remarks>
public sealed record Visibility : StatelessWidget
{
    /// <summary>子を表示するかを取得します。</summary>
    public bool Visible { get; init; } = true;

    /// <summary>表示時に構築する子ウィジェットを取得します。</summary>
    public required Widget Child { get; init; }

    /// <summary>非表示時に構築する置換ウィジェットを取得します。</summary>
    public Widget Replacement { get; init; } = new SizedBox();

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context)
    {
        ArgumentNullException.ThrowIfNull(Child);
        ArgumentNullException.ThrowIfNull(Replacement);
        return Visible ? Child : Replacement;
    }
}
