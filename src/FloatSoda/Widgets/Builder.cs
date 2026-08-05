using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// 指定されたデリゲートを、このウィジェット自身の構築コンテキストで実行するウィジェットです。
/// </summary>
/// <remarks>
/// 新しい構築コンテキストを挟むため、このウィジェットより上にある
/// <see cref="InheritedWidget"/>を<see cref="ChildBuilder"/>から参照できます。
/// </remarks>
public record Builder : StatelessWidget
{
    /// <summary>
    /// このウィジェットの直下に配置するウィジェットを構築するデリゲートを取得します。
    /// </summary>
    /// <exception cref="ArgumentNullException"><see langword="null"/>が指定されました。</exception>
    public required Func<IBuildContext, Widget> ChildBuilder
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context) => ChildBuilder(context);
}
