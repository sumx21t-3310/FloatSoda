namespace FloatSoda.Engine;

/// <summary>
/// エンジンの開始と終了をまとめるためのシェルです。
/// </summary>
public class EngineShell : IDisposable
{
    /// <summary>
    /// 指定したキャンセルトークンを使用してエンジンを開始します。
    /// </summary>
    /// <param name="token">エンジンの停止を通知するキャンセルトークン。</param>
    public void Start(CancellationToken token) {}


    /// <summary>
    /// エンジンシェルが保持するリソースを解放します。
    /// </summary>
    public void Dispose()
    {
        // TODO マネージリソースをここで解放します
    }
}
