using FloatSoda.Engine;
using Microsoft.Extensions.DependencyInjection;

namespace FloatSoda;

public static class FloatSodaAppExtensions
{
    /// <summary>
    /// フレームレートを指定します。デフォルトは 30fps です。
    /// </summary>
    public static FloatSodaAppBuilder WithTargetFrameRate(this FloatSodaAppBuilder builder, int fps)
    {
        builder.Services.AddScoped<IFrameLimiter>(_ => new FrameLimiter(fps));
        return builder;
    }
}