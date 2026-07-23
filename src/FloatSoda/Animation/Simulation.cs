namespace FloatSoda.Animation;

/// <summary>経過時間に応じた値と速度を算出するアニメーションシミュレーションを表します。</summary>
public interface ISimulation
{
    // TODO:  あまりにも単純すぎる名前なので適切な命名を考える
    /// <summary>指定時刻における値を取得します。</summary>
    /// <param name="time">シミュレーション開始からの経過秒数。</param>
    /// <returns>指定時刻における値。</returns>
    double X(double time);

    // TODO: あまりにも単純すぎる名前なので適切な命名を考える
    /// <summary>指定時刻における1秒あたりの値の変化量を取得します。</summary>
    /// <param name="time">シミュレーション開始からの経過秒数。</param>
    /// <returns>指定時刻における1秒あたりの値の変化量。</returns>
    double Dx(double time);
    /// <summary>指定時刻でシミュレーションが完了しているかを判定します。</summary>
    /// <param name="time">シミュレーション開始からの経過秒数。</param>
    /// <returns>完了している場合は<see langword="true"/>、継続中の場合は<see langword="false"/>。</returns>
    bool IsDone(double time);
}

/// <summary>開始値から終了値までを指定された曲線で補間するシミュレーションです。</summary>
/// <param name="begin">開始時の値。</param>
/// <param name="end">完了時の値。</param>
/// <param name="duration">補間に要する時間。</param>
/// <param name="curve">正規化された進行率へ適用する曲線。</param>
/// <seealso cref="AnimationController"/>
public class InterpolationSimulation(double begin, double end, TimeSpan duration, ICurve curve) : ISimulation
{
    private readonly double _durationInSeconds = duration.TotalSeconds;


    /// <inheritdoc />
    public double X(double time)
    {
        var t = Math.Clamp(time / _durationInSeconds, 0, 1);

        return t switch
        {
            0 => begin,
            1 => end,
            _ => begin + (end - begin) * curve.Transform(t)
        };
    }


    /// <inheritdoc />
    public double Dx(double time)
    {
        const double epsilon = 1e-4;
        return (X(time + epsilon) - X(time - epsilon)) / (2 * epsilon);
    }

    /// <inheritdoc />
    public bool IsDone(double time) => time > _durationInSeconds;
}