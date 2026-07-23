using SkiaSharp;

namespace FloatSoda.Abstractions.Geometries;

/// <summary>二次元座標または移動量を倍精度の成分で表します。</summary>
/// <param name="X">水平方向の成分。単位は利用する座標系に従い、既定値は0です。</param>
/// <param name="Y">垂直方向の成分。単位は利用する座標系に従い、既定値は0です。</param>
/// <seealso cref="SKPoint"/>
public readonly record struct Offset(double X = 0, double Y = 0)
{
    /// <summary>両方の成分が0の座標または移動量を取得します。</summary>
    public static Offset Zero => default;
    /// <summary>両方の成分が1の座標または移動量を取得します。</summary>
    public static Offset One => new(1, 1);

    /// <summary>二つの値の対応する成分を加算します。</summary>
    /// <param name="a">加算される値。</param>
    /// <param name="b">加算する値。</param>
    /// <returns>両方の値の対応する成分を加算した結果。</returns>
    public static Offset operator +(Offset a, Offset b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>一つ目の値から二つ目の値の対応する成分を減算します。</summary>
    /// <param name="a">減算される値。</param>
    /// <param name="b">減算する値。</param>
    /// <returns>一つ目の値から二つ目の値の対応する成分を減算した結果。</returns>
    public static Offset operator -(Offset a, Offset b) => new(a.X - b.X, a.Y - b.Y);
    /// <summary>各成分の符号を反転します。</summary>
    /// <param name="a">符号を反転する値。</param>
    /// <returns>各成分の符号を反転した結果。</returns>
    public static Offset operator -(Offset a) => new(-a.X, -a.Y);

    /// <summary>各成分を指定された倍率で拡大または縮小します。</summary>
    /// <param name="a">拡大または縮小する値。</param>
    /// <param name="b">各成分に乗算する倍率。</param>
    /// <returns>各成分に倍率を乗算した結果。</returns>
    public static Offset operator *(Offset a, double b) => new(a.X * b, a.Y * b);
    /// <summary>指定された倍率で各成分を拡大または縮小します。</summary>
    /// <param name="a">各成分に乗算する倍率。</param>
    /// <param name="b">拡大または縮小する値。</param>
    /// <returns>各成分に倍率を乗算した結果。</returns>
    public static Offset operator *(double a, Offset b) => b * a;

    /// <summary>各成分を指定された除数で除算します。</summary>
    /// <param name="a">除算する値。</param>
    /// <param name="b">各成分を除算する除数。</param>
    /// <returns>各成分を除数で除算した結果。</returns>
    /// <remarks>除数が0の場合の結果は倍精度浮動小数点数の除算規則に従います。</remarks>
    public static Offset operator /(Offset a, double b) => new(a.X / b, a.Y / b);

    /// <summary><see cref="SKPoint"/>を同じ成分の値へ変換します。</summary>
    /// <param name="point">変換する単精度の座標。</param>
    /// <returns>各成分を倍精度で保持する変換結果。</returns>
    public static implicit operator Offset(SKPoint point) => new(point.X, point.Y);

    /// <summary>値を同じ成分の<see cref="SKPoint"/>へ変換します。</summary>
    /// <param name="offset">変換する倍精度の座標または移動量。</param>
    /// <returns>各成分を単精度へ変換した座標。</returns>
    /// <remarks>各成分は単精度へ変換されるため、精度が低下する場合があります。</remarks>
    public static implicit operator SKPoint(Offset offset) => new ((float)offset.X, (float)offset.Y);
}