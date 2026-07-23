using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Geometrics;

/// <summary>矩形の各辺から内側へ確保する余白を表す不変値です。</summary>
/// <param name="Left">左辺の余白。単位は論理ピクセルで、既定値は0です。</param>
/// <param name="Top">上辺の余白。単位は論理ピクセルで、既定値は0です。</param>
/// <param name="Right">右辺の余白。単位は論理ピクセルで、既定値は0です。</param>
/// <param name="Bottom">下辺の余白。単位は論理ピクセルで、既定値は0です。</param>
/// <seealso cref="Offset"/>
public readonly record struct EdgeInsets(double Left = 0, double Top = 0, double Right = 0, double Bottom = 0)
{
    /// <summary>すべての辺の余白が0の値を取得します。</summary>
    public static readonly EdgeInsets Zero = All(0);
    /// <summary>すべての辺へ同じ余白を設定した値を作成します。</summary>
    /// <param name="value">各辺へ適用する論理ピクセル単位の余白。</param>
    /// <returns>すべての辺が同じ余白の新しい値。</returns>
    public static EdgeInsets All(double value) => new(value, value, value, value);

    /// <summary>垂直方向と水平方向でそれぞれ共通の余白を設定した値を作成します。</summary>
    /// <param name="vertical">上辺と下辺へ適用する論理ピクセル単位の余白。既定値は0です。</param>
    /// <param name="horizontal">左辺と右辺へ適用する論理ピクセル単位の余白。既定値は0です。</param>
    /// <returns>軸ごとに対称な余白を持つ新しい値。</returns>
    public static EdgeInsets Symmetric(double vertical = 0, double horizontal = 0) => new()
    {
        Top = vertical,
        Bottom = vertical,
        Left = horizontal,
        Right = horizontal
    };

    /// <summary>左辺と上辺の余白をオフセットとして取得します。</summary>
    public Offset TopLeft => new(Left, Top);
    /// <summary>右辺と上辺の余白をオフセットとして取得します。</summary>
    public Offset TopRight => new(Right, Top);
    /// <summary>左辺と下辺の余白をオフセットとして取得します。</summary>
    public Offset BottomLeft => new(Left, Bottom);
    /// <summary>右辺と下辺の余白をオフセットとして取得します。</summary>
    public Offset BottomRight => new(Right, Bottom);
    
    /// <summary>左辺と右辺、および上辺と下辺を入れ替えた余白を取得します。</summary>
    public EdgeInsets Flipped => new(Right, Bottom, Left, Top);
}