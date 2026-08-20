using System.Collections.Concurrent;
using FloatSoda.Core.Providers;
using SkiaSharp;
using Topten.RichTextKit;

namespace FloatSoda.Painting;

/// <summary>FontProviderをRichTextKitのフォントファミリへ関連付けます。</summary>
internal sealed class FontResolver
{
    internal static FontResolver Shared { get; } = new();

    private const string FamilyPrefix = "$FloatSoda.Font.";
    private readonly ConcurrentDictionary<FontProvider, string> _families = [];
    private readonly ConcurrentDictionary<string, FontProvider> _providers = [];
    private readonly ConcurrentDictionary<FontProvider, Lazy<FontResource>> _resources = [];
    private long _nextFamilyId;

    private FontResolver()
    {
    }

    internal string Register(FontProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);

        var family = _families.GetOrAdd(
            provider,
            _ => FamilyPrefix + Interlocked.Increment(ref _nextFamilyId));
        _providers.TryAdd(family, provider);
        return family;
    }

    internal bool TryResolve(IStyle style, out SKTypeface? typeface)
    {
        if (!_providers.TryGetValue(style.FontFamily, out var provider))
        {
            typeface = null;
            return false;
        }

        // TypefaceFromStyleはレイアウト/描画中に同期的に呼ばれるため、I/Oランナーへ投げて待つと
        // 先にキューへ並んだ画像読み込みが全部終わるまでフレームスレッドが止まる。
        // ここでは呼び出しスレッド上で直接読み込み、キュー越しのブロックを避ける。
        var lazy = _resources.GetOrAdd(
            provider,
            static key => new Lazy<FontResource>(
                () => key.LoadResource(),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            typeface = lazy.Value.Resolve(style.FontWeight, style.FontItalic);
            return true;
        }
        catch (Exception)
        {
            // ここで投げるとレイアウト経路を貫通してFloatSodaApp.MainLoopまで届き、
            // フォント1つの失敗でアプリ全体が停止する。既定の書体へフォールバックする。
            //
            // Lazyは例外もキャッシュするため、失敗したエントリを取り除いて次フレーム以降で
            // 再試行できるようにする。取り除かないと、原因を解消しても永久に失敗し続ける。
            _resources.TryRemove(new KeyValuePair<FontProvider, Lazy<FontResource>>(provider, lazy));
            typeface = null;
            return false;
        }
    }
}

/// <summary>FloatSodaのFontProviderを優先してRichTextKitの書体へ変換します。</summary>
internal sealed class FloatSodaFontMapper : FontMapper
{
    internal static FloatSodaFontMapper Instance { get; } = new();

    private FloatSodaFontMapper()
    {
    }

    public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants)
    {
        return FontResolver.Shared.TryResolve(style, out var typeface)
            ? typeface!
            : base.TypefaceFromStyle(style, ignoreFontVariants);
    }
}
