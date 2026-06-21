using System.Text.Json;
using System.Text.Json.Serialization;

namespace FloatSoda.OVR;

public class VRManifest
{
    [JsonPropertyName("source")] public string Source { get; init; } = "builtin";

    [JsonPropertyName("applications")] public List<VRApplication> Applications { get; init; } = new();

    public string ToJson()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        return JsonSerializer.Serialize(this, options);
    }

    public static VRManifest FromJson(string json)
    {
        return JsonSerializer.Deserialize<VRManifest>(json) ??
               throw new JsonException("Failed to deserialize VRManifest: result was null.");
    }


    public override string ToString() => ToJson();
}

public record VRApplication
{
    [JsonPropertyName("app_key")] public required AppKey AppKey { get; init; }

    [JsonPropertyName("launch_type")] public required LaunchType LaunchType { get; init; }

    [JsonPropertyName("binary_path_windows")]
    public string? BinaryPathWindows { get; init; }

    [JsonPropertyName("binary_path_linux")]
    public string? BinaryPathLinux { get; init; }

    [JsonPropertyName("binary_path_osx")] public string? BinaryPathOsx { get; init; }

    // スペース区切りの引数文字列（例: "--vr --fullscreen"）
    [JsonPropertyName("binary_args")] public string? BinaryArgs { get; init; }

    [JsonPropertyName("working_directory")]
    public string? WorkingDirectory { get; init; }

    [JsonPropertyName("image_path")] public string? ImagePath { get; init; }

    [JsonPropertyName("is_dashboard_overlay")]
    public bool IsDashboardOverlay { get; init; }

    [JsonPropertyName("is_template")] public bool IsTemplate { get; init; }

    [JsonPropertyName("is_instanced")] public bool IsInstanced { get; init; }

    [JsonPropertyName("is_internal")] public bool IsInternal { get; init; }

    // { "en_us": { "name": "My App", "description": "..." } }
    [JsonPropertyName("strings")] public Dictionary<string, AppStrings>? Strings { get; init; }
}

public record AppKey(params string[] parts)
{
    public override string ToString() => string.Join(".", parts.Select(p => p.ToLowerInvariant().Split(' ', '_')));
}

public enum LaunchType
{
    Binary,
    Url,
    Scene
}

public record class AppStrings
{
    [JsonPropertyName("name")] public required string Name { get; init; }

    [JsonPropertyName("description")] public string? Description { get; init; }
}