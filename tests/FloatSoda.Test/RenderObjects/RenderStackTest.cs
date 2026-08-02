using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderStackTest
{
    [Fact]
    public void PerformLayout_非Positioned子をAlignmentで配置する()
    {
        var child = Box(40, 20);
        var stack = new RenderStack
        {
            Alignment = Alignment.BottomRight,
            Children = { child }
        };

        stack.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new SKSize(200, 100), stack.Size);
        Assert.Equal(new SKSize(40, 20), child.Size);
        Assert.Equal(new Offset(160, 80), Assert.IsType<StackParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void PerformLayout_FitExpandで非Positioned子をStack全体へ広げる()
    {
        var child = Box(40, 20);
        var stack = new RenderStack
        {
            Fit = StackFit.Expand,
            Children = { child }
        };

        stack.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new SKSize(200, 100), child.Size);
        Assert.Equal(Offset.Zero, Assert.IsType<StackParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void PerformLayout_FitPassthroughで親の最小制約も子へ渡す()
    {
        var child = Box(40, 20);
        var stack = new RenderStack
        {
            Fit = StackFit.Passthrough,
            Children = { child }
        };

        stack.Layout(new BoxConstraints(80, 200, 60, 100));

        Assert.Equal(new SKSize(80, 60), child.Size);
        Assert.Equal(new SKSize(80, 60), stack.Size);
    }

    [Fact]
    public void PerformLayout_LeftRightとTopHeightからPositioned子を配置する()
    {
        var child = Box(10, 10);
        var stack = new RenderStack { Children = { child } };
        var parentData = Assert.IsType<StackParentData>(child.ParentData);
        parentData.Left = 10;
        parentData.Right = 20;
        parentData.Top = 5;
        parentData.Height = 30;

        stack.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new SKSize(170, 30), child.Size);
        Assert.Equal(new Offset(10, 5), parentData.Offset);
    }

    [Fact]
    public void PerformLayout_RightBottomと寸法からPositioned子を配置する()
    {
        var child = Box(10, 10);
        var stack = new RenderStack { Children = { child } };
        var parentData = Assert.IsType<StackParentData>(child.ParentData);
        parentData.Right = 15;
        parentData.Bottom = 10;
        parentData.Width = 50;
        parentData.Height = 20;

        stack.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new SKSize(50, 20), child.Size);
        Assert.Equal(new Offset(135, 70), parentData.Offset);
    }

    [Fact]
    public void PerformLayout_Positionedの未指定軸をAlignmentで配置する()
    {
        var child = Box(10, 10);
        var stack = new RenderStack
        {
            Alignment = Alignment.BottomRight,
            Children = { child }
        };
        var parentData = Assert.IsType<StackParentData>(child.ParentData);
        parentData.Left = 10;
        parentData.Width = 20;
        parentData.Height = 30;

        stack.Layout(BoxConstraints.Tight(200, 100));

        Assert.Equal(new Offset(10, 70), parentData.Offset);
    }

    [Fact]
    public void ApplyParentData_Positioned変更時に親をLayoutDirtyにする()
    {
        var child = Box(20, 20);
        var stack = new RenderStack { Children = { child } };
        var initial = new Positioned { Left = 10, Top = 5, Child = new SizedBox() };
        initial.ApplyParentData(child);
        stack.Layout(BoxConstraints.Tight(100, 80));
        Assert.False(stack.NeedsLayout);

        var updated = initial with { Left = 30 };
        updated.ApplyParentData(child);

        Assert.True(stack.NeedsLayout);
        stack.Layout(BoxConstraints.Tight(100, 80));
        Assert.Equal(new Offset(30, 5), Assert.IsType<StackParentData>(child.ParentData).Offset);
    }

    [Fact]
    public void ApplyParentData_水平軸を3項目指定するとInvalidOperationExceptionを投げる()
    {
        var child = Box(20, 20);
        var stack = new RenderStack { Children = { child } };
        var positioned = new Positioned
        {
            Left = 0,
            Right = 0,
            Width = 20,
            Child = new SizedBox()
        };

        Assert.Throws<InvalidOperationException>(() => positioned.ApplyParentData(child));
    }

    [Fact]
    public void ApplyParentData_幅が負ならArgumentOutOfRangeExceptionを投げる()
    {
        var child = Box(20, 20);
        var stack = new RenderStack { Children = { child } };
        var positioned = new Positioned
        {
            Width = -1,
            Child = new SizedBox()
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => positioned.ApplyParentData(child));
    }

    private static RenderConstrainedBox Box(double width, double height) => new()
    {
        AdditionalConstraints = BoxConstraints.Tight(width, height)
    };
}
