using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class RenderWrapTest
{
    [Fact]
    public void PerformLayout_横方向で子が親幅を超える_次のRunへ折り返す()
    {
        var first = Box(60, 20);
        var second = Box(50, 30);
        var wrap = new RenderWrap { Children = { first, second } };

        wrap.Layout(new BoxConstraints(MaxWidth: 100, MaxHeight: 100));

        Assert.Equal(new SKSize(60, 50), wrap.Size);
        Assert.Equal(Offset.Zero, ParentData(first).Offset);
        Assert.Equal(new Offset(0, 20), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_SpacingとRunSpacingを指定_Run内とRun間へ反映する()
    {
        var first = Box(30, 10);
        var second = Box(30, 10);
        var third = Box(30, 10);
        var wrap = new RenderWrap
        {
            Spacing = 5,
            RunSpacing = 7,
            Children = { first, second, third }
        };

        wrap.Layout(new BoxConstraints(MaxWidth: 70, MaxHeight: 100));

        Assert.Equal(new SKSize(65, 27), wrap.Size);
        Assert.Equal(Offset.Zero, ParentData(first).Offset);
        Assert.Equal(new Offset(35, 0), ParentData(second).Offset);
        Assert.Equal(new Offset(0, 17), ParentData(third).Offset);
    }

    [Theory]
    [InlineData(WrapAlignment.Start, 0, 20)]
    [InlineData(WrapAlignment.End, 60, 80)]
    [InlineData(WrapAlignment.Center, 30, 50)]
    [InlineData(WrapAlignment.SpaceBetween, 0, 80)]
    [InlineData(WrapAlignment.SpaceAround, 15, 65)]
    [InlineData(WrapAlignment.SpaceEvenly, 20, 60)]
    public void PerformLayout_Alignmentを指定_Run内の子を主軸の期待位置へ配置する(
        WrapAlignment alignment,
        double expectedFirst,
        double expectedSecond)
    {
        var first = Box(20, 10);
        var second = Box(20, 10);
        var wrap = new RenderWrap
        {
            Alignment = alignment,
            Children = { first, second }
        };

        wrap.Layout(BoxConstraints.Tight(100, 20));

        Assert.Equal(new Offset(expectedFirst, 0), ParentData(first).Offset);
        Assert.Equal(new Offset(expectedSecond, 0), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_RunAlignmentEndを指定_Run全体を交差軸終了側へ寄せる()
    {
        var first = Box(50, 10);
        var second = Box(50, 10);
        var wrap = new RenderWrap
        {
            RunAlignment = WrapAlignment.End,
            Children = { first, second }
        };

        wrap.Layout(BoxConstraints.Tight(50, 100));

        Assert.Equal(new Offset(0, 80), ParentData(first).Offset);
        Assert.Equal(new Offset(0, 90), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_CrossAxisAlignmentEndを指定_Run内の子を交差軸終了側へ寄せる()
    {
        var shortChild = Box(20, 10);
        var tallChild = Box(20, 30);
        var wrap = new RenderWrap
        {
            CrossAxisAlignment = WrapCrossAlignment.End,
            Children = { shortChild, tallChild }
        };

        wrap.Layout(new BoxConstraints(MaxWidth: 100, MaxHeight: 100));

        Assert.Equal(new Offset(0, 20), ParentData(shortChild).Offset);
        Assert.Equal(new Offset(20, 0), ParentData(tallChild).Offset);
    }

    [Fact]
    public void PerformLayout_DirectionVerticalで子が親高を超える_次のRunへ折り返す()
    {
        var first = Box(20, 40);
        var second = Box(30, 40);
        var wrap = new RenderWrap
        {
            Direction = Axis.Vertical,
            Children = { first, second }
        };

        wrap.Layout(new BoxConstraints(MaxWidth: 100, MaxHeight: 70));

        Assert.Equal(new SKSize(50, 40), wrap.Size);
        Assert.Equal(Offset.Zero, ParentData(first).Offset);
        Assert.Equal(new Offset(20, 0), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_VerticalDirectionUpを指定_垂直主軸の開始側を下端にする()
    {
        var first = Box(20, 20);
        var second = Box(20, 20);
        var wrap = new RenderWrap
        {
            Direction = Axis.Vertical,
            VerticalDirection = VerticalDirection.Up,
            Children = { first, second }
        };

        wrap.Layout(BoxConstraints.Tight(20, 100));

        Assert.Equal(new Offset(0, 80), ParentData(first).Offset);
        Assert.Equal(new Offset(0, 60), ParentData(second).Offset);
    }

    [Fact]
    public void PerformLayout_主軸制約がUnbounded_単一Runに全ての子を配置する()
    {
        var first = Box(60, 20);
        var second = Box(50, 30);
        var wrap = new RenderWrap
        {
            Spacing = 5,
            Children = { first, second }
        };

        wrap.Layout(BoxConstraints.Unbounded);

        Assert.Equal(new SKSize(115, 30), wrap.Size);
        Assert.Equal(Offset.Zero, ParentData(first).Offset);
        Assert.Equal(new Offset(65, 0), ParentData(second).Offset);
    }

    [Fact]
    public void GetIntrinsicDimension_横方向の最小最大幅と折返し後の高さ_Flutter相当を返す()
    {
        var first = IntrinsicBox(40, 10);
        var second = IntrinsicBox(30, 20);
        var wrap = new RenderWrap
        {
            Spacing = 5,
            RunSpacing = 7,
            Children = { first, second }
        };

        Assert.Equal(40, wrap.GetMinIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(70, wrap.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(37, wrap.GetMinIntrinsicHeight(60));
        Assert.Equal(37, wrap.GetMaxIntrinsicHeight(60));
    }

    [Fact]
    public void ComputeDryLayout_折返し制約を指定_実レイアウトと同じサイズを返す()
    {
        var wrap = new RenderWrap
        {
            Spacing = 5,
            RunSpacing = 7,
            Children = { Box(30, 10), Box(30, 10), Box(30, 10) }
        };

        Assert.Equal(
            new SKSize(65, 27),
            wrap.GetDryLayout(new BoxConstraints(MaxWidth: 70, MaxHeight: 100)));
    }

    private static RenderConstrainedBox Box(double width, double height) => new()
    {
        AdditionalConstraints = BoxConstraints.Tight(width, height)
    };

    private static IntrinsicTestBox IntrinsicBox(double width, double height) => new(width, height);

    private static WrapParentData ParentData(RenderBox child) => Assert.IsType<WrapParentData>(child.ParentData);

    private sealed class IntrinsicTestBox(double width, double height) : RenderBox
    {
        public override void PerformLayout() => Size = Constraints.Constrain(width, height);

        internal override SKSize ComputeDryLayout(BoxConstraints constraints) => constraints.Constrain(width, height);

        protected override double ComputeMinIntrinsicWidth(double availableHeight) => width;

        protected override double ComputeMaxIntrinsicWidth(double availableHeight) => width;

        protected override double ComputeMinIntrinsicHeight(double availableWidth) => height;

        protected override double ComputeMaxIntrinsicHeight(double availableWidth) => height;

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}
