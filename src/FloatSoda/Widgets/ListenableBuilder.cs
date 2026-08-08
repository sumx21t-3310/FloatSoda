using System.ComponentModel;
using FloatSoda.Elements;

namespace FloatSoda.Widgets;

/// <summary>
/// <see cref="INotifyPropertyChanged.PropertyChanged"/>を購読し、通知のたびに直下のウィジェットだけを再構築します。
/// </summary>
/// <remarks>
/// 通知は、このウィジェットがマウントされたスレッド（通常はFloatSodaのメインループ）から発火する必要があります。
/// バックグラウンドスレッドで状態を更新する場合は、呼び出し側でメインループへマーシャリングしてください。
/// </remarks>
public record ListenableBuilder : StatefulWidget<ListenableBuilder>
{
    /// <summary>変更通知を購読する状態オブジェクトを取得します。</summary>
    /// <exception cref="ArgumentNullException"><see langword="null"/>が指定されました。</exception>
    public required INotifyPropertyChanged Listenable
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <summary>現在の状態から、このウィジェットの直下に配置するウィジェットを構築するデリゲートを取得します。</summary>
    /// <exception cref="ArgumentNullException"><see langword="null"/>が指定されました。</exception>
    public required Func<IBuildContext, Widget> ChildBuilder
    {
        get;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            field = value;
        }
    }

    /// <inheritdoc/>
    public override Element CreateElement() => new ListenableBuilderElement
    {
        Widget = this
    };

    /// <inheritdoc/>
    public override State<ListenableBuilder> CreateState() => new ListenableBuilderState();

    private sealed class ListenableBuilderElement : StatefulElement<ListenableBuilder>
    {
        protected override void Deactivate()
        {
            ((ListenableBuilderState)State).DetachListener();
            base.Deactivate();
        }
    }

    private sealed class ListenableBuilderState : State<ListenableBuilder>
    {
        private int _mountThreadId;
        private bool _subscribed;

        public override void InitState()
        {
            _mountThreadId = Environment.CurrentManagedThreadId;
            Subscribe(Widget!.Listenable);
        }

        public override void DidUpdateWidget(ListenableBuilder oldWidget)
        {
            if (ReferenceEquals(oldWidget.Listenable, Widget!.Listenable)) return;

            Unsubscribe(oldWidget.Listenable);
            Subscribe(Widget.Listenable);
        }

        public override Widget Build(IBuildContext context) => Widget!.ChildBuilder(context);

        public override void Dispose()
        {
            DetachListener();
            base.Dispose();
        }

        public void DetachListener() => Unsubscribe(Widget!.Listenable);

        private void Subscribe(INotifyPropertyChanged listenable)
        {
            listenable.PropertyChanged += OnPropertyChanged;
            _subscribed = true;
        }

        private void Unsubscribe(INotifyPropertyChanged listenable)
        {
            if (!_subscribed) return;

            listenable.PropertyChanged -= OnPropertyChanged;
            _subscribed = false;
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
        {
            if (Environment.CurrentManagedThreadId != _mountThreadId)
            {
                throw new InvalidOperationException(
                    "ListenableBuilderのPropertyChangedは、ウィジェットをマウントしたスレッドから通知してください。");
            }

            Element?.MarkNeedsBuild();
        }
    }
}
