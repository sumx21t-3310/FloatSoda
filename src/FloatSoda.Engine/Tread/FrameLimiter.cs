using System.Diagnostics;

namespace FloatSoda.Engine.Tread;

public class FrameLimiter(int targetFrameRate = 30)
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