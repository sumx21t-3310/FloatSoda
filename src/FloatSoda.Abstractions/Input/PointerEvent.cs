using FloatSoda.Abstractions.Geometries;

namespace FloatSoda.Abstractions.Input;

/// <summary>ポインターイベントのライフサイクル上の段階を表します。</summary>
/// <seealso cref="PointerEvent"/>
public enum PointerEventPhase
{
    /// <summary>ポインターの主ボタンが離された段階です。</summary>
    Up,
    /// <summary>ポインターの主ボタンが押された段階です。</summary>
    Down,
    /// <summary>押下中のポインターが移動した段階です。</summary>
    Move,
    /// <summary>新しいポインターが入力領域へ追加された段階です。</summary>
    Add,
    /// <summary>ポインターが入力領域から削除された段階です。</summary>
    Remove
};

/// <summary>正規化されたポインター入力を識別子、段階、および座標とともに表します。</summary>
/// <param name="PointerId">同じポインターの追加から削除までを識別する番号。</param>
/// <param name="Phase">ポインターイベントのライフサイクル上の段階。</param>
/// <param name="Position">イベント発生位置。単位と原点はイベントを生成した入力領域の座標系に従います。</param>
/// <param name="Transform">イベントに関連付ける任意の座標変換量。指定しない場合は<see langword="null"/>です。</param>
/// <seealso cref="PointerController"/>
public readonly record struct PointerEvent(
    int PointerId,
    PointerEventPhase Phase,
    Offset Position,
    Offset? Transform = null);