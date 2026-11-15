using FloatSoda.Elements;

namespace FloatSoda.Testing;

/// <summary>
/// 現在のElementツリーから、条件に一致するElementを探します。
/// Flutterの<c>Finder</c>に相当します。
/// </summary>
/// <remarks>
/// インスタンスは<see cref="CommonFinders"/>から作ります。このアセンブリの外で派生型は作れません。
/// Finder自体は状態を持たず、ツリーを変更しません。
/// </remarks>
/// <seealso cref="WidgetTester.Find"/>
public abstract class Finder
{
    /// <summary>
    /// 派生型の共通部分を初期化します。派生型をこのアセンブリの中に限るため、外からは呼べません。
    /// </summary>
    private protected Finder() { }

    /// <summary>
    /// 探した条件を、人が読める形で説明した文字列を取得します。
    /// </summary>
    /// <value><see cref="WidgetAssertionException"/>のメッセージに埋め込む説明。例: 「型が Text のWidget」。</value>
    internal abstract string Description { get; }

    /// <summary>
    /// 現在のElementツリーを探し直し、一致したElementを返します。
    /// 呼ぶたびに探し直すので、<see cref="WidgetTester.Pump"/>のあとも同じFinderで更新後のツリーを調べられます。
    /// </summary>
    /// <returns>
    /// 一致したElementを、深さ優先でたどった順に並べた列。呼んだ時点のツリーで確定した列で、あとのPumpでは変わりません。
    /// 一致がないときと、<see cref="WidgetTester.PumpWidget"/>の前は、空の列です。
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// <see cref="First"/>または<see cref="Last"/>で作ったFinderで、絞り込み前の一致が0件のとき。
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <see cref="At"/>で作ったFinderで、indexが絞り込み前の一致の範囲外のとき。
    /// </exception>
    public IEnumerable<Element> Evaluate()
    {
        // 挙動:
        // 1. testerの現在のルートElementから、VisitChildrenで深さ優先に全Elementをたどる。
        // 2. skipOffstageが真なら、IsOffstageが真のOffstageの子と、IndexedStackの表示していない子へは降りない。
        // 3. 条件に合うElementを、たどった順に返す。
        // 4. 結果をキャッシュしない。呼ぶたびに探し直す。
        // 5. PumpWidgetの前(ルートなし)は、空の列を返す。
        // 呼んだ時点のツリーで結果を確定させるため、遅延評価にせずリストで返す。
        return EvaluateCore();
    }

    /// <summary>
    /// 現在のElementツリーを探し、一致したElementを集めます。<see cref="Evaluate"/>の中身です。
    /// </summary>
    /// <returns>一致したElementを、深さ優先でたどった順に並べた新しいリスト。</returns>
    private protected abstract List<Element> EvaluateCore();

    /// <summary>一致した最初のElementだけに絞り込んだ、新しいFinderを取得します。</summary>
    /// <value>
    /// このFinderの結果の先頭1件を返すFinder。取得した時点では探さず、<see cref="Evaluate"/>のたびにこのFinderから探し直します。
    /// </value>
    public Finder First
    {
        // 挙動: 絞り込み前のEvaluate()の先頭1件だけを返すFinder。
        // 絞り込み前が0件のとき、Evaluate()でInvalidOperationExceptionを投げる(Flutterのparent.firstと同じ)。
        get => new IndexFinder(this, $"{Description} の先頭", elements => elements.First());
    }

    /// <summary>一致した最後のElementだけに絞り込んだ、新しいFinderを取得します。</summary>
    /// <value>
    /// このFinderの結果の末尾1件を返すFinder。取得した時点では探さず、<see cref="Evaluate"/>のたびにこのFinderから探し直します。
    /// </value>
    public Finder Last
    {
        // 挙動: 絞り込み前のEvaluate()の末尾1件だけを返すFinder。
        // 絞り込み前が0件のとき、Evaluate()でInvalidOperationExceptionを投げる(Flutterのparent.lastと同じ)。
        get => new IndexFinder(this, $"{Description} の末尾", elements => elements.Last());
    }

