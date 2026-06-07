using FloatSoda.Geometrics;
using SkiaSharp;
using Xunit.Abstractions;

namespace FloatSoda.GeometryTest;


public class BoxConstraintsTest(ITestOutputHelper helper)
{
    [Fact]
    public void Tight()
    {
        var boxConstraint = BoxConstraints.Tight(100, 100);
        helper.WriteLine(boxConstraint.Loosen.Constrain(new SKSize(140, 100)).ToString());
    }

    [Fact]
    public void TightFor()
    {
        var boxConstraint = BoxConstraints.TightFor(100, 100);
        
        helper.WriteLine(boxConstraint.ToString());
    }
}