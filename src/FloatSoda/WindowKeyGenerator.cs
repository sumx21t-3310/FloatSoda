using System.Reflection;

namespace FloatSoda;

internal class WindowKeyGenerator
{
    private static readonly string Endpoint = 
        Assembly.GetEntryAssembly()?.GetName().Name ?? $"overlay_app.{Guid.NewGuid().ToString("N").ToLower()}";

    public static string CreateWindowKey(string windowName)
    {
        var sanitizedEndPointName = Sanitized(Endpoint);
        var sanitizedWindowName = Sanitized(windowName);
        
        return $"{sanitizedEndPointName}.{sanitizedWindowName}";
    }

    private static string Sanitized(string text) => text.Replace(" ", "").Replace(".", "_").ToLower();
}