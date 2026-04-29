using System.Reflection;
using System.Text.RegularExpressions;

namespace FloatSoda.Engine.OVR;

public class SteamVRKeyUtil
{
    public static string CreateKeyFromAssembly()
    {
        var domain = Assembly.GetExecutingAssembly().GetName().Name ?? "";

        return !string.IsNullOrEmpty(domain) ? Sanitize(domain) : $"float_soda_app_{Guid.NewGuid():N}";
    }

    public static string CreateWindowKey(string key, string name)
    {
        return $"{Sanitize(key)}.{Sanitize(name)}";
    }

    public static string Sanitize(string str)
    {
        if (string.IsNullOrEmpty(str)) return "";
        
        var result = str.ToLower().Replace(" ", "_");
        return Regex.Replace(result,  "[^a-z0-9._-]", "");
    }
}