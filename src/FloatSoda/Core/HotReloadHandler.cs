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
    /// <summary>更新された型に関連するキャッシュを破棄します。</summary>
    /// <param name="updatedTypes">更新された型の配列。更新対象を特定できない場合は<see langword="null"/>です。</param>
    /// <remarks>現在は破棄対象のキャッシュを持たず、呼び出しをコンソールへ記録します。</remarks>
    public static void ClearCache(Type[]? updatedTypes)
    {
        // 現状破棄すべきキャッシュはない（ImageProvider等のキャッシュ導入時にここへ接続する）
        Console.WriteLine("[🥤FloatSoda HotReload] 🗑️ ClearCache");
    }

    /// <summary>コード更新を稼働中のアプリケーションへ通知します。</summary>
    /// <param name="updatedTypes">更新された型の配列。更新対象を特定できない場合は<see langword="null"/>です。</param>
    /// <remarks>稼働中の各アプリケーションへ再構築を予約し、実際のWidgetツリー再構築は各メインループの次フレームで行われます。</remarks>
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
