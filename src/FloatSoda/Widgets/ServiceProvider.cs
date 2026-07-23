using FloatSoda.Elements;
using Microsoft.Extensions.DependencyInjection;

namespace FloatSoda.Widgets;

/// <summary>
/// 子孫のBuildContextへ依存性注入コンテナーを公開するInheritedWidgetです。
/// </summary>
public record ServiceProvider : InheritedWidget
{
    /// <summary>
    /// 子孫からサービスを解決するときに使用する依存性注入コンテナーを取得します。
    /// </summary>
    public required IServiceProvider Services { get; init; }

    /// <summary>
    /// 最も近い祖先の<see cref="ServiceProvider"/>から依存性注入コンテナーを取得し、
    /// 呼び出し元をそのInheritedWidgetの依存対象として登録します。
    /// </summary>
    /// <param name="context">サービスを要求するウィジェットのBuildContext。</param>
    /// <returns>最も近い祖先が公開している依存性注入コンテナー。</returns>
    /// <exception cref="InvalidOperationException">
    /// 祖先に<see cref="ServiceProvider"/>が存在しない場合にスローされます。
    /// </exception>
    public static IServiceProvider Of(IBuildContext context)
    {
        return context.DependOnInheritedWidgetOfExactType<ServiceProvider>()?.Services ??
               throw new InvalidOperationException("ServiceScopeが祖先に見つかりません。");
    }

    /// <summary>
    /// 依存性注入コンテナーの参照が変更され、依存するElementの再ビルドが必要かを判定します。
    /// </summary>
    /// <param name="oldWidget">更新前のInheritedWidget。</param>
    /// <returns>
    /// 更新前のウィジェットが<see cref="ServiceProvider"/>であり、
    /// <see cref="Services"/>の参照が変更された場合は<see langword="true"/>。
    /// それ以外の場合は<see langword="false"/>。
    /// </returns>
    public override bool UpdateShouldNotify(InheritedWidget oldWidget)
    {
        return oldWidget is ServiceProvider oldScope && !ReferenceEquals(oldScope.Services, Services);
    }
}

/// <summary>
/// BuildContextから祖先の依存性注入コンテナーを使用してサービスを解決する拡張メソッドを提供します。
/// </summary>
public static class ServiceScopeExtension
{
    /// <summary>
    /// 最も近い祖先の依存性注入コンテナーから、登録必須のサービスを取得します。
    /// </summary>
    /// <typeparam name="T">取得するサービスの型。</typeparam>
    /// <param name="context">サービスを要求するウィジェットのBuildContext。</param>
    /// <returns>依存性注入コンテナーに登録されているサービス。</returns>
    /// <exception cref="InvalidOperationException">
    /// 祖先に<see cref="ServiceProvider"/>が存在しない場合、または要求したサービスが登録されていない場合に
    /// スローされます。
    /// </exception>
    public static T GetRequiredService<T>(this IBuildContext context) where T : notnull
    {
        return ServiceProvider.Of(context).GetRequiredService<T>();
    }

    /// <summary>
    /// 最も近い祖先の依存性注入コンテナーから、登録されている場合にサービスを取得します。
    /// </summary>
    /// <typeparam name="T">取得するサービスの型。</typeparam>
    /// <param name="context">サービスを要求するウィジェットのBuildContext。</param>
    /// <returns>登録されているサービス。登録されていない場合は<see langword="null"/>。</returns>
    /// <exception cref="InvalidOperationException">
    /// 祖先に<see cref="ServiceProvider"/>が存在しない場合にスローされます。
    /// </exception>
    public static T? GetService<T>(this IBuildContext context)
    {
        return ServiceProvider.Of(context).GetService<T>();
    }
}