using SkiaSharp;

namespace FloatSoda.Geometrics;

using static Double;

/// <summary>レイアウトで許容する幅と高さの範囲を論理ピクセル単位で表す不変値です。</summary>
/// <param name="MinWidth">許容する最小幅。既定値は0です。</param>
/// <param name="MaxWidth">許容する最大幅。既定値は正の無限大です。</param>
/// <param name="MinHeight">許容する最小高さ。既定値は0です。</param>
/// <param name="MaxHeight">許容する最大高さ。既定値は正の無限大です。</param>
/// <seealso cref="FloatSoda.RenderObjects.RenderBox"/>
public readonly record struct BoxConstraints(
    double MinWidth = 0,
    double MaxWidth = PositiveInfinity,
    double MinHeight = 0,
    double MaxHeight = PositiveInfinity)
{
    /// <summary>
    /// 下限0・上限無限の完全に緩い制約。子はコンテンツサイズに収縮（shrink-wrap）します。
    /// （<c>default(BoxConstraints)</c> は全て0になるため、無限上限が必要な場合はこれを使うこと）
    /// </summary>
    public static BoxConstraints Unbounded => new(0, PositiveInfinity, 0, PositiveInfinity);

    /// <summary>幅と高さを指定サイズへ固定する制約を作成します。</summary>
    /// <param name="size">固定する論理ピクセル単位の大きさ。</param>
    /// <returns>最小値と最大値が指定サイズに等しい制約。</returns>
    public static BoxConstraints Tight(SKSize size) => Tight(size.Width, size.Height);

    /// <summary>幅と高さを指定値へ固定する制約を作成します。</summary>
    /// <param name="width">固定する論理ピクセル単位の幅。</param>
    /// <param name="height">固定する論理ピクセル単位の高さ。</param>
    /// <returns>最小値と最大値が指定値に等しい制約。</returns>
    public static BoxConstraints Tight(double width, double height) => new(width, width, height, height);

    /// <summary>指定された軸だけを固定し、未指定の軸を制限しない制約を作成します。</summary>
    /// <param name="width">固定する論理ピクセル単位の幅。<see langword="null"/>の場合は幅を固定しません。</param>
    /// <param name="height">固定する論理ピクセル単位の高さ。<see langword="null"/>の場合は高さを固定しません。</param>
    /// <returns>指定された軸だけが固定された制約。</returns>
    public static BoxConstraints TightFor(double? width = null, double? height = null) => new()
    {
        MinWidth = width ?? 0,
        MaxWidth = width ?? PositiveInfinity,
        MinHeight = height ?? 0,
        MaxHeight = height ?? PositiveInfinity
    };

    /// <summary>最小値を0とし、指定値までを許容する緩い制約を作成します。</summary>
    /// <param name="width">許容する論理ピクセル単位の最大幅。</param>
    /// <param name="height">許容する論理ピクセル単位の最大高さ。</param>
    /// <returns>最小値が0で最大値が指定値の制約。</returns>
    public static BoxConstraints Loose(double width, double height) => new()
    {
        MaxHeight = height,
        MaxWidth = width
    };

    /// <summary>最小値を0とし、指定サイズまでを許容する緩い制約を作成します。</summary>
    /// <param name="size">許容する論理ピクセル単位の最大サイズ。</param>
    /// <returns>最小値が0で最大値が指定サイズの制約。</returns>
    public static BoxConstraints Loose(SKSize size) => Loose(size.Width, size.Height);


    /// <summary>この制約の各境界値を、指定された制約が許容する範囲内へ収めます。</summary>
    /// <param name="constraints">各境界値の下限と上限として適用する制約。</param>
    /// <returns>各境界値を指定範囲内へ収めた新しい制約。</returns>
    /// <exception cref="ArgumentException"><paramref name="constraints"/>のいずれかの最小値が対応する最大値を上回っています。</exception>
    public BoxConstraints Enforce(BoxConstraints constraints) => new(
        Math.Clamp(MinWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MaxWidth, constraints.MinWidth, constraints.MaxWidth),
        Math.Clamp(MinHeight, constraints.MinHeight, constraints.MaxHeight),
        Math.Clamp(MaxHeight, constraints.MinHeight, constraints.MaxHeight)
    );

    /// <summary>幅をこの制約の最小幅と最大幅の範囲内へ収めます。</summary>
    /// <param name="width">制約する論理ピクセル単位の幅。</param>
    /// <returns>許容範囲内へ収めた幅。</returns>
    /// <exception cref="ArgumentException"><see cref="MinWidth"/>が<see cref="MaxWidth"/>を上回っています。</exception>
    public double ConstrainWidth(double width) => Math.Clamp(width, MinWidth, MaxWidth);
    /// <summary>高さをこの制約の最小高さと最大高さの範囲内へ収めます。</summary>
    /// <param name="height">制約する論理ピクセル単位の高さ。</param>
    /// <returns>許容範囲内へ収めた高さ。</returns>
    /// <exception cref="ArgumentException"><see cref="MinHeight"/>が<see cref="MaxHeight"/>を上回っています。</exception>
    public double ConstrainHeight(double height) => Math.Clamp(height, MinHeight, MaxHeight);

    /// <summary>最大値を維持したまま、幅と高さの最小値を0にした制約を取得します。</summary>
    public BoxConstraints Loosen => new(0, MaxWidth, 0, MaxHeight);
    /// <summary>この制約が許容する最小のサイズを取得します。</summary>
    /// <exception cref="ArgumentException">いずれかの最小値が対応する最大値を上回っています。</exception>
    public SKSize Smallest => new((float)ConstrainWidth(0), (float)ConstrainHeight(0));

    /// <summary>指定サイズをこの制約が許容する範囲内へ収めます。</summary>
    /// <param name="size">制約する論理ピクセル単位のサイズ。</param>
    /// <returns>各軸を許容範囲内へ収めたサイズ。</returns>
    /// <exception cref="ArgumentException">いずれかの最小値が対応する最大値を上回っています。</exception>
    public SKSize Constrain(SKSize size) => Constrain(size.Width, size.Height);

    /// <summary>指定された幅と高さをこの制約が許容する範囲内へ収めます。</summary>
    /// <param name="width">制約する論理ピクセル単位の幅。</param>
    /// <param name="height">制約する論理ピクセル単位の高さ。</param>
    /// <returns>各軸を許容範囲内へ収めたサイズ。</returns>
    /// <exception cref="ArgumentException">いずれかの最小値が対応する最大値を上回っています。</exception>
    public SKSize Constrain(double width, double height) => new(
        (float)ConstrainWidth(width),
        (float)ConstrainHeight(height)
    );


    /// <summary>幅の下限が上限以上であり、自由に選べる幅がないかを取得します。</summary>
    public bool HasTightWidth => MinWidth >= MaxWidth;
    /// <summary>高さの下限が上限以上であり、自由に選べる高さがないかを取得します。</summary>
    public bool HasTightHeight => MinHeight >= MaxHeight;

    /// <summary>幅と高さの両方について自由に選べる大きさがないかを取得します。</summary>
    public bool IsTight => HasTightWidth && HasTightHeight;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"BoxConstraints {{ width: (min = {MinWidth}, max = {MaxWidth}), height: (min = {MinHeight}, max = {MaxHeight}) }}";
    }
}