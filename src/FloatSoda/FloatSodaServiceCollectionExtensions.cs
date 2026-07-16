using FloatSoda.Abstractions.Scheduling;
using FloatSoda.Engine;
using FloatSoda.OVR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace FloatSoda;

/// <summary>
/// FloatSodaをGeneric Hostへ登録する拡張メソッドです。
/// </summary>
public static class FloatSodaServiceCollectionExtensions
{
    /// <summary>
    /// FloatSodaランタイムをGeneric Hostへ登録します。
    /// WindowはHostの構築後に<see cref="FloatSodaApp.CreateWindow"/>で作成します。
    /// </summary>
    /// <param name="services">Hostのサービスコレクション。</param>
    /// <param name="options">FloatSodaランタイムの設定。</param>
    /// <returns>同じサービスコレクション。</returns>
    public static IServiceCollection AddFloatSoda(
        this IServiceCollection services,
        FloatSodaOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        options ??= new FloatSodaOptions();

        if (options.TargetFrameRate <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.TargetFrameRate,
                "TargetFrameRateは1以上である必要があります。");
        }

        services.TryAddSingleton(options);
        services.TryAddSingleton(new OVRAppInfo(options.AppKey));
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<IFramePacer>(_ => new FramePacer(options.TargetFrameRate));

        services.TryAddSingleton<FloatSodaApp>(provider =>
        {
            var mainFramePacer = provider.GetRequiredService<IFramePacer>();
            var renderFramePacer = provider.GetRequiredService<IFramePacer>();
            var appInfo = provider.GetRequiredService<OVRAppInfo>();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var timeProvider = provider.GetRequiredService<TimeProvider>();

            return new FloatSodaApp(
                mainFramePacer,
                renderFramePacer,
                appInfo,
                loggerFactory,
                timeProvider);
        });

        services.AddHostedService<FloatSodaHostedService>();

        return services;
    }
}
