using FloatSoda.Elements;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Testing.Test;

/// <summary>
/// flutter_test の <c>finders_test.dart</c> と、<c>indexed_stack_test.dart</c> の skipOffstage の検証を移植したテスト。
/// </summary>
public class FindersTest
{
    private abstract record BaseWidget : StatelessWidget;

    private sealed record DerivedWidget : BaseWidget
    {
        public override Widget Build(IBuildContext context) => new Text("derived");
    }

    private sealed record OtherDerivedWidget : BaseWidget
    {
        public override Widget Build(IBuildContext context) => new Text("other");
    }

    private sealed record Holder : StatelessWidget
    {
        public override Widget Build(IBuildContext context) =>
            new Flex
            {
                Children = [new DerivedWidget(), new OtherDerivedWidget(), new DerivedWidget()],
            };
    }

    // finders_test.dart: group('text') 'finds Text widgets'
    [Fact]
    public async Task Text_Textをマウントしたとき_一致する文字列が1件見つかること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Text("test"));

        Assert.Single(tester.Find.Text("test").Evaluate());
    }

    [Fact]
    public async Task Text_文字列が部分一致のとき_一致しないこと()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Text("test"));

        Assert.Empty(tester.Find.Text("tes").Evaluate());
    }

    [Fact]
    public void Text_nullを渡したとき_ArgumentNullExceptionを投げること()
    {
        using var tester = new WidgetTester();

        Assert.Throws<ArgumentNullException>(() => tester.Find.Text(null!));
    }

    [Fact]
    public async Task ByType_派生型のWidgetがあるとき_完全一致の型だけ見つかること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Holder());

        Assert.Equal(2, tester.Find.ByType<DerivedWidget>().Evaluate().Count());
        Assert.Empty(tester.Find.ByType<BaseWidget>().Evaluate());
    }

    // finders_test.dart: 'finds multiple subtypes'
    [Fact]
    public async Task BySubtype_基底型を指定したとき_派生型すべてに一致すること()
    {
        using var tester = new WidgetTester();

        await tester.PumpWidget(new Holder());

        Assert.Equal(3, tester.Find.BySubtype<BaseWidget>().Evaluate().Count());
        Assert.Single(tester.Find.BySubtype<OtherDerivedWidget>().Evaluate());
        Assert.Single(tester.Find.BySubtype<Flex>().Evaluate());
    }

    [Fact]
    public async Task BySubtype_Widgetを指定したとき_ツリー内のすべてのWidgetに一致すること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Holder());

        int all = tester.Find.BySubtype<Widget>().Evaluate().Count();

        // Holder 1 + Flex 1 + Derived 2 + OtherDerived 1 + それぞれが build する Text 3
        // + Text がそれぞれ build する RichText 3
        int sum =
            tester.Find.ByType<Holder>().Evaluate().Count()
            + tester.Find.ByType<Flex>().Evaluate().Count()
            + tester.Find.ByType<DerivedWidget>().Evaluate().Count()
            + tester.Find.ByType<OtherDerivedWidget>().Evaluate().Count()
            + tester.Find.ByType<Text>().Evaluate().Count()
            + tester.Find.ByType<RichText>().Evaluate().Count();
        Assert.Equal(11, all);
        Assert.Equal(sum, all);
    }

    [Fact]
    public async Task ByKey_キーが一致するとき_そのWidgetだけ見つかること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(
            new Flex
            {
                Children =
                [
                    new Text("a") { Key = new ValueKey<int>(1) },
                    new Text("b") { Key = new ValueKey<int>(2) },
                ],
            }
        );

        Finder finder = tester.Find.ByKey(new ValueKey<int>(2));

        Assert.Equal("b", tester.Widget<Text>(finder).Data);
    }

    [Fact]
    public void ByKey_nullを渡したとき_ArgumentNullExceptionを投げること()
    {
        using var tester = new WidgetTester();

        Assert.Throws<ArgumentNullException>(() => tester.Find.ByKey(null!));
    }

    // finders_test.dart: group('first and last') 相当(順序と範囲)
    [Fact]
    public async Task FirstLastAt_複数件一致したとき_先頭_末尾_指定位置を返すこと()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Flex { Children = [new Text("a"), new Text("b"), new Text("c")] });
        Finder texts = tester.Find.ByType<Text>();

        Assert.Equal("a", tester.Widget<Text>(texts.First).Data);
        Assert.Equal("c", tester.Widget<Text>(texts.Last).Data);
        Assert.Equal("b", tester.Widget<Text>(texts.At(1)).Data);
    }

    [Fact]
    public async Task FirstLast_一致が0件のとき_EvaluateでInvalidOperationExceptionを投げること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Text("a"));

        Assert.Throws<InvalidOperationException>(() => tester.Find.ByType<Flex>().First.Evaluate().ToList());
        Assert.Throws<InvalidOperationException>(() => tester.Find.ByType<Flex>().Last.Evaluate().ToList());
    }

    [Fact]
    public async Task At_範囲外のindexのとき_EvaluateでArgumentOutOfRangeExceptionを投げること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Text("a"));

        Assert.Throws<ArgumentOutOfRangeException>(() => tester.Find.ByType<Text>().At(5).Evaluate().ToList());
        Assert.Throws<ArgumentOutOfRangeException>(() => tester.Find.ByType<Text>().At(-1).Evaluate().ToList());
    }

    [Fact]
    public void Evaluate_PumpWidgetの前のとき_空の列を返すこと()
    {
        using var tester = new WidgetTester();

        Assert.Empty(tester.Find.ByType<Text>().Evaluate());
    }

    // indexed_stack_test.dart: expectFindsChild
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public async Task Text_IndexedStackのとき_既定では表示中の子だけ見つかること(int index)
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(IndexedStackOfTexts(index));

        for (int i = 0; i < 3; i++)
        {
            Assert.Single(tester.Find.Text($"{i}", skipOffstage: false).Evaluate());
            Assert.Equal(i == index ? 1 : 0, tester.Find.Text($"{i}").Evaluate().Count());
        }
    }

    // indexed_stack_test.dart: IndexedStack with null index
    [Fact]
    public async Task ByType_IndexedStackのindexがnullのとき_既定では何も見つからずskipOffstage_falseで全件見つかること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(IndexedStackOfTexts(null));

        Assert.Empty(tester.Find.ByType<Text>().Evaluate());
        Assert.Equal(3, tester.Find.ByType<Text>(skipOffstage: false).Evaluate().Count());
    }

    [Fact]
    public async Task ByType_IndexedStackのindexを変えて再ポンプしたとき_表示中の子が切り替わること()
    {
        using var tester = new WidgetTester();
        Finder visible = tester.Find.ByType<Text>();

        await tester.PumpWidget(IndexedStackOfTexts(null));
        Assert.Empty(visible.Evaluate());

        for (int i = 0; i < 3; i++)
        {
            await tester.PumpWidget(IndexedStackOfTexts(i));
            Assert.Equal($"{i}", tester.Widget<Text>(visible).Data);
        }
    }

    // finders_test.dart: Offstage 内の Text を find.text(skipOffstage: false) で探す
    [Fact]
    public async Task Text_Offstageの子のとき_既定では見つからずskipOffstage_falseで見つかること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Offstage { IsOffstage = true, Child = new Text("Target") });

        Assert.Empty(tester.Find.Text("Target").Evaluate());
        Assert.Single(tester.Find.Text("Target", skipOffstage: false).Evaluate());
    }

    [Fact]
    public async Task Text_IsOffstageがfalseのOffstageの子のとき_既定でも見つかること()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Offstage { IsOffstage = false, Child = new Text("Target") });

        Assert.Single(tester.Find.Text("Target").Evaluate());
    }

    [Fact]
    public async Task ByType_Offstageの深い子孫のとき_既定では見つからないこと()
    {
        using var tester = new WidgetTester();
        await tester.PumpWidget(new Offstage { Child = new Flex { Children = [new Text("deep")] } });

        Assert.Empty(tester.Find.ByType<Flex>().Evaluate());
        Assert.Single(tester.Find.ByType<Flex>(skipOffstage: false).Evaluate());
        Assert.Single(tester.Find.ByType<Offstage>().Evaluate());
    }

    private static IndexedStack IndexedStackOfTexts(int? index) =>
        new() { Index = index, Children = [new Text("0"), new Text("1"), new Text("2")] };
}
