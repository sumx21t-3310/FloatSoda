# `InheritedWidget` を変えると、誰が作り直されるの?

`InheritedWidget` の下に、値を読む子と読まない子を置き、それぞれの `Build` が呼ばれた回数を数えます。`InheritedWidget` の値を変えると、回数が増えるのは値を読む子だけです。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `InheritedRebuild.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;

var depend = !args.Contains("--no-depend");
var view = new RenderView();
var owner = new BuildOwner(() => { });
Widget children = new Column { Children = [new Reader { Depend = depend }, new Bystander()] };

// 1フレーム目: Theme の値は 1
var root = new RenderObjectToWidgetAdapter { Container = view, Child = new Theme { Value = 1, Child = children } }
    .AttachToRenderTree(owner, null);
Console.WriteLine($"1フレーム目: {string.Join(", ", Builds.Count)}");

// 2フレーム目: Theme の値だけを 2 に変える。子の Widget は同じものを渡す
new RenderObjectToWidgetAdapter { Container = view, Child = new Theme { Value = 2, Child = children } }
    .AttachToRenderTree(owner, root);
owner.BuildScope();
Console.WriteLine($"2フレーム目: {string.Join(", ", Builds.Count)}");

static class Builds { public static readonly Dictionary<string, int> Count = new(); }

record Theme : InheritedWidget
{
    public int Value { get; init; }
    public override bool UpdateShouldNotify(InheritedWidget oldWidget) => ((Theme)oldWidget).Value != Value;
}

// Theme に依存する子
record Reader : StatelessWidget
{
    public bool Depend { get; init; }
    public override Widget Build(IBuildContext context)
    {
        Builds.Count["Reader"] = Builds.Count.GetValueOrDefault("Reader") + 1;
        if (Depend) context.DependOnInheritedWidgetOfExactType<Theme>();
        return new SizedBox();
    }
}

// Theme に依存しない子
record Bystander : StatelessWidget
{
    public override Widget Build(IBuildContext context)
    {
        Builds.Count["Bystander"] = Builds.Count.GetValueOrDefault("Bystander") + 1;
        return new SizedBox();
    }
}
```

リポジトリのルートで実行します。

```bash
dotnet run InheritedRebuild.cs
```

`Reader` は `DependOnInheritedWidgetOfExactType<Theme>()` で `Theme` を読む子、`Bystander` は読まない子です。どちらも `Build` が呼ばれるたびに回数を1つ増やします。この実験は作り直しの回数だけを見るので、レイアウトと描画はしません。終わったら、`InheritedRebuild.cs` を消します。

## 見えるもの

回数は、1フレーム目からの合計です。`Theme` の値を 1 から 2 に変えた2フレーム目で、`Reader` だけが 1 回増えます。

```text
1フレーム目: [Reader, 1], [Bystander, 1]
2フレーム目: [Reader, 2], [Bystander, 1]
```

`Bystander` は、同じ `Theme` の下にあっても作り直されません。

## 壊して直す

`--no-depend` を付けて実行すると、`Reader` は `Theme` を読まなくなります。

```bash
dotnet run InheritedRebuild.cs -- --no-depend
```

```text
1フレーム目: [Reader, 1], [Bystander, 1]
2フレーム目: [Reader, 1], [Bystander, 1]
```

`Reader` の回数も増えなくなります。`--no-depend` を外すと、`Reader` だけが作り直される状態に戻ります。

## なぜそうなるか

`DependOnInheritedWidgetOfExactType<Theme>()` を呼ぶと、呼んだ子は、いちばん近い `Theme` の Element に「依存している子」として登録されます。`Theme` が新しい Widget に変わり、`UpdateShouldNotify` が `true` を返すと、`Theme` の Element は登録された子にだけ作り直しの印を付けます。`Bystander` は登録されていないので、印が付きません。

`Theme` の下の Widget は、2フレーム目でも1フレーム目と同じもの(`children`)を渡しています。Element は、新しい Widget が前と同じなら、その下を作り直さずに済ませます。そのため、`Theme` が変わっても、下の子が作り直されるのは依存を登録した子だけです。この差分ビルドの仕組みは、[三つのツリーの仕組みと理由](../explanation/Trees.md) の差分ビルドの節で説明します。

## 関連

- [三つのツリーの仕組みと理由](../explanation/Trees.md) — 差分ビルドと `BuildOwner`
- [State](../../concepts/State.md) — 利用者から見た `InheritedWidget`
- [実験の一覧](Home.md)
