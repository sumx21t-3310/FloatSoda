using FloatSoda.Animation;

namespace FloatSoda.Test.Animation;

public class CurveTest
{
    // 端点を保証すべき代表カーブ(単調・非オーバーシュート系)
    public static TheoryData<ICurve> EndpointCurves =>
    [
        Curves.Linear,
        Curves.Decelerate,
        Curves.Ease,
        Curves.EaseIn,
        Curves.EaseOut,
        Curves.EaseInOut,
        Curves.EaseInOutCubicEmphasized,
        Curves.FastOutSlowIn,
        Curves.FastEaseInToSlowEaseOut,
        Curves.BounceIn,
        Curves.BounceOut,
        Curves.BounceInOut,
        Curves.ElasticIn,
        Curves.ElasticOut,
        Curves.ElasticInOut,
        new SawTooth(3),
        new Interval(0.25, 0.75),
        new Threshold(0.5),
    ];

    [Theory]
    [MemberData(nameof(EndpointCurves))]
    public void Transform_PreservesEndpoints(ICurve curve)
    {
        Assert.Equal(0.0, curve.Transform(0.0), precision: 10);
        Assert.Equal(1.0, curve.Transform(1.0), precision: 10);
    }

    [Fact]
    public void Linear_IsIdentity()
    {
        var curve = Curves.Linear;
        Assert.Equal(0.25, curve.Transform(0.25), precision: 10);
        Assert.Equal(0.5, curve.Transform(0.5), precision: 10);
        Assert.Equal(0.75, curve.Transform(0.75), precision: 10);
    }

    [Fact]
    public void EaseInOut_IsSymmetricAroundMidpoint()
    {
        var curve = Curves.EaseInOut;
        Assert.Equal(0.5, curve.Transform(0.5), precision: 6);
        // 点対称: f(t) + f(1-t) == 1
        Assert.Equal(1.0, curve.Transform(0.3) + curve.Transform(0.7), precision: 3);
    }

    [Fact]
    public void Cubic_MatchesKnownValue()
    {
        // fastOutSlowIn = Cubic(0.4, 0.0, 0.2, 1.0)。Flutter既知値と突き合わせ。
        var curve = new Cubic(0.4, 0.0, 0.2, 1.0);
        Assert.Equal(0.237, curve.Transform(0.25), precision: 2);
        Assert.Equal(0.776, curve.Transform(0.5), precision: 2);
    }

    [Fact]
    public void Cubic_IsMonotonicForEaseCurves()
    {
        var curve = Curves.EaseInOut;
        var previous = 0.0;
        for (var i = 1; i <= 100; i++)
        {
            var value = curve.Transform(i / 100.0);
            Assert.True(value >= previous - 1e-9, $"non-monotonic at t={i / 100.0}");
            previous = value;
        }
    }

    [Fact]
    public void Flipped_IsTimeAndValueReversedCurve()
    {
        var curve = Curves.EaseIn;
        var flipped = ((Curve)curve).Flipped;
        foreach (var t in new[] { 0.1, 0.3, 0.5, 0.7, 0.9 })
        {
            Assert.Equal(1.0 - curve.Transform(1.0 - t), flipped.Transform(t), precision: 10);
        }
    }

    [Fact]
    public void SawTooth_RepeatsRisingRamp()
    {
        var curve = new SawTooth(2);
        Assert.Equal(0.5, curve.Transform(0.25), precision: 10);
        // 0.5直後は次の歯の立ち上がりへ戻る
        Assert.Equal(0.0, curve.Transform(0.5), precision: 10);
        Assert.Equal(0.5, curve.Transform(0.75), precision: 10);
    }

    [Fact]
    public void Interval_ClampsOutsideAndMapsInside()
    {
        var curve = new Interval(0.25, 0.75);
        Assert.Equal(0.0, curve.Transform(0.1), precision: 10);
        Assert.Equal(0.0, curve.Transform(0.25), precision: 10);
        Assert.Equal(0.5, curve.Transform(0.5), precision: 10);
        Assert.Equal(1.0, curve.Transform(0.75), precision: 10);
        Assert.Equal(1.0, curve.Transform(0.9), precision: 10);
    }

    [Fact]
    public void Threshold_StepsAtThreshold()
    {
        var curve = new Threshold(0.5);
        Assert.Equal(0.0, curve.Transform(0.49), precision: 10);
        Assert.Equal(1.0, curve.Transform(0.5), precision: 10);
        Assert.Equal(1.0, curve.Transform(0.6), precision: 10);
    }

    [Fact]
    public void BounceOut_OvershootsThenSettlesAtOne()
    {
        var curve = Curves.BounceOut;
        // 途中で1を超えないが、終端で1に収束する
        Assert.True(curve.Transform(0.5) < 1.0);
        Assert.Equal(1.0, curve.Transform(1.0), precision: 10);
    }

    [Fact]
    public void ElasticOut_SettlesAtOne()
    {
        var curve = Curves.ElasticOut;
        Assert.Equal(1.0, curve.Transform(1.0), precision: 10);
        // 途中でオーバーシュートして1を超える瞬間がある
        Assert.True(curve.Transform(0.2) > 1.0);
    }
}
