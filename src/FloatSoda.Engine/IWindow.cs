using FloatSoda.Common.Layer;

namespace FloatSoda.Engine;

public interface IWindow : IDisposable
{
    string Key { get; }
    ILayer Root { get; set; }

    void Update();

    /// <summary>
    /// 描画先テクスチャ／サーフェスのサイズを変更します。レンダースレッド上で呼ぶ必要があります。
    /// </summary>
    void Resize(int width, int height);
}