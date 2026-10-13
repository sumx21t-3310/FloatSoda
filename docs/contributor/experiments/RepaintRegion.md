# 1フレームで、どこが描き直されているの?

赤い四角を2つ並べ、右の四角だけ `SetState` で青に変えます。そのフレームで描き直された部分だけを絵にすると、`RepaintBoundary` がないときは左右の両方が写ります。右の四角を `RepaintBoundary` で包むと、写るのは右の四角だけになります。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `RepaintRegion.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Rendering;
using FloatSoda.Rendering.Layers;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var useBoundary = args.Contains("--boundary");
Widget right = useBoundary ? new RepaintBoundary { Child = new Counter() } : new Counter();

var view = new RenderView { FixedSize = new SKSize(200, 100) };
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
var owner = new BuildOwner(() => { });
new RenderObjectToWidgetAdapter { Container = view, Child = new Row { Children = [new Counter(), right] } }
    .AttachToRenderTree(owner, null);
view.PrepareInitialFrame();
pipeline.FlushLayout();
pipeline.FlushPaint();

var before = Pictures(view.Layer!).ToHashSet();  // 1フレーム目に記録した絵
Counter.Last!.Bump();                             // 右の Counter で SetState を呼ぶ
owner.BuildScope();
pipeline.FlushLayout();
pipeline.FlushPaint();

// 2フレーム目に新しく記録した絵だけを残す = 描き直された範囲
var repainted = (ContainerLayer)view.Layer!.Clone();
Keep(repainted, p => !before.Contains(p.Picture!));
using var bitmap = new LayerBitmapRenderer().Render(repainted, new SKSizeI(200, 100));
var area = bitmap.Pixels.Count(c => c.Alpha != 0);
Console.WriteLine($"boundary={useBoundary}: 描き直した面積 {area} px");
using (var file = File.OpenWrite($"repaint-{useBoundary}.png")) bitmap.Encode(file, SKEncodedImageFormat.Png, 100);

IEnumerable<SKPicture> Pictures(ILayer layer) => layer switch
{
    PictureLayer p => [p.Picture!],
    ContainerLayer c => c.Children.SelectMany(Pictures),
    _ => [],
};

void Keep(ContainerLayer layer, Func<PictureLayer, bool> keep)
{
    layer.Children.RemoveAll(l => l is PictureLayer p && !keep(p));
    foreach (var child in layer.Children.OfType<ContainerLayer>()) Keep(child, keep);
}

record Counter : StatefulWidget<Counter>
{
    public static CounterState? Last;
    public override State<Counter> CreateState() => Last = new CounterState();
}

class CounterState : State<Counter>
{
    int _count;
    public void Bump() => SetState(() => _count++);
    public override Widget Build(IBuildContext context) => new ColoredBox
    {
        Color = _count % 2 == 0 ? SKColors.Red : SKColors.Blue,
        Child = new SizedBox { Width = 50, Height = 50 },
    };
}
```

リポジトリのルートで実行します。

```bash
dotnet run RepaintRegion.cs
```

1フレーム目を描いたあと、Layer ツリーにある絵(`SKPicture`)を覚えておきます。2フレーム目を描いたら、覚えておいた絵を Layer ツリーから外し、新しく記録された絵だけを描きます。写ったところが、2フレーム目で描き直された範囲です。終わったら、`RepaintRegion.cs` と、できた PNG を消します。

## 見えるもの

`RepaintBoundary` がないと、`SetState` を呼んでいない左の四角まで描き直されます。

```text
boundary=False: 描き直した面積 5000 px
```

`repaint-False.png` には、赤い左の四角と青い右の四角の両方が写ります。50px 四方の四角2つ分で、5000px です。

## 壊して直す

`--boundary` を付けて実行すると、右の `Counter` を `RepaintBoundary` で包みます。

```bash
dotnet run RepaintRegion.cs -- --boundary
```

```text
boundary=True: 描き直した面積 2500 px
```

`repaint-True.png` に写るのは、青い右の四角だけになります。描き直された範囲は四角1つ分の 2500px に縮みます。`--boundary` を外すと、左の四角も描き直される状態に戻ります。

## なぜそうなるか

描き直しの印は、印を付けた RenderObject から親へ向かって、いちばん近い再描画の境界まで上がります。描き直しは、その境界の中をまとめて描く単位で行います。`RepaintBoundary` がないと、いちばん近い境界はルートの `RenderView` です。そのため、右の四角の色が変わるだけで、`RenderView` の中の全部が新しい絵に記録し直されます。

`RepaintBoundary` は、その位置に新しい境界を作ります。右の四角に付いた印は `RepaintBoundary` で止まり、描き直しはその中だけで行われます。境界ごとに自分の Layer を持つので、描き直さなかった左の四角は、前のフレームの絵(`PictureLayer`)をそのまま使います。この差分更新の流れは、[三つのツリーの仕組みと理由](../explanation/Trees.md) の差分更新の節で説明します。

## 関連

- [三つのツリーの仕組みと理由](../explanation/Trees.md) — 差分更新と再描画の境界
- [何も変わらないフレームは何をしているの?](IdleFrame.md) — 描き直しが起きないフレームを数える実験
- [描画の仕組みと理由](../explanation/Rendering.md) — Layer ツリー
- [実験の一覧](Home.md)
