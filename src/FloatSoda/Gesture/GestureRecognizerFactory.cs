namespace FloatSoda.Gesture;

/// <summary>
/// 認識器インスタンスの生成と設定（コールバック注入）を分離するファクトリの基底。
/// Widget は毎フレーム作り直されるため、rebuild を跨いで認識器インスタンスを温存しつつ
/// コールバックだけ差し替えるためにこの分離が要る。
/// </summary>
public abstract class GestureRecognizerFactory
{
    /// <summary>新しい認識器を生成する。</summary>
    public abstract GestureRecognizer ConstructRaw();

    /// <summary>既存／新規の認識器へコールバック等を設定する。</summary>
    public abstract void InitializeRaw(GestureRecognizer recognizer);
}

/// <summary>コンストラクタと初期化子のデリゲートで認識器を組み立てる汎用ファクトリ。</summary>
/// <typeparam name="T">生成する認識器の型。</typeparam>
public sealed class GestureRecognizerFactory<T> : GestureRecognizerFactory where T : GestureRecognizer
{
    private readonly Func<T> _constructor;
    private readonly Action<T> _initializer;

    /// <param name="constructor">認識器を生成するデリゲート。</param>
    /// <param name="initializer">生成済み認識器へコールバックを設定するデリゲート。</param>
    public GestureRecognizerFactory(Func<T> constructor, Action<T> initializer)
    {
        _constructor = constructor;
        _initializer = initializer;
    }

    public override GestureRecognizer ConstructRaw() => _constructor();

    public override void InitializeRaw(GestureRecognizer recognizer) => _initializer((T)recognizer);
}
