namespace FloatSoda.Abstractions.Scheduling;

public interface IFramePacer
{
    /// <summary>
    /// 次のフレーム時刻まで待機します。
    /// </summary>
    void WaitForNextFrame(CancellationToken cancellationToken = default);
}
