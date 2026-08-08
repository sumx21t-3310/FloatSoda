using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;

namespace FloatSoda.Widgets.Layout;

/// <summary>Row、Column、Flex の余剰主軸領域を比率で受け取り、子が割当量以下の大きさを選べるようにします。</summary>
/// <remarks>Row、Column、Flex の直接の子として使用してください。</remarks>
/// <seealso cref="Expanded"/>
/// <seealso cref="Spacer"/>
public sealed record Flexible : ParentDataWidget<FlexParentData>
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

    /// <summary>割り当てられた主軸領域を子へ適用する方法を取得します。</summary>
    public FlexFit Fit
    {
        get;
        init
        {
            if (!Enum.IsDefined(value))
            {
                throw new ArgumentOutOfRangeException(nameof(Fit), value, "定義済みの FlexFit を指定してください。");
            }

            field = value;
        }
    } = FlexFit.Loose;

    /// <inheritdoc/>
    protected override bool ApplyParentData(FlexParentData parentData)
    {
        var changed = parentData.Flex != Flex || parentData.Fit != Fit;
        if (!changed) return false;

        parentData.Flex = Flex;
        parentData.Fit = Fit;
        return true;
    }
}
