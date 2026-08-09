using System.Diagnostics;
using System.Numerics;
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

/// <summary>ヒットした対象と、その対象のローカル座標へ変換する行列を表す不変値です。</summary>
/// <param name="Target">ポインターイベントを受け取る対象。</param>
/// <param name="Transform">グローバル座標から対象のローカル座標へ変換する行列。未指定の場合は<see langword="null"/>です。</param>
/// <seealso cref="IHitTestTarget"/>
/// <seealso cref="HitTestResult"/>
public readonly record struct HitTestEntry(IHitTestTarget Target, Matrix3x2? Transform = null);

/// <summary>ポインター位置でヒットした対象の経路と、走査中の座標変換を保持します。</summary>
/// <remarks>座標変換は<see cref="PushOffset"/>または<see cref="PushTransform"/>と<see cref="PopTransform"/>を対にして管理し、<see cref="Add"/>時点の累積値を各エントリへ保存します。</remarks>
/// <seealso cref="HitTestEntry"/>
public class HitTestResult
{
    /// <summary>ヒットした対象を配送順に保持する読み取り専用の経路を取得します。</summary>
    public IReadOnlyList<HitTestEntry> Path => _pathInternal;

    private readonly List<HitTestEntry> _pathInternal = [];

    private readonly List<Matrix3x2> _transforms = [Matrix3x2.Identity];

    private readonly List<Matrix3x2> _localTransform = [];


    private void GlobalizeTransform()
    {
        if (_localTransform.Count == 0) return;

        var last = _transforms[^1];

        foreach (var part in _localTransform)
        {
            last *= part;
            _transforms.Add(last);
        }

        _localTransform.Clear();
    }

    /// <summary>現在のグローバル座標からローカル座標への累積変換を取得します。</summary>
    /// <remarks>まだ確定していない変換がある場合は、取得時に累積変換へ反映します。</remarks>
    public Matrix3x2 LastTransform
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
    public void PushOffset(Offset offset) => PushTransform(
        Matrix3x2.CreateTranslation((float)offset.X, (float)offset.Y));

    /// <summary>子要素のヒットテスト中に適用するアフィン座標変換を積みます。</summary>
    /// <param name="transform">現在の座標から子要素のローカル座標へ変換する行列。</param>
    /// <remarks>走査を戻る際は<see cref="PopTransform"/>を同じ回数だけ呼び出します。</remarks>
    public void PushTransform(Matrix3x2 transform) => _localTransform.Add(transform);

    /// <summary>直前に積まれた座標変換を取り除きます。</summary>
    /// <remarks><see cref="PushOffset"/>または<see cref="PushTransform"/>と対にして呼び出す必要があります。未確定の変換があればそれを、なければ確定済みの累積変換を1段戻します。</remarks>
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

        try
        {
            return hitTest(this, (Offset)transform);
        }
        finally
        {
            if (offset is not null) PopTransform();
        }
    }

    /// <summary>描画時のアフィン変換を座標へ反映し、子要素のヒットテストを実行します。</summary>
    /// <param name="transform">子要素のローカル座標から親座標へ変換する描画行列。</param>
    /// <param name="position">親座標系におけるポインター位置。</param>
    /// <param name="hitTest">この結果と子要素のローカル座標を受け取り、ヒット判定を行う処理。</param>
    /// <returns>子要素がヒットした場合は<see langword="true"/>、ヒットしなかった場合は<see langword="false"/>。</returns>
    /// <remarks>逆行列を計算できない場合はヒットしません。逆変換は処理の呼び出し中だけ積まれ、処理が戻ると元の変換状態へ戻されます。</remarks>
    public bool AddWithPaintTransform(
        Matrix3x2 transform,
        Offset position,
        Func<HitTestResult, Offset, bool> hitTest)
    {
        if (!Matrix3x2.Invert(transform, out var inverse)) return false;

        var transformed = Vector2.Transform(new Vector2((float)position.X, (float)position.Y), inverse);
        PushTransform(inverse);
        try
        {
            return hitTest(this, new Offset(transformed.X, transformed.Y));
        }
        finally
        {
            PopTransform();
        }
    }
}
