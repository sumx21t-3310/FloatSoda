using System.Reflection;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Test.RenderObjects;

public class IntrinsicLayoutTest
{
    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.NegativeInfinity)]
    public void GetIntrinsicDimension_負値NaN負の無限大_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        var box = new TestIntrinsicBox(20, 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => box.GetMinIntrinsicWidth(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => box.GetMaxIntrinsicWidth(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => box.GetMinIntrinsicHeight(value));
        Assert.Throws<ArgumentOutOfRangeException>(() => box.GetMaxIntrinsicHeight(value));
    }

    [Fact]
    public void GetIntrinsicDimension_未実装RenderBox_NotSupportedExceptionを投げる()
    {
        Assert.Throws<NotSupportedException>(() => new UnsupportedBox().GetMaxIntrinsicWidth(double.PositiveInfinity));
    }

    [Fact]
    public void PublicApi_RenderBoxのintrinsic表面_スカラー値のみを公開しdryLayoutを内部に保つ()
    {
        var methods = typeof(RenderBox).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(method => method.Name.StartsWith("Get", StringComparison.Ordinal) && method.Name.Contains("Intrinsic", StringComparison.Ordinal))
            .ToArray();

        Assert.Equal(4, methods.Length);
        Assert.All(methods, method =>
        {
            Assert.Equal(typeof(double), method.ReturnType);
            Assert.Single(method.GetParameters(), parameter => parameter.ParameterType == typeof(double));
        });

        var dryLayout = typeof(RenderBox).GetMethod("ComputeDryLayout", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Assert.NotNull(dryLayout);
        Assert.True(dryLayout!.IsAssembly);
        Assert.False(dryLayout.IsFamily);
    }

    [Fact]
    public void GetIntrinsicDimension_問い合わせ前後_通常レイアウト状態を変更しない()
    {
        var layer = new ContainerLayer();
        var parentData = new BoxParentData { Offset = new Offset(7, 9) };
        var box = new TestIntrinsicBox(53, 17) { ParentData = parentData, Layer = layer };
        box.Layout(BoxConstraints.Tight(80, 40));
        box.NeedsLayout = false;
        box.NeedsPaint = false;

        Assert.Equal(53, box.GetMinIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(53, box.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(17, box.GetMinIntrinsicHeight(53));
        Assert.Equal(17, box.GetMaxIntrinsicHeight(53));

        Assert.Equal(new SKSize(80, 40), box.Size);
        Assert.Equal(new Offset(7, 9), parentData.Offset);
        Assert.False(box.NeedsLayout);
        Assert.False(box.NeedsPaint);
        Assert.Same(layer, box.Layer);
    }

    [Fact]
    public void GetIntrinsicDimension_Text_最小幅最大幅と改行高さが一貫する()
    {
        var paragraph = new RenderParagraph { Text = new TextSpan("short longest-word") };

        var minWidth = paragraph.GetMinIntrinsicWidth(double.PositiveInfinity);
        var maxWidth = paragraph.GetMaxIntrinsicWidth(double.PositiveInfinity);
        var wideHeight = paragraph.GetMaxIntrinsicHeight(maxWidth);
        var narrowHeight = paragraph.GetMaxIntrinsicHeight(minWidth);

        Assert.True(double.IsFinite(minWidth));
        Assert.True(minWidth > 0);
        Assert.True(maxWidth >= minWidth);
        Assert.True(narrowHeight >= wideHeight);
    }

    [Fact]
    public void GetIntrinsicDimension_SizedBox相当の固定制約_四種類すべて固定寸法を返す()
    {
        var box = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(120, 45) };

        Assert.Equal(120, box.GetMinIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(120, box.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(45, box.GetMinIntrinsicHeight(double.PositiveInfinity));
        Assert.Equal(45, box.GetMaxIntrinsicHeight(double.PositiveInfinity));
    }

    [Fact]
    public void GetIntrinsicDimension_ConstrainedBox_子の自然寸法を追加制約へ収める()
    {
        var box = new RenderConstrainedBox
        {
            AdditionalConstraints = new BoxConstraints(40, 80, 20, 60),
            Child = new TestIntrinsicBox(100, 10)
        };

        Assert.Equal(80, box.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(20, box.GetMaxIntrinsicHeight(80));
    }

    [Fact]
    public void GetIntrinsicDimension_Flex_RowとColumnで主軸は合計し交差軸は最大を返す()
    {
        var row = CreateFlex(Axis.Horizontal);
        var column = CreateFlex(Axis.Vertical);

        Assert.Equal(70, row.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(30, row.GetMaxIntrinsicHeight(70));
        Assert.Equal(40, column.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(50, column.GetMaxIntrinsicHeight(40));
    }

    [Fact]
    public void PerformLayout_IntrinsicWidthとHeight_stepへ切り上げてnested制約を適用する()
    {
        var height = new RenderIntrinsicHeight
        {
            StepHeight = 10,
            Child = new TestIntrinsicBox(53, 17)
        };
        var width = new RenderIntrinsicWidth
        {
            StepWidth = 20,
            Child = height
        };

        Assert.Equal(60, width.GetMaxIntrinsicWidth(double.PositiveInfinity));
        Assert.Equal(20, height.GetMaxIntrinsicHeight(60));

        width.Layout(new BoxConstraints(MaxWidth: 100, MaxHeight: 100));

        Assert.Equal(new SKSize(60, 20), width.Size);
        Assert.Equal(new SKSize(60, 20), height.Size);
        Assert.Equal(new SKSize(60, 20), height.Child!.Size);
    }

    [Fact]
    public void PerformLayout_IntrinsicWidth_親の最大制約と交差して子を狭める()
    {
        var box = new RenderIntrinsicWidth { StepWidth = 20, Child = new TestIntrinsicBox(53, 17) };

        box.Layout(new BoxConstraints(MaxWidth: 50, MaxHeight: 100));

        Assert.Equal(new SKSize(50, 17), box.Size);
        Assert.Equal(50, box.Child!.Constraints.MaxWidth);
    }

    [Fact]
    public void PerformLayout_IntrinsicWidthとHeightに子がない_親制約の最小サイズを返す()
    {
        var constraints = new BoxConstraints(12, 100, 8, 100);
        var width = new RenderIntrinsicWidth();
        var height = new RenderIntrinsicHeight();

        width.Layout(constraints);
        height.Layout(constraints);

        Assert.Equal(new SKSize(12, 8), width.Size);
        Assert.Equal(new SKSize(12, 8), height.Size);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Step_0以下または非有限値_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderIntrinsicWidth { StepWidth = value });
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderIntrinsicHeight { StepHeight = value });
    }

    private static RenderFlex CreateFlex(Axis direction)
    {
        var flex = new RenderFlex { Direction = direction, MainAxisSize = MainAxisSize.Min };
        flex.AddChild(new TestIntrinsicBox(30, 20));
        flex.AddChild(new TestIntrinsicBox(40, 30));
        return flex;
    }

    private sealed class TestIntrinsicBox(double width, double height) : RenderBox
    {
        public override void PerformLayout() => Size = Constraints.Constrain(width, height);

        protected override double ComputeMinIntrinsicWidth(double availableHeight) => width;

        protected override double ComputeMaxIntrinsicWidth(double availableHeight) => width;

        protected override double ComputeMinIntrinsicHeight(double availableWidth) => height;

        protected override double ComputeMaxIntrinsicHeight(double availableWidth) => height;

        public override void Paint(PaintingContext context, Offset offset) { }
    }

    private sealed class UnsupportedBox : RenderBox
    {
        public override void PerformLayout() => Size = Constraints.Smallest;

        public override void Paint(PaintingContext context, Offset offset) { }
    }
}
