using System.Reflection.Metadata;
using FloatSoda.Core;

[assembly: MetadataUpdateHandler(typeof(HotReloadHandler))]

namespace FloatSoda.Core;

/// <summary>
/// dotnet watch / IDE のホットリロード通知を受け取り、
/// 稼働中の全<see cref="FloatSoda.FloatSodaApp"/>のWidgetツリー再ビルドをスケジュールする。
/// 任意スレッドから呼ばれるため、実際の再ビルドはメインループへマーシャリングされる。
/// </summary>
public static class HotReloadHandler
{
    public static void ClearCache(Type[]? updatedTypes)
    {
        // 現状破棄すべきキャッシュはない（ImageProvider等のキャッシュ導入時にここへ接続する）
        Console.WriteLine("[🥤FloatSoda HotReload] 🗑️ ClearCache");
    }

    public static void UpdateApplication(Type[]? updatedTypes)
    {
        Console.WriteLine("[🥤FloatSoda HotReload] 🔥 コード差し替えを検知、Widgetツリーを再ビルドします");

        if (updatedTypes != null)
        {
            foreach (var type in updatedTypes)
            {
                Console.WriteLine($"  ✏️ {type.FullName}");
            }
        }

        FloatSoda.FloatSodaApp.ScheduleReassembleAll();
    }
}
