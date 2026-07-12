namespace FloatSoda.Animation;

/// <summary>値が完了・初期状態のどちら側にあるか、どちらへ進行中かを表します。</summary>
public enum AnimationStatus
{
    Dismissed, // 停止・先頭(LowerBound)
    Forward, // 順方向に進行中
    Reverse, // 逆方向に進行中
    Completed, // 停止・末尾(UpperBound)
}

/// <summary>変更通知を発行できるオブジェクト。FlutterのListenable相当。</summary>
public interface IListenable
{
    /// <summary>値が変化したフレームで発火します。</summary>
    event Action? Changed;
}

/// <summary>時間経過で変化する値。FlutterのAnimation&lt;T&gt;相当。</summary>
public interface IAnimation<out T> : IListenable
{
    T Value { get; }
    AnimationStatus Status { get; }

    /// <summary>Statusが遷移したときに新しい値を伴って発火します。</summary>
    event Action<AnimationStatus>? StatusChanged;
}

public class AnimationController : IAnimation<double>, IDisposable
{
    /// <summary>tickの供給元。通常はState側のITickerProvider実装を渡します。</summary>
    public required ITickerProvider Vsync { get; init; }

    /// <summary>Forward/Reverseの標準所要時間。</summary>
    public required TimeSpan Duration { get; init; }

    public double LowerBound { get; init; } = 0.0;
    public double UpperBound { get; init; } = 1.0;
    public ICurve Curve { get; init; } = new LinearCurve(); // Curveに静的プロパティを足す想定

    private WidgetTicker? _ticker;
    private ISimulation? _simulation;

    /// <summary>完了時に遷移するStatus(Completed/Dismissed)。進行方向を表します。</summary>
    private AnimationStatus _completionStatus = AnimationStatus.Completed;

    private double? _value;

    public double Value => _value ?? LowerBound;
    public AnimationStatus Status { get; private set; } = AnimationStatus.Dismissed;

    public event Action? Changed;
    public event Action<AnimationStatus>? StatusChanged;

    /// <summary>UpperBoundへ向けて再生します。fromを指定するとその値から開始します。</summary>
    public void Forward(double? from = null)
    {
        AnimateTo(UpperBound, AnimationStatus.Forward, from);
    }

    /// <summary>LowerBoundへ向けて再生します。</summary>
    public void Reverse(double? from = null)
    {
        AnimateTo(LowerBound, AnimationStatus.Reverse, from);
    }

    private void AnimateTo(double target, AnimationStatus direction, double? from)
    {
        if (from is { } fromValue)
        {
            SetValue(fromValue);
        }

        // 途中からの再開でも進行速度が変わらないよう、残り割合でDurationをスケールする
        var range = UpperBound - LowerBound;
        var remainingFraction = range == 0 ? 0 : Math.Abs(target - Value) / range;

        if (remainingFraction == 0)
        {
            // 既に終端にいる場合はTickerを回さず即座に完了させる
            SetValue(target);
            SetStatus(CompletionStatusOf(direction));
            return;
        }

        SetStatus(direction);
        StartSimulation(
            new InterpolationSimulation(Value, target, Duration * remainingFraction, Curve),
            direction);
    }

    /// <summary>任意のシミュレーション(スプリング等)で駆動します。完了時のStatusはCompletedになります。</summary>
    public void AnimateWith(ISimulation simulation)
    {
        SetStatus(AnimationStatus.Forward);
        StartSimulation(simulation, AnimationStatus.Forward);
    }

    private void StartSimulation(ISimulation simulation, AnimationStatus direction)
    {
        _completionStatus = CompletionStatusOf(direction);
        _simulation = simulation;

        _ticker ??= Vsync.CreateTicker(OnTick);
        _ticker.Stop(); // 経過時間の基準点をリセットする
        _ticker.Start();
    }

    private static AnimationStatus CompletionStatusOf(AnimationStatus direction) =>
        direction == AnimationStatus.Reverse ? AnimationStatus.Dismissed : AnimationStatus.Completed;

    private void OnTick(TimeSpan elapsed)
    {
        if (_simulation is not { } simulation) return;

        var seconds = elapsed.TotalSeconds;

        SetValue(simulation.X(seconds));

        if (!simulation.IsDone(seconds)) return;

        _simulation = null;
        _ticker?.Stop();
        SetStatus(_completionStatus);
    }

    private void SetValue(double value)
    {
        var clamped = Math.Clamp(value, LowerBound, UpperBound);
        var changed = Value != clamped;

        _value = clamped;

        if (changed) Changed?.Invoke();
    }

    private void SetStatus(AnimationStatus status)
    {
        if (Status == status) return;

        Status = status;
        StatusChanged?.Invoke(status);
    }

    /// <summary>再生を止めます。Valueは現在値のまま保持されます。</summary>
    public void Stop()
    {
        _simulation = null;
        _ticker?.Stop();
    }

    public void Dispose()
    {
        _ticker?.Dispose();
        _ticker = null;
        _simulation = null;
        Changed = null;
        StatusChanged = null;
    }
}
