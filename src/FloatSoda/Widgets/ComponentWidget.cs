using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// 可変状態を持たず、現在の構成から子ウィジェットを構築するウィジェットの基底型です。
/// </summary>
/// <seealso cref="StatelessElement"/>
public abstract record StatelessWidget : Widget
{
    /// <summary>
    /// このウィジェットの構築結果を管理するElementを生成します。
    /// </summary>
    /// <returns>このウィジェットを保持する未マウントのElement。</returns>
    public override Element CreateElement() => new StatelessElement
    {
        Widget = this
    };

    /// <summary>
    /// 現在の構成から、このウィジェットの直下に配置するウィジェットを構築します。
    /// </summary>
    /// <param name="context">このウィジェットが配置されている構築コンテキスト。</param>
    /// <returns>このウィジェットの直下に配置するウィジェット。</returns>
    public abstract Widget Build(IBuildContext context);
}

/// <summary>
/// Elementの存続期間にわたって保持される可変状態を持つウィジェットの基底型です。
/// </summary>
/// <typeparam name="T">この状態と関連付ける具象ウィジェットの型。</typeparam>
/// <seealso cref="State{T}"/>
/// <seealso cref="StatefulElement{T}"/>
public abstract record StatefulWidget<T> : Widget where T : StatefulWidget<T>
{
    /// <summary>
    /// このウィジェットと状態のライフサイクルを管理するElementを生成します。
    /// </summary>
    /// <returns>このウィジェットを保持する未マウントのElement。</returns>
    public override Element CreateElement() => new StatefulElement<T>
    {
        Widget = this
    };

    /// <summary>
    /// このウィジェットに関連付ける新しい状態オブジェクトを生成します。
    /// </summary>
    /// <returns>Elementがツリーから恒久的に外れるまで保持する新しい状態オブジェクト。</returns>
    public abstract State<T> CreateState();
}

/// <summary>
/// StatefulWidgetの可変状態と構築ライフサイクルを保持する基底型です。
/// </summary>
/// <typeparam name="T">この状態が関連付けられる具象ウィジェットの型。</typeparam>
/// <seealso cref="StatefulWidget{T}"/>
/// <seealso cref="StatefulElement{T}"/>
public abstract class State<T> where T : StatefulWidget<T>
{
    /// <summary>
    /// 現在この状態に関連付けられているウィジェットを取得または設定します。
    /// </summary>
    /// <remarks>Elementへのマウント前は<see langword="null"/>です。更新時は同じ状態を維持したまま新しいウィジェットへ置き換わります。</remarks>
    public T? Widget { get; set; }

    /// <summary>
    /// この状態のライフサイクルを管理するElementを取得または設定します。
    /// </summary>
    /// <remarks>Elementへのマウント前は<see langword="null"/>です。</remarks>
    public StatefulElement<T>? Element { get; set; }
    /// <summary>
    /// この状態が配置されている構築コンテキストを取得します。
    /// </summary>
    /// <remarks>Elementへマウントされた後から破棄されるまで使用できます。</remarks>
    public IBuildContext Context => Element!;

    /// <summary>
    /// 状態がElementへ関連付けられた直後に、一度だけ初期化を行います。
    /// </summary>
    public virtual void InitState() { }

    /// <summary>
    /// 状態を変更する処理を実行し、このElementへ再構築を要求します。
    /// </summary>
    /// <param name="action">状態のフィールドを変更する処理。</param>
    protected virtual void SetState(Action action)
    {
        action();
        Element?.MarkNeedsBuild();
    }

    /// <summary>
    /// 同じElementでウィジェットの構成が更新されたときに通知を受け取ります。
    /// </summary>
    /// <param name="oldWidget">更新前にこの状態へ関連付けられていたウィジェット。</param>
    /// <remarks>呼び出し時点では<see cref="Widget"/>が更新後のウィジェットを参照しています。</remarks>
    public virtual void DidUpdateWidget(T oldWidget) { }

    /// <summary>
    /// 依存するInheritedWidgetが変更されたとき、または初回構築の直前に通知を受け取ります。
    /// </summary>
    public virtual void DidChangeDependencies() { }

    /// <summary>
    /// この State がツリーから恒久的に取り除かれるときに一度だけ呼ばれる。
    /// 購読・タイマー・認識器など、State が確保した外部リソースをここで解放する。
    /// </summary>
    public virtual void Dispose() { }

    /// <summary>
    /// 現在の状態と構成から、このウィジェットの直下に配置するウィジェットを構築します。
    /// </summary>
    /// <param name="context">この状態が関連付けられている構築コンテキスト。</param>
    /// <returns>このウィジェットの直下に配置するウィジェット。</returns>
    public abstract Widget Build(IBuildContext context);
}