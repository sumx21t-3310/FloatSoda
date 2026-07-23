using FloatSoda.Widgets;

namespace FloatSoda.Elements;

/// <summary>
/// Widgetの構築結果として得られる単一の子Widgetを管理するElementの基底クラスです。
/// </summary>
public abstract class ComponentElement : Element
{
    private Element? Child { get; set; }

    /// <summary>
    /// Elementツリーへ接続し、最初の構築を直ちに実行します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    public override void Mount(Element? parent)
    {
        base.Mount(parent);
        FirstBuild();
    }

    /// <summary>
    /// このElementの初回構築を実行します。
    /// </summary>
    public virtual void FirstBuild() => Rebuild();


    /// <summary>
    /// 現在のWidgetから子Widgetを構築し、既存の子Elementを再利用できる場合は更新して置き換えます。
    /// </summary>
    public override void PerformRebuild()
    {
        var built = Build();
        Dirty = false;
        Child = UpdateChild(Child, built);
    }

    /// <summary>
    /// このElementが管理するWidgetから、子として構成するWidgetを生成します。
    /// </summary>
    /// <returns>このElementの直下に配置するWidget。</returns>
    public abstract Widget Build();

    /// <inheritdoc/>
    public override void VisitChildren(Action<Element> visitor)
    {
        if (Child != null) visitor(Child);
    }
}

/// <summary>
/// StatelessWidgetの構築結果を単一の子Elementとして管理するElementです。
/// </summary>
/// <seealso cref="StatelessWidget"/>
public class StatelessElement : ComponentElement
{
    /// <summary>
    /// 管理しているStatelessWidgetから子Widgetを構築します。
    /// </summary>
    /// <returns>StatelessWidgetが構築した子Widget。</returns>
    public override Widget Build() => ((StatelessWidget)Widget!).Build(this);

    /// <summary>
    /// 管理するStatelessWidgetを置き換え、新しい構成で直ちに再構築します。
    /// </summary>
    /// <param name="newWidget">このElementを引き継ぐ新しいStatelessWidget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);
        Dirty = true;
        Rebuild();
    }
}

/// <summary>
/// StatefulWidgetと、そのライフサイクルを担うStateを結び付けて管理するElementです。
/// </summary>
/// <typeparam name="T">このElementが管理するStatefulWidgetの型。</typeparam>
/// <seealso cref="StatefulWidget{T}"/>
/// <seealso cref="State{T}"/>
public class StatefulElement<T> : ComponentElement where T : StatefulWidget<T>
{
    /// <summary>
    /// このElementが所有するStateを取得します。
    /// </summary>
    /// <remarks>Stateはマウント時に生成され、Elementがツリーから恒久的に外れるまで保持されます。</remarks>
    public State<T> State => _stateInternal!;
    private State<T>? _stateInternal = null;
    private bool _didChangeDependencies = false;

    /// <summary><see cref="State{T}.Dispose"/>をすでに呼び出したかどうか。</summary>
    private bool _stateDisposed = false;

    /// <summary>
    /// WidgetからStateを生成して相互に関連付けた後、Elementツリーへ接続します。
    /// </summary>
    /// <param name="parent">接続先の親Element。ルートとして接続する場合は<see langword="null"/>。</param>
    public override void Mount(Element? parent)
    {
        _stateInternal = ((StatefulWidget<T>)Widget!).CreateState();
        State.Element = this;
        State.Widget = (T)Widget!;
        base.Mount(parent);
    }

    /// <summary>
    /// Stateの初期化と初回の依存変更通知を行った後、最初のWidget構築を実行します。
    /// </summary>
    public override void FirstBuild()
    {
        State.InitState();
        State.DidChangeDependencies();
        base.FirstBuild();
    }

    /// <summary>
    /// 保留中の依存変更をStateへ通知した後、Stateから子Widgetを再構築します。
    /// </summary>
    public override void PerformRebuild()
    {
        if (_didChangeDependencies)
        {
            State.DidChangeDependencies();
            _didChangeDependencies = false;
        }

        base.PerformRebuild();
    }


    /// <summary>
    /// 所有するStateから子Widgetを構築します。
    /// </summary>
    /// <returns>Stateが構築した子Widget。</returns>
    public override Widget Build() => State.Build(this);

    /// <summary>
    /// 管理するStatefulWidgetを置き換え、Stateへ更新前のWidgetを通知して直ちに再構築します。
    /// </summary>
    /// <param name="newWidget">既存のStateを引き継ぐ新しいStatefulWidget。</param>
    public override void Update(Widget newWidget)
    {
        base.Update(newWidget);

        var oldWidget = State.Widget!;
        Dirty = true;
        State.Widget = (T)Widget!;
        State.DidUpdateWidget(oldWidget);
        Rebuild();
    }

    /// <summary>
    /// 依存するInheritedWidgetの変更を次の再構築時にStateへ通知するよう予約します。
    /// </summary>
    public override void DidChangeDependencies()
    {
        base.DidChangeDependencies();
        _didChangeDependencies = true;
    }

    /// <summary>
    /// このElement(＝State)がツリーから恒久的に取り除かれるときに呼ばれる。
    /// 子を先に非活性化してから自身のStateを破棄することで、深い側から順に解放される。
    /// 再活性プールを持たない現状のツリーでは Deactivate が終端(unmount相当)となる。
    /// </summary>
    protected override void Deactivate()
    {
        base.Deactivate();

        if (_stateDisposed) return;
        _stateDisposed = true;
        State.Dispose();
    }
}
