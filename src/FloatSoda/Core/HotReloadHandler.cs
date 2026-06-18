using System.Reflection.Metadata;
using FloatSoda.Core;

[assembly: MetadataUpdateHandler(typeof(HotReloadHandler))]

namespace FloatSoda.Core;

public static class HotReloadHandler
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        Console.WriteLine("[🥤Float Soda HotReloadHandler] 🗑️Clearing cache");

        if (updatedTypes == null) return;

        foreach (var type in updatedTypes)
        {
            Console.WriteLine($"✏️{type.FullName}");
        }
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Console.WriteLine("[🥤Float Soda HotReloadHandler]🔥 application hot reloaded");

        if (updatedTypes == null) return;

        foreach (var type in updatedTypes)
        {
            Console.WriteLine($"{type.FullName} {type.Assembly.GetName().Name}");
        }
    }
}