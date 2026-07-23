using SkiaSharp;

namespace FloatSoda.Geometrics;

/// <summary>矩形の四隅に適用する角丸半径を表す不変値です。</summary>
/// <param name="TopLeft">左上隅の水平半径と垂直半径。既定値は0です。</param>
/// <param name="TopRight">右上隅の水平半径と垂直半径。既定値は0です。</param>
/// <param name="BottomRight">右下隅の水平半径と垂直半径。既定値は0です。</param>
/// <param name="BottomLeft">左下隅の水平半径と垂直半径。既定値は0です。</param>
/// <seealso cref="Radius"/>
public readonly record struct BorderRadius(
    Radius TopLeft = default,
    Radius TopRight = default,
    Radius BottomRight = default,
    Radius BottomLeft = default)
{
    /// <summary>四隅へ同じ角丸半径を設定した値を作成します。</summary>
    /// <param name="radius">四隅へ適用する半径。</param>
    /// <returns>四隅が同じ半径の新しい値。</returns>
    public static BorderRadius All(Radius radius) => new(radius, radius, radius, radius);
    /// <summary>すべての半径が0の角丸値を取得します。</summary>
    public static readonly BorderRadius Zero = All(Radius.Zero);

    /// <summary>四隅へ同じ円形半径を設定した値を作成します。</summary>
    /// <param name="radius">水平および垂直方向へ適用する論理ピクセル単位の半径。</param>
    /// <returns>四隅が同じ円形半径の新しい値。</returns>
    public static BorderRadius Circular(float radius) => All(Radius.Circular(radius));


    /// <summary>指定された矩形へこの角丸半径を適用した描画用矩形を作成します。</summary>
    /// <param name="rect">角丸を適用する矩形。半径と同じ座標単位を使用します。</param>
    /// <returns>各隅に対応する半径を持つ新しい描画用矩形。</returns>
    public SKRoundRect ToRoundRect(SKRect rect)
    {
        var roundRect = new SKRoundRect(rect);

        roundRect.SetRectRadii(rect, [TopLeft, TopRight, BottomRight, BottomLeft]);

        return roundRect;
    }
}

/// <summary>角丸の水平半径と垂直半径を表す値です。</summary>
/// <param name="X">水平方向の半径。単位は論理ピクセルで、既定値は0です。</param>
/// <param name="Y">垂直方向の半径。単位は論理ピクセルで、既定値は0です。</param>
/// <seealso cref="BorderRadius"/>
public record struct Radius(float X = 0, float Y = 0)
{
    /// <summary>水平半径と垂直半径が等しい円形半径を作成します。</summary>
    /// <param name="radius">両方向へ適用する論理ピクセル単位の半径。</param>
    /// <returns>両方向が等しい新しい半径。</returns>
    public static Radius Circular(float radius) => new(radius, radius);
    /// <summary>水平半径と垂直半径が0の値を取得します。</summary>
    public static readonly Radius Zero = new(0, 0);
    /// <summary>半径を同じ成分を持つ描画用座標へ変換します。</summary>
    /// <param name="radius">変換する半径。</param>
    /// <returns>水平半径をX、垂直半径をYに保持する座標。</returns>
    public static implicit operator SKPoint(Radius radius) => new(radius.X, radius.Y);
}