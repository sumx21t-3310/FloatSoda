namespace FloatSoda.Core.Providers;

/// <summary>テキスト描画に使用するフォントファミリを読み込む方法を表します。</summary>
/// <remarks>
/// フォントは、最初に使われたレイアウトの中で同期的に一度だけ読み込み、アプリケーションの終了まで保持します。
/// 等しいプロバイダー(recordの値の等価性)は同じ読み込み結果を共有します。
/// 画像(<see cref="ImageProvider"/>)と違って解放や入れ替えを行わないため、利用者が破棄するものはありません。
/// </remarks>
public abstract record FontProvider
{
    /// <summary>フォントファミリを同期的に読み込みます。</summary>
    /// <returns>読み込んだフォントリソース。所有権はFloatSodaへ移ります。</returns>
    /// <remarks>
    /// テキストのレイアウト中に、フレームを処理するスレッドから呼び出されます。
    /// 失敗した場合は既定の書体で描画し、次に使われたときに読み込みをやり直します。
    /// </remarks>
    protected abstract FontResource Load();

    internal FontResource LoadResource() => Load();
}

/// <summary>システムへインストールされたフォントファミリを使用するプロバイダーです。</summary>
public sealed record SystemFontProvider : FontProvider
{
    /// <summary>システム上のフォントファミリ名を指定して初期化します。</summary>
    /// <param name="familyName">システム上のフォントファミリ名。</param>
    public SystemFontProvider(string familyName)
    {
        if (string.IsNullOrWhiteSpace(familyName))
        {
            throw new ArgumentException("フォントファミリ名を指定してください。", nameof(familyName));
        }

        FamilyName = familyName;
    }

    /// <summary>システム上のフォントファミリ名を取得します。</summary>
    public string FamilyName { get; }

    /// <inheritdoc/>
    protected override FontResource Load() => FontResource.FromSystem(FamilyName);
}

/// <summary>ファイルからフォントファミリを読み込むプロバイダーです。</summary>
public sealed record FileFontProvider : FontProvider
{
    /// <summary>フォントファイルのパスを指定して初期化します。</summary>
    /// <param name="path">TrueTypeまたはOpenTypeフォントファイルのパス。</param>
    public FileFontProvider(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("フォントファイルのパスを指定してください。", nameof(path));
        }

        Path = path;
    }

    /// <summary>フォントファイルのパスを取得します。</summary>
    public string Path { get; }

    /// <inheritdoc/>
    protected override FontResource Load() => FontResource.FromData(File.ReadAllBytes(Path));
}

/// <summary>
/// 読み込まれたフォントファミリを表し、内部のネイティブフォントリソースを所有します。
/// </summary>
public sealed class FontResource : IDisposable
{
    private readonly string? _systemFamilyName;
    private readonly SkiaSharp.SKTypeface? _fixedTypeface;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<(int Weight, bool IsItalic), SkiaSharp.SKTypeface>
        _systemTypefaces = [];
    private readonly Lock _gate = new();
    private bool _disposed;

    private FontResource(string systemFamilyName)
    {
        _systemFamilyName = systemFamilyName;
        FamilyName = systemFamilyName;
    }

    private FontResource(SkiaSharp.SKTypeface typeface)
    {
        _fixedTypeface = typeface;
        FamilyName = typeface.FamilyName;
    }

    /// <summary>フォントファミリ名を取得します。</summary>
    public string FamilyName { get; }

    /// <summary>TrueTypeまたはOpenTypeのバイト列からフォントリソースを生成します。</summary>
    /// <param name="data">フォントファイルの内容。</param>
    /// <param name="collectionIndex">フォントコレクション内の書体インデックス。</param>
    /// <returns>生成したフォントリソース。</returns>
    /// <exception cref="ArgumentException">フォントデータが空、または有効なフォントではありません。</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="collectionIndex"/>が負です。</exception>
    public static FontResource FromData(ReadOnlyMemory<byte> data, int collectionIndex = 0)
    {
        if (data.IsEmpty)
        {
            throw new ArgumentException("フォントデータを指定してください。", nameof(data));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(collectionIndex);

        using var skData = SkiaSharp.SKData.CreateCopy(data.Span);
        var typeface = SkiaSharp.SKTypeface.FromData(skData, collectionIndex)
            ?? throw new ArgumentException("有効なフォントデータを指定してください。", nameof(data));

        return FromTypeface(typeface);
    }

    internal static FontResource FromSystem(string familyName) => new(familyName);

    internal static FontResource FromTypeface(SkiaSharp.SKTypeface typeface) => new(typeface);

    internal SkiaSharp.SKTypeface Resolve(int weight, bool isItalic)
    {
        // Disposeと直列化しないと、Clearの後にGetOrAddが書体を追加して誰も破棄しなくなる。
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_fixedTypeface is not null)
            {
                return _fixedTypeface;
            }

            return _systemTypefaces.GetOrAdd((weight, isItalic), key =>
                SkiaSharp.SKTypeface.FromFamilyName(
                    _systemFamilyName!,
                    key.Weight,
                    (int)SkiaSharp.SKFontStyleWidth.Normal,
                    key.IsItalic ? SkiaSharp.SKFontStyleSlant.Italic : SkiaSharp.SKFontStyleSlant.Upright)
                ?? throw new InvalidOperationException($"フォントファミリを解決できませんでした: {_systemFamilyName}"));
        }
    }

    /// <summary>所有するネイティブフォントリソースを解放します。</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed) return;

            _disposed = true;
            _fixedTypeface?.Dispose();
            foreach (var typeface in _systemTypefaces.Values)
            {
                typeface.Dispose();
            }

            _systemTypefaces.Clear();
        }
    }
}
