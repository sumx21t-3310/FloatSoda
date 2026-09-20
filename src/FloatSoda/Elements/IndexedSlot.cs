namespace FloatSoda.Elements;

/// <summary>
/// 複数の子を持つ親が、それぞれの子Elementへ割り当てる<see cref="Element.Slot"/>の値です。
/// </summary>
/// <param name="Index">親の子リストの中での位置。</param>
/// <param name="Previous">直前の兄弟Element。先頭の子では<see langword="null"/>。</param>
/// <remarks>
/// 子のRenderObjectは、<paramref name="Previous"/>のRenderObjectの次へ挿入されます。
/// 位置が同じでも直前の兄弟が入れ替わればslotは等しくなくなり、RenderObjectの移動が起きます。
/// Flutterの<c>IndexedSlot</c>に対応します。
/// </remarks>
public readonly record struct IndexedSlot(int Index, Element? Previous);
