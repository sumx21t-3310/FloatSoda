using System.Diagnostics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Rendering.Layers;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>
/// レイアウト、描画、ヒットテストに参加するレンダーツリーの基底クラスです。
/// </summary>
/// <remarks>
/// 親子関係とパイプラインへの接続状態を保持し、レイアウトDirtyおよび描画Dirtyを必要な境界まで伝播します。
/// 派生型は<see cref="PerformLayout"/>、<see cref="Paint"/>、<see cref="HandleEvent"/>を実装します。
/// </remarks>
public abstract class RenderObject : IHitTestTarget
{
    /// <summary>直近のレイアウトに使用した制約を取得します。</summary>
    public BoxConstraints Constraints { get; private set; }

    /// <summary>親がこのRenderObjectへ割り当てた配置情報を取得または設定します。</summary>
    /// <value>親固有の配置情報。親へ組み込まれていない場合は<see langword="null"/>です。</value>
    public IParentData? ParentData { get; set; }

    /// <summary>レンダーツリー上の親を取得または設定します。</summary>
    /// <value>親RenderObject。ルートまたは親へ組み込まれていない場合は<see langword="null"/>です。</value>
    public RenderObject? Parent { get; set; }

    /// <summary>このRenderObjectを管理する描画パイプラインを取得または設定します。</summary>
    /// <value>接続先のパイプライン。デタッチ中は<see langword="null"/>です。</value>
    public RenderPipeline? Owner { get; set; }

    /// <summary>このRenderObjectが保持する合成レイヤーを取得または設定します。</summary>
    /// <value>再利用可能なレイヤー。レイヤーをまだ生成していない場合は<see langword="null"/>です。</value>
    public ILayer? Layer { get; set; }

    /// <summary>レイアウトで決定されたサイズを取得します。</summary>
    /// <value>このRenderObjectが占める論理ピクセル単位のサイズ。</value>
    public abstract SKSize Size { get; protected set; }

    /// <summary>描画の再実行が必要かどうかを取得または設定します。</summary>
    /// <value>描画Dirtyの場合は<see langword="true"/>、描画結果が最新の場合は<see langword="false"/>です。</value>
    public bool NeedsPaint { get; set; } = true;

    /// <summary>レイアウトの再実行が必要かどうかを取得または設定します。</summary>
    /// <value>レイアウトDirtyの場合は<see langword="true"/>、レイアウト結果が最新の場合は<see langword="false"/>です。</value>
    public bool NeedsLayout { get; set; } = true;

    /// <summary>レンダーツリー内の深さを取得または設定します。</summary>
    /// <value>ルートを0とした深さ。子は親より大きい値を持ちます。</value>
    public int Depth { get; set; } = 0;

    /// <summary>このRenderObjectが再描画境界かどうかを取得します。</summary>
    /// <value>独立した合成レイヤーへ描画する場合は<see langword="true"/>です。</value>
    public virtual bool IsRepaintBoundary { get; }

    /// <summary>描画パイプラインへ接続されているかどうかを取得します。</summary>
    public bool Attached => Owner != null;

    /// <summary>レイアウトDirtyの伝播が停止するRenderObjectを取得または設定します。</summary>
    /// <value>再レイアウト境界。境界がまだ決定されていない場合は<see langword="null"/>です。</value>
    public RenderObject? RelayoutBoundary { get; set; }

    /// <summary>親から渡された制約だけで自身のサイズを決定できるかどうかを取得します。</summary>
    /// <value>子のレイアウト結果を参照せずにサイズを決定できる場合は<see langword="true"/>です。</value>
    public virtual bool SizedByParent { get; } = false;

    /// <summary>現在の<see cref="Constraints"/>に従って自身と必要な子をレイアウトします。</summary>
    /// <remarks>派生型は処理の完了前に<see cref="Size"/>を確定します。</remarks>
    public abstract void PerformLayout();

