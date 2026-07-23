using FloatSoda.Geometrics;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>
/// 親から受け取った制約に追加の制約を適用して子をレイアウトするRenderObjectです。
/// </summary>
public class RenderConstrainedBox : RenderProxyBox
{
    /// <summary>
    /// 親の制約へ追加して子に適用する制約を取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、このRenderObjectをLayout Dirtyとしてマークし、
    /// 次のパイプライン更新時に子のサイズを再計算します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public BoxConstraints AdditionalConstraints
    {
        get;
        set
        {
            if (value == field) return;

            field = value;
            MarkNeedsLayout();
        }
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        var enforcedConstraints = AdditionalConstraints.Enforce(Constraints);
        Child?.Layout(enforcedConstraints, parentUseSize: true);

        Size = Child?.Size ?? enforcedConstraints.Constrain(Size);
    }
}
