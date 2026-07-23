namespace FloatSoda.Geometrics;

/// <summary>レイアウトの主方向となる軸を表します。</summary>
public enum Axis
{
    /// <summary>左から右へ延びる水平軸です。</summary>
    Horizontal,
    /// <summary>上から下へ延びる垂直軸です。</summary>
    Vertical
}

/// <summary><see cref="Axis"/>に関する判定と変換を提供します。</summary>
public static class AxisExtension
{
    /// <summary>水平軸と垂直軸を入れ替えます。</summary>
    /// <param name="axis">入れ替える軸。</param>
    /// <returns>水平軸の場合は垂直軸、垂直軸の場合は水平軸。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="axis"/>が定義済みの値ではありません。</exception>
    public static Axis Flip(this Axis axis) => axis switch
    {
        Axis.Horizontal => Axis.Vertical,
        Axis.Vertical => Axis.Horizontal,
        _ => throw new ArgumentOutOfRangeException(nameof(axis), axis, null)
    };

    /// <summary>指定された軸の開始側が上端または左端に当たるかを判定します。</summary>
    /// <param name="direction">判定する軸。</param>
    /// <param name="verticalDirection">垂直軸の進行方向。水平軸では結果に影響しません。</param>
    /// <returns>開始側が左端または上端の場合は<see langword="true"/>、下端の場合は<see langword="false"/>。</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="direction"/>または<paramref name="verticalDirection"/>が定義済みの値ではありません。</exception>
    public static bool StartIsTopLeft(this Axis direction, VerticalDirection verticalDirection)
    {
        return (direction) switch
        {
            Axis.Horizontal => true,
            Axis.Vertical => verticalDirection switch
            {
                VerticalDirection.Down => true,
                VerticalDirection.Up => false,
                _ => throw new ArgumentOutOfRangeException(nameof(verticalDirection), verticalDirection, null)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
    }
}

/// <summary>主軸上で子要素を配置する方法を表します。</summary>
public enum MainAxisAlignment
{
    /// <summary>子要素を主軸の開始側へ寄せます。</summary>
    Start,
    /// <summary>子要素を主軸の終了側へ寄せます。</summary>
    End,
    /// <summary>子要素を主軸の中央へ寄せます。</summary>
    Center,
    /// <summary>先頭と末尾を両端へ置き、子要素間に均等な空きを配置します。</summary>
    SpaceBetween,
    /// <summary>各子要素の前後に均等な空きを配置し、両端の空きを要素間の半分にします。</summary>
    SpaceAround,
    /// <summary>両端を含むすべての空きを均等に配置します。</summary>
    SpaceEvenly,
}

/// <summary>交差軸上で子要素を配置する方法を表します。</summary>
public enum CrossAxisAlignment
{
    /// <summary>子要素を交差軸の開始側へ寄せます。</summary>
    Start,
    /// <summary>子要素を交差軸の終了側へ寄せます。</summary>
    End,
    /// <summary>子要素を交差軸の中央へ寄せます。</summary>
    Center,
    /// <summary>子要素を交差軸の利用可能な大きさまで広げます。</summary>
    Stretch,
    /// <summary>子要素の文字基準線をそろえます。</summary>
    Baseline,
}

/// <summary>主軸方向にレイアウトが占める大きさを表します。</summary>
public enum MainAxisSize
{
    /// <summary>子要素を収めるために必要な最小の大きさを使用します。</summary>
    Min,
    /// <summary>主軸方向に利用可能な最大の大きさを使用します。</summary>
    Max,
}

/// <summary>垂直軸の進行方向を表します。</summary>
public enum VerticalDirection
{
    /// <summary>下端から上端へ進みます。</summary>
    Up,
    /// <summary>上端から下端へ進みます。</summary>
    Down,
}