using SkiaSharp;

namespace FloatSoda.Common.Geometries;

public readonly record struct Matrix(
    float M11, float M12, float M13,
    float M21, float M22, float M23,
    float M31, float M32, float M33)
{
    public static implicit operator SKMatrix(Matrix m) => new()
    {
        Values =
        [
            m.M11, m.M12, m.M13,
            m.M21, m.M22, m.M23,
            m.M31, m.M32, m.M33
        ]
    };
}

