using System.Diagnostics;
using Valve.VR;

namespace FloatSoda.Engine;

public interface IFrameLimiter
{
    void Wait();
}

public class OpenVRFrameLimiter : IFrameLimiter
{
    private readonly TrackedDevicePose_t[] _renderPoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
    private readonly TrackedDevicePose_t[] _gamePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

    /// <summary>
    /// WaitGetPoses後のレンダリング用ポーズ（現在フレーム向け予測済み）
    /// </summary>
    public ReadOnlySpan<TrackedDevicePose_t> RenderPoses => _renderPoses;

    /// <summary>
    /// ゲームロジック用ポーズ（物理演算・AI等に使う）
    /// </summary>
    public ReadOnlySpan<TrackedDevicePose_t> GamePoses => _gamePoses;

    public void Wait()
    {
        var error = OpenVR.Compositor.WaitGetPoses(_renderPoses, _gamePoses);

        if (error != EVRCompositorError.None)
        {
            // DoNotHaveFocus は一時的な状態なので警告どまりでOK
            // それ以外は深刻なエラーとして扱う
            if (error == EVRCompositorError.DoNotHaveFocus)
                Console.WriteLine($"[OpenVR] Compositor warning: {error}");
            else
                throw new InvalidOperationException($"[OpenVR] WaitGetPoses failed: {error}");
        }
    }
}

public class FrameLimiter(int targetFrameRate = 30) : IFrameLimiter
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    // 1フレームあたりの目標時間を「ティック単位」で計算
    private readonly double _targetTicksPerFrame = Stopwatch.Frequency / (double)targetFrameRate;

    public void Wait()
    {
        // 1. 前回のSyncから経過した時間を取得
        double elapsedTicks = _stopwatch.ElapsedTicks;

        // 2. 待機が必要か判定
        double ticksToWait = _targetTicksPerFrame - elapsedTicks;

        if (ticksToWait > 0)
        {
            // 残り時間が長い場合は、CPU負荷を下げるために Sleep を使う
            // ただし、Sleepの誤差を考慮して「1ミリ秒前」には切り上げる
            int msToSleep = (int)((ticksToWait / Stopwatch.Frequency) * 1000) - 1;

            if (msToSleep > 0)
            {
                Thread.Sleep(msToSleep);
            }

            // 3. 【重要】最後の微調整。目標時間に達するまでループで待機（高精度）
            while (_stopwatch.ElapsedTicks < _targetTicksPerFrame)
            {
                // Thread.Yield() は他のスレッドに処理権を譲りつつ、即座に戻ってくる
                Thread.Yield();
            }
        }

        // 4. ストップウォッチをリセットして再スタート
        _stopwatch.Restart();
    }
}