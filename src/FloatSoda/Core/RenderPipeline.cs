using FloatSoda.RenderObjects;

namespace FloatSoda.Core;

/// <summary>RenderObjectツリーの未処理レイアウトと未処理描画をフレーム単位で実行します。</summary>
/// <remarks>Dirty状態のRenderObjectを深さ順に処理し、現在このパイプラインに所属するノードだけを更新します。</remarks>
/// <seealso cref="RenderView"/>
public class RenderPipeline
{
    /// <summary>次フレームで視覚更新が必要になったときに呼び出す処理を取得します。</summary>
    /// <remarks>Dirtyリストが空の状態から更新が予約された際に、フレーム処理を起動するために使用します。</remarks>
    public required Action OnNeedVisualUpdate { get; init; }

    /// <summary>このパイプラインが管理するRenderObjectツリーのルートを取得または設定します。</summary>
    /// <remarks>設定したルートは直ちにこのパイプラインへ接続されます。</remarks>
    public required RenderView RenderView
    {
        get;
        set
        {
            value.Attach(this);
            field = value;
        }
    }

    /// <summary>次回の描画フラッシュを待つRenderObjectの一覧を取得します。</summary>
    /// <remarks>描画フラッシュ開始時に一覧は消去され、処理中に新たにDirtyとなったノードは次回の描画対象になります。</remarks>
    public List<RenderObject> NodesNeedingPaint { get; } = [];
    /// <summary>次回のレイアウトフラッシュを待つRenderObjectの一覧を取得します。</summary>
    /// <remarks>レイアウト中に新たなDirtyノードが登録された場合は、一覧が空になるまで同じフラッシュ内で処理します。</remarks>
    public List<RenderObject> NodesNeedingLayout { get; } = [];

    /// <summary>レイアウトがDirtyなノードを祖先から子孫の順に再レイアウトします。</summary>
    /// <remarks>処理によって追加されたDirtyノードも、待機一覧が空になるまで同じ呼び出し内で処理します。</remarks>
    public void FlushLayout()
    {
        while (NodesNeedingLayout.Count != 0)
        {
            var dirtyNodes = NodesNeedingLayout.OrderBy(x => x.Depth).ToList();

            NodesNeedingLayout.Clear();

            foreach (var node in dirtyNodes)
            {
                if (node.NeedsLayout && node.Owner == this)
                {
                    node.LayoutWithoutResize();
                }
            }
        }
    }

    /// <summary>描画がDirtyなノードを祖先から子孫の順に再描画します。</summary>
    /// <remarks>開始時点の待機一覧を消去して処理するため、処理中に追加されたDirtyノードは次回の描画フラッシュで処理されます。</remarks>
    public void FlushPaint()
    {
        var dirtyNodes = NodesNeedingPaint.OrderBy(x => x.Depth).ToList();

        NodesNeedingPaint.Clear();

        foreach (var node in dirtyNodes)
        {
            if (node.NeedsPaint && node.Owner == this)
            {
                PaintingContext.RepaintCompositedChild(node);
            }
        }
    }

    /// <summary>視覚更新が必要であることをフレーム管理側へ通知します。</summary>
    /// <remarks>この呼び出し自体はレイアウトや描画を実行せず、次フレームの更新を予約します。</remarks>
    public void RequestVisualUpdate() => OnNeedVisualUpdate();
}