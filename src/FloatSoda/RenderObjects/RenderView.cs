using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>
/// RenderObjectツリーのルートとして、表示領域のサイズ、初期レイアウト、初期描画を管理します。
/// </summary>
public class RenderView : RenderObject, IHasSingleChildRenderObject
{
    /// <inheritdoc/>
    public override SKSize Size { get; protected set; }

    private readonly SingleChildContainer<RenderBox> _child;

    /// <summary>
    /// 表示領域へ配置するルートの子を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 子を差し替えると、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に表示領域と子のサイズを再計算します。
    /// 同じ子を再設定した場合も、子の取り外しと追加が行われるためLayout Dirtyとなります。
    /// </remarks>
    public RenderBox? Child
    {
        get => _child.Child;
        set => _child.Child = value;
    }

    RenderObject? IHasSingleChildRenderObject.Child
    {
        get => Child;
        set => Child = (RenderBox?)value;
    }

    /// <summary>
    /// 指定した初期サイズでRenderObjectツリーのルートを初期化します。
    /// </summary>
    /// <param name="width">初期の表示幅。</param>
    /// <param name="height">初期の表示高さ。</param>
    public RenderView(float width = 1000, float height = 1000)
    {
        Size = new SKSize(width, height);
        _child = new SingleChildContainer<RenderBox>(this);
    }

    /// <inheritdoc/>
    public override bool IsRepaintBoundary => true;

    /// <summary>
    /// ウィンドウの固定サイズ。null の場合は子のレイアウト結果サイズに追従します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に表示領域と子のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public SKSize? FixedSize
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    /// <summary>
    /// <see cref="FixedSize"/> があれば Tight 制約でレイアウトし、なければ Loosen（無限上限）制約を
    /// 渡して子ウィジェットのレイアウト結果サイズを <see cref="Size"/>（＝オーバーレイサイズ）に採用します。
    /// </summary>
    public override void PerformLayout()
    {
        if (Child == null)
        {
            Size = FixedSize ?? SKSize.Empty;
            return;
        }

        var constraints = FixedSize is { } fixedSize
            ? BoxConstraints.Tight(fixedSize)
            : BoxConstraints.Unbounded;

        Child.Layout(constraints, parentUseSize: true);
        Size = FixedSize ?? Child.Size;
    }


    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child != null) context.PaintChild(Child, offset);
    }

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    /// <summary>
    /// 最初のフレームに必要なレイアウトと描画をパイプラインへ登録します。
    /// </summary>
    /// <remarks>
    /// このRenderObjectをLayout DirtyおよびPaint Dirtyの処理対象として登録し、
    /// 自身をレイアウト境界、生成したルートレイヤーを再描画境界として設定します。
    /// </remarks>
    public void PrepareInitialFrame()
    {
        ScheduleInitialLayout();
        ScheduleInitialPaint(new TransformLayer());
    }

    private void ScheduleInitialPaint(ContainerLayer rootLayer)
    {
        Layer = rootLayer;
        Owner?.NodesNeedingPaint.Add(this);
    }

    private void ScheduleInitialLayout()
    {
        RelayoutBoundary = this;
        Owner?.NodesNeedingLayout.Add(this);
    }

    /// <summary>
    /// 指定位置に対して子からルートまでのヒットテスト結果を構築します。
    /// </summary>
    /// <param name="result">ヒットした対象を追加する結果。</param>
    /// <param name="position">表示領域のローカル座標。</param>
    /// <returns>ルートは常にヒット対象となるため、常に<c>true</c>。</returns>
    public bool HitTest(HitTestResult result, Offset position)
    {
        Child?.HitTest(result, position);

        result.Add(new HitTestEntry(this));

        return true;
    }

    /// <inheritdoc/>
    public override void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry)
    {
        // Do nothing
    }
}
