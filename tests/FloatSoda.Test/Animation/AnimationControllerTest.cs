using FloatSoda.Animation;

namespace FloatSoda.Test.Animation;

public class AnimationControllerTest
{
    private readonly FakeFrameScheduler _scheduler = new();
    private readonly TickerProvider _provider;

    public AnimationControllerTest()
    {
        _provider = new TickerProvider { ResolveScheduler = () => _scheduler };
    }

    private AnimationController CreateController(TimeSpan? duration = null) => new()
    {
        Vsync = _provider,
        Duration = duration ?? TimeSpan.FromSeconds(1)
    };

    [Fact]
    public void Forward_ProgressesLinearlyAndCompletes()
    {
        var controller = CreateController();
        var statuses = new List<AnimationStatus>();
        controller.StatusChanged += statuses.Add;

        controller.Forward();

        Assert.Equal([AnimationStatus.Forward], statuses);

        _scheduler.Pump(TimeSpan.Zero);
        Assert.Equal(0.0, controller.Value);

        _scheduler.Pump(TimeSpan.FromSeconds(0.5));
        Assert.Equal(0.5, controller.Value, precision: 10);

        _scheduler.Pump(TimeSpan.FromSeconds(1.5));
        Assert.Equal(1.0, controller.Value);
        Assert.Equal([AnimationStatus.Forward, AnimationStatus.Completed], statuses);

        // 完了後はTickerが止まり、以後のフレームで値が動かない
        Assert.Equal(0, _scheduler.PendingCount);
    }

    [Fact]
    public void Forward_RaisesChangedWhenValueChanges()
    {
        var controller = CreateController();
        var changedCount = 0;
        controller.Changed += () => changedCount++;

        controller.Forward();

        _scheduler.Pump(TimeSpan.Zero); // 初回Tickは値が変わらない(0のまま)
        _scheduler.Pump(TimeSpan.FromSeconds(0.25));
        _scheduler.Pump(TimeSpan.FromSeconds(0.5));

        Assert.Equal(2, changedCount);
    }

    [Fact]
    public void Reverse_ProgressesToLowerBoundAndDismisses()
    {
        var controller = CreateController();
        var statuses = new List<AnimationStatus>();
        controller.StatusChanged += statuses.Add;

        controller.Reverse(from: 1.0);

        _scheduler.Pump(TimeSpan.Zero);
        Assert.Equal(1.0, controller.Value);

        _scheduler.Pump(TimeSpan.FromSeconds(0.5));
        Assert.Equal(0.5, controller.Value, precision: 10);

        _scheduler.Pump(TimeSpan.FromSeconds(1.5));
        Assert.Equal(0.0, controller.Value);
        Assert.Equal([AnimationStatus.Reverse, AnimationStatus.Dismissed], statuses);
    }

    [Fact]
    public void Forward_FromMidpoint_ScalesRemainingDuration()
    {
        var controller = CreateController(TimeSpan.FromSeconds(1));

        controller.Forward(from: 0.5);

        _scheduler.Pump(TimeSpan.Zero);
        Assert.Equal(0.5, controller.Value);

        // 残り半分なので0.5秒(+ε)で完了する
        _scheduler.Pump(TimeSpan.FromSeconds(0.6));
        Assert.Equal(1.0, controller.Value);
        Assert.Equal(AnimationStatus.Completed, controller.Status);
    }

    [Fact]
    public void Forward_WhenAlreadyAtUpperBound_CompletesImmediately()
    {
        var controller = CreateController();
        var statuses = new List<AnimationStatus>();
        controller.StatusChanged += statuses.Add;

        controller.Forward(from: 1.0);

        // Tickerを回さず即完了する
        Assert.Equal([AnimationStatus.Completed], statuses);
        Assert.Equal(0, _scheduler.PendingCount);
    }

    [Fact]
    public void Stop_RetainsCurrentValue()
    {
        var controller = CreateController();

        controller.Forward();

        _scheduler.Pump(TimeSpan.Zero);
        _scheduler.Pump(TimeSpan.FromSeconds(0.3));
        Assert.Equal(0.3, controller.Value, precision: 10);

        controller.Stop();

        Assert.Equal(0, _scheduler.PendingCount);

        _scheduler.Pump(TimeSpan.FromSeconds(0.6));
        Assert.Equal(0.3, controller.Value, precision: 10);
        Assert.Equal(AnimationStatus.Forward, controller.Status); // Statusは進行方向のまま(Flutter同様)
    }

    [Fact]
    public void AnimateWith_DrivesValueFromCustomSimulation()
    {
        var controller = CreateController();

        controller.AnimateWith(new InterpolationSimulation(0.0, 1.0, TimeSpan.FromSeconds(2), new LinearCurve()));

        _scheduler.Pump(TimeSpan.Zero);
        _scheduler.Pump(TimeSpan.FromSeconds(1));
        Assert.Equal(0.5, controller.Value, precision: 10);

        _scheduler.Pump(TimeSpan.FromSeconds(2.1));
        Assert.Equal(1.0, controller.Value);
        Assert.Equal(AnimationStatus.Completed, controller.Status);
    }
}
