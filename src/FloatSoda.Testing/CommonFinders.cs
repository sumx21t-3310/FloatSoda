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
public sealed class CommonFinders
{
    private readonly Func<Element?> _root;

    internal CommonFinders(Func<Element?> root) => _root = root;

    /// <summary>型が<typeparamref name="T"/>に完全に一致するWidgetを探します。</summary>
    /// <typeparam name="T">探すWidgetの型。</typeparam>
    /// <param name="skipOffstage">画面に出ていない子要素を対象から外すか。</param>
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
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。</param>
    public Finder BySubtype<T>(bool skipOffstage = true) where T : Widget
    {
        // 挙動: element.Widget is T のElementに一致する。T自身と派生型の両方が一致する。
        return Match(skipOffstage, element => element.Widget is T, $"{typeof(T).Name} の派生型のWidget");
    }

    /// <summary>キーが一致するWidgetを探します。</summary>
    /// <param name="key">探すキー。</param>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。</param>
    public Finder ByKey(IKey key, bool skipOffstage = true)
    {
        // 挙動: Equals(element.Widget.Key, key) のElementに一致する。
        // keyがnullならArgumentNullExceptionを投げる。
        ArgumentNullException.ThrowIfNull(key);
        return Match(skipOffstage, element => Equals(element.Widget?.Key, key), $"キーが {key} のWidget");
    }

    /// <summary>文字列が一致するテキストを探します。</summary>
    /// <param name="text">探す文字列。</param>
    /// <param name="skipOffstage">画面に出ていない子を対象から外すか。</param>
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

    private Finder.MatchFinder Match(bool skipOffstage, Func<Element, bool> predicate, string description) =>
        new(_root, skipOffstage, predicate, description);
}
