using FloatSoda.Geometrics;
using SkiaSharp;
using Xunit.Abstractions;

namespace FloatSoda.GeometryTest;

public class AlignmentTest(ITestOutputHelper helper)
{
    [Fact]
    public void TopLeft()
    {
        List<Alignment> alignments =
        [
            Alignment.TopLeft,
            Alignment.TopCenter,
            Alignment.TopRight,
            Alignment.CenterLeft,
            Alignment.Center,
            Alignment.CenterRight,
            Alignment.BottomLeft,
            Alignment.BottomCenter,
            Alignment.BottomRight
        ];

        
        var parentSize = new SKSize(1000, 1000);
        var childSize = new SKSize(100, 100);

        foreach (var alignment in alignments)
        {
            var offset = alignment.ComputeOffset(parentSize, childSize);

            helper.WriteLine($"alignment: {alignment} offset: {offset}");
        }
    }
}