using FloatSoda.Common.Geometries;
using FloatSoda.Common.Input;

namespace FloatSoda.Common.Test.Input;

public class PointerEventTest
{
    [Fact]
    public void DownAndMoveEventsAreDownByDefault()
    {
        Assert.True(new PointerDownEvent().Down);
        Assert.True(new PointerMoveEvent().Down);
    }

    [Fact]
    public void OtherEventsAreNotDownByDefault()
    {
        Assert.False(new PointerHoverEvent().Down);
        Assert.False(new PointerUpEvent().Down);
        Assert.False(new PointerCancelEvent().Down);
        Assert.False(new PointerAddedEvent().Down);
    }

    [Fact]
    public void RecordEqualityComparesByValue()
    {
        var a = new PointerDownEvent { Pointer = 1, Position = new Offset(10, 20), Buttons = PointerButtons.Primary };
        var b = new PointerDownEvent { Pointer = 1, Position = new Offset(10, 20), Buttons = PointerButtons.Primary };
        Assert.Equal(a, b);
    }

    [Fact]
    public void WithExpressionPreservesOtherProperties()
    {
        var move = new PointerMoveEvent { Pointer = 2, Kind = PointerDeviceKind.Controller, Position = new Offset(1, 1) };
        var next = move with { Position = new Offset(2, 2), Delta = new Offset(1, 1) };

        Assert.Equal(2, next.Pointer);
        Assert.Equal(PointerDeviceKind.Controller, next.Kind);
        Assert.True(next.Down);
        Assert.Equal(new Offset(2, 2), next.Position);
    }
}