    /// <summary>指定した制約でこのRenderObjectをレイアウトします。</summary>
    /// <param name="constraints">サイズの有効範囲を定める制約。</param>
    /// <param name="parentUseSize">親がこのRenderObjectの<see cref="Size"/>を使用する場合は<see langword="true"/>です。</param>
    /// <remarks>
    /// 制約、レイアウトDirty状態、再レイアウト境界のいずれも変化していない場合は何もしません。
    /// レイアウト後はこのRenderObjectのレイアウトDirtyを解除し、描画Dirtyを再描画境界まで伝播します。
    /// 再レイアウト境界が変化した場合は、境界に属していた子孫をレイアウトDirtyにします。
    /// </remarks>
    public void Layout(BoxConstraints constraints, bool parentUseSize = false)
    {
        Debug.WriteLine("Call Layout");

        var relayoutBoundary = !parentUseSize || SizedByParent || constraints.IsTight || Parent == null
            ? this
            : Parent.RelayoutBoundary;

        if (!NeedsLayout && Constraints == constraints && RelayoutBoundary == relayoutBoundary) return;

        Constraints = constraints;

        if (RelayoutBoundary != null && relayoutBoundary != RelayoutBoundary)
        {
            VisitChildren(child => child.CleanChildRelayoutBoundary());
        }

        RelayoutBoundary = relayoutBoundary;

        PerformLayout();

        NeedsLayout = false;

        MarkNeedsPaint();
    }

    /// <summary>現在の制約を変更せずにレイアウトを再実行します。</summary>
    /// <remarks>
    /// <see cref="RenderPipeline"/>が登録済みのレイアウトDirtyを処理するときに使用します。
    /// レイアウト後はこのRenderObjectのレイアウトDirtyを解除し、描画Dirtyを再描画境界まで伝播します。
    /// </remarks>
    public void LayoutWithoutResize()
    {
        PerformLayout();

        NeedsLayout = false;

        MarkNeedsPaint();
    }

    /// <summary>子孫が保持する無効な再レイアウト境界を消去します。</summary>
    /// <remarks>
    /// 自身が再レイアウト境界でない場合、このRenderObjectをレイアウトDirtyにし、同じ処理を子孫へ伝播します。
    /// 自身が再レイアウト境界の場合は伝播を停止します。
    /// </remarks>
    public void CleanChildRelayoutBoundary()
    {
        if (RelayoutBoundary != this)
        {
            RelayoutBoundary = null;
            NeedsLayout = true;
            VisitChildren(child => child.CleanChildRelayoutBoundary());
        }
    }

    /// <summary>レイアウトの再実行を要求します。</summary>
    /// <remarks>
    /// すでにレイアウトDirtyの場合は何もしません。
    /// 再レイアウト境界では自身をレイアウトDirtyとしてパイプラインへ登録し、表示更新を要求します。
    /// 境界でない場合は親方向へ伝播し、最初の再レイアウト境界で登録されます。
    /// </remarks>
    public void MarkNeedsLayout()
    {
        if (NeedsLayout) return;
        if (RelayoutBoundary != this)
        {
            MarkParentNeedsLayout();
        }
        else
        {
            NeedsLayout = true;
            Owner?.NodesNeedingLayout.Add(this);
            Owner?.RequestVisualUpdate();
        }
    }

    /// <summary>このRenderObjectと親へレイアウトの再実行を要求します。</summary>
    /// <remarks>
    /// このRenderObjectをレイアウトDirtyにし、親の<see cref="MarkNeedsLayout"/>を通じて再レイアウト境界まで伝播します。
    /// 親が存在しない場合、このメソッドだけではパイプラインへ登録されません。
    /// </remarks>
    public void MarkParentNeedsLayout()
    {
        NeedsLayout = true;
        Parent?.MarkNeedsLayout();
    }


    /// <summary>このRenderObjectを指定した描画パイプラインへ接続します。</summary>
    /// <param name="owner">接続先のパイプライン。パイプラインを関連付けずに接続する場合は<see langword="null"/>です。</param>
    /// <remarks>
    /// 接続前からレイアウトDirtyで再レイアウト境界が設定済みの場合は、境界へレイアウト要求を登録し直します。
    /// 描画Dirtyで既存レイヤーがある場合は、再描画境界へ描画要求を登録し直します。
    /// 派生型は保持する子にも接続を伝播します。
    /// </remarks>
    public virtual void Attach(RenderPipeline? owner)
    {
        Debug.WriteLine("Call Attach");

        Owner = owner;

        if (NeedsLayout && RelayoutBoundary != null)
        {
            NeedsLayout = false;

            MarkNeedsLayout();
        }

        if (!NeedsPaint || Layer is null) return;

        NeedsPaint = false;
        MarkNeedsPaint();
    }

