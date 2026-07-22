using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>
/// ポインター列を監視し、それがタップ／ドラッグ等の意味あるジェスチャかを判定する状態機械の基底。
/// <see cref="AddPointer"/> で Down を受けると、共有の <see cref="PointerRouter"/> へ後続イベントの
/// 購読を登録し、<see cref="GestureArenaManager"/> のアリーナへ参加する。以降のイベントは
/// ルータ経由で <see cref="HandleEvent"/> に届く。
/// </summary>
public abstract class GestureRecognizer : IGestureArenaMember, IDisposable
{
    /// <summary>参加するアリーナ。<see cref="Bind"/> で注入される。</summary>
    protected GestureArenaManager Arena { get; private set; } = null!;

    /// <summary>後続イベントを購読するルータ。<see cref="Bind"/> で注入される。</summary>
    protected PointerRouter Router { get; private set; } = null!;

    /// <summary>ウィンドウの共有アリーナ／ルータを注入する。生成直後に一度だけ呼ばれる。</summary>
    /// <param name="arena">認識器が参加するウィンドウ単位のジェスチャアリーナ。</param>
    /// <param name="router">後続のポインターイベントを配送するルータ。</param>
    internal void Bind(GestureArenaManager arena, PointerRouter router)
    {
        Arena = arena;
        Router = router;
    }

    /// <summary>
    /// Down イベントを受けて監視を開始する入口。ルータ購読とアリーナ参加を行い、
    /// サブクラスの <see cref="AddAllowedPointer"/> を呼ぶ。
    /// </summary>
    /// <param name="downEvent">監視を開始するDownフェーズのポインターイベント。</param>
    /// <remarks>Down以外のイベントを指定した場合は何も行いません。</remarks>
    public void AddPointer(PointerEvent downEvent)
    {
        if (downEvent.Phase != PointerEventPhase.Down) return;

        Router.AddRoute(downEvent.PointerId, HandleEvent);
        Arena.Add(downEvent.PointerId, this);
        AddAllowedPointer(downEvent);
    }

    /// <summary>アリーナへ勝敗を宣言する。</summary>
    /// <param name="pointer">勝敗を宣言するポインター識別子。</param>
    /// <param name="disposition">宣言する受理または辞退の状態。</param>
    protected void Resolve(int pointer, GestureDisposition disposition)
        => Arena.Resolve(pointer, this, disposition);

    /// <summary>このポインターの購読を解除する（勝敗確定後の後始末）。</summary>
    /// <param name="pointer">購読を解除するポインター識別子。</param>
    protected void StopTrackingPointer(int pointer)
        => Router.RemoveRoute(pointer, HandleEvent);

    /// <summary>Down 受理時にサブクラスが初期状態を作る。</summary>
    /// <param name="downEvent">受理したDownフェーズのポインターイベント。</param>
    protected abstract void AddAllowedPointer(PointerEvent downEvent);

    /// <summary>ルータ経由で届く後続イベント（Down/Move/Up）を処理する。</summary>
    /// <param name="pointerEvent">処理するポインターイベント。</param>
    protected abstract void HandleEvent(PointerEvent pointerEvent);

    /// <inheritdoc />
    public abstract void AcceptGesture(int pointer);

    /// <inheritdoc />
    public abstract void RejectGesture(int pointer);

    /// <inheritdoc />
    public virtual void Dispose() { }
}
