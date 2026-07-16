namespace FloatSoda.Abstractions.Geometries;

/// <summary>
/// 角度を表す値型。内部的にはラジアンで保持します。
/// </summary>
public readonly record struct Angle(float Radians)
{
    /// <summary>角度ゼロを表します。</summary>
    public static Angle Zero => new(0f);

    /// <summary>ラジアンから生成します。</summary>
    public static Angle FromRadians(float radians) => new(radians);

    /// <summary>度数から生成します。</summary>
    public static Angle FromDegrees(float degrees) => new(degrees * MathF.PI / 180f);

    /// <summary>度数へ変換した値です。</summary>
    public float Degrees => Radians * 180f / MathF.PI;
}
