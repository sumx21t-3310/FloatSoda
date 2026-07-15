namespace FloatSoda.Abstractions.Geometries;

/// <summary>
/// テクスチャの物理密度（dots per meter）を表す値型。
/// ピクセル数と物理長（メートル）の相互変換を担います。
/// </summary>
public readonly record struct Dpm(float Value)
{
    /// <summary>既定の密度（4000 dpm = 1px あたり 0.25mm）。</summary>
    public static Dpm Default => new(4000f);

    /// <summary>1ピクセルあたりの物理長（ミリメートル）から生成します。</summary>
    public static Dpm FromMillimetersPerPixel(float millimeters) => new(1000f / millimeters);

    /// <summary>ピクセル数を物理長（メートル）へ変換します。</summary>
    public float ToMeters(float pixels) => pixels / Value;
}