    /// <summary>子をこのRenderObjectのレンダーツリーへ組み込みます。</summary>
    /// <param name="child">組み込む子。別の親から事前に取り外されている必要があります。</param>
    /// <remarks>
    /// 子の親データを準備し、このRenderObjectへレイアウトDirtyを要求します。
    /// 接続中の場合は子を同じパイプラインへ接続し、子孫の深さも更新します。
    /// </remarks>
    public void AdoptChild(RenderObject child)
    {
        SetupParentData(child);

        MarkNeedsLayout();

        child.Parent = this;

        if (Attached)
        {
            child.Attach(Owner);
        }

        RedepthChild(child);
    }

    /// <summary>指定した子とその子孫の深さを必要に応じて更新します。</summary>
    /// <param name="child">このRenderObjectの直下にある子。</param>
    /// <remarks>子の深さがすでに親より大きい場合は何もしません。</remarks>
    public void RedepthChild(RenderObject child)
    {
        if (child.Depth <= Depth)
        {
            child.Depth = Depth + 1;
            child.RedepthChildren();
        }
    }

    /// <summary>直下の子を起点として子孫の深さを更新します。</summary>
    /// <remarks>子を保持する派生型がオーバーライドします。</remarks>
    public virtual void RedepthChildren() { }

    /// <summary>直下の各子に処理を適用します。</summary>
    /// <param name="visitor">各子に一度ずつ適用する処理。</param>
    /// <remarks>子を保持する派生型がオーバーライドします。</remarks>
    public virtual void VisitChildren(Action<RenderObject> visitor) { }


    /// <summary>指定した子が必要とする親データを準備します。</summary>
    /// <param name="child">このRenderObjectへ組み込む子。</param>
    /// <remarks>親固有の<see cref="IParentData"/>を使用する派生型がオーバーライドします。</remarks>
    public virtual void SetupParentData(RenderObject child) { }


    /// <summary>指定した位置を原点として、このRenderObjectを描画コンテキストへ記録します。</summary>
    /// <param name="context">描画命令と合成レイヤーを記録するコンテキスト。</param>
    /// <param name="offset">親の座標系における描画原点。</param>
    public abstract void Paint(PaintingContext context, Offset offset);

    /// <summary>描画の再実行を要求します。</summary>
    /// <remarks>
    /// すでに描画Dirtyの場合は何もしません。
    /// 再描画境界では自身を描画Dirtyとしてパイプラインへ登録し、表示更新を要求します。
    /// 境界でない場合は親方向へ伝播し、最初の再描画境界で登録されます。
    /// </remarks>
    public void MarkNeedsPaint()
    {
        if (NeedsPaint) return;

        NeedsPaint = true;

        if (IsRepaintBoundary)
        {
            Owner?.NodesNeedingPaint.Add(this);
            Owner?.RequestVisualUpdate();
        }
        else
        {
            Parent?.MarkNeedsPaint();
        }
    }

    /// <summary>このRenderObjectを描画パイプラインから切り離します。</summary>
    /// <remarks>接続先を解除します。子を保持する派生型は子にも切り離しを伝播します。</remarks>
    public virtual void Detach() => Owner = null;

    /// <summary>子をこのRenderObjectのレンダーツリーから取り外します。</summary>
    /// <param name="child">取り外す子。</param>
    /// <remarks>
    /// 子の再レイアウト境界、親データ、親への参照を消去し、接続中の場合は子をデタッチします。
    /// 取り外し後はこのRenderObjectへレイアウトDirtyを要求します。
    /// </remarks>
    public void DropChild(RenderObject child)
    {
        child.CleanChildRelayoutBoundary();
        child.ParentData = null;
        child.Parent = null;

        if (Attached)
        {
            child.Detach();
        }

        MarkNeedsLayout();
    }

    
    /// <summary>ヒットテストで選択されたこのRenderObjectへポインターイベントを配信します。</summary>
    /// <param name="pointerEvent">配信するポインターイベント。</param>
    /// <param name="entry">ヒットテスト時に生成された、このRenderObjectに対応する結果。</param>
    public abstract void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry);
}
