using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Testing.Test;

/// <summary>
/// flutter_test の <c>widget_tester_test.dart</c> / <c>controller_test.dart</c> のうち、
/// pumpWidget・pump・widget&lt;T&gt; に当たる範囲を移植したテスト。
/// ポインタ入力・pumpAndSettle・テスト時計は #239 の対象。
/// </summary>
public class WidgetTesterTest
{
    private sealed class Counter
    {
        public int Value;
        public int InitCount;
        public int DisposeCount;
        public CounterState? State;
    }

    private sealed record CounterWidget : StatefulWidget<CounterWidget>
    {
        public required Counter Counter { get; init; }

        public override State<CounterWidget> CreateState() => new CounterState();
    }

    private sealed class CounterState : State<CounterWidget>
    {
        public override void InitState()
        {
            Widget!.Counter.InitCount++;
            Widget!.Counter.State = this;
        }

        public void Increment() => SetState(() => Widget!.Counter.Value++);

        public override void Dispose() => Widget!.Counter.DisposeCount++;

        public override Widget Build(IBuildContext context) =>
            new Text($"count {Widget!.Counter.Value}");
    }

    [Fact]
    public async Task Evaluate_Textをマウントしたとき_1件一致すること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Text("Hello"));
        await tester.Pump();

        Assert.Single(tester.Find.ByType<Text>().Evaluate());
    }

    [Fact]
    public async Task Widget_Textをマウントしたとき_マウントしたWidgetが返ること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Text("Hello"));

        Assert.Equal("Hello", tester.Widget<Text>(tester.Find.ByType<Text>()).Data);
    }

    [Fact]
    public async Task Widget_一致が0件のとき_WidgetAssertionExceptionを投げること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Text("Hello"));

        Assert.Throws<WidgetAssertionException>(() =>
            tester.Widget<Text>(tester.Find.Text("nothing"))
        );
    }

    [Fact]
    public async Task Widget_一致が2件以上のとき_WidgetAssertionExceptionを投げること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Flex { Children = [new Text("a"), new Text("b")] });

        Assert.Throws<WidgetAssertionException>(() =>
            tester.Widget<Text>(tester.Find.ByType<Text>())
        );
    }

    [Fact]
    public async Task Widget_型が合わないとき_WidgetAssertionExceptionを投げること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Text("a"));

        Assert.Throws<WidgetAssertionException>(() =>
            tester.Widget<Flex>(tester.Find.ByType<Text>())
        );
    }

    [Fact]
    public async Task PumpWidget_Expandedを含むFlexをルートにしたとき_800x600の制約でレイアウトすること()
    {
        using var tester = new WidgetTester();

        // ビューポートの制約が無限だと、RenderFlex が Expanded を割り当てられずに例外を投げる。
        await tester.PumpWidget(new Flex { Children = [new Expanded { Child = new Text("a") }] });

        var root = tester.Find.ByType<Flex>().Evaluate().Single().RenderObject!;
        Assert.Equal(new SkiaSharp.SKSize(800, 600), root.Size);
    }

    [Fact]
    public async Task PumpWidget_再呼び出しのとき_ルートが差し替わること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Text("first"));
        await tester.PumpWidget(new Text("second"));

        Assert.Empty(tester.Find.Text("first").Evaluate());
        Assert.Single(tester.Find.Text("second").Evaluate());
    }

    [Fact]
    public async Task PumpWidget_同じ型のWidgetで再呼び出しするとき_Stateを引き継ぐこと()
    {
        using var tester = new WidgetTester();
        var counter = new Counter();

        await tester.PumpWidget(new CounterWidget { Counter = counter });
        counter.State!.Increment();
        await tester.Pump();
        await tester.PumpWidget(new CounterWidget { Counter = counter });

        Assert.Equal(1, counter.InitCount);
        Assert.Equal(0, counter.DisposeCount);
        Assert.Single(tester.Find.Text("count 1").Evaluate());
    }

    [Fact]
    public async Task PumpWidget_別の型のWidgetで再呼び出しするとき_古いStateを破棄すること()
    {
        using var tester = new WidgetTester();
        var counter = new Counter();

        await tester.PumpWidget(new CounterWidget { Counter = counter });
        await tester.PumpWidget(new Text("other"));

        Assert.Equal(1, counter.DisposeCount);
    }

    [Fact]
    public async Task Pump_SetStateのあと_再ビルドが反映されること()
    {
        using var tester = new WidgetTester();
        var counter = new Counter();
        await tester.PumpWidget(new CounterWidget { Counter = counter });
        Finder text = tester.Find.ByType<Text>();

        counter.State!.Increment();
        // Pump の前は、まだ古いツリーのまま。
        Assert.Equal("count 0", tester.Widget<Text>(text).Data);
        await tester.Pump();

        // 同じ Finder を評価し直すと、更新後のツリーを調べられる。
        Assert.Equal("count 1", tester.Widget<Text>(text).Data);
    }

    [Fact]
    public async Task Pump_PumpWidgetの前のとき_InvalidOperationExceptionを投げること()
    {
        using var tester = new WidgetTester();

        await Assert.ThrowsAsync<InvalidOperationException>(() => tester.Pump());
    }

    [Fact]
    public async Task PumpWidget_nullを渡したとき_ArgumentNullExceptionを投げること()
    {
        using var tester = new WidgetTester();

        await Assert.ThrowsAsync<ArgumentNullException>(() => tester.PumpWidget(null!));
    }

    [Fact]
    public async Task Dispose_マウント済みのとき_Stateを破棄すること()
    {
        var tester = new WidgetTester();
        var counter = new Counter();
        await tester.PumpWidget(new CounterWidget { Counter = counter });

        tester.Dispose();

        Assert.Equal(1, counter.DisposeCount);
    }

    [Fact]
    public async Task Dispose_2回呼んだとき_Stateを二重に破棄しないこと()
    {
        var tester = new WidgetTester();
        var counter = new Counter();
        await tester.PumpWidget(new CounterWidget { Counter = counter });

        tester.Dispose();
        tester.Dispose();

        Assert.Equal(1, counter.DisposeCount);
    }

    [Fact]
    public async Task PumpWidget_破棄したあと_ObjectDisposedExceptionを投げること()
    {
        var tester = new WidgetTester();
        tester.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => tester.PumpWidget(new Text("a")));
        await Assert.ThrowsAsync<ObjectDisposedException>(() => tester.Pump());
    }
}
