# 半透明の子が重なると、どう混ざるの?

赤と青の四角を重ね、半透明にする `OpacityLayer` を親にかける場合と、子ごとにかける場合を比べます。親にかけると、重なった部分は半透明の青になります。子ごとにかけると、赤と青が混ざった紫になります。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `OpacityBlend.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Rendering
using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using SkiaSharp;

// 不透明な四角を1枚記録した PictureLayer を作る
PictureLayer Square(float x, float y, SKColor color)
{
    using var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(SKRect.Create(0, 0, 200, 200));
    canvas.DrawRect(SKRect.Create(x, y, 100, 100), new SKPaint { Color = color });
    return new PictureLayer { Picture = recorder.EndRecording() };
}

// A: 親に OpacityLayer を1枚かける(2枚をまとめて半透明にする)
var groupRoot = new ContainerLayer();
var group = new OpacityLayer { Alpha = 128 };
group.Children.Add(Square(20, 20, SKColors.Red));
group.Children.Add(Square(80, 80, SKColors.Blue));
groupRoot.Children.Add(group);

// B: 子ごとに OpacityLayer をかける(1枚ずつ半透明にしてから重ねる)
var eachRoot = new ContainerLayer();
foreach (var square in new[] { Square(20, 20, SKColors.Red), Square(80, 80, SKColors.Blue) })
{
    var each = new OpacityLayer { Alpha = 128 };
    each.Children.Add(square);
    eachRoot.Children.Add(each);
}

var renderer = new LayerBitmapRenderer();
foreach (var (name, root) in new[] { ("group", groupRoot), ("each", eachRoot) })
{
    using var bitmap = renderer.Render(root, new SKSizeI(200, 200));
    Console.WriteLine($"{name}: 重なった部分 (100,100) = {bitmap.GetPixel(100, 100)}");
    using var file = File.OpenWrite($"opacity-{name}.png");
    bitmap.Encode(file, SKEncodedImageFormat.Png, 100);
}
```

リポジトリのルートで実行します。

```bash
dotnet run OpacityBlend.cs
```

Layer ツリーを手で組み、`LayerBitmapRenderer` でビットマップへ描いています。終わったら、`OpacityBlend.cs` と、できた PNG を消します。

## 見えるもの

重なった部分の色が、2つの組み方で変わります。色は `#AARRGGBB`(不透明度・赤・緑・青)の形で出ます。

```text
group: 重なった部分 (100,100) = #800000ff
each: 重なった部分 (100,100) = #bf5400ab
```

- `group`(親にかけた場合): 重なった部分は半透明の青になる。下の赤は透けない
- `each`(子ごとにかけた場合): 重なった部分は赤と青が混ざった紫になり、ほかの部分より濃くなる

`opacity-group.png` と `opacity-each.png` を開くと、同じ違いを絵で確かめられます。

## 壊して直す

`OpacityLayer` をかける位置を入れ替えると、重なった部分の色が変わります。コードの A と B は、かける位置だけが違います。B の組み方を A に戻すと、重なった部分は半透明の青に戻ります。

アプリで「まとめて薄くしたのに、重なった部分だけ濃い」ときは、`Opacity` を子ごとにかけていないかを確かめます。`Opacity` ウィジェットは、不透明度が 0 と 1 の間のとき、描く時点で `PaintingContext.PushOpacity` を呼んで `OpacityLayer` を1枚積みます。`Opacity` で `Stack` 全体を包むと A になり、`Stack` の子をそれぞれ包むと B になります。

## なぜそうなるか

`OpacityLayer` は、子をいったん別の描画先へ不透明のまま描き、最後にその描画先全体を指定した不透明度で重ねます(`OpacityLayer.Paint` の `SKCanvas.SaveLayer`)。A では、赤の上に青を描いた時点で、重なった部分は青だけになります。そのあとで全体が半透明になるので、赤は透けません。B では、赤と青がそれぞれ半透明になってから重なるので、青を通して下の赤が見えます。

Layer ツリーの組み方と描き方は、[描画の仕組みと理由](../explanation/Rendering.md) の Layer ツリーの節で説明します。

## 関連

- [描画の仕組みと理由](../explanation/Rendering.md) — Layer ツリーの種類と、描画の流れ
- [`src/FloatSoda.Rendering/Layers/`](https://github.com/sumx21t-3310/FloatSoda/tree/main/src/FloatSoda.Rendering/Layers) — Layer の種類の一覧は、このディレクトリにある
- [実験の一覧](Home.md)
