using FloatSoda.Core;
using FloatSoda.Core.Providers;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets.Paint;
using FloatSoda.Engine;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class FontProviderTest
{
    private sealed class LoadCounter
    {
        public int LoadCount { get; private set; }
        public string? ThreadName { get; private set; }

        public void Increment()
        {
            LoadCount++;
            ThreadName = Thread.CurrentThread.Name;
        }
    }

    private sealed record CountingFontProvider(Guid Key, LoadCounter Counter) : FontProvider
    {

        protected override FontResource Load(CancellationToken cancellationToken)
        {
            Counter.Increment();
            return FontResource.FromSystem("Arial");
        }
    }

    [Fact]
    public async Task LoadAsync_システムフォントを指定_フォントリソースを返す()
    {
        using var resource = await new SystemFontProvider("Arial").LoadAsync();

        Assert.Equal("Arial", resource.FamilyName);
    }

    [Fact]
    public async Task LoadAsync_IOTaskRunnerを指定_専用スレッドで読み込む()
    {
        using var runner = new IOTaskRunner("ProviderIOThread");
        var provider = new CountingFontProvider(Guid.NewGuid(), new LoadCounter());
        runner.Start();

        using var resource = await provider.LoadAsync(runner);

        Assert.Equal("ProviderIOThread", provider.Counter.ThreadName);
    }

    [Fact]
    public async Task LoadAsync_有効なフォントファイルを指定_フォントリソースを返す()
    {
        var path = Path.Combine(Path.GetTempPath(), $"floatsoda-font-{Guid.NewGuid():N}.ttf");
        using var stream = SKTypeface.Default.OpenStream();
        Assert.NotNull(stream);
        var data = new byte[stream.Length];
        Assert.Equal(data.Length, stream.Read(data, data.Length));
        await File.WriteAllBytesAsync(path, data);

        try
        {
            using var resource = await new FileFontProvider(path).LoadAsync();

            Assert.False(string.IsNullOrWhiteSpace(resource.FamilyName));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SystemFontProvider_フォントファミリ名が空白_ArgumentExceptionを投げる(string familyName)
    {
        Assert.Throws<ArgumentException>(() => new SystemFontProvider(familyName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void FileFontProvider_パスが空白_ArgumentExceptionを投げる(string path)
    {
        Assert.Throws<ArgumentException>(() => new FileFontProvider(path));
    }

    [Fact]
    public void FromData_フォントデータが空_ArgumentExceptionを投げる()
    {
        Assert.Throws<ArgumentException>(() => FontResource.FromData(ReadOnlyMemory<byte>.Empty));
    }

    [Fact]
    public void Layout_同じFontProviderを複数回使用_リソースを一度だけ読み込む()
    {
        var counter = new LoadCounter();
        var provider = new CountingFontProvider(Guid.NewGuid(), counter);
        var style = new TextStyle { Font = provider };

        var first = new TextPainter { Text = new TextSpan("first") { Style = style } };
        first.Layout(0, double.PositiveInfinity);

        var second = new TextPainter { Text = new TextSpan("second") { Style = style } };
        second.Layout(0, double.PositiveInfinity);

        Assert.Equal(1, counter.LoadCount);
    }

    [Fact]
    public void Build_IconDataを指定_Textと同じFontProviderへ委譲する()
    {
        var provider = new SystemFontProvider("Arial");
        var icon = new Icon(new IconData(0xe88a, provider));

        var sizedBox = Assert.IsType<FloatSoda.Widgets.Layout.SizedBox>(icon.Build(null!));
        var center = Assert.IsType<FloatSoda.Widgets.Layout.Center>(sizedBox.Child);
        var richText = Assert.IsType<FloatSoda.Widgets.RichText>(center.Child);

        Assert.Same(provider, richText.Text.Style!.Font);
        Assert.Equal(char.ConvertFromUtf32(0xe88a), richText.Text.Text);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0xd800)]
    [InlineData(0x110000)]
    public void IconData_コードポイントが無効_ArgumentOutOfRangeExceptionを投げる(int codePoint)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IconData(codePoint, new SystemFontProvider("Arial")));
    }
}