    /// <summary>一致した<paramref name="index"/>番目のElementだけに絞り込んだ、新しいFinderを返します。</summary>
    /// <param name="index">0始まりの位置。範囲は、このメソッドではなく<see cref="Evaluate"/>のときに確かめます。</param>
    /// <returns>
    /// このFinderの結果の<paramref name="index"/>番目の1件を返すFinder。<see cref="Evaluate"/>のたびにこのFinderから探し直します。
    /// </returns>
    public Finder At(int index)
    {
        // 挙動: 絞り込み前のEvaluate()のindex番目だけを返すFinder。
        // 範囲外や負のindexは、Evaluate()でElementAtが投げるArgumentOutOfRangeExceptionのまま通す(FlutterのelementAt(index)と同じ)。
        return new IndexFinder(
            this,
            $"{Description} の{index}番目",
            elements => elements.ElementAt(index)
        );
    }

    /// <summary>ツリーをたどり、条件に合うElementを集めるFinder。<see cref="CommonFinders"/>の各入口が作ります。</summary>
    /// <param name="root">
    /// たどり始めるルートElementを返す処理。<see cref="Evaluate"/>のたびに呼びます。
    /// <see langword="null"/>を返したときは、ルートがないものとして空の列を返します。
    /// </param>
    /// <param name="skipOffstage">
    /// <see langword="true"/>なら、画面に出ていない子(<c>Offstage</c>と<c>IndexedStack</c>が隠す子)とその子孫へ降りません。
    /// </param>
    /// <param name="predicate">一致とみなすElementで<see langword="true"/>を返す条件。</param>
    /// <param name="description">失敗時のメッセージに使う、条件の説明。</param>
    internal sealed class MatchFinder(
        Func<Element?> root,
        bool skipOffstage,
        Func<Element, bool> predicate,
        string description
    ) : Finder
    {
        /// <inheritdoc/>
        /// <value>コンストラクターに渡した条件の説明。画面外も探すときは「(画面外を含む)」を付けます。</value>
        internal override string Description { get; } =
            skipOffstage ? description : $"{description}(画面外を含む)";

        /// <inheritdoc/>
        /// <remarks>ルートElement自身は対象にせず、その子孫だけを調べます。</remarks>
        private protected override List<Element> EvaluateCore()
        {
            var result = new List<Element>();
            if (root() is not { } rootElement)
                return result;

            // ルート自身は対象にせず、その子孫だけをたどる(FlutterのcollectAllElementsFromと同じ)。
            VisitDescendants(rootElement);
            return result;

            void VisitDescendants(Element element)
            {
                Action<Element> visit = child =>
                {
                    if (predicate(child))
                        result.Add(child);
                    VisitDescendants(child);
                };

                if (skipOffstage)
                    element.VisitOnstageChildren(visit);
                else
                    element.VisitChildren(visit);
            }
        }
    }

    /// <summary>親のFinderの結果から1件だけを取り出すFinder。<see cref="First"/>、<see cref="Last"/>、<see cref="At"/>が作ります。</summary>
    /// <param name="parent">絞り込む前のFinder。<see cref="Evaluate"/>のたびに探し直させます。</param>
    /// <param name="description">失敗時のメッセージに使う、絞り込みを含めた条件の説明。</param>
    /// <param name="select">
    /// 親の結果から1件を選ぶ処理。選べないときに投げた例外は、そのまま<see cref="Evaluate"/>から投げられます。
    /// </param>
    private sealed class IndexFinder(
        Finder parent,
        string description,
        Func<List<Element>, Element> select
    ) : Finder
    {
        /// <inheritdoc/>
        internal override string Description { get; } = description;

        /// <inheritdoc/>
        /// <returns>親の結果から選んだ1件だけを含むリスト。</returns>
        private protected override List<Element> EvaluateCore() => [select(parent.EvaluateCore())];
    }
}
