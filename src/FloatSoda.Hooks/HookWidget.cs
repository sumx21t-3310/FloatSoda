using FloatSoda.Elements;
using FloatSoda.Widgets;
using R3;

namespace FloatSoda.Hooks;

public abstract record HookWidget : Widget
{
    public Widget Child { get; init; }

    // 現在ビルド中のElementを追跡するためのスレッドローカル（または引数経由）の仕組み
    // ここでは単純化のため、Build時にContextから取得する設計にします
    [ThreadStatic] internal static HookElement? CurrentElementBuilding;

    protected ReactiveProperty<T> UseState<T>(T initState)
    {
        if (CurrentElementBuilding == null)
        {
            throw new InvalidOperationException("UseState は Build メソッド内でのみ呼び出すことができます。");
        }

        return CurrentElementBuilding.GetOrCreateState(initState);
    }

    public abstract Widget Build(IBuildContext context);
}

public class HookElement : ComponentElement, IDisposable
{
    public DisposableBag Bag  = new();
    private readonly Subject<Unit> _rebuildSubject = new();
    public Observable<Unit> OnRebuildRequired => _rebuildSubject;

    // 状態（ReactiveProperty）を順番に管理するためのリストとインデックス
    private readonly List<object> _states = new();
    private int _stateIndex = 0;

    public HookWidget HookWidget { get; }

    public HookElement(HookWidget widget)
    {
        HookWidget = widget;
        _rebuildSubject = new Subject<Unit>();
        _rebuildSubject.Subscribe(_ => Rebuild());
    }


    // HookWidgetのBuildを呼び出すコアロジック
    public Widget PerformBuild(IBuildContext context)
    {
        // ビルド開始前にインデックスをリセットし、静的コンテキストに自身をセット
        _stateIndex = 0;
        HookWidget.CurrentElementBuilding = this;

        try
        {
            return HookWidget.Build(context);
        }
        finally
        {
            // ビルド終了後にクリーンアップ
            HookWidget.CurrentElementBuilding = null;
        }
    }

    // 状態の取得または生成
    internal ReactiveProperty<T> GetOrCreateState<T>(T initState)
    {
        // 初回ビルド時：新しくReactivePropertyを作成
        if (_stateIndex >= _states.Count)
        {
            var prop = new ReactiveProperty<T>(initState).AddTo(ref Bag);

            // 値が変更されたら再描画を要求
            prop.Subscribe(_ => _rebuildSubject.OnNext(Unit.Default)).AddTo(ref Bag);

            _states.Add(prop);
        }

        // 既存の状態を取得してインデックスを進める
        var state = (ReactiveProperty<T>)_states[_stateIndex];
        _stateIndex++;
        return state;
    }

    // 外部のObservableと紐付けて再描画させるユーティリティ
    public void SubscribeBuildEvent<T>(Observable<T> observer) => observer.Subscribe(_ => _rebuildSubject.OnNext(Unit.Default)).AddTo(ref Bag);

    public void Dispose()
    {
        _rebuildSubject.OnCompleted();
        Bag.Dispose();
    }

    protected override void Rebuild()
    {
        throw new NotImplementedException();
    }

    public override Widget Build()
    {
        throw new NotImplementedException();
    }
}