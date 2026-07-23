using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;

namespace FloatSoda.Widgets.Layout;

internal record Padding : SingleChildRenderObjectWidget<RenderSiftedBox>
{
    public EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;


    public override RenderSiftedBox CreateRenderObject()
    {
        return new RenderSiftedBox();
    }

    public override void UpdateRenderObject(RenderSiftedBox renderObject)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// 単一のRenderBoxを保持し、親データで指定されたオフセットを加えて描画するRenderObjectです。
/// </summary>
public class RenderSiftedBox : RenderBox, IHasSingleChildRenderObject
{
    private readonly SingleChildContainer<RenderBox> _child;

    /// <summary>
    /// 子を持たないRenderObjectを生成します。
    /// </summary>
    public RenderSiftedBox() => _child = new SingleChildContainer<RenderBox>(this);

    /// <summary>
    /// レイアウトおよび描画の対象となる子RenderBoxを取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値を置き換えると、以前の子との親子関係を解除し、新しい子をこのRenderObjectの子として登録します。
    /// この設定だけではDirty状態を変更しません。
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
    /// 子RenderObjectへ位置オフセットを保持する親データを設定します。
    /// </summary>
    /// <param name="child">このRenderObjectの子として親データを設定するRenderObject。</param>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <summary>
    /// このRenderObjectと子RenderObjectを指定された描画パイプラインへ接続します。
    /// </summary>
    /// <param name="owner">接続先の描画パイプライン。パイプラインを関連付けない場合は<see langword="null"/>。</param>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        _child.Attach(owner);
    }

    /// <summary>
    /// 子RenderObjectを含むこのRenderObjectを描画パイプラインから切り離します。
    /// </summary>
    public override void Detach()
    {
        base.Detach();
        _child.Detach();
    }

    /// <summary>
    /// 子RenderObjectが存在する場合に、指定された処理を一度適用します。
    /// </summary>
    /// <param name="visitor">子RenderObjectへ適用する処理。</param>
    public override void VisitChildren(Action<RenderObject> visitor) => _child.VisitChildren(visitor);

    /// <summary>
    /// 子RenderObjectの深さを、このRenderObjectとの親子関係に合わせて更新します。
    /// </summary>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    /// <summary>
    /// このRenderObjectと子RenderObjectのサイズおよび位置を計算します。
    /// </summary>
    /// <exception cref="NotImplementedException">この型ではレイアウト処理が実装されていません。</exception>
    public override void PerformLayout()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// 子RenderObjectを、その親データに保持された位置オフセットを加えて描画します。
    /// </summary>
    /// <param name="context">描画命令の記録先を提供するコンテキスト。</param>
    /// <param name="offset">このRenderObjectを描画する親座標系での原点。</param>
    /// <remarks>子が<see langword="null"/>の場合は何も描画しません。</remarks>
    public override void Paint(PaintingContext context, Offset offset)
    {
        var child = Child;

        if (child == null) return;

        var childParentData = child.ParentData as BoxParentData;
        child.Paint(context, offset + childParentData?.Offset ?? Offset.Zero);
    }
}

/// <summary>
/// <see cref="Spacing"/>で指定した余白を保持するRenderObjectです。
/// </summary>
/// <remarks>
/// レイアウト処理は未実装です。<see cref="Spacing"/>の値は保持されるのみで、
/// 基底の<see cref="RenderSiftedBox.PerformLayout"/>実装により、
/// レイアウト時に<see cref="NotImplementedException"/>がスローされます。
/// </remarks>
public class RenderPadding : RenderSiftedBox
{
    /// <summary>子RenderBoxの周囲に適用する余白を取得します。</summary>
    public required EdgeInsets Spacing { get; init; } = EdgeInsets.Zero;
}
