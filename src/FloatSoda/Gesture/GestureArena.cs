namespace FloatSoda.Gesture;

/// <summary>アリーナでの勝敗の宣言。</summary>
public enum GestureDisposition
{
    /// <summary>このポインタ列を自分のジェスチャとして受理する（勝利宣言）。</summary>
    Accepted,

    /// <summary>このポインタ列は自分のジェスチャではないと辞退する。</summary>
    Rejected,
}

/// <summary>
/// ジェスチャアリーナの参加者。1本のポインタを複数の認識器が奪い合うとき、
/// 勝敗が確定した時点で <see cref="AcceptGesture"/> / <see cref="RejectGesture"/> が呼ばれる。
/// </summary>
public interface IGestureArenaMember
{
    /// <summary>このメンバーがアリーナの勝者になったとき呼ばれる。</summary>
    void AcceptGesture(int pointer);

    /// <summary>このメンバーが敗者になった（他が勝った/自分が辞退した）とき呼ばれる。</summary>
    void RejectGesture(int pointer);
}

/// <summary>
/// アリーナ参加の控え。認識器はこれ経由で勝敗を宣言できる。
/// <see cref="GestureArenaManager.Add"/> の戻り値。
/// </summary>
public sealed class GestureArenaEntry
{
    private readonly GestureArenaManager _arena;
    private readonly int _pointer;
    private readonly IGestureArenaMember _member;

    internal GestureArenaEntry(GestureArenaManager arena, int pointer, IGestureArenaMember member)
        => (_arena, _pointer, _member) = (arena, pointer, member);

    /// <summary>このメンバーとして勝敗を宣言する。</summary>
    public void Resolve(GestureDisposition disposition)
        => _arena.Resolve(_pointer, _member, disposition);
}

/// <summary>
/// ポインタごとにジェスチャ認識器を集め、勝者を1人に絞り込む調停器。Flutter の
/// <c>GestureArenaManager</c> に相当する。<see cref="Add"/> で参加、<see cref="Close"/> で
/// 受付締切、<see cref="Sweep"/> で締切後の既定勝者確定を行う。
/// </summary>
public sealed class GestureArenaManager
{
    private sealed class ArenaState
    {
        public readonly List<IGestureArenaMember> Members = [];

        /// <summary>まだ新規参加を受け付けているか（Down ディスパッチ中は true）。</summary>
        public bool IsOpen = true;

        /// <summary>Close 前に Accepted 宣言したメンバー（締切時に勝者化する）。</summary>
        public IGestureArenaMember? EagerWinner;
    }

    private readonly Dictionary<int, ArenaState> _arenas = [];

    /// <summary>ポインタのアリーナにメンバーを参加させる。</summary>
    public GestureArenaEntry Add(int pointer, IGestureArenaMember member)
    {
        if (!_arenas.TryGetValue(pointer, out var state))
        {
            state = new ArenaState();
            _arenas[pointer] = state;
        }

        state.Members.Add(member);
        return new GestureArenaEntry(this, pointer, member);
    }

    /// <summary>
    /// 新規参加の受付を締め切る（通常は Down のディスパッチ完了時）。
    /// メンバーが1人だけなら即座にそのメンバーを勝者にする。
    /// </summary>
    public void Close(int pointer)
    {
        if (!_arenas.TryGetValue(pointer, out var state)) return;

        state.IsOpen = false;
        TryToResolveArena(pointer, state);
    }

    /// <summary>
    /// 締切後も未確定なら、先頭メンバーを既定の勝者にする（通常は Up のディスパッチ時）。
    /// </summary>
    public void Sweep(int pointer)
    {
        if (!_arenas.TryGetValue(pointer, out var state)) return;

        _arenas.Remove(pointer);

        if (state.Members.Count == 0) return;

        state.Members[0].AcceptGesture(pointer);
        for (var i = 1; i < state.Members.Count; i++)
        {
            state.Members[i].RejectGesture(pointer);
        }
    }

    /// <summary>メンバーからの勝敗宣言を処理する。</summary>
    public void Resolve(int pointer, IGestureArenaMember member, GestureDisposition disposition)
    {
        if (!_arenas.TryGetValue(pointer, out var state)) return;

        if (disposition == GestureDisposition.Rejected)
        {
            state.Members.Remove(member);
            member.RejectGesture(pointer);

            if (!state.IsOpen) TryToResolveArena(pointer, state);
        }
        else if (state.IsOpen)
        {
            // 締切前の勝利宣言は保留し、Close 時に確定する。
            state.EagerWinner ??= member;
        }
        else
        {
            ResolveInFavorOf(pointer, state, member);
        }
    }

    private void TryToResolveArena(int pointer, ArenaState state)
    {
        switch (state.Members.Count)
        {
            case 0:
                _arenas.Remove(pointer);
                break;
            case 1:
                _arenas.Remove(pointer);
                state.Members[0].AcceptGesture(pointer);
                break;
            default:
                if (state.EagerWinner is { } winner) ResolveInFavorOf(pointer, state, winner);
                break;
        }
    }

    private void ResolveInFavorOf(int pointer, ArenaState state, IGestureArenaMember member)
    {
        _arenas.Remove(pointer);

        foreach (var m in state.Members)
        {
            if (!ReferenceEquals(m, member)) m.RejectGesture(pointer);
        }

        member.AcceptGesture(pointer);
    }
}
