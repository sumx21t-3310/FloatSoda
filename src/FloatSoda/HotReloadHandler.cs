using System.Reflection.Metadata;

[assembly: MetadataUpdateHandler(typeof(FloatSoda.HotReloadHandler))]

namespace FloatSoda;

public static class HotReloadHandler
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("Clearing cache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Console.WriteLine("🔥 application hot reloaded");
    }
}