namespace FloatSoda.Animation;

/// <summary>値が完了・初期状態のどちら側にあるか、どちらへ進行中かを表します。</summary>
public enum AnimationStatus
{
    /// <summary>値が下限にあり、停止している状態です。</summary>
    Dismissed, // 停止・先頭(LowerBound)
    /// <summary>値が上限へ向かって進行している状態です。</summary>
    Forward, // 順方向に進行中
    /// <summary>値が下限へ向かって進行している状態です。</summary>
    Reverse, // 逆方向に進行中
    /// <summary>値が上限にあり、停止している状態です。</summary>
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
    /// <summary>現在のアニメーション値を取得します。</summary>
    T Value { get; }
    /// <summary>現在の進行状態を取得します。</summary>
    AnimationStatus Status { get; }

    /// <summary>Statusが遷移したときに新しい値を伴って発火します。</summary>
    event Action<AnimationStatus>? StatusChanged;
}

/// <summary>指定された範囲の値をフレーム単位で更新するアニメーションを制御します。</summary>
/// <remarks>再生中は<see cref="Vsync"/>からフレーム通知を受け、値または状態が変化した場合に通知を発行します。</remarks>
/// <seealso cref="IAnimation{T}"/>
/// <seealso cref="ISimulation"/>
public class AnimationController : IAnimation<double>, IDisposable
{
    /// <summary>tickの供給元。通常はState側のITickerProvider実装を渡します。</summary>
    public required ITickerProvider Vsync { get; init; }

    /// <summary>Forward/Reverseの標準所要時間。</summary>
    public required TimeSpan Duration { get; init; }

    /// <summary>アニメーション値の下限を取得します。既定値は0です。</summary>
    public double LowerBound { get; init; } = 0.0;
    /// <summary>アニメーション値の上限を取得します。既定値は1です。</summary>
    public double UpperBound { get; init; } = 1.0;
    /// <summary>範囲内の進行率へ適用する曲線を取得します。既定では線形に補間します。</summary>
    public ICurve Curve { get; init; } = new LinearCurve(); // Curveに静的プロパティを足す想定

    private WidgetTicker? _ticker;
    private ISimulation? _simulation;

    /// <summary>完了時に遷移するStatus(Completed/Dismissed)。進行方向を表します。</summary>
    private AnimationStatus _completionStatus = AnimationStatus.Completed;

    private double? _value;

    /// <inheritdoc />
    public double Value => _value ?? LowerBound;
    /// <inheritdoc />
    public AnimationStatus Status { get; private set; } = AnimationStatus.Dismissed;

    /// <inheritdoc />
    public event Action? Changed;
    /// <inheritdoc />
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

    /// <summary>フレーム通知を停止し、このコントローラーが保持する通知先を解放します。</summary>
    public void Dispose()
    {
        _ticker?.Dispose();
        _ticker = null;
        _simulation = null;
        Changed = null;
        StatusChanged = null;
    }
}
