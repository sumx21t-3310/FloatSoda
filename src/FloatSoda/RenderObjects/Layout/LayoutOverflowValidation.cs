using FloatSoda.Geometrics;

namespace FloatSoda.RenderObjects.Layout;

/// <summary>overflow系レイアウトRenderObjectの共通入力検証を提供します。</summary>
internal static class LayoutOverflowValidation
{
    /// <summary>配置値が有限であることを検証します。</summary>
    internal static void ValidateAlignment(Alignment alignment, string parameterName)
    {
        if (!float.IsFinite(alignment.X) || !float.IsFinite(alignment.Y))
        {
            throw new ArgumentOutOfRangeException(parameterName, alignment, "配置値には有限値を指定してください。");
        }
    }

    /// <summary>最小寸法が0以上の有限値であることを検証します。</summary>
    internal static void ValidateMinimum(double? value, string parameterName)
    {
        if (value is not null && (!double.IsFinite(value.Value) || value.Value < 0))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "最小寸法には0以上の有限値を指定してください。");
        }
    }

    /// <summary>最大寸法が0以上の値または正の無限大であることを検証します。</summary>
    internal static void ValidateMaximum(double? value, string parameterName)
    {
        if (value is not null && (double.IsNaN(value.Value) || double.IsNegativeInfinity(value.Value) || value.Value < 0))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "最大寸法には0以上の値または正の無限大を指定してください。");
        }
    }

    /// <summary>最小寸法が最大寸法以下であることを検証します。</summary>
    internal static void ValidateRange(double minimum, double maximum, string parameterName)
    {
        if (minimum > maximum)
        {
            throw new ArgumentException("最小寸法は最大寸法以下である必要があります。", parameterName);
        }
    }
}
