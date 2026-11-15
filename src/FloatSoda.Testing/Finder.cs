using FloatSoda.Elements;

namespace FloatSoda.Testing;

/// <summary>
/// 現在のElementツリーから、条件に一致するElementを探します。
/// Flutterの<c>Finder</c>に相当します。
/// </summary>
public abstract class Finder
{
    private protected Finder() { }

    /// <summary>失敗時のメッセージに使う、探した条件の説明。</summary>
    internal abstract string Description { get; }

    /// <summary>
    /// 現在のElementツリーを探し直し、一致したElementを返します。
    /// 呼ぶたびに探し直すので、<see cref="WidgetTester.Pump"/>のあとも同じFinderで更新後のツリーを調べられます。
    /// </summary>
    /// <returns>一致したElementの列。</returns>
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

    private protected abstract List<Element> EvaluateCore();

    /// <summary>一致した最初のElementだけに絞り込んだ、新しいFinderを取得します。</summary>
    public Finder First
    {
        // 挙動: 絞り込み前のEvaluate()の先頭1件だけを返すFinder。
        // 絞り込み前が0件のとき、Evaluate()でInvalidOperationExceptionを投げる(Flutterのparent.firstと同じ)。
        get => new IndexFinder(this, $"{Description} の先頭", elements => elements.First());
    }

    /// <summary>一致した最後のElementだけに絞り込んだ、新しいFinderを取得します。</summary>
    public Finder Last
    {
        // 挙動: 絞り込み前のEvaluate()の末尾1件だけを返すFinder。
        // 絞り込み前が0件のとき、Evaluate()でInvalidOperationExceptionを投げる(Flutterのparent.lastと同じ)。
        get => new IndexFinder(this, $"{Description} の末尾", elements => elements.Last());
    }

    /// <summary>一致した<paramref name="index"/>番目のElementだけに絞り込んだ、新しいFinderを返します。</summary>
    /// <param name="index">0始まりの位置。</param>
    public Finder At(int index)
    {
        // 挙動: 絞り込み前のEvaluate()のindex番目だけを返すFinder。
        // 範囲外や負のindexは、Evaluate()でElementAtが投げるArgumentOutOfRangeExceptionのまま通す(FlutterのelementAt(index)と同じ)。
        return new IndexFinder(this, $"{Description} の{index}番目", elements => elements.ElementAt(index));
    }

    /// <summary>ツリーをたどり、条件に合うElementを集めるFinder。</summary>
    internal sealed class MatchFinder(
        Func<Element?> root,
        bool skipOffstage,
        Func<Element, bool> predicate,
        string description
    ) : Finder
    {
        internal override string Description { get; } =
            skipOffstage ? description : $"{description}(画面外を含む)";

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

    /// <summary>親のFinderの結果から1件だけを取り出すFinder。</summary>
    private sealed class IndexFinder(
        Finder parent,
        string description,
        Func<List<Element>, Element> select
    ) : Finder
    {
        internal override string Description { get; } = description;

        private protected override List<Element> EvaluateCore() => [select(parent.EvaluateCore())];
    }
}
