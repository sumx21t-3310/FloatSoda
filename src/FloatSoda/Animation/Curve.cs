using System.Diagnostics;
using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Animation;

/// <summary>正規化された時刻 t を別の値へ写す関数。FlutterのCurve/Curve系の最小契約。</summary>
public interface ICurve
{
    /// <summary>t∈[0,1] を受け取り、変換後の値を返します。</summary>
    public double Transform(double t);
}

/// <summary>
/// t∈[0,1] を写像するイージング曲線の基底。FlutterのCurve相当。
/// 端点(t==0/t==1)はそのまま返し、その間だけ<see cref="TransformInternal"/>へ委譲します。
/// </summary>
public abstract record Curve : ICurve
{
    /// <summary>t∈[0,1] を変換します。端点は保証され、内部処理は<see cref="TransformInternal"/>に委譲します。</summary>
    public double Transform(double t)
    {
        Debug.Assert(t is >= 0.0 and <= 1.0, $"parametric value {t} is outside of [0, 1].");

        // 端点は数値誤差を避けるため素通しする(Flutterと同じ挙動)
        if (t == 0.0 || t == 1.0)
        {
            return t;
        }

        return TransformInternal(t);
    }

    /// <summary>t∈(0,1) の変換本体。派生クラスが実装します。</summary>
    protected abstract double TransformInternal(double t);

    /// <summary>時間軸・値軸を反転したカーブ(Flutterの<c>flipped</c>)。</summary>
    public Curve Flipped => new FlippedCurve(this);
}

/// <summary>t をそのまま返す線形カーブ。</summary>
public record LinearCurve : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t) => t;
}

/// <summary>0→1 を<paramref name="Count"/>回繰り返すノコギリ波。</summary>
public record SawTooth(int Count) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        t *= Count;
        return t - Math.Truncate(t);
    }
}

/// <summary>
/// <see cref="Begin"/>〜<see cref="End"/> の区間だけ<see cref="CurveValue"/>を適用し、
/// 区間外は0または1にクランプするカーブ。
/// </summary>
public record Interval(double Begin, double End, ICurve? CurveValue = null) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        Debug.Assert(Begin >= 0.0);
        Debug.Assert(Begin <= 1.0);
        Debug.Assert(End >= 0.0);
        Debug.Assert(End <= 1.0);
        Debug.Assert(End >= Begin);

        t = Math.Clamp((t - Begin) / (End - Begin), 0.0, 1.0);
        if (t == 0.0 || t == 1.0)
        {
            return t;
        }

        return (CurveValue ?? new LinearCurve()).Transform(t);
    }
}

/// <summary><see cref="ThresholdValue"/>未満は0、以上は1を返すステップ関数。</summary>
public record Threshold(double ThresholdValue) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        Debug.Assert(ThresholdValue >= 0.0);
        Debug.Assert(ThresholdValue <= 1.0);
        return t < ThresholdValue ? 0.0 : 1.0;
    }
}

/// <summary>制御点(<paramref name="A"/>,<paramref name="B"/>)(<paramref name="C"/>,<paramref name="D"/>)を持つ3次ベジェ曲線。CSSの<c>cubic-bezier</c>相当。</summary>
public record Cubic(double A, double B, double C, double D) : Curve
{
    private const double CubicErrorBound = 0.001;

    private static double EvaluateCubic(double a, double b, double m) =>
        3 * a * (1 - m) * (1 - m) * m +
        3 * b * (1 - m) * m * m +
        m * m * m;

    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        var start = 0.0;
        var end = 1.0;
        while (true)
        {
            var midpoint = (start + end) / 2;
            var estimate = EvaluateCubic(A, C, midpoint);
            if (Math.Abs(t - estimate) < CubicErrorBound)
            {
                return EvaluateCubic(B, D, midpoint);
            }

            if (estimate < t)
            {
                start = midpoint;
            }
            else
            {
                end = midpoint;
            }
        }
    }
}

