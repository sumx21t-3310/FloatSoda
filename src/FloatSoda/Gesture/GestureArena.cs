namespace FloatSoda.Gesture;

/// <summary>アリーナでの勝敗の宣言。</summary>
public enum GestureDisposition
{
    /// <summary>このポインター列を自分のジェスチャとして受理する（勝利宣言）。</summary>
    Accepted,

    /// <summary>このポインター列は自分のジェスチャではないと辞退する。</summary>
    Rejected,
}

/// <summary>
/// ジェスチャアリーナの参加者。1本のポインターを複数の認識器が奪い合うとき、
/// 勝敗が確定した時点で <see cref="AcceptGesture"/> / <see cref="RejectGesture"/> が呼ばれる。
/// </summary>
public interface IGestureArenaMember
{
    /// <summary>このメンバーがアリーナの勝者になったとき呼ばれる。</summary>
    /// <param name="pointer">勝敗が確定したポインターの識別子。</param>
    void AcceptGesture(int pointer);

    /// <summary>このメンバーが敗者になった（他が勝った/自分が辞退した）とき呼ばれる。</summary>
    /// <param name="pointer">勝敗が確定したポインターの識別子。</param>
    void RejectGesture(int pointer);
}

/// <summary>
/// アリーナ参加の控え。認識器はこれ経由で勝敗を宣言できる。
/// <see cref="GestureArenaManager.Add"/> の戻り値。
/// </summary>
public sealed class GestureArenaEntry
{
    /// <summary>この参加を管理するジェスチャアリーナ。</summary>
    private readonly GestureArenaManager _arena;

    /// <summary>この参加が属するポインターの識別子。</summary>
    private readonly int _pointer;

    /// <summary>この参加に対応するアリーナメンバー。</summary>
    private readonly IGestureArenaMember _member;

    /// <summary>ジェスチャアリーナへの参加を表すエントリを初期化します。</summary>
    /// <param name="arena">参加を管理するジェスチャアリーナ。</param>
    /// <param name="pointer">参加対象のポインター識別子。</param>
    /// <param name="member">参加するアリーナメンバー。</param>
    internal GestureArenaEntry(GestureArenaManager arena, int pointer, IGestureArenaMember member)
        => (_arena, _pointer, _member) = (arena, pointer, member);

    /// <summary>このメンバーとして勝敗を宣言する。</summary>
    /// <param name="disposition">宣言する受理または辞退の状態。</param>
    public void Resolve(GestureDisposition disposition)
        => _arena.Resolve(_pointer, _member, disposition);
}

/// <summary>
/// ポインターごとにジェスチャ認識器を集め、勝者を1人に絞り込む調停器。Flutter の
/// <c>GestureArenaManager</c> に相当する。<see cref="Add"/> で参加、<see cref="Close"/> で
/// 受付締切、<see cref="Sweep"/> で締切後の既定勝者確定を行う。
/// </summary>
public sealed class GestureArenaManager
{
    /// <summary>1本のポインターについて未確定の参加者と受付状態を保持します。</summary>
    private sealed class ArenaState
    {
        /// <summary>現在アリーナに残っているメンバー。</summary>
        public readonly List<IGestureArenaMember> Members = [];

        /// <summary>まだ新規参加を受け付けているか（Down ディスパッチ中は true）。</summary>
        public bool IsOpen = true;

        /// <summary>Close 前に Accepted 宣言したメンバー（締切時に勝者化する）。</summary>
        public IGestureArenaMember? EagerWinner;
    }

    /// <summary>ポインター識別子ごとの未確定アリーナ。</summary>
    private readonly Dictionary<int, ArenaState> _arenas = [];

    /// <summary>ポインターのアリーナにメンバーを参加させる。</summary>
    /// <param name="pointer">参加対象のポインター識別子。</param>
    /// <param name="member">参加するジェスチャ認識器。</param>
    /// <returns>参加したメンバーが勝敗を宣言するためのエントリ。</returns>
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
    /// <param name="pointer">受付を締め切るポインター識別子。</param>
    /// <remarks>対象のアリーナが存在しない場合は何も行いません。</remarks>
    public void Close(int pointer)
    {
        if (!_arenas.TryGetValue(pointer, out var state)) return;

        state.IsOpen = false;
        TryToResolveArena(pointer, state);
    }

    /// <summary>
    /// 締切後も未確定なら、先頭メンバーを既定の勝者にする（通常は Up のディスパッチ時）。
    /// </summary>
    /// <param name="pointer">勝者を確定するポインター識別子。</param>
    /// <remarks>対象のアリーナを削除し、先頭以外の残存メンバーへ敗北を通知します。</remarks>
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
    /// <param name="pointer">宣言対象のポインター識別子。</param>
    /// <param name="member">勝敗を宣言するメンバー。</param>
    /// <param name="disposition">宣言された受理または辞退の状態。</param>
    /// <remarks>
    /// 辞退したメンバーには直ちに敗北を通知します。受付中の受理は締切まで保留し、
    /// 受付終了後の受理は宣言したメンバーを勝者としてアリーナを確定します。
    /// </remarks>
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

    /// <summary>受付終了後の残存メンバー数と先行勝利宣言から勝敗を確定します。</summary>
    /// <param name="pointer">確定対象のポインター識別子。</param>
    /// <param name="state">確定対象のアリーナ状態。</param>
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

    /// <summary>指定したメンバーを勝者とし、ほかのメンバーへ敗北を通知します。</summary>
    /// <param name="pointer">確定対象のポインター識別子。</param>
    /// <param name="state">確定対象のアリーナ状態。</param>
    /// <param name="member">勝者とするメンバー。</param>
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
