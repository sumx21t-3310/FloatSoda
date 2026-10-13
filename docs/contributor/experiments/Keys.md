# `Key` をつけると何が変わるの?

状態を持つ子を2つ並べ、Widget の並びを入れ替えます。`Key` がないと、状態は位置に残り、入れ替えた Widget とは別の子に付いていきます。`Key` をつけると、状態は Widget と一緒に動きます。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `Keys.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Rendering;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var useKey = args.Contains("--key");
Widget Item(string name) => new Tile { Name = name, Key = useKey ? new ValueKey<string>(name) : null };

var view = new RenderView { FixedSize = new SKSize(100, 50) };
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
var owner = new BuildOwner(() => { });
var root = new RenderObjectToWidgetAdapter { Container = view, Child = new Row { Children = [Item("Red"), Item("Blue")] } }
    .AttachToRenderTree(owner, null);
view.PrepareInitialFrame();
Show("並べ替える前");

// Widget の並びを入れ替えて、次のフレームを作る
new RenderObjectToWidgetAdapter { Container = view, Child = new Row { Children = [Item("Blue"), Item("Red")] } }
    .AttachToRenderTree(owner, root);
Show("並べ替えた後");

void Show(string label)
{
    owner.BuildScope();
    pipeline.FlushLayout();
    pipeline.FlushPaint();
    using var bitmap = new LayerBitmapRenderer().Render(view.Layer!.Clone(), new SKSizeI(100, 50));
    Console.WriteLine($"key={useKey} {label}: 左 {bitmap.GetPixel(25, 25)} / 右 {bitmap.GetPixel(75, 25)}");
}

// 作られたときの Name を State に覚え、その色で描く
record Tile : StatefulWidget<Tile>
{
    public required string Name { get; init; }
    public override State<Tile> CreateState() => new TileState();
}

class TileState : State<Tile>
{
    SKColor _remembered;
    public override void InitState() => _remembered = Widget!.Name == "Red" ? SKColors.Red : SKColors.Blue;
    public override Widget Build(IBuildContext context) =>
        new ColoredBox { Color = _remembered, Child = new SizedBox { Width = 50, Height = 50 } };
}
```

リポジトリのルートで実行します。

```bash
dotnet run Keys.cs
```

`Tile` の `State` は、作られたときの `Name` を色として覚え、その色で描きます。覚えた色は、あとで Widget が変わっても変えません。`State` がどの子に付いているかを、色で見分けるためです。終わったら、`Keys.cs` を消します。

## 見えるもの

`Key` がないと、Widget の並びを `Blue`、`Red` に入れ替えても、絵は赤、青のままです。色は `#AARRGGBB` の形で出ます。

```text
key=False 並べ替える前: 左 #ffff0000 / 右 #ff0000ff
key=False 並べ替えた後: 左 #ffff0000 / 右 #ff0000ff
```

並べ替えたあと、左の位置には `Name = "Blue"` の Widget があります。ところが、左に描かれているのは、`Red` を覚えた `State` の色です。`State` は位置に残り、Widget だけが入れ替わっています。

## 壊して直す

`--key` を付けて実行すると、それぞれの `Tile` に `ValueKey<string>(name)` をつけます。

```bash
dotnet run Keys.cs -- --key
```

```text
key=True 並べ替える前: 左 #ffff0000 / 右 #ff0000ff
key=True 並べ替えた後: 左 #ff0000ff / 右 #ffff0000
```

並べ替えたあとは左が青、右が赤になり、`State` が Widget と一緒に動きます。`--key` を外すと、`State` が位置に残る状態に戻ります。

## なぜそうなるか

`State` は Widget ではなく Element が持ちます。フレームごとに新しい Widget の並びが来ると、FloatSoda は古い Element と新しい Widget を組み合わせ、Element を使い回します。組み合わせてよいかは `Widget.CanUpdate` が決め、型が同じで `Key` が等しければ使い回します。`Key` がないと、どちらの `Tile` も型が同じで `Key` も `null` 同士なので、同じ位置の Element がそのまま使われます。そのため、`Red` を覚えた `State` は左の位置に残り、`Blue` の Widget を受け取ります。`InitState` は Element が最初に組み立てられるときに1回だけ呼ばれるので、覚えた色は変わりません。

`Key` をつけると、複数の子を持つ Element は、古い子を `Key` で探して新しい Widget と組み合わせます。`ValueKey<string>("Blue")` の Widget は、もともと `Blue` の Widget を持っていた Element と組み合わさり、Element ごと左へ移ります。Widget と Element の役割と、並びの差分の取り方は、[フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) の Widget と Element の節で説明します。

## 関連

- [フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) — Widget と Element、並びの差分
- [State](../../concepts/State.md) — 利用者から見た `State`
- [実験の一覧](Home.md)
