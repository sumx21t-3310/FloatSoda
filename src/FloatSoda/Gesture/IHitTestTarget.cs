using System.Diagnostics;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Abstractions.Input;

namespace FloatSoda.Gesture;

/// <summary>ヒットテスト経路を通じてポインターイベントを受け取る対象を表します。</summary>
/// <seealso cref="HitTestEntry"/>
/// <seealso cref="HitTestResult"/>
public interface IHitTestTarget
{
    /// <summary>この対象が含まれるヒットテスト経路へ配送されたポインターイベントを処理します。</summary>
    /// <param name="pointerEvent">配送されたポインターイベント。</param>
    /// <param name="entry">この対象と座標変換を保持するヒットテストエントリ。</param>
    void HandleEvent(PointerEvent pointerEvent, HitTestEntry entry);
}

/// <summary>ヒットした対象と、その対象のローカル座標へ変換するオフセットを表す不変値です。</summary>
/// <param name="Target">ポインターイベントを受け取る対象。</param>
/// <param name="Transform">グローバル座標へ加算して対象のローカル座標へ変換するオフセット。未指定の場合は<see langword="null"/>です。</param>
/// <seealso cref="IHitTestTarget"/>
/// <seealso cref="HitTestResult"/>
public readonly record struct HitTestEntry(IHitTestTarget Target, Offset? Transform = null);

/// <summary>ポインター位置でヒットした対象の経路と、走査中の座標変換を保持します。</summary>
/// <remarks>座標変換は<see cref="PushOffset"/>と<see cref="PopTransform"/>を対にして管理し、<see cref="Add"/>時点の累積値を各エントリへ保存します。</remarks>
/// <seealso cref="HitTestEntry"/>
public class HitTestResult
{
    /// <summary>ヒットした対象を配送順に保持する読み取り専用の経路を取得します。</summary>
    public IReadOnlyList<HitTestEntry> Path => _pathInternal;

    private readonly List<HitTestEntry> _pathInternal = [];

    private readonly List<Offset> _transforms = [Offset.Zero];

    private readonly List<Offset> _localTransform = [];


    private void GlobalizeTransform()
    {
        if (_localTransform.Count == 0) return;

        var last = _transforms[^1];

        foreach (var part in _localTransform)
        {
            last = part + last;
            _transforms.Add(last);
        }

        _localTransform.Clear();
    }

    /// <summary>現在のグローバル座標からローカル座標への累積オフセットを取得します。</summary>
    /// <remarks>まだ確定していないオフセットがある場合は、取得時に累積変換へ反映します。</remarks>
    public Offset LastTransform
    {
        get
        {
            GlobalizeTransform();
            return _transforms[^1];
        }
    }


    /// <summary>子要素のヒットテスト中に適用する座標オフセットを積みます。</summary>
    /// <param name="offset">現在の座標へ加算する論理ピクセル単位のオフセット。</param>
    /// <remarks>走査を戻る際は<see cref="PopTransform"/>を同じ回数だけ呼び出します。</remarks>
    public void PushOffset(Offset offset) => _localTransform.Add(offset);

    /// <summary>直前に積まれた座標オフセットを取り除きます。</summary>
    /// <remarks><see cref="PushOffset"/>と対にして呼び出す必要があります。未確定のオフセットがあればそれを、なければ確定済みの累積変換を1段戻します。</remarks>
    public void PopTransform()
    {
        if (_localTransform.Count != 0)
        {
            _localTransform.RemoveAt(_localTransform.Count - 1);
        }
        else
        {
            _transforms.RemoveAt(_transforms.Count - 1);
            Debug.Assert(_transforms.Count != 0);
        }
    }

    /// <summary>現在の累積座標変換を設定してヒットテスト経路へエントリを追加します。</summary>
    /// <param name="entry">追加する対象。指定されている座標変換は現在の累積値で置き換えられます。</param>
    public void Add(HitTestEntry entry) => _pathInternal.Add(entry with { Transform = LastTransform });

    /// <summary>描画時の子要素オフセットを座標へ反映し、子要素のヒットテストを実行します。</summary>
    /// <param name="offset">親座標系における子要素の論理ピクセル単位の位置。位置がない場合は<see langword="null"/>です。</param>
    /// <param name="position">親座標系におけるポインター位置。</param>
    /// <param name="hitTest">この結果と子要素のローカル座標を受け取り、ヒット判定を行う処理。</param>
    /// <returns>子要素がヒットした場合は<see langword="true"/>、ヒットしなかった場合は<see langword="false"/>。</returns>
    /// <remarks>指定したオフセットは処理の呼び出し中だけ積まれ、処理が戻ると元の変換状態へ戻されます。</remarks>
    public bool AddWidthPaintOffset(Offset? offset, Offset position, Func<HitTestResult, Offset, bool> hitTest)
    {
        var transform = offset is not null ? position - offset : position;

        if (offset is not null) PushOffset(-offset.Value);

        var isHit = hitTest(this, (Offset)transform);

        if (offset is not null) PopTransform();

        return isHit;
    }
}
