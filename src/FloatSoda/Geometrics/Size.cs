namespace FloatSoda.Geometrics;

/// <summary>論理ピクセル単位の幅と高さを表す不変値です。</summary>
/// <param name="Width">幅</param>
/// <param name="Height">高さ</param>
public readonly record struct Size(double Width, double Height)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="sizeI"></param>
    /// <returns></returns>
    public static implicit operator Size(SizeI sizeI) => new(sizeI.Width, sizeI.Height);

    /// <summary>
    ///
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static implicit operator SizeI(Size size) => new((int)size.Width, (int)size.Height);
}

/// <summary>論理ピクセル単位の幅と高さを表す不変値です。</summary>
/// <param name="Width">幅</param>
/// <param name="Height">高さ</param>
public readonly record struct SizeI(int Width, int Height)
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="sizeI"></param>
    /// <returns></returns>
    public static implicit operator Size(SizeI sizeI) => new(sizeI.Width, sizeI.Height);

    /// <summary>
    ///
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static implicit operator SizeI(Size size) => new((int)size.Width, (int)size.Height);
}
