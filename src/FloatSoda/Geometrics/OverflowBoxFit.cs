namespace FloatSoda.Geometrics;

/// <summary><see cref="Widgets.Layout.OverflowBox"/>が自身のサイズを決める方法を指定します。</summary>
public enum OverflowBoxFit
{
    /// <summary>親の制約で許される最大サイズを使用します。</summary>
    Max,

    /// <summary>親の制約内で子のサイズに従います。</summary>
    DeferToChild,
}
