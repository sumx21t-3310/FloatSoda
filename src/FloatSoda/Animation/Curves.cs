using FloatSoda.Common.Geometries;

namespace FloatSoda.Animation;

/// <summary>よく使うイージング曲線の標準インスタンス集。FlutterのCurvesに対応します。</summary>
public static class Curves
{
    /// <summary>入力をそのまま返す線形カーブ。</summary>
    public static readonly ICurve Linear = new LinearCurve();

    /// <summary>放物線状に一定減速するカーブ。</summary>
    public static readonly ICurve Decelerate = new DecelerateCurve();

    /// <summary>直線的に始まり、ゆっくりとease inへ移行するカーブ。</summary>
    public static readonly ICurve FastLinearToSlowEaseIn = new Cubic(0.18, 1.0, 0.04, 1.0);

    /// <summary>素早くease inし、ゆっくりease outで終わるカーブ。</summary>
    public static readonly ICurve FastEaseInToSlowEaseOut = new ThreePointCubic(
        new Offset(0.056, 0.024),
        new Offset(0.108, 0.3085),
        new Offset(0.198, 0.541),
        new Offset(0.3655, 1.0),
        new Offset(0.5465, 0.989));

    /// <summary>緩やかに加速し緩やかに減速する標準的なease。</summary>
    public static readonly ICurve Ease = new Cubic(0.25, 0.1, 0.25, 1.0);

    /// <summary>ゆっくり始まり素早く終わるease in。</summary>
    public static readonly ICurve EaseIn = new Cubic(0.42, 0.0, 1.0, 1.0);

    /// <summary>ease inから線形へと繋がるカーブ。</summary>
    public static readonly ICurve EaseInToLinear = new Cubic(0.67, 0.03, 0.65, 0.09);

    /// <summary>サイン波状のease in。</summary>
    public static readonly ICurve EaseInSine = new Cubic(0.47, 0.0, 0.745, 0.715);

    /// <summary>2次関数状のease in。</summary>
    public static readonly ICurve EaseInQuad = new Cubic(0.55, 0.085, 0.68, 0.53);

    /// <summary>3次関数状のease in。</summary>
    public static readonly ICurve EaseInCubic = new Cubic(0.55, 0.055, 0.675, 0.19);

    /// <summary>4次関数状のease in。</summary>
    public static readonly ICurve EaseInQuart = new Cubic(0.895, 0.03, 0.685, 0.22);

    /// <summary>5次関数状のease in。</summary>
    public static readonly ICurve EaseInQuint = new Cubic(0.755, 0.05, 0.855, 0.06);

    /// <summary>指数関数状のease in。</summary>
    public static readonly ICurve EaseInExpo = new Cubic(0.95, 0.05, 0.795, 0.035);

    /// <summary>円弧状のease in。</summary>
    public static readonly ICurve EaseInCirc = new Cubic(0.6, 0.04, 0.98, 0.335);

    /// <summary>いったん戻ってから進むease in(オーバーシュートあり)。</summary>
    public static readonly ICurve EaseInBack = new Cubic(0.6, -0.28, 0.735, 0.045);

    /// <summary>素早く始まりゆっくり終わるease out。</summary>
    public static readonly ICurve EaseOut = new Cubic(0.0, 0.0, 0.58, 1.0);

    /// <summary>線形からease outへと繋がるカーブ。</summary>
    public static readonly ICurve LinearToEaseOut = new Cubic(0.35, 0.91, 0.33, 0.97);

    /// <summary>サイン波状のease out。</summary>
    public static readonly ICurve EaseOutSine = new Cubic(0.39, 0.575, 0.565, 1.0);

    /// <summary>2次関数状のease out。</summary>
    public static readonly ICurve EaseOutQuad = new Cubic(0.25, 0.46, 0.45, 0.94);

    /// <summary>3次関数状のease out。</summary>
    public static readonly ICurve EaseOutCubic = new Cubic(0.215, 0.61, 0.355, 1.0);

    /// <summary>4次関数状のease out。</summary>
    public static readonly ICurve EaseOutQuart = new Cubic(0.165, 0.84, 0.44, 1.0);

