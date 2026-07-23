using FloatSoda.Widgets;

namespace FloatSoda.Animation;

/// <summary>
/// <see cref="ITickerProvider"/>を提供する<see cref="State{T}"/>基底。
/// FlutterのTickerProviderStateMixin相当で、AnimationControllerの<c>Vsync = this</c>に渡せます。
/// 生成されたTickerは自Elementが属するウィンドウのWidgetBinding(BuildOwner.FrameScheduler)から
/// フレームコールバックを受け取ります。
/// </summary>
public abstract class TickerProviderState<T> : State<T>, ITickerProvider
    where T : StatefulWidget<T>
{
    // 継承ではなくTickerProviderへの委譲で実装する。
    // スケジューラはマウント後でないと確定しないため遅延解決にしている。
    private TickerProvider? _tickerProvider;

    private TickerProvider TickerProvider => _tickerProvider ??= new TickerProvider
    {
        ResolveScheduler = () => Element?.Owner?.FrameScheduler
    };

    /// <inheritdoc />
    public WidgetTicker CreateTicker(Action<TimeSpan> onTick) => TickerProvider.CreateTicker(onTick);

    /// <inheritdoc />
    public void RemoveTicker(WidgetTicker ticker) => TickerProvider.RemoveTicker(ticker);
}
