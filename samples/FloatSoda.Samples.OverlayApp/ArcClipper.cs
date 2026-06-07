using FloatSoda.Render.Painting;
using SkiaSharp;

namespace FloatSoda.Samples.OverlayApp;

public class ArcClipper : CustomClipper<SKPath>
{
    public override SKPath GetClip(SKSize size)
    {
        var path = new SKPath();

        path.LineTo(0f, size.Height - 30);
        var firstControlPoint = new SKPoint(size.Width / 4, size.Height);
        var firstPoint = new SKPoint(size.Width / 2, size.Height);
        
        path.QuadTo(firstControlPoint, firstPoint);
        
        var secondControlPoint = new SKPoint(size.Width - size.Width / 4, size.Height);
        var secondPoint = new SKPoint(size.Width, size.Height - 30);
        
        path.QuadTo(secondControlPoint, secondPoint);
        
        path.LineTo(size.Width, 0);
        
        path.Close();
        
        return path;
    }

    public override bool ShouldReclip(CustomClipper<SKPath> oldClipper) => false;
}