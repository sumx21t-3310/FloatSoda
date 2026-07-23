using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// UI要素の構成を宣言する不変なオブジェクトの基底型です。
/// Elementツリーの作成と更新に必要な構成情報を保持します。
/// </summary>
public abstract record Widget
{
    /// <summary>
    /// Elementツリーの差分更新で、このウィジェットを識別するキーを取得します。
    /// キーを指定しない場合は、同じ位置にある同一ランタイム型のウィジェットを更新対象として再利用できます。
    /// </summary>
    public IKey? Key { get; init; }
    
    /// <summary>
    /// 既存のElementを維持したまま、古いウィジェットを新しいウィジェットで更新できるかを判定します。
    /// </summary>
    /// <param name="oldWidget">既存のElementに現在関連付けられているウィジェット。</param>
    /// <param name="newWidget">同じ位置へ新たに配置するウィジェット。</param>
    /// <returns>
    /// 両方のランタイム型が同一で、かつ<see cref="Key"/>が等しい場合は<see langword="true"/>。
    /// それ以外の場合は<see langword="false"/>。
    /// </returns>
    public static bool CanUpdate(Widget oldWidget, Widget newWidget)
    {
        return oldWidget.GetType() == newWidget.GetType() && Equals(oldWidget.Key, newWidget.Key);
    }

    /// <summary>
    /// このウィジェットの構成をElementツリーへ反映するElementを生成します。
    /// </summary>
    /// <returns>このウィジェットに対応する未マウントのElement。</returns>
    public abstract Element CreateElement();

    /// <inheritdoc/>
    public override int GetHashCode() => (Key != null ? Key.GetHashCode() : 0);
}