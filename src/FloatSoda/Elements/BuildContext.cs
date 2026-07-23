using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// Widgetの構築時に、現在の構成と祖先から伝播した情報へアクセスするためのコンテキストです。
/// </summary>
/// <seealso cref="Element"/>
public interface IBuildContext
{
    /// <summary>
    /// このコンテキストに対応する現在のWidgetを取得します。
    /// </summary>
    Widget Widget { get; }
    /// <summary>
    /// このコンテキストが属するElementツリーのビルド管理者を取得します。
    /// ツリーへ接続されていない場合は<see langword="null"/>です。
    /// </summary>
    BuildOwner? Owner { get; }
    /// <summary>
    /// 指定した型として登録された最も近い祖先のInheritedWidgetを取得し、
    /// このコンテキストとの依存関係を登録します。
    /// </summary>
    /// <typeparam name="T">検索するInheritedWidgetの型。</typeparam>
    /// <returns>
    /// 該当する祖先のInheritedWidget。該当する祖先が存在しない場合は<see langword="null"/>。
    /// </returns>
    /// <remarks>
    /// 取得したInheritedWidgetが更新を通知すると、このコンテキストに対応するElementは再構築を要求されます。
    /// </remarks>
    /// <seealso cref="InheritedElement"/>
    T? DependOnInheritedWidgetOfExactType<T>() where T : InheritedWidget;

    /// <summary>
    /// 指定した型として登録された最も近い祖先のInheritedWidgetに対応するElementを取得します。
    /// </summary>
    /// <typeparam name="T">検索するInheritedWidgetの型。</typeparam>
    /// <returns>
    /// 該当する祖先の<see cref="InheritedElement"/>。
    /// 該当する祖先が存在しない場合は<see langword="null"/>。
    /// </returns>
    /// <remarks>このメソッドは取得したElementへの依存関係を登録しません。</remarks>
    /// <seealso cref="InheritedWidget"/>
    InheritedElement? GetElementForInheritedWidgetOfExactType<T>() where T : InheritedWidget;
}