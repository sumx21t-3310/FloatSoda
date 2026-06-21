using System.Reflection;

namespace FloatSoda.Core;

internal class WindowKeyGenerator
{
    private string _pattern = "";
    
    private static readonly string Endpoint = 
        Assembly.GetEntryAssembly()?.GetName().Name ?? $"overlay_app.{Guid.NewGuid().ToString("N").ToLower()}";

    public static string GenerateKey(string windowName)
    {
        var sanitizedEndPointName = Sanitized(Endpoint);
        var sanitizedWindowName = Sanitized(windowName);
        
        return $"{sanitizedEndPointName}.{sanitizedWindowName}";
    }

    private static string Sanitized(string text) => text.Replace(" ", "").Replace(".", "_").ToLower();
}