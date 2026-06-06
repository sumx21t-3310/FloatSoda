using SkiaSharp;

namespace FloatSoda.Common.Geometries;

public static class GeometryExtension
{
    extension(SKRect rect)
    {
        public SKRect MakeOffset(float dx, float dy) => new()
        {
            Top = rect.Top + dy,
            Bottom = rect.Bottom + dy,
            Left = rect.Right + dx,
            Right = rect.Right + dx,
        };

        public SKRect MakeOffset(SKPoint offset) => rect.MakeOffset(offset.X, offset.Y);

        public SKRect RoundOut() => new()
        {
            Top = MathF.Round(rect.Top),
            Bottom = MathF.Round(rect.Bottom),
            Left = MathF.Round(rect.Left),
            Right = MathF.Round(rect.Right)
        };
    }
}