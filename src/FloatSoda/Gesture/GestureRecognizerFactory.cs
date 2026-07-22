namespace FloatSoda.Gesture;

/// <summary>
/// 認識器インスタンスの生成と設定（コールバック注入）を分離するファクトリの基底。
/// Widgetは再構築のたびに作り直されるため、再構築をまたいで認識器インスタンスを温存しつつ
/// コールバックだけ差し替えるためにこの分離が要る。
/// </summary>
public abstract class GestureRecognizerFactory
{
    /// <summary>新しい認識器を生成する。</summary>
    /// <returns>このファクトリが生成した未設定のジェスチャ認識器。</returns>
    public abstract GestureRecognizer ConstructRaw();

    /// <summary>既存／新規の認識器へコールバック等を設定する。</summary>
    /// <param name="recognizer">このファクトリと同じ認識器型のインスタンス。</param>
    public abstract void InitializeRaw(GestureRecognizer recognizer);
}

/// <summary>コンストラクタと初期化子のデリゲートで認識器を組み立てる汎用ファクトリ。</summary>
/// <typeparam name="T">生成する認識器の型。</typeparam>
public sealed class GestureRecognizerFactory<T> : GestureRecognizerFactory where T : GestureRecognizer
{
    /// <summary>新しい認識器を生成するデリゲート。</summary>
    private readonly Func<T> _constructor;

    /// <summary>認識器へ最新のコールバック設定を適用するデリゲート。</summary>
    private readonly Action<T> _initializer;

    /// <summary>認識器の生成処理と初期化処理を指定してファクトリを初期化します。</summary>
    /// <param name="constructor">認識器を生成するデリゲート。</param>
    /// <param name="initializer">生成済み認識器へコールバックを設定するデリゲート。</param>
    public GestureRecognizerFactory(Func<T> constructor, Action<T> initializer)
    {
        _constructor = constructor;
        _initializer = initializer;
    }

    /// <inheritdoc />
    public override GestureRecognizer ConstructRaw() => _constructor();

    /// <inheritdoc />
    /// <exception cref="InvalidCastException"><paramref name="recognizer"/>が<typeparamref name="T"/>ではない場合。</exception>
    public override void InitializeRaw(GestureRecognizer recognizer) => _initializer((T)recognizer);
}
