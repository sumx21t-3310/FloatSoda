using SkiaSharp;

namespace FloatSoda.Abstractions.Geometries;

/// <summary>幾何型の移動および範囲判定を補助する拡張メソッドを提供します。</summary>
public static class GeometryExtension
{
    /// <summary>角の半径を維持したまま、丸角矩形を指定量だけ移動した新しい値を作成します。</summary>
    /// <param name="roundRect">移動元の丸角矩形。このオブジェクト自体は変更されません。</param>
    /// <param name="dx">水平方向の移動量。単位は丸角矩形の座標系に従います。</param>
    /// <param name="dy">垂直方向の移動量。単位は丸角矩形の座標系に従います。</param>
    /// <returns>指定量だけ移動し、元と同じ角半径を持つ丸角矩形。</returns>
    public static SKRoundRect MakeOffset(this SKRoundRect roundRect, float dx, float dy)
    {
        var rect = roundRect.Rect;
        rect.Offset(dx, dy);

        var copy = new SKRoundRect();
        copy.SetRectRadii(rect, roundRect.Radii);

        return copy;
    }

    /// <summary>角の半径を維持したまま、丸角矩形を指定量だけ移動した新しい値を作成します。</summary>
    /// <param name="roundRect">移動元の丸角矩形。このオブジェクト自体は変更されません。</param>
    /// <param name="offset">水平および垂直方向の移動量。単位は丸角矩形の座標系に従います。</param>
    /// <returns>指定量だけ移動し、元と同じ角半径を持つ丸角矩形。</returns>
    public static SKRoundRect MakeOffset(this SKRoundRect roundRect, Offset offset) =>
        roundRect.MakeOffset((float)offset.X, (float)offset.Y);

    /// <summary>原点を左上とするサイズの範囲内に指定座標が含まれるかを判定します。</summary>
    /// <param name="size">判定する範囲の幅と高さ。</param>
    /// <param name="offset">判定する座標。単位はサイズと同じである必要があります。</param>
    /// <returns>座標が左端と上端を含み、右端と下端を含まない範囲内なら<see langword="true"/>、それ以外は<see langword="false"/>。</returns>
    public static bool Contains(this SKSize size, Offset offset)
    {
        return offset.X >= 0 && offset.X < size.Width && offset.Y >= 0 && offset.Y < size.Height;
    }
}
