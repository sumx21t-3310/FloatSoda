using System.Diagnostics;

namespace FloatSoda.Engine;

public class FrameTimer
{
    private readonly Stopwatch _stopwatch = new();
    public FrameTimer() => _stopwatch.Start();

    public float DeltaTime { get; private set; }

    public async Task WaitForNextFrame(float targetFrameRate = 60)
    {
        var targetMs = 1000.0 / targetFrameRate;

        var elapsedMs = _stopwatch.Elapsed.TotalMilliseconds;
        var delay = (int)(targetMs - elapsedMs);

        if (delay > 0)
        {
            await Task.Delay(delay);
        }

        DeltaTime = (float)(_stopwatch.Elapsed.TotalSeconds);
        _stopwatch.Restart();
    }
}