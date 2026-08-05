using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class ListenableBuilderTest
{
    private sealed class TestNotifier : INotifyPropertyChanged
    {
        private PropertyChangedEventHandler? _propertyChanged;

        public int SubscriberCount { get; private set; }

        public event PropertyChangedEventHandler? PropertyChanged
        {
            add
            {
                _propertyChanged += value;
                SubscriberCount++;
            }
            remove
            {
                _propertyChanged -= value;
                SubscriberCount--;
            }
        }

        public void Notify(string? propertyName = null)
            => _propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class Holder
    {
        public required TestNotifier First { get; init; }
        public required TestNotifier Second { get; init; }
        public TestNotifier Current { get; set; } = null!;
        public bool ShowBuilder { get; set; } = true;
        public int BuilderBuildCount { get; set; }
        public int SiblingBuildCount { get; set; }
        public bool NotifyDuringChildDispose { get; set; }
        public HostState? State { get; set; }
    }

    private record Host : StatefulWidget<Host>
    {
        public required Holder Holder { get; init; }

        public override State<Host> CreateState() => new HostState();
    }

    private sealed class HostState : State<Host>
    {
        public override void InitState() => Widget!.Holder.State = this;

        public void Use(TestNotifier notifier) => SetState(() => Widget!.Holder.Current = notifier);

        public void HideBuilder() => SetState(() => Widget!.Holder.ShowBuilder = false);

        public override Widget Build(IBuildContext context)
            => new Row
            {
                Children =
                [
                    Widget!.Holder.ShowBuilder
                        ? new ListenableBuilder
                        {
                            Listenable = Widget.Holder.Current,
                            ChildBuilder = _ =>
                            {
                                Widget.Holder.BuilderBuildCount++;
                                return Widget.Holder.NotifyDuringChildDispose
                                    ? new NotifyOnDispose { Notifier = Widget.Holder.Current }
                                    : new SizedBox { Width = 10, Height = 10 };
                            }
                        }
                        : new SizedBox { Width = 10, Height = 10 },
                    new Builder
                    {
                        ChildBuilder = _ =>
                        {
                            Widget.Holder.SiblingBuildCount++;
                            return new SizedBox { Width = 10, Height = 10 };
                        }
                    }
                ]
            };
    }

    private record NotifyOnDispose : StatefulWidget<NotifyOnDispose>
    {
        public required TestNotifier Notifier { get; init; }

        public override State<NotifyOnDispose> CreateState() => new NotifyOnDisposeState();
    }

    private sealed class NotifyOnDisposeState : State<NotifyOnDispose>
    {
        public override Widget Build(IBuildContext context) => new SizedBox { Width = 10, Height = 10 };

        public override void Dispose()
        {
            Widget!.Notifier.Notify();
            base.Dispose();
        }
    }

    [Fact]
    public void InitState_マウント時に購読して初回構築する()
    {
        var (holder, _) = MountHost();

        Assert.Equal(1, holder.First.SubscriberCount);
        Assert.Equal(1, holder.BuilderBuildCount);
    }

    [Fact]
    public void PropertyChanged_通知ごとにBuilder配下だけを再構築する()
    {
        var (holder, owner) = MountHost();
        var siblingBuildCount = holder.SiblingBuildCount;

        holder.First.Notify(nameof(Holder.Current));
        owner.BuildScope();
        holder.First.Notify();
        owner.BuildScope();

        Assert.Equal(3, holder.BuilderBuildCount);
        Assert.Equal(siblingBuildCount, holder.SiblingBuildCount);
    }

    [Fact]
    public void DidUpdateWidget_Listenable差し替え時に購読を付け替える()
    {
        var (holder, owner) = MountHost();

        holder.State!.Use(holder.Second);
        owner.BuildScope();
        var buildCountAfterSwap = holder.BuilderBuildCount;

        Assert.Equal(0, holder.First.SubscriberCount);
        Assert.Equal(1, holder.Second.SubscriberCount);

        holder.First.Notify();
        owner.BuildScope();
        Assert.Equal(buildCountAfterSwap, holder.BuilderBuildCount);

        holder.Second.Notify();
        owner.BuildScope();
        Assert.Equal(buildCountAfterSwap + 1, holder.BuilderBuildCount);
    }

    [Fact]
    public void Dispose_ツリーから外れた後は通知を受けない()
    {
        var (holder, owner) = MountHost();

        holder.State!.HideBuilder();
        owner.BuildScope();
        var buildCountAfterUnmount = holder.BuilderBuildCount;

        Assert.Equal(0, holder.First.SubscriberCount);

        holder.First.Notify();
        owner.BuildScope();

        Assert.Equal(buildCountAfterUnmount, holder.BuilderBuildCount);
    }

    [Fact]
    public void Dispose_子の破棄中に通知されても再構築しない()
    {
        var (holder, owner) = MountHost(notifyDuringChildDispose: true);

        holder.State!.HideBuilder();
        owner.BuildScope();

        Assert.Equal(1, holder.BuilderBuildCount);
        Assert.Equal(0, holder.First.SubscriberCount);
    }

    [Fact]
    public void PropertyChanged_同一フレームの再entrant通知を一度の再構築へまとめる()
    {
        var (holder, owner) = MountHost();
        var reentered = false;
        holder.First.PropertyChanged += (_, _) =>
        {
            if (reentered) return;

            reentered = true;
            holder.First.Notify();
        };

        holder.First.Notify();
        owner.BuildScope();

        Assert.Equal(2, holder.BuilderBuildCount);
    }

    [Fact]
    public void PropertyChanged_マウントスレッド以外からの通知を拒否する()
    {
        var (holder, _) = MountHost();
        Exception? exception = null;
        var thread = new Thread(() => exception = Record.Exception(() => holder.First.Notify()));

        thread.Start();
        thread.Join();

        var invalidOperationException = Assert.IsType<InvalidOperationException>(exception);
        Assert.Contains("マウントしたスレッド", invalidOperationException.Message);
    }

    [Fact]
    public void RequiredProperties_Null_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new ListenableBuilder
        {
            Listenable = null!,
            ChildBuilder = _ => new SizedBox()
        });
        Assert.Throws<ArgumentNullException>(() => new ListenableBuilder
        {
            Listenable = new TestNotifier(),
            ChildBuilder = null!
        });
    }

    [Fact]
    public void PublicMembers_必須プロパティとBCL依存のみを公開する()
    {
        var type = typeof(ListenableBuilder);
        var listenable = type.GetProperty(nameof(ListenableBuilder.Listenable))!;
        var childBuilder = type.GetProperty(nameof(ListenableBuilder.ChildBuilder))!;

        Assert.Equal(typeof(INotifyPropertyChanged), listenable.PropertyType);
        Assert.Equal(typeof(Func<IBuildContext, Widget>), childBuilder.PropertyType);
        Assert.NotNull(listenable.GetCustomAttribute<RequiredMemberAttribute>());
        Assert.NotNull(childBuilder.GetCustomAttribute<RequiredMemberAttribute>());

        var publicSignatureTypes = type
            .GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(GetSignatureTypes);
        Assert.DoesNotContain(publicSignatureTypes, signatureType =>
            signatureType.Namespace?.StartsWith("R3", StringComparison.Ordinal) == true ||
            signatureType.Namespace?.StartsWith("CommunityToolkit", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(typeof(ListenableBuilder).Assembly.GetReferencedAssemblies(), reference =>
            reference.Name?.StartsWith("R3", StringComparison.Ordinal) == true ||
            reference.Name?.StartsWith("CommunityToolkit", StringComparison.Ordinal) == true);
    }

    private static (Holder Holder, BuildOwner Owner) MountHost(bool notifyDuringChildDispose = false)
    {
        var first = new TestNotifier();
        var second = new TestNotifier();
        var holder = new Holder
        {
            First = first,
            Second = second,
            Current = first,
            NotifyDuringChildDispose = notifyDuringChildDispose
        };
        var renderView = new RenderView(100, 100);
        _ = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = renderView
        };
        var owner = new BuildOwner(() => { });

        _ = new RenderObjectToWidgetAdapter
        {
            Container = renderView,
            Child = new Host { Holder = holder }
        }.AttachToRenderTree(owner, null);

        return (holder, owner);
    }

    private static IEnumerable<Type> GetSignatureTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
        _ => []
    };
}
