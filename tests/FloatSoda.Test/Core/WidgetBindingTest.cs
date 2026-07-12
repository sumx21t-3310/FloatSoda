using FloatSoda.Common.Geometries;
using FloatSoda.Core;
using FloatSoda.Engine;

namespace FloatSoda.Test.Core;

public class WidgetBindingTest
{
    /// <summary>
    /// レンダースレッドを起動せずに初期化したWidgetBindingを作る。
    /// PostTaskはキューに積むだけなのでオーバーレイ生成（GL/OpenVR）は走らない。
    /// </summary>
    private static WidgetBinding CreateInitializedBinding()
    {
        var binding = new WidgetBinding();
        var runner = new RenderThreadRunner("TestRenderThread", new FrameLimiter());

        binding.EnsureInitialized("test_window", runner,
            _ => throw new InvalidOperationException("オーバーレイ生成はテストでは呼ばれない想定"),
            Dpm.Default);

        return binding;
    }

    [Fact]
    public void BeginFrame_PassesTimestampToScheduledCallback()
    {
        var binding = CreateInitializedBinding();
        TimeSpan? received = null;

        binding.ScheduleFrameCallback(elapsed => received = elapsed);

        var timestamp = TimeSpan.FromMilliseconds(16.6);
        binding.BeginFrame(timestamp);

        Assert.Equal(timestamp, received);
    }

    [Fact]
    public void BeginFrame_ClearsCallbacksAfterInvocation()
    {
        var binding = CreateInitializedBinding();
        var callCount = 0;

        binding.ScheduleFrameCallback(_ => callCount++);

        binding.BeginFrame(TimeSpan.FromMilliseconds(10));
        binding.BeginFrame(TimeSpan.FromMilliseconds(20));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void BeginFrame_DoesNotInvokeCancelledCallback()
    {
        var binding = CreateInitializedBinding();
        var invoked = false;

        var id = binding.ScheduleFrameCallback(_ => invoked = true);
        binding.CancelFrameCallback(id);

        binding.BeginFrame(TimeSpan.FromMilliseconds(10));

        Assert.False(invoked);
    }

    [Fact]
    public void BeginFrame_DoesNothingWhenUninitialized()
    {
        var binding = new WidgetBinding();
        var invoked = false;

        binding.ScheduleFrameCallback(_ => invoked = true);

        binding.BeginFrame(TimeSpan.FromMilliseconds(10));

        Assert.False(invoked);
    }
}
