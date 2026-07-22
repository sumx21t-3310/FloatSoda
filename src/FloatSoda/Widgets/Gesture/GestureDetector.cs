using FloatSoda.Abstractions.Geometries;
using FloatSoda.Elements;
using FloatSoda.Gesture;

namespace FloatSoda.Widgets.Gesture;

/// <summary>
/// 子に対する高レベルなジェスチャ（タップ・ドラッグ等）を検出するウィジェット。
/// 設定されたコールバックに応じて認識器を組み立て、内部の <see cref="RawGestureDetector"/> へ委譲する。
/// </summary>
public record GestureDetector : StatelessWidget
{
    /// <summary>ジェスチャを検出する対象の子ウィジェットを取得します。</summary>
    public required Widget Child { get; init; }

    /// <summary>ヒットテストでの振る舞い。空白領域を掴みたい場合は <see cref="HitTestBehaviour.Opaque"/>。</summary>
    public HitTestBehaviour Behaviour { get; init; } = HitTestBehaviour.DeferToChild;

    /// <summary>タップ成立時。</summary>
    public Action? OnTap { get; init; }

    /// <summary>ドラッグ開始時。引数は開始位置。</summary>
    public Action<Offset>? OnPanStart { get; init; }

    /// <summary>ドラッグ中に、前回位置からの移動量を通知するコールバックを取得します。</summary>
    public Action<Offset>? OnPanUpdate { get; init; }

    /// <summary>ドラッグ終了時。</summary>
    public Action? OnPanEnd { get; init; }

    /// <summary>設定されたコールバックに対応する認識器ファクトリを構築します。</summary>
    /// <param name="context">このウィジェットのビルドコンテキスト。</param>
    /// <returns>認識器の所有とポインターイベントの橋渡しを行う<see cref="RawGestureDetector"/>。</returns>
    public override Widget Build(IBuildContext context)
    {
        var gestures = new Dictionary<Type, GestureRecognizerFactory>();

        if (OnTap is not null)
        {
            gestures[typeof(TapGestureRecognizer)] = new GestureRecognizerFactory<TapGestureRecognizer>(
                () => new TapGestureRecognizer(),
                r => r.OnTap = OnTap);
        }

        if (OnPanStart is not null || OnPanUpdate is not null || OnPanEnd is not null)
        {
            gestures[typeof(PanGestureRecognizer)] = new GestureRecognizerFactory<PanGestureRecognizer>(
                () => new PanGestureRecognizer(),
                r =>
                {
                    r.OnPanStart = OnPanStart;
                    r.OnPanUpdate = OnPanUpdate;
                    r.OnPanEnd = OnPanEnd;
                });
        }

        return new RawGestureDetector
        {
            Gestures = gestures,
            Behaviour = Behaviour,
            Child = Child
        };
    }
}

/// <summary>
/// 認識器の集合を直接受け取り、内部 <see cref="Listener"/> の Down を各認識器へ橋渡しする低レベル層。
/// 認識器インスタンスは再構築をまたいで保持され、コールバックのみ差し替えられる。
/// </summary>
public record RawGestureDetector : StatefulWidget<RawGestureDetector>
{
    /// <summary>ジェスチャを検出する対象の子ウィジェットを取得します。</summary>
    public required Widget Child { get; init; }

    /// <summary>ポインターのヒットテストで自身と子要素をどのように扱うかを取得します。</summary>
    public HitTestBehaviour Behaviour { get; init; } = HitTestBehaviour.DeferToChild;

    /// <summary>認識器型ごとの生成処理と初期化処理を取得します。</summary>
    /// <remarks>同じ型の認識器は再構築をまたいで再利用され、初期化処理だけが再実行されます。</remarks>
    public Dictionary<Type, GestureRecognizerFactory> Gestures { get; init; } = [];

    /// <inheritdoc />
    public override State<RawGestureDetector> CreateState() => new RawGestureDetectorState();
}

/// <summary><see cref="RawGestureDetector"/>が所有する認識器のライフタイムと設定更新を管理します。</summary>
internal class RawGestureDetectorState : State<RawGestureDetector>
{
    /// <summary>ジェスチャ種別ごとに再構築をまたいで保持する認識器インスタンス。</summary>
    private Dictionary<Type, GestureRecognizer> _recognizers = new();

    /// <inheritdoc />
    public override Widget Build(IBuildContext context)
    {
        SyncAll(Widget!.Gestures);

        return new Listener
        {
            OnPointerDown = HandlePointerDown,
            Behaviour = Widget!.Behaviour,
            Child = Widget!.Child,
        };
    }

    /// <summary>
    /// ファクトリ集合と現在の認識器集合を突き合わせ、既存インスタンスは温存してコールバックだけ更新、
    /// 新規は生成してアリーナ／ルータを注入、消えた種別は破棄する。
    /// </summary>
    private void SyncAll(Dictionary<Type, GestureRecognizerFactory> gestures)
    {
        var binding = Context.Owner?.GestureBinding
            ?? throw new InvalidOperationException("GestureBinding が未設定です（BuildOwner に紐づいていません）。");

        var old = _recognizers;
        var updated = new Dictionary<Type, GestureRecognizer>(gestures.Count);

        foreach (var (type, factory) in gestures)
        {
            if (!old.TryGetValue(type, out var recognizer))
            {
                recognizer = factory.ConstructRaw();
                recognizer.Bind(binding.GestureArena, binding.PointerRouter);
            }

            factory.InitializeRaw(recognizer);
            updated[type] = recognizer;
        }

        foreach (var (type, recognizer) in old)
        {
            if (!updated.ContainsKey(type)) recognizer.Dispose();
        }

        _recognizers = updated;
    }

    /// <summary>Downイベントを現在のすべての認識器へ渡して監視を開始します。</summary>
    /// <param name="downEvent">監視を開始するDownフェーズのポインターイベント。</param>
    private void HandlePointerDown(Abstractions.Input.PointerEvent downEvent)
    {
        foreach (var recognizer in _recognizers.Values)
        {
            recognizer.AddPointer(downEvent);
        }
    }

    /// <summary>所有するすべての認識器を破棄し、状態から取り除きます。</summary>
    public override void Dispose()
    {
        foreach (var recognizer in _recognizers.Values)
        {
            recognizer.Dispose();
        }

        _recognizers.Clear();
        base.Dispose();
    }
}
