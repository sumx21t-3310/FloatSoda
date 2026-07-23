using FloatSoda.Abstractions.Geometries;
using SkiaSharp;

namespace FloatSoda.Geometrics;

/// <summary>矩形内の位置を中心からの正規化された相対座標で表す不変値です。</summary>
/// <param name="X">水平方向の相対位置。-1は左端、0は中央、1は右端です。既定値は0です。</param>
/// <param name="Y">垂直方向の相対位置。-1は上端、0は中央、1は下端です。既定値は0です。</param>
/// <seealso cref="Offset"/>
public readonly record struct Alignment(float X = 0, float Y = 0)
{
    /// <summary>左上を表す配置値を取得します。</summary>
    public static readonly Alignment TopLeft = new(-1, -1);
    /// <summary>上端中央を表す配置値を取得します。</summary>
    public static readonly Alignment TopCenter = new(0, -1);
    /// <summary>右上を表す配置値を取得します。</summary>
    public static readonly Alignment TopRight = new(1, -1);
    /// <summary>左端中央を表す配置値を取得します。</summary>
    public static readonly Alignment CenterLeft = new(-1, 0);
    /// <summary>中央を表す配置値を取得します。</summary>
    public static readonly Alignment Center = default;
    /// <summary>右端中央を表す配置値を取得します。</summary>
    public static readonly Alignment CenterRight = new(1, 0);
    /// <summary>左下を表す配置値を取得します。</summary>
    public static readonly Alignment BottomLeft = new(-1, 1);
    /// <summary>下端中央を表す配置値を取得します。</summary>
    public static readonly Alignment BottomCenter = new(0, 1);
    /// <summary>右下を表す配置値を取得します。</summary>
    public static readonly Alignment BottomRight = new(1, 1);

    /// <summary>指定された矩形サイズの中心を原点として、この配置が示す位置を算出します。</summary>
    /// <param name="size">基準とする矩形の大きさ。単位は論理ピクセルです。</param>
    /// <returns>矩形中心から配置位置までの論理ピクセル単位のオフセット。</returns>
    public Offset Pivot(SKSize size)
    {
        var centerX = size.Width / 2f;
        var centerY = size.Height / 2f;

        var offsetX = centerX * X;
        var offsetY = centerY * Y;

        return new Offset(offsetX, offsetY);
    }

    /// <summary>親矩形内で子矩形をこの配置に置くための左上座標を算出します。</summary>
    /// <param name="parent">親矩形の大きさ。単位は論理ピクセルです。</param>
    /// <param name="child">子矩形の大きさ。単位は論理ピクセルです。</param>
    /// <returns>親矩形の左上を原点とする、子矩形左上の論理ピクセル単位のオフセット。</returns>
    public Offset ComputeOffset(SKSize parent, SKSize child)
    {
        var remainingSpace = new SKSize(parent.Width - child.Width, parent.Height - child.Height);
        var centerSpace = new Offset(remainingSpace.Width / 2f, remainingSpace.Height / 2f);
        return centerSpace + Pivot(remainingSpace);
    }

    /// <summary>水平および垂直方向を中央に対して反転した配置値を返します。</summary>
    /// <returns>両成分の符号を反転した新しい配置値。</returns>
    public Alignment FlipAll() => new(-X, -Y);
    /// <summary>水平方向を中央に対して反転した配置値を返します。</summary>
    /// <returns>水平成分の符号だけを反転した新しい配置値。</returns>
    public Alignment FlipX() => this with { X = -X };
    /// <summary>垂直方向を中央に対して反転した配置値を返します。</summary>
    /// <returns>垂直成分の符号だけを反転した新しい配置値。</returns>
    public Alignment FlipY() => this with { Y = -Y };
}