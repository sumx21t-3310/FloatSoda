namespace FloatSoda.Geometrics;

/// <summary>論理ピクセル単位の幅と高さを表す不変値です。</summary>
/// <param name="Width">幅。</param>
/// <param name="Height">高さ。</param>
public readonly record struct Size(double Width, double Height);
