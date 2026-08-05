using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// 子サブツリーを、このウィジェットに指定した<see cref="Widget.Key"/>で識別するウィジェットです。
/// </summary>
/// <remarks>
/// 同じ位置で同じキーを使う間は子サブツリーのElementとStateが再利用され、
/// キーを変更するとサブツリーが差し替えられます。
/// </remarks>
public record KeyedSubtree : StatelessWidget
{
    /// <summary>識別対象とする子ウィジェットを取得します。</summary>
    /// <exception cref="ArgumentNullException"><see langword="null"/>が指定されました。</exception>
    public required Widget Child
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context) => Child;
}