/// <summary>2つの<see cref="Cubic"/>を中点<paramref name="Midpoint"/>で連結した曲線。強調イージング等に使用します。</summary>
public record ThreePointCubic(Offset A1, Offset B1, Offset Midpoint, Offset A2, Offset B2) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        var firstCurve = t < Midpoint.X;
        var scaleX = firstCurve ? Midpoint.X : 1.0 - Midpoint.X;
        var scaleY = firstCurve ? Midpoint.Y : 1.0 - Midpoint.Y;
        var scaledT = (t - (firstCurve ? 0.0 : Midpoint.X)) / scaleX;

        if (firstCurve)
        {
            return new Cubic(A1.X / scaleX, A1.Y / scaleY, B1.X / scaleX, B1.Y / scaleY).Transform(scaledT) * scaleY;
        }

        return new Cubic(
            (A2.X - Midpoint.X) / scaleX,
            (A2.Y - Midpoint.Y) / scaleY,
            (B2.X - Midpoint.X) / scaleX,
            (B2.Y - Midpoint.Y) / scaleY).Transform(scaledT) * scaleY + Midpoint.Y;
    }
}

/// <summary><paramref name="CurveValue"/>を時間軸・値軸で反転したカーブ(<c>1 - curve(1 - t)</c>)。</summary>
public record FlippedCurve(ICurve CurveValue) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t) => 1.0 - CurveValue.Transform(1.0 - t);
}

/// <summary>放物線状に減速するカーブ(<c>1 - (1 - t)^2</c>相当)。FlutterのCurves.decelerate。</summary>
public record DecelerateCurve : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        t = 1.0 - t;
        return 1.0 - t * t;
    }
}

/// <summary>ゴムひものように加速しながら振動して立ち上がる弾性In。<paramref name="Period"/>は振動周期。</summary>
public record ElasticInCurve(double Period = 0.4) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        var s = Period / 4.0;
        t -= 1.0;
        return -Math.Pow(2.0, 10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / Period);
    }
}

/// <summary>目標を行き過ぎてから振動して収束する弾性Out。<paramref name="Period"/>は振動周期。</summary>
public record ElasticOutCurve(double Period = 0.4) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        var s = Period / 4.0;
        return Math.Pow(2.0, -10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / Period) + 1.0;
    }
}

/// <summary>始点・終点の両方で弾性的に振動する弾性InOut。<paramref name="Period"/>は振動周期。</summary>
public record ElasticInOutCurve(double Period = 0.4) : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        var s = Period / 4.0;
        t = 2.0 * t - 1.0;
        if (t < 0.0)
        {
            return -0.5 * Math.Pow(2.0, 10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / Period);
        }

        return Math.Pow(2.0, -10.0 * t) * Math.Sin((t - s) * (Math.PI * 2.0) / Period) * 0.5 + 1.0;
    }
}

/// <summary>ボールが跳ねるように弾む曲線群の共通実装。</summary>
internal static class BounceCurve
{
    /// <summary>Flutterの<c>_bounce</c>相当。t∈[0,1] を弾む値へ写します。</summary>
    public static double Bounce(double t)
    {
        if (t < 1.0 / 2.75)
        {
            return 7.5625 * t * t;
        }

        if (t < 2.0 / 2.75)
        {
            t -= 1.5 / 2.75;
            return 7.5625 * t * t + 0.75;
        }

        if (t < 2.5 / 2.75)
        {
            t -= 2.25 / 2.75;
            return 7.5625 * t * t + 0.9375;
        }

        t -= 2.625 / 2.75;
        return 7.5625 * t * t + 0.984375;
    }
}

/// <summary>跳ねながら立ち上がるバウンスIn。</summary>
public record BounceInCurve : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t) => 1.0 - BounceCurve.Bounce(1.0 - t);
}

/// <summary>終端で跳ねてから静止するバウンスOut。</summary>
public record BounceOutCurve : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t) => BounceCurve.Bounce(t);
}

/// <summary>始点・終点の両方で跳ねるバウンスInOut。</summary>
public record BounceInOutCurve : Curve
{
    /// <inheritdoc/>
    protected override double TransformInternal(double t)
    {
        if (t < 0.5)
        {
            return (1.0 - BounceCurve.Bounce(1.0 - t * 2.0)) * 0.5;
        }

        return BounceCurve.Bounce(t * 2.0 - 1.0) * 0.5 + 0.5;
    }
}
