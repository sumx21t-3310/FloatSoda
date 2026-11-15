namespace FloatSoda.Geometrics;

/// <summary>論理ピクセル単位の幅と高さを表す不変値です。</summary>
/// <param name="Width">幅</param>
/// <param name="Height">高さ</param>
/// <seealso cref="SizeI"/>
public readonly record struct Size(double Width, double Height);

/// <summary>論理ピクセル単位の幅と高さを表す不変値です。</summary>
/// <param name="Width">幅</param>
/// <param name="Height">高さ</param>
/// <remarks>
/// <see cref="Size"/>との変換演算子は、この型にだけ定義します。
/// 両方の型に同じ変換を定義すると、変換する箇所で候補が2つになり、CS0457でコンパイルできなくなるためです。
/// </remarks>
/// <seealso cref="Size"/>
public readonly record struct SizeI(int Width, int Height)
{
    /// <summary>整数の幅と高さを、そのまま<see cref="Size"/>に変換します。</summary>
    /// <param name="sizeI">変換する大きさ。</param>
    /// <returns><paramref name="sizeI"/>と同じ幅と高さを持つ<see cref="Size"/>。値は失われません。</returns>
    public static implicit operator Size(SizeI sizeI) => new(sizeI.Width, sizeI.Height);

    /// <summary><see cref="Size"/>の幅と高さを、整数に変換します。</summary>
    /// <param name="size">変換する大きさ。</param>
    /// <returns>
    /// 幅と高さの小数部を0の方向へ切り捨てた<see cref="SizeI"/>。
    /// <see cref="int"/>の範囲外の値とNaNの結果は、C#の<c>(int)</c>キャストの規則に従います。
    /// </returns>
    public static implicit operator SizeI(Size size) => new((int)size.Width, (int)size.Height);
}
