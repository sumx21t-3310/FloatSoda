using System.Numerics;
using System.Text.Json.Nodes;
using FloatSoda.OVR;
using FloatSoda.OVR.Input;

namespace FloatSoda.Test.Input;

public class ActionManifestWriterTest : IDisposable
{
    private readonly string _outputDirectory =
        Path.Combine(Path.GetTempPath(), "FloatSoda.Test", Guid.NewGuid().ToString("N"));

    private static InputActionMap CreateMap() => new()
    {
        Name = "main",
        Actions =
        [
            new InputAction<bool>
            {
                Name = "grab",
                SuggestedPath = "/user/hand/right/input/trigger/click",
            },
            new InputAction<float>
            {
                Name = "squeeze",
                SuggestedPath = "/user/hand/right/input/trigger/pull",
            },
            new InputAction<Vector2>
            {
                Name = "scroll",
                DefaultBindings =
                [
                    new() { Controller = ControllerType.Index, Path = "/user/hand/right/input/thumbstick" },
                    new() { Controller = ControllerType.OculusTouch, Path = "/user/hand/right/input/joystick" },
                ],
            },
        ],
    };

    private JsonObject WriteAndParseManifest(out string manifestPath)
    {
        manifestPath = ActionManifestWriter.Write(new AppKey("com.example.test"), [CreateMap()], _outputDirectory);
        return (JsonObject)JsonNode.Parse(File.ReadAllText(manifestPath))!;
    }

    private JsonObject ParseBindings(ControllerType controller)
    {
        var name = controller switch
        {
            ControllerType.Index => "knuckles",
            ControllerType.ViveWand => "vive_controller",
            _ => "oculus_touch",
        };
        var path = Path.Combine(_outputDirectory, $"bindings_{name}.json");
        return (JsonObject)JsonNode.Parse(File.ReadAllText(path))!;
    }

    [Fact]
    public void Write_ReturnsManifestPathInsideOutputDirectory()
    {
        WriteAndParseManifest(out var manifestPath);

        Assert.Equal(_outputDirectory, Path.GetDirectoryName(manifestPath));
        Assert.True(File.Exists(manifestPath));
    }

    [Fact]
    public void Manifest_ContainsActionSetAndActionPaths()
    {
        var manifest = WriteAndParseManifest(out _);

        var setNames = manifest["action_sets"]!.AsArray().Select(n => (string)n!["name"]!);
        Assert.Contains("/actions/main", setNames);

        var actionNames = manifest["actions"]!.AsArray().Select(n => (string)n!["name"]!).ToList();
        Assert.Contains("/actions/main/in/grab", actionNames);
        Assert.Contains("/actions/main/in/squeeze", actionNames);
        Assert.Contains("/actions/main/in/scroll", actionNames);
    }

    [Fact]
    public void Manifest_MapsActionValueTypesToManifestTypes()
    {
        var manifest = WriteAndParseManifest(out _);

        var types = manifest["actions"]!.AsArray()
            .ToDictionary(n => (string)n!["name"]!, n => (string)n!["type"]!);

        Assert.Equal("boolean", types["/actions/main/in/grab"]);
        Assert.Equal("vector1", types["/actions/main/in/squeeze"]);
        Assert.Equal("vector2", types["/actions/main/in/scroll"]);
    }

    [Fact]
    public void Manifest_ReferencesBindingFileForEveryControllerType()
    {
        var manifest = WriteAndParseManifest(out _);

        var controllers = manifest["default_bindings"]!.AsArray()
            .Select(n => (string)n!["controller_type"]!).ToList();

        Assert.Equal(["knuckles", "vive_controller", "oculus_touch"], controllers);
        foreach (var url in manifest["default_bindings"]!.AsArray().Select(n => (string)n!["binding_url"]!))
        {
            Assert.True(File.Exists(Path.Combine(_outputDirectory, url)));
        }
    }

    [Fact]
    public void SuggestedPath_IsReplicatedToEveryControllerType()
    {
        WriteAndParseManifest(out _);

        foreach (var controller in new[] { ControllerType.Index, ControllerType.ViveWand, ControllerType.OculusTouch })
        {
            var sources = ParseBindings(controller)["bindings"]!["/actions/main"]!["sources"]!.AsArray();
            var grab = sources.Single(s =>
                (string)s!["inputs"]!["click"]?["output"]! == "/actions/main/in/grab");

            Assert.Equal("/user/hand/right/input/trigger", (string)grab!["path"]!);
            Assert.Equal("button", (string)grab["mode"]!);
        }
    }

    [Fact]
    public void DefaultBindings_AreEmittedOnlyForDeclaredController()
    {
        WriteAndParseManifest(out _);

        var indexSources = ParseBindings(ControllerType.Index)["bindings"]!["/actions/main"]!["sources"]!.AsArray();
        var scroll = indexSources.Single(s => (string)s!["path"]! == "/user/hand/right/input/thumbstick");
        Assert.Equal("joystick", (string)scroll!["mode"]!);
        Assert.Equal("/actions/main/in/scroll", (string)scroll["inputs"]!["position"]!["output"]!);

        var viveSources = ParseBindings(ControllerType.ViveWand)["bindings"]!["/actions/main"]!["sources"]!.AsArray();
        Assert.DoesNotContain(viveSources,
            s => (string?)s?["inputs"]?["position"]?["output"] == "/actions/main/in/scroll");
    }

    [Fact]
    public void FloatAction_UsesTriggerModeWithPullComponent()
    {
        WriteAndParseManifest(out _);

        var sources = ParseBindings(ControllerType.Index)["bindings"]!["/actions/main"]!["sources"]!.AsArray();
        var squeeze = sources.Single(s =>
            (string?)s?["inputs"]?["pull"]?["output"] == "/actions/main/in/squeeze");

        Assert.Equal("/user/hand/right/input/trigger", (string)squeeze!["path"]!);
        Assert.Equal("trigger", (string)squeeze["mode"]!);
    }

    [Fact]
    public void InputAction_RejectsUnsupportedValueType()
    {
        Assert.Throws<TypeInitializationException>(() => new InputAction<int> { Name = "bad" });
    }

    public void Dispose()
    {
        if (Directory.Exists(_outputDirectory)) Directory.Delete(_outputDirectory, recursive: true);
    }
}
