using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderFlexTest
{
    [Fact]
    public void PerformLayout_水平方向で固定子とFlex子へ比率どおりに余剰幅を分配する()
    {
        var fixedChild = Box(60, 20);
        var firstFlex = Box(10, 30);
        var secondFlex = Box(10, 40);
        var flex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            CrossAxisAlignment = CrossAxisAlignment.Center,
            Children = { fixedChild, firstFlex, secondFlex }
        };
        SetFlex(firstFlex, 1, FlexFit.Tight);
        SetFlex(secondFlex, 2, FlexFit.Tight);

        flex.Layout(BoxConstraints.Tight(300, 100));

        Assert.Equal(new SKSize(60, 20), fixedChild.Size);
        Assert.Equal(new SKSize(80, 30), firstFlex.Size);
        Assert.Equal(new SKSize(160, 40), secondFlex.Size);
        Assert.Equal(new Offset(0, 40), ParentData(fixedChild).Offset);
        Assert.Equal(new Offset(60, 35), ParentData(firstFlex).Offset);
        Assert.Equal(new Offset(140, 30), ParentData(secondFlex).Offset);
    }

    [Fact]
    public void PerformLayout_垂直方向でFlex子へ比率どおりに余剰高さを分配する()
    {
        var first = Box(20, 10);
        var second = Box(30, 10);
        var flex = new RenderFlex
        {
            Direction = Axis.Vertical,
            CrossAxisAlignment = CrossAxisAlignment.End,
            Children = { first, second }
        };
        SetFlex(first, 1, FlexFit.Tight);
        SetFlex(second, 2, FlexFit.Tight);

        flex.Layout(BoxConstraints.Tight(100, 300));

        Assert.Equal(new SKSize(20, 100), first.Size);
        Assert.Equal(new SKSize(30, 200), second.Size);
        Assert.Equal(new Offset(80, 0), ParentData(first).Offset);
        Assert.Equal(new Offset(70, 100), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_FlexFitLooseの子は割当量より小さい自然サイズを保持する()
    {
        var fixedChild = Box(60, 20);
        var looseChild = Box(40, 30);
        var flex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            MainAxisAlignment = MainAxisAlignment.SpaceBetween,
            Children = { fixedChild, looseChild }
        };
        SetFlex(looseChild, 1, FlexFit.Loose);

        flex.Layout(BoxConstraints.Tight(300, 100));

        Assert.Equal(new SKSize(40, 30), looseChild.Size);
        Assert.Equal(new Offset(260, 35), ParentData(looseChild).Offset);
    }

    [Fact]
    public void PerformLayout_CrossAxisAlignmentStretchでFlex子を交差軸いっぱいに広げる()
    {
        var child = Box(20, 10);
        var flex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            CrossAxisAlignment = CrossAxisAlignment.Stretch,
            Children = { child }
        };
        SetFlex(child, 1, FlexFit.Tight);

        flex.Layout(BoxConstraints.Tight(120, 80));

        Assert.Equal(new SKSize(120, 80), child.Size);
        Assert.Equal(Offset.Zero, ParentData(child).Offset);
    }

    [Theory]
    [InlineData(Axis.Horizontal)]
    [InlineData(Axis.Vertical)]
    public void PerformLayout_主軸の最大制約が非有限でFlex子を持つとInvalidOperationExceptionを投げる(Axis direction)
    {
        var child = Box(20, 10);
        var flex = new RenderFlex { Direction = direction, Children = { child } };
        SetFlex(child, 1, FlexFit.Loose);
        var constraints = direction == Axis.Horizontal
            ? new BoxConstraints(MaxHeight: 100)
            : new BoxConstraints(MaxWidth: 100);

        var exception = Assert.Throws<InvalidOperationException>(() => flex.Layout(constraints));

        Assert.Contains("有限", exception.Message);
    }

    [Fact]
    public void ComputeDryLayout_Flex比率とTightを実レイアウトと同じように反映する()
    {
        var first = Box(10, 20);
        var second = Box(10, 30);
        var flex = new RenderFlex { Direction = Axis.Horizontal, Children = { first, second } };
        SetFlex(first, 1, FlexFit.Tight);
        SetFlex(second, 3, FlexFit.Tight);

        var size = flex.GetDryLayout(BoxConstraints.Tight(200, 60));

        Assert.Equal(new SKSize(200, 60), size);
    }

    [Fact]
    public void GetIntrinsicDimension_RowのFlex比率と固定子を反映する()
    {
        var fixedChild = IntrinsicBox(20, 10);
        var firstFlex = IntrinsicBox(100, 10);
        var secondFlex = IntrinsicBox(10, 10);
        var flex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            Children = { fixedChild, firstFlex, secondFlex }
        };
        SetFlex(firstFlex, 1, FlexFit.Tight);
        SetFlex(secondFlex, 1, FlexFit.Loose);

        Assert.Equal(220, flex.GetMinIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(220, flex.GetMaxIntrinsicWidth(double.PositiveInfinity));
    }

    [Fact]
    public void GetIntrinsicDimension_ColumnのFlex比率と固定子を反映する()
    {
        var fixedChild = IntrinsicBox(10, 20);
        var firstFlex = IntrinsicBox(10, 40);
        var secondFlex = IntrinsicBox(10, 30);
        var flex = new RenderFlex
        {
            Direction = Axis.Vertical,
            Children = { fixedChild, firstFlex, secondFlex }
        };
        SetFlex(firstFlex, 1, FlexFit.Loose);
        SetFlex(secondFlex, 3, FlexFit.Tight);

        Assert.Equal(180, flex.GetMinIntrinsicHeight(double.PositiveInfinity));
        Assert.Equal(180, flex.GetMaxIntrinsicHeight(double.PositiveInfinity));
    }

    [Fact]
    public void PerformLayout_IntrinsicWidthはFlex比率を満たす主軸寸法を使用する()
    {
        var first = IntrinsicBox(100, 10);
        var second = IntrinsicBox(10, 10);
        var flex = new RenderFlex { Direction = Axis.Horizontal, Children = { first, second } };
        SetFlex(first, 1, FlexFit.Tight);
        SetFlex(second, 1, FlexFit.Tight);
        var intrinsic = new RenderIntrinsicWidth { Child = flex };

        intrinsic.Layout(new BoxConstraints(MaxWidth: 500, MaxHeight: 100));

        Assert.Equal(200, intrinsic.Size.Width);
        Assert.Equal(100, first.Size.Width);
        Assert.Equal(100, second.Size.Width);
    }

    [Fact]
    public void GetMaxIntrinsicHeight_RowはFlex子へ割り当てた幅で交差軸を測定する()
    {
        var first = IntrinsicBox(100, 10, heightForWidth: width => width >= 100 ? 20 : 80);
        var second = IntrinsicBox(10, 10, heightForWidth: width => width >= 100 ? 15 : 60);
        var flex = new RenderFlex { Direction = Axis.Horizontal, Children = { first, second } };
        SetFlex(first, 1, FlexFit.Tight);
        SetFlex(second, 1, FlexFit.Loose);

        Assert.Equal(20, flex.GetMaxIntrinsicHeight(200));
    }

    [Fact]
    public void GetMaxIntrinsicWidth_ColumnはFlex子へ割り当てた高さで交差軸を測定する()
    {
        var first = IntrinsicBox(10, 100, widthForHeight: height => height <= 100 ? 20 : 80);
        var second = IntrinsicBox(10, 10, widthForHeight: height => height <= 100 ? 15 : 60);
        var flex = new RenderFlex { Direction = Axis.Vertical, Children = { first, second } };
        SetFlex(first, 1, FlexFit.Loose);
        SetFlex(second, 1, FlexFit.Tight);

        Assert.Equal(20, flex.GetMaxIntrinsicWidth(200));
    }

    [Fact]
    public void GetMinIntrinsicHeight_Rowは固定子を除いた幅をFlex子へ割り当てる()
    {
        var fixedChild = IntrinsicBox(50, 10);
        var flexChild = IntrinsicBox(100, 10, heightForWidth: width => width >= 150 ? 20 : 80);
        var flex = new RenderFlex
        {
            Direction = Axis.Horizontal,
            Children = { fixedChild, flexChild }
        };
        SetFlex(flexChild, 1, FlexFit.Tight);

        Assert.Equal(20, flex.GetMinIntrinsicHeight(200));
    }

    [Fact]
    public void FlexParentData_不正なFlexとFlexFitを拒否する()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlexParentData { Flex = -1 });
        Assert.Throws<ArgumentOutOfRangeException>(() => new FlexParentData { Fit = (FlexFit)999 });
    }

    private static RenderConstrainedBox Box(double width, double height) => new()
    {
        AdditionalConstraints = BoxConstraints.Tight(width, height)
    };

    private static IntrinsicTestBox IntrinsicBox(
        double width,
        double height,
        Func<double, double>? widthForHeight = null,
        Func<double, double>? heightForWidth = null) =>
        new(width, height, widthForHeight, heightForWidth);

    private static FlexParentData ParentData(RenderBox child)
        => Assert.IsType<FlexParentData>(child.ParentData);

    private static void SetFlex(RenderBox child, int flex, FlexFit fit)
    {
        var parentData = ParentData(child);
        parentData.Flex = flex;
        parentData.Fit = fit;
    }

    private sealed class IntrinsicTestBox(
        double width,
        double height,
        Func<double, double>? widthForHeight,
        Func<double, double>? heightForWidth) : RenderBox
    {
        public override void PerformLayout() => Size = Constraints.Constrain(width, height);

        protected override double ComputeMinIntrinsicWidth(double availableHeight) =>
            widthForHeight?.Invoke(availableHeight) ?? width;

        protected override double ComputeMaxIntrinsicWidth(double availableHeight) =>
            widthForHeight?.Invoke(availableHeight) ?? width;

        protected override double ComputeMinIntrinsicHeight(double availableWidth) =>
            heightForWidth?.Invoke(availableWidth) ?? height;

        protected override double ComputeMaxIntrinsicHeight(double availableWidth) =>
            heightForWidth?.Invoke(availableWidth) ?? height;

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}
