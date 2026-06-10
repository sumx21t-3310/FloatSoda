using SkiaSharp;

namespace FloatSoda.Render;

public abstract record ImageProvider
{
    public abstract SKImage Load();
}

public record FileImageProvider(string Path) : ImageProvider
{
    public override SKImage Load()
    {
        using var stream = File.OpenRead(Path);
        return SKImage.FromEncodedData(stream);
    }
}