    /// <summary>5次関数状のease out。</summary>
    public static readonly ICurve EaseOutQuint = new Cubic(0.23, 1.0, 0.32, 1.0);

    /// <summary>指数関数状のease out。</summary>
    public static readonly ICurve EaseOutExpo = new Cubic(0.19, 1.0, 0.22, 1.0);

    /// <summary>円弧状のease out。</summary>
    public static readonly ICurve EaseOutCirc = new Cubic(0.075, 0.82, 0.165, 1.0);

    /// <summary>行き過ぎてから戻るease out(オーバーシュートあり)。</summary>
    public static readonly ICurve EaseOutBack = new Cubic(0.175, 0.885, 0.32, 1.275);

    /// <summary>ゆっくり始まりゆっくり終わる標準的なease in out。</summary>
    public static readonly ICurve EaseInOut = new Cubic(0.42, 0.0, 0.58, 1.0);

    /// <summary>サイン波状のease in out。</summary>
    public static readonly ICurve EaseInOutSine = new Cubic(0.445, 0.05, 0.55, 0.95);

    /// <summary>2次関数状のease in out。</summary>
    public static readonly ICurve EaseInOutQuad = new Cubic(0.455, 0.03, 0.515, 0.955);

    /// <summary>3次関数状のease in out。</summary>
    public static readonly ICurve EaseInOutCubic = new Cubic(0.645, 0.045, 0.355, 1.0);

    /// <summary>中央を強調した3次のease in out(Material強調イージング)。</summary>
    public static readonly ICurve EaseInOutCubicEmphasized = new ThreePointCubic(
        new Offset(0.05, 0.0),
        new Offset(0.133333, 0.06),
        new Offset(0.166666, 0.4),
        new Offset(0.208333, 0.82),
        new Offset(0.25, 1.0));

    /// <summary>4次関数状のease in out。</summary>
    public static readonly ICurve EaseInOutQuart = new Cubic(0.77, 0.0, 0.175, 1.0);

    /// <summary>5次関数状のease in out。</summary>
    public static readonly ICurve EaseInOutQuint = new Cubic(0.86, 0.0, 0.07, 1.0);

    /// <summary>指数関数状のease in out。</summary>
    public static readonly ICurve EaseInOutExpo = new Cubic(1.0, 0.0, 0.0, 1.0);

    /// <summary>円弧状のease in out。</summary>
    public static readonly ICurve EaseInOutCirc = new Cubic(0.785, 0.135, 0.15, 0.86);

    /// <summary>両端で行き過ぎるease in out(オーバーシュートあり)。</summary>
    public static readonly ICurve EaseInOutBack = new Cubic(0.68, -0.55, 0.265, 1.55);

    /// <summary>素早く動き出しゆっくり止まるMaterialの標準イージング。</summary>
    public static readonly ICurve FastOutSlowIn = new Cubic(0.4, 0.0, 0.2, 1.0);

    /// <summary>中央がゆっくりになるカーブ。</summary>
    public static readonly ICurve SlowMiddle = new Cubic(0.15, 0.85, 0.85, 0.15);

    /// <summary>跳ねながら立ち上がるバウンスIn。</summary>
    public static readonly ICurve BounceIn = new BounceInCurve();

    /// <summary>終端で跳ねて静止するバウンスOut。</summary>
    public static readonly ICurve BounceOut = new BounceOutCurve();

    /// <summary>両端で跳ねるバウンスInOut。</summary>
    public static readonly ICurve BounceInOut = new BounceInOutCurve();

    /// <summary>ゴムひものように振動しながら立ち上がる弾性In。</summary>
    public static readonly ICurve ElasticIn = new ElasticInCurve();

    /// <summary>行き過ぎてから振動して収束する弾性Out。</summary>
    public static readonly ICurve ElasticOut = new ElasticOutCurve();

    /// <summary>両端で弾性的に振動する弾性InOut。</summary>
    public static readonly ICurve ElasticInOut = new ElasticInOutCurve();
}
