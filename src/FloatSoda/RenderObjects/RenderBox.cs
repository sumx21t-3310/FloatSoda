using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>矩形のサイズを持ち、ボックス制約によるレイアウトとヒットテストを行うRenderObjectの基底クラスです。</summary>
public abstract class RenderBox : RenderObject
{
    /// <inheritdoc/>
    public override SKSize Size { get; protected set; } = SKSize.Empty;

    /// <summary>指定したローカル座標がこのRenderBoxまたはその子にヒットするかを判定します。</summary>
    /// <param name="result">ヒットした対象を奥から手前の順に追加する結果。</param>
    /// <param name="position">このRenderBoxのローカル座標系における判定位置。</param>
    /// <returns>自身または子がヒットした場合は<see langword="true"/>、それ以外の場合は<see langword="false"/>です。</returns>
    /// <remarks>
    /// 位置が<see cref="Size"/>の範囲外の場合は子と自身を判定しません。
    /// ヒットした場合は子の結果に続けて自身のエントリを追加します。
    /// </remarks>
    public virtual bool HitTest(HitTestResult result, Offset position)
    {
        if (!Size.Contains(position)) return false;

        if (!HitTestChildren(result, position) && !HitTestSelf(position)) return false;

        result.Add(new HitTestEntry(this));

        return true;
    }

    /// <summary>指定したローカル座標が子にヒットするかを判定します。</summary>
    /// <param name="result">ヒットした子を追加する結果。</param>
    /// <param name="position">このRenderBoxのローカル座標系における判定位置。</param>
    /// <returns>子がヒットした場合は<see langword="true"/>、それ以外の場合は<see langword="false"/>です。</returns>
    public virtual bool HitTestChildren(HitTestResult result, Offset position) => false;

    /// <summary>指定したローカル座標で自身がヒットするかを判定します。</summary>
    /// <param name="position">このRenderBoxのローカル座標系における判定位置。</param>
    /// <returns>自身がヒット対象の場合は<see langword="true"/>、それ以外の場合は<see langword="false"/>です。</returns>
    public virtual bool HitTestSelf(Offset position) => false;

    /// <summary>このRenderBoxへ配信されたポインターイベントを処理します。</summary>
    /// <param name="pointerEvent">配信されたポインターイベント。</param>
    /// <param name="entry">このRenderBoxに対応するヒットテスト結果。</param>
    /// <remarks>既定の実装はイベントを処理しません。</remarks>
    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        // do nothing
    }
}

/// <summary>単一の子へレイアウト、描画、ヒットテストを委譲するRenderBoxの基底クラスです。</summary>
public abstract class RenderProxyBox : RenderBox, IHasSingleChildRenderObject
{
    /// <summary>子の親子関係と接続ライフサイクルを管理するコンテナです。</summary>
    private readonly SingleChildContainer<RenderBox> _child;

    /// <summary>子を持たないRenderProxyBoxを初期化します。</summary>
    protected RenderProxyBox() => _child = new SingleChildContainer<RenderBox>(this);

    /// <summary>レイアウト、描画、ヒットテストを委譲する子を取得または設定します。</summary>
    /// <value>保持する子。子を持たない場合は<see langword="null"/>です。</value>
    /// <remarks>
    /// 値を設定すると旧子をレンダーツリーから取り外し、新しい子を組み込みます。
    /// その過程でこのRenderObjectがレイアウトDirtyになり、次のパイプライン更新時にサイズが再計算されます。
    /// </remarks>
    public RenderBox? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    /// <inheritdoc/>
    RenderObject? IHasSingleChildRenderObject.Child
    {
        get => Child;
        set => Child = (RenderBox?)value;
    }

    /// <summary>子へボックス配置用の親データを割り当てます。</summary>
    /// <param name="child">このRenderProxyBoxへ組み込む子。</param>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <summary>子を現在の制約でレイアウトし、そのサイズを自身へ反映します。</summary>
    /// <remarks>子が存在しない場合は、現在の制約で許される最小サイズを使用します。</remarks>
    public override void PerformLayout()
    {
        if (Child != null)
        {
            Child.Layout(Constraints, parentUseSize: true);
            Size = Child.Size;
        }
        else
        {
            Size = Constraints.Smallest;
        }
    }

    /// <summary>子を指定した位置へ描画します。</summary>
    /// <param name="context">描画命令と合成レイヤーを記録するコンテキスト。</param>
    /// <param name="offset">親の座標系における描画原点。</param>
    /// <remarks>子が存在しない場合は何も記録しません。</remarks>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null) context.PaintChild(Child, offset);
    }

    /// <summary>このRenderProxyBoxと子を指定した描画パイプラインへ接続します。</summary>
    /// <param name="owner">接続先のパイプライン。パイプラインを関連付けずに接続する場合は<see langword="null"/>です。</param>
    /// <remarks>自身に対するDirty状態の再登録後、同じ接続先を子へ伝播します。</remarks>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    /// <summary>このRenderProxyBoxと子を描画パイプラインから切り離します。</summary>
    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    /// <summary>保持している子に処理を適用します。</summary>
    /// <param name="visitor">子が存在する場合に一度適用する処理。</param>
    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    /// <summary>保持している子とその子孫の深さを更新します。</summary>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    /// <summary>指定したローカル座標が子にヒットするかを判定します。</summary>
    /// <param name="result">ヒットした子を追加する結果。</param>
    /// <param name="position">このRenderProxyBoxのローカル座標系における判定位置。</param>
    /// <returns>子が存在してヒットした場合は<see langword="true"/>、それ以外の場合は<see langword="false"/>です。</returns>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        return Child?.HitTest(result, position) ?? false;
    }
}
