using FloatSoda.Abstractions.Scheduling;
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
        builder.Services.AddTransient<IFramePacer>(_ => new FramePacer(fps));
        return builder;
    }
}
