using FloatSoda.Geometrics;

namespace FloatSoda.Test.Geometrics;

public class SizeITest
{
    // 変換演算子が Size と SizeI の両方にあると、ここが CS0457 でコンパイルできない。
    [Fact]
    public void ImplicitOperatorSize_SizeIを代入したとき_同じ幅と高さのSizeになること()
    {
        Size size = new SizeI(3, 4);

        Assert.Equal(new Size(3, 4), size);
    }

    [Fact]
    public void ImplicitOperatorSizeI_小数部を持つSizeを代入したとき_0の方向へ切り捨てること()
    {
        SizeI sizeI = new Size(3.9, -4.9);

        Assert.Equal(new SizeI(3, -4), sizeI);
    }
}
