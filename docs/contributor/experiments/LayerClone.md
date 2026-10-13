# なぜ Layer ツリーは Clone してからレンダースレッドへ渡すの?

メインスレッドで毎フレーム描き直しながら、別のスレッドでその Layer ツリーを描き続けます。Clone せずに渡すと、描いた絵の多くが空になり、例外も出ます。Clone して渡すと、どちらも 0 回になります。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `LayerClone.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Core;
using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

var useClone = args.Contains("--clone");

var box = new RenderColoredBox();
var view = new RenderView { FixedSize = new SKSize(100, 100), Child = box };
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
view.PrepareInitialFrame();

ILayer? posted = null;           // メインスレッドからレンダースレッドへ渡したツリー
var done = false;
int rendered = 0, blank = 0, crashed = 0;

// レンダースレッドの代わり: 渡されたツリーを描き続ける
var renderThread = new Thread(() =>
{
    var renderer = new LayerBitmapRenderer();
    while (!Volatile.Read(ref done))
    {
        if (Volatile.Read(ref posted) is not { } layer) continue;
        try
        {
            using var bitmap = renderer.Render(layer, new SKSizeI(100, 100));
            if (bitmap.GetPixel(50, 50).Alpha == 0) blank++;
        }
        catch (Exception) { crashed++; }
        rendered++;
    }
});
renderThread.Start();

// メインスレッド: 毎フレーム色を変えて描き直し、ツリーを渡す
for (var frame = 0; frame < 20000; frame++)
{
    box.Color = frame % 2 == 0 ? SKColors.Red : SKColors.Blue;
    pipeline.FlushLayout();
    pipeline.FlushPaint();
    Volatile.Write(ref posted, useClone ? view.Layer!.Clone() : view.Layer);
}

Volatile.Write(ref done, true);
renderThread.Join();
Console.WriteLine($"clone={useClone}: 描いた {rendered} 回 / 空の絵 {blank} 回 / 例外 {crashed} 回");
```

リポジトリのルートで、まず Clone せずに実行します。

```bash
dotnet run LayerClone.cs
```

`RenderColoredBox` の色を毎フレーム赤と青で入れ替え、描き直した Layer ツリーを別のスレッドへ渡しています。別のスレッドは、FloatSoda のレンダースレッドの代わりです。終わったら、`LayerClone.cs` を消します。

## 見えるもの

Clone せずに渡すと、別のスレッドで描いた絵の多くが空になり、例外も出ます。回数は実行のたびに変わります。このページを書くときに5回実行したところ、どの回も空の絵と例外の両方が出ました。次はその1回の出力です。

```text
clone=False: 描いた 5099 回 / 空の絵 4000 回 / 例外 1022 回
```

四角は 100px 四方の全体を塗るので、正しく描けていれば空の絵は 0 回です。例外は、実行したときは `NullReferenceException` でした。

## 壊して直す

`--clone` を付けて実行すると、渡す前に `ILayer.Clone()` で複製します。

```bash
dotnet run LayerClone.cs -- --clone
```

```text
clone=True: 描いた 6182 回 / 空の絵 0 回 / 例外 0 回
```

空の絵と例外は 0 回になります。`--clone` を外すと、また出ます。FloatSoda 本体では、`WidgetBinding.DrawFrame` が `RenderView.Layer` を Clone してから、レンダースレッドへ渡しています。

## なぜそうなるか

メインスレッドは、描き直すたびに同じ Layer を使い回します。`PaintingContext.RepaintCompositedChild` は、描き直す境界の `TransformLayer` の子を消し、新しい `PictureLayer` を足してから記録を始めます。記録が終わるまで、その `PictureLayer` の `Picture` は空です。Clone しないと、別のスレッドは書き換えの途中のツリーを読みます。子が消えた瞬間や記録の途中を読むと、空の絵になります。子のリストを書き換えている最中に読むと、例外になります。

Clone すると、別のスレッドは、渡された時点の子のリストを持つ別のツリーを読みます。メインスレッドが次のフレームで元のツリーを書き換えても、渡したツリーは変わりません。記録済みの `SKPicture` は複製せずに共有しますが、メインスレッドは描き直すたびに新しい `PictureLayer` と `SKPicture` を作るので、渡した `SKPicture` を書き換えません。

Clone して渡す設計の本体は、[描画: 仕組みと理由](../rendering/Explanation.md) の「Clone して渡す理由」で説明します。新しい Layer 型を足すときに守る条件は、[REVIEW.md](../../../REVIEW.md) の「Layer」にあります。

## 関連

- [描画: 仕組みと理由](../rendering/Explanation.md) — Layer ツリーと、Clone して渡す理由
- [REVIEW.md](../../../REVIEW.md) — 「4. FloatSoda 固有の不変条件」の「Layer」
- [実験の一覧](Home.md)
