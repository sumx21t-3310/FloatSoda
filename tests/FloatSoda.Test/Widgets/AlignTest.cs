using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class AlignTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    public void Factor_負数またはNaNを指定_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Align { WidthFactor = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new Align { HeightFactor = value });
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RenderPositionedBox { WidthFactor = value }
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new RenderPositionedBox { HeightFactor = value }
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.PositiveInfinity)]
    public void Factor_0または正の無限大を指定_受け入れる(double value)
    {
        var renderObject = new Align
        {
            WidthFactor = value,
            HeightFactor = value,
        }.CreateRenderObject();

        Assert.Equal(value, renderObject.WidthFactor);
        Assert.Equal(value, renderObject.HeightFactor);
    }

    [Fact]
    public void WidthFactor_負数を指定_ArgumentOutOfRangeExceptionを投げる()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Align { WidthFactor = -1 });
    }
}
