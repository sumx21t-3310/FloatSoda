using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Gesture;

/// <summary>
/// 子要素の描画とレイアウトを維持したまま、ポインターのヒットを自身で吸収できるRenderObjectです。
/// </summary>
/// <seealso cref="Widgets.Gesture.AbsorbPointer"/>
public class RenderAbsorbPointer : RenderProxyBox
{
    /// <summary>子要素へのヒットテストを遮断し、自身の領域でヒットを吸収するかどうかを取得または設定します。</summary>
    /// <remarks>変更は次回のヒットテストから反映され、Layout DirtyまたはPaint Dirtyにはしません。</remarks>
    public bool Absorbing { get; set; }

    /// <summary>吸収中は自身の領域内かだけを判定し、子要素をヒットパスへ追加しません。</summary>
    /// <param name="result">ヒットした対象を格納する結果。</param>
    /// <param name="position">このRenderObjectのローカル座標で表した判定位置。</param>
    /// <returns>吸収中は位置が自身の領域内にある場合、それ以外は基底実装がヒットした場合に<see langword="true"/>。</returns>
    public override bool HitTest(HitTestResult result, Offset position)
    {
        return Absorbing ? Size.Contains(position) : base.HitTest(result, position);
    }
}
