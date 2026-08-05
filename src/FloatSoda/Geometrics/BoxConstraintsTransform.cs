namespace FloatSoda.Geometrics;

/// <summary>
/// 親から受け取ったボックス制約を、子へ渡す別のボックス制約へ変換します。
/// </summary>
/// <param name="constraints">親から受け取った正規化済みの制約。</param>
/// <returns>子へ渡す正規化済みの制約。</returns>
/// <remarks>
/// 戻り値の最小値は0以上の有限値、最大値は対応する最小値以上の値または正の無限大である必要があります。
/// </remarks>
public delegate BoxConstraints BoxConstraintsTransform(BoxConstraints constraints);
