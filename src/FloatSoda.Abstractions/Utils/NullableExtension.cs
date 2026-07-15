namespace FloatSoda.Abstractions.Utils;

public static class NullableExtension
{
    public static void IfNotNull<T>(this T? nullable, Action<T> action) where T : struct
    {
        if (nullable.HasValue) action(nullable.Value);
    }

    public static void IfNotNull<T>(this T? nullable, Action<T> action) where T : class
    {
        if (nullable != null) action(nullable);
    }
}