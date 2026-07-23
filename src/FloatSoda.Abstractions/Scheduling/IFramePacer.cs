namespace FloatSoda.Abstractions.Scheduling;

/// <summary>フレーム間隔を調整するための待機処理を提供します。</summary>
public interface IFramePacer
{
    /// <summary>
    /// 次のフレーム時刻まで待機します。
    /// </summary>
    /// <param name="cancellationToken">待機を中断するためのトークン。既定値では中断を要求しません。</param>
    void WaitForNextFrame(CancellationToken cancellationToken = default);
}
