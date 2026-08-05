using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>すべての子をレイアウトし、指定された一つの子だけを描画およびヒットテストするStackです。</summary>
public class RenderIndexedStack : RenderStack
{
    /// <summary>表示する子の0始まりの位置を取得または設定します。<see langword="null"/>の場合はどの子も表示しません。</summary>
    public int? Index
    {
        get;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Index), value, "Indexには0以上の位置またはnullを指定してください。");
            }

            if (field == value) return;
            field = value;
            MarkNeedsPaint();
        }
    } = 0;

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        ValidateIndex();
        base.PerformLayout();
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (GetSelectedChild() is not { } child) return;
        var parentData = (StackParentData)child.ParentData!;
        context.PaintChild(child, offset + parentData.Offset);
    }

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position)
    {
        if (GetSelectedChild() is not { } child) return false;
        var parentData = (StackParentData)child.ParentData!;
        return result.AddWidthPaintOffset(
            parentData.Offset,
            position,
            (testResult, transformed) => child.HitTest(testResult, transformed));
    }

    private RenderBox? GetSelectedChild()
    {
        ValidateIndex();
        return Index is { } index ? Children.ElementAt(index) : null;
    }

    private void ValidateIndex()
    {
        if (Index is { } index && (index < 0 || index >= Children.Count))
        {
            throw new ArgumentOutOfRangeException(nameof(Index), Index, "IndexにはChildren内の位置またはnullを指定してください。");
        }
    }
}
