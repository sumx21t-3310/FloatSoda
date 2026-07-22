namespace FloatSoda.Gesture;

/// <summary>ポインターのヒットテストで自身と子要素をどのように扱うかを指定します。</summary>
public enum HitTestBehaviour
{
    /// <summary>子要素がヒットした場合に限り、自身もヒットとして扱います。</summary>
    DeferToChild,

    /// <summary>自身の領域全体をヒットとして扱い、背後の兄弟要素への探索を停止します。</summary>
    Opaque,

    /// <summary>自身をヒットパスへ追加しつつ、背後の兄弟要素への探索を継続します。</summary>
    Translucent
}
