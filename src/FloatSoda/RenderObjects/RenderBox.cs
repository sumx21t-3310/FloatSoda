using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;
using FloatSoda.Core;
using FloatSoda.Gesture;
using FloatSoda.Geometrics;
using SkiaSharp;

namespace FloatSoda.RenderObjects;

/// <summary>矩形のサイズを持ち、ボックス制約によるレイアウトとヒットテストを行うRenderObjectの基底クラスです。</summary>
/// <remarks>
/// intrinsic測定は通常レイアウトとは独立した追加走査であり、入れ子では最悪O(N²)になり得ます。
/// スクロール領域や大規模ツリーでは繰り返し問い合わせず、可能なら固定制約または通常レイアウトを使用してください。
/// </remarks>
public abstract class RenderBox : RenderObject
{
    /// <inheritdoc/>
    public override SKSize Size { get; protected set; } = SKSize.Empty;

    /// <summary>指定した高さで内容を欠落させずに表示できる最小の幅を問い合わせます。</summary>
    /// <param name="height">利用可能な高さ。0以上の有限値または正の無限大を指定します。</param>
    /// <returns>0以上の論理ピクセル単位の幅。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="height"/>が負、NaN、または負の無限大です。</exception>
    /// <exception cref="NotSupportedException">派生型がintrinsic測定を実装していません。</exception>
    /// <remarks>問い合わせは通常レイアウトの<see cref="Size"/>、Dirty状態、親データ、描画状態を変更しません。</remarks>
    public double GetMinIntrinsicWidth(double height) => ValidateIntrinsicResult(
        ComputeMinIntrinsicWidth(ValidateDimension(height, nameof(height))), nameof(GetMinIntrinsicWidth));

    /// <summary>指定した高さで内容が自然に占める最大の幅を問い合わせます。</summary>
    /// <param name="height">利用可能な高さ。0以上の有限値または正の無限大を指定します。</param>
    /// <returns>0以上の論理ピクセル単位の幅。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="height"/>が負、NaN、または負の無限大です。</exception>
    /// <exception cref="NotSupportedException">派生型がintrinsic測定を実装していません。</exception>
    /// <remarks>問い合わせは通常レイアウトの状態を変更しません。複数回の問い合わせは高コストになり得ます。</remarks>
    public double GetMaxIntrinsicWidth(double height) => ValidateIntrinsicResult(
        ComputeMaxIntrinsicWidth(ValidateDimension(height, nameof(height))), nameof(GetMaxIntrinsicWidth));

    /// <summary>指定した幅で内容を欠落させずに表示できる最小の高さを問い合わせます。</summary>
    /// <param name="width">利用可能な幅。0以上の有限値または正の無限大を指定します。</param>
    /// <returns>0以上の論理ピクセル単位の高さ。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/>が負、NaN、または負の無限大です。</exception>
    /// <exception cref="NotSupportedException">派生型がintrinsic測定を実装していません。</exception>
    /// <remarks>問い合わせは通常レイアウトの状態を変更しません。複数回の問い合わせは高コストになり得ます。</remarks>
    public double GetMinIntrinsicHeight(double width) => ValidateIntrinsicResult(
        ComputeMinIntrinsicHeight(ValidateDimension(width, nameof(width))), nameof(GetMinIntrinsicHeight));

    /// <summary>指定した幅で内容が自然に占める最大の高さを問い合わせます。</summary>
    /// <param name="width">利用可能な幅。0以上の有限値または正の無限大を指定します。</param>
    /// <returns>0以上の論理ピクセル単位の高さ。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/>が負、NaN、または負の無限大です。</exception>
    /// <exception cref="NotSupportedException">派生型がintrinsic測定を実装していません。</exception>
    /// <remarks>問い合わせは通常レイアウトの状態を変更しません。複数回の問い合わせは高コストになり得ます。</remarks>
    public double GetMaxIntrinsicHeight(double width) => ValidateIntrinsicResult(
        ComputeMaxIntrinsicHeight(ValidateDimension(width, nameof(width))), nameof(GetMaxIntrinsicHeight));

