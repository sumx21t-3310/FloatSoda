using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>Row、Column、Flex の余剰主軸領域を比率で受け取り、子を割当量いっぱいに広げます。</summary>
/// <remarks>Row、Column、Flex の直接の子として使用してください。</remarks>
/// <seealso cref="Flexible"/>
/// <seealso cref="Spacer"/>
public sealed record Expanded : ParentDataWidget<FlexParentData>
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
    protected override bool ApplyParentData(FlexParentData parentData)
    {
        var changed = parentData.Flex != Flex || parentData.Fit != Geometrics.FlexFit.Tight;
        if (!changed) return false;

        parentData.Flex = Flex;
        parentData.Fit = Geometrics.FlexFit.Tight;
        return true;
    }
}
