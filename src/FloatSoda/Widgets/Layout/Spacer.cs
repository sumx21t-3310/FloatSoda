using FloatSoda.Elements;

namespace FloatSoda.Widgets.Layout;

/// <summary>Row、Column、Flex の余剰主軸領域に、比率指定できる空白を挿入します。</summary>
public sealed record Spacer : StatelessWidget
{
    /// <summary>余剰領域を他の flex 子と分配する比率を取得します。</summary>
    public int Flex
    {
        get;
        init
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Flex), value, "Flex には 1 以上の整数を指定してください。");
            }

            field = value;
        }
    } = 1;

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context) => new Expanded
    {
        Flex = Flex,
        Child = new SizedBox()
    };
}