    /// <summary>通常レイアウトを実行せず、指定制約で決まるサイズを計算します。</summary>
    /// <param name="constraints">副作用なしで適用する制約。</param>
    /// <returns>指定制約で決まるサイズ。</returns>
    /// <exception cref="NotSupportedException">派生型がdry layoutを実装していません。</exception>
    internal virtual SKSize ComputeDryLayout(BoxConstraints constraints) => throw CreateUnsupportedIntrinsicException();

    /// <summary>派生型固有の最小intrinsic幅を計算します。</summary>
    protected virtual double ComputeMinIntrinsicWidth(double height) =>
        GetDryLayout(new BoxConstraints(MaxHeight: height)).Width;

    /// <summary>派生型固有の最大intrinsic幅を計算します。</summary>
    protected virtual double ComputeMaxIntrinsicWidth(double height) =>
        GetDryLayout(new BoxConstraints(MaxHeight: height)).Width;

    /// <summary>派生型固有の最小intrinsic高さを計算します。</summary>
    protected virtual double ComputeMinIntrinsicHeight(double width) =>
        GetDryLayout(new BoxConstraints(MaxWidth: width)).Height;

    /// <summary>派生型固有の最大intrinsic高さを計算します。</summary>
    protected virtual double ComputeMaxIntrinsicHeight(double width) =>
        GetDryLayout(new BoxConstraints(MaxWidth: width)).Height;

    internal SKSize GetDryLayout(BoxConstraints constraints)
    {
        ValidateConstraints(constraints);
        var result = ComputeDryLayout(constraints);
        if (!float.IsFinite(result.Width) || result.Width < 0 || !float.IsFinite(result.Height) || result.Height < 0)
        {
            throw new InvalidOperationException($"{GetType().Name} returned an invalid dry layout size: {result}.");
        }

        return result;
    }

    private static double ValidateDimension(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsNegativeInfinity(value) || value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Intrinsic測定の入力には0以上の値または正の無限大を指定してください。");
        }

        return value;
    }

    private static double ValidateIntrinsicResult(double value, string operation)
    {
        if (double.IsNaN(value) || double.IsNegativeInfinity(value) || value < 0)
        {
            throw new InvalidOperationException($"{operation} returned an invalid intrinsic dimension: {value}.");
        }

        return value;
    }

    private static void ValidateConstraints(BoxConstraints constraints)
    {
        ValidateConstraintAxis(constraints.MinWidth, constraints.MaxWidth, nameof(constraints.MinWidth));
        ValidateConstraintAxis(constraints.MinHeight, constraints.MaxHeight, nameof(constraints.MinHeight));
    }

    private static void ValidateConstraintAxis(double minimum, double maximum, string parameterName)
    {
        if (!double.IsFinite(minimum) || minimum < 0 || double.IsNaN(maximum)
            || double.IsNegativeInfinity(maximum) || maximum < minimum)
        {
            throw new ArgumentException("Dry layoutの制約は正規化された非負の値である必要があります。", parameterName);
        }
    }

    private NotSupportedException CreateUnsupportedIntrinsicException() => new(
        $"{GetType().Name} はintrinsic測定をサポートしていません。派生型でComputeDryLayoutまたはintrinsic計算メソッドを実装してください。");

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

    /// <inheritdoc/>
    internal override SKSize ComputeDryLayout(BoxConstraints constraints) =>
        Child?.GetDryLayout(constraints) ?? constraints.Smallest;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicWidth(double height) => Child?.GetMinIntrinsicWidth(height) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicWidth(double height) => Child?.GetMaxIntrinsicWidth(height) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMinIntrinsicHeight(double width) => Child?.GetMinIntrinsicHeight(width) ?? 0;

    /// <inheritdoc/>
    protected override double ComputeMaxIntrinsicHeight(double width) => Child?.GetMaxIntrinsicHeight(width) ?? 0;

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
