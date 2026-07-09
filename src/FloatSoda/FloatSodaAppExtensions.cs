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

    /// <summary>
    /// OpenVR Compositor の WaitGetPoses でフレームタイミングを制御します。
    /// SteamVR が起動していない場合は初期化時に例外がスローされます。
    /// </summary>
    public static FloatSodaAppBuilder WithOpenVRFrameLimiter(this FloatSodaAppBuilder builder)
    {
        builder.Services.AddScoped<IFrameLimiter, OpenVRFrameLimiter>();
        return builder;
    }
}