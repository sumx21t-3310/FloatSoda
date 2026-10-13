# 親はどうやって子の大きさと位置を決めるの?

子を円周上に12個並べる RenderObject を書いて、時計の文字盤を作ります。親は、子に制約を渡して大きさを決めてもらい、そのあとで子の位置を決めます。制約を渡し忘れると、子は大きさが 0 のままになり、絵から消えます。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `CircleLayout.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using SkiaSharp;

var circle = new RenderCircle { ForgetConstraints = args.Contains("--forget") };
for (var i = 0; i < 12; i++)
    circle.Children.Add(new RenderColoredBox { Color = i == 0 ? SKColors.Red : SKColors.Black });

using var bitmap = new RenderObjectBitmapRenderer().Render(circle, new SKSizeI(200, 200));
using (var file = File.OpenWrite("circle.png")) bitmap.Encode(file, SKEncodedImageFormat.Png, 100);
foreach (var child in circle.Children)
    Console.WriteLine($"子の大きさ {child.Size}, 位置 {((BoxParentData)child.ParentData!).Offset}");

// 子を円周上に12個並べる RenderObject
class RenderCircle : RenderBox
{
    public bool ForgetConstraints;
    public MultiChildrenCollection<RenderBox> Children { get; }
    public RenderCircle() => Children = new MultiChildrenCollection<RenderBox>(this);

    public override void PerformLayout()
    {
        Size = new SKSize(200, 200);                                  // 自分の大きさを決める
        for (var i = 0; i < Children.Count; i++)
        {
            var child = Children.ElementAt(i);
            if (!ForgetConstraints)
                child.Layout(BoxConstraints.Tight(16, 16));           // 1. 子に制約を渡し、大きさを決めてもらう
            var angle = Math.PI * 2 * i / Children.Count - Math.PI / 2;
            ((BoxParentData)child.ParentData!).Offset = new Offset(  // 2. 子の位置を親が決める
                100 + 80 * Math.Cos(angle) - child.Size.Width / 2,
                100 + 80 * Math.Sin(angle) - child.Size.Height / 2);
        }
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        foreach (var child in Children)
        {
            var o = ((BoxParentData)child.ParentData!).Offset;
            context.PaintChild(child, new Offset(offset.X + o.X, offset.Y + o.Y));
        }
    }

    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();
    public override void Attach(FloatSoda.Core.RenderPipeline? owner) { base.Attach(owner); Children.Attach(owner); }
    public override void Detach() { base.Detach(); Children.Detach(); }
    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);
    public override void RedepthChildren() => VisitChildren(RedepthChild);
}
```

リポジトリのルートで実行します。

```bash
dotnet run CircleLayout.cs
```

`RenderObjectBitmapRenderer` が、1フレーム分のレイアウトと描画をして、ビットマップを返します。`PerformLayout` と `Paint` が、この実験の本体です。末尾の5つのメソッドは、子を持つ RenderObject に毎回要る決まった処理(親子のつなぎ方)です。終わったら、`CircleLayout.cs` と `circle.png` を消します。

## 見えるもの

`circle.png` に、16px 四方の四角が12個、円く並んだ時計の文字盤ができます。12時の位置だけ赤です。コンソールには、子ごとに、親が決めた大きさと位置が出ます。先頭の3行は次のとおりです。

```text
子の大きさ {Width=16, Height=16}, 位置 Offset { X = 92, Y = 12 }
子の大きさ {Width=16, Height=16}, 位置 Offset { X = 132, Y = 22.7179676972449 }
子の大きさ {Width=16, Height=16}, 位置 Offset { X = 161.28203230275508, Y = 52 }
```

## 壊して直す

`--forget` を付けて実行すると、`PerformLayout` は子の `Layout` を呼ばずに、位置だけを決めます。

```bash
dotnet run CircleLayout.cs -- --forget
```

```text
子の大きさ {Width=0, Height=0}, 位置 Offset { X = 100, Y = 20 }
```

子の大きさは 0 のままになり、`circle.png` は何も描かれない透明な絵になります。位置は決まっていても、大きさが 0 の四角は描かれません。`--forget` を外すと、文字盤が戻ります。

## なぜそうなるか

子の大きさを決めるのは子自身で、親は制約を渡すだけです。親が `child.Layout(制約)` を呼ぶと、子は `PerformLayout` の中で、制約の範囲に収まる大きさを自分の `Size` に書きます。この実験の `RenderColoredBox` は子を持たないので、制約が許すいちばん小さい大きさを選びます。`BoxConstraints.Tight(16, 16)` は 16 以外を許さないので、16px 四方になります。`Layout` を呼ばなければ、子の `Size` は初期値の空のままです。

位置は親が決めます。親は、子の `ParentData`(ここでは `BoxParentData.Offset`)に位置を書き、`Paint` でその位置を足して子を描きます。子は、自分がどこに置かれるかを知りません。この「制約は親から子へ下り、大きさは子から親へ上がり、位置は親が決める」流れの本体は、[フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) の制約とレイアウトの節で説明します。利用者から見た制約の考え方は、[Layout](../../concepts/Layout.md) にあります。

## 関連

- [フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) — 制約とレイアウトの流れ
- [Layout](../../concepts/Layout.md) — 利用者から見た制約・サイズ・配置
- [`src/FloatSoda/RenderObjects/Layout/`](https://github.com/sumx21t-3310/FloatSoda/tree/main/src/FloatSoda/RenderObjects/Layout) — 組み込みのレイアウト用 RenderObject は、このディレクトリにある
- [実験の一覧](Home.md)
