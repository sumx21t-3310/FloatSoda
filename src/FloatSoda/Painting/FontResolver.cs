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
        var resource = _resources.GetOrAdd(
            provider,
            static key => new Lazy<FontResource>(
                () => key.LoadResource(),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;

        typeface = resource.Resolve(style.FontWeight, style.FontItalic);
        return true;
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
