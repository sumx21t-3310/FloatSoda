using FloatSoda.Elements;
using FloatSoda.Widgets;

namespace FloatSoda.Testing;

/// <summary>
/// <see cref="Finder"/>を作る入口です。<see cref="WidgetTester.Find"/>から使います。
/// Flutterの<c>CommonFinders</c>(<c>find</c>)に相当します。
/// </summary>
/// <remarks>
/// どの入口も<c>skipOffstage</c>を受け取ります。
/// <see langword="true"/>のとき、<c>IsOffstage</c>が<see langword="true"/>の<c>Offstage</c>の子と、
/// <c>IndexedStack</c>の表示していない子を探す対象から外します。
/// </remarks>
/// <seealso cref="WidgetTester.Find"/>
public sealed class CommonFinders
{
    /// <summary>探し始めるルートElementを返す処理。作ったFinderへそのまま渡します。</summary>
    private readonly Func<Element?> _root;

    /// <summary>
    /// <paramref name="root"/>から探すFinderを作る入口を初期化します。<see cref="WidgetTester"/>だけが作ります。
    /// </summary>
    /// <param name="root">
    /// 探し始めるルートElementを返す処理。Finderの<see cref="Finder.Evaluate"/>のたびに呼ばれます。
    /// マウント前は<see langword="null"/>を返します。
    /// </param>
    internal CommonFinders(Func<Element?> root) => _root = root;

    /// <summary>型が<typeparamref name="T"/>に完全に一致するWidgetを探します。</summary>
    /// <typeparam name="T">探すWidgetの型。派生型は一致しません。派生型も含めるときは<see cref="BySubtype{T}"/>を使います。</typeparam>
    /// <param name="skipOffstage">画面に出ていない子要素を対象から外すか。既定値は<see langword="true"/>です。</param>
    /// <returns>条件に合うElementを探すFinder。作った時点では探しません。</returns>
    public Finder ByType<T>(bool skipOffstage = true) where T : Widget
    {
        // 挙動: element.Widget.GetType() == typeof(T) のElementに一致する。派生型は一致しない。
        return Match(
            skipOffstage,
            element => element.Widget?.GetType() == typeof(T),
            $"型が {typeof(T).Name} のWidget"
        );
    }

    /// <summary><c>is T</c>に当てはまるWidgetを探します。</summary>
    /// <typeparam name="T">探すWidgetの型。派生型も一致します。</typeparam>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。既定値は<see langword="true"/>です。</param>
    /// <returns>条件に合うElementを探すFinder。作った時点では探しません。</returns>
    public Finder BySubtype<T>(bool skipOffstage = true) where T : Widget
    {
        // 挙動: element.Widget is T のElementに一致する。T自身と派生型の両方が一致する。
        return Match(skipOffstage, element => element.Widget is T, $"{typeof(T).Name} の派生型のWidget");
    }

    /// <summary>キーが一致するWidgetを探します。</summary>
    /// <param name="key">探すキー。<c>Equals</c>で比べます。<see langword="null"/>は指定できません。</param>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。既定値は<see langword="true"/>です。</param>
    /// <returns>条件に合うElementを探すFinder。作った時点では探しません。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="key"/>が<see langword="null"/>のとき。</exception>
    public Finder ByKey(IKey key, bool skipOffstage = true)
    {
        // 挙動: Equals(element.Widget.Key, key) のElementに一致する。
        // keyがnullならArgumentNullExceptionを投げる。
        ArgumentNullException.ThrowIfNull(key);
        return Match(skipOffstage, element => Equals(element.Widget?.Key, key), $"キーが {key} のWidget");
    }

    /// <summary>文字列が一致するテキストを探します。</summary>
    /// <param name="text">探す文字列。<see cref="Widgets.Text.Data"/>と完全に一致するものだけを探します。<see langword="null"/>は指定できません。</param>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。既定値は<see langword="true"/>です。</param>
    /// <returns><see cref="Widgets.Text"/>のElementを探すFinder。作った時点では探しません。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/>が<see langword="null"/>のとき。</exception>
    /// <remarks>一致するのは<see cref="Widgets.Text"/>だけです。<see cref="RichText"/>は対象にしません。</remarks>
    public Finder Text(string text, bool skipOffstage = true)
    {
        // 挙動: Textウィジェットのうち、表示する文字列がtextと完全一致するElementに一致する。部分一致はしない。
        // textがnullならArgumentNullExceptionを投げる。
        // 【未決】RichText相当のウィジェットが増えたとき、どこまで一致させるか。
        ArgumentNullException.ThrowIfNull(text);
        return Match(
            skipOffstage,
            element => element.Widget is Widgets.Text { Data: var data } && data == text,
            $"文字列が \"{text}\" のText"
        );
    }

    /// <summary>このインスタンスのルートから探すFinderを作ります。</summary>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。</param>
    /// <param name="predicate">一致とみなすElementで<see langword="true"/>を返す条件。</param>
    /// <param name="description">失敗時のメッセージに使う、条件の説明。</param>
    /// <returns>新しいFinder。</returns>
    private Finder.MatchFinder Match(bool skipOffstage, Func<Element, bool> predicate, string description) =>
        new(_root, skipOffstage, predicate, description);
}
