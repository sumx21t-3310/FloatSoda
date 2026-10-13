# 何も変わらないフレームは何をしているの?

四角を2つ並べた画面で5フレームを回し、フレームごとに `Paint` が呼ばれた回数を数えます。何も変わらないフレームでは 0 回です。`SetState` を1回呼んだフレームだけ、回数が増えます。SteamVR は要りません。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意します。次のコードを、リポジトリのルートに `IdleFrame.cs` という名前で保存します。

```csharp
#:project src/FloatSoda.Testing
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using SkiaSharp;

var view = new RenderView { FixedSize = new SKSize(200, 100) };
var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
var owner = new BuildOwner(() => { });
new RenderObjectToWidgetAdapter { Container = view, Child = new Row { Children = [new Counter(), new Counter()] } }
    .AttachToRenderTree(owner, null);
view.PrepareInitialFrame();

for (var frame = 1; frame <= 5; frame++)
{
    if (frame == 3) Counter.Last!.Bump();   // 3フレーム目だけ、右の Counter で SetState を呼ぶ
    RenderSquare.Paints = 0;
    owner.BuildScope();                     // Build
    pipeline.FlushLayout();                 // Layout
    pipeline.FlushPaint();                  // Paint
    Console.WriteLine($"frame {frame}: Paint {RenderSquare.Paints} 回");
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
    public override Widget Build(IBuildContext context) =>
        new Square { Color = _count % 2 == 0 ? SKColors.Red : SKColors.Blue };
}

record Square : LeafRenderObjectWidget<RenderSquare>
{
    public SKColor Color { get; init; }
    public override RenderSquare CreateRenderObject() => new() { Color = Color };
    public override void UpdateRenderObject(RenderSquare r) { if (r.Color != Color) { r.Color = Color; r.MarkNeedsPaint(); } }
}

class RenderSquare : RenderBox
{
    public static int Paints;
    public SKColor Color;
    public override void PerformLayout() => Size = new SKSize(50, 50);
    public override void Paint(PaintingContext context, Offset offset)
    {
        Paints++;
        context.Canvas.DrawRect(SKRect.Create(offset, Size), new SKPaint { Color = Color });
    }
}
```

リポジトリのルートで実行します。

```bash
dotnet run IdleFrame.cs
```

ループの中の3行が、1フレームでする仕事(Build、Layout、Paint)です。`RenderSquare` は、`Paint` が呼ばれるたびに数を1つ増やす四角です。終わったら、`IdleFrame.cs` を消します。

## 見えるもの

最初のフレームと、`SetState` を呼んだ3フレーム目だけ、`Paint` が呼ばれます。ほかのフレームは 0 回です。

```text
frame 1: Paint 2 回
frame 2: Paint 0 回
frame 3: Paint 2 回
frame 4: Paint 0 回
frame 5: Paint 0 回
```

3フレーム目で `SetState` を呼んだのは右の `Counter` だけですが、`Paint` は左の四角でも呼ばれて 2 回になります。描き直す範囲がどう決まるかは、[1フレームで、どこが描き直されているの?](RepaintRegion.md) で確かめられます。

## 壊して直す

`if (frame == 3) Counter.Last!.Bump();` の行を消して実行すると、3フレーム目も 0 回になります。行を戻すと、3フレーム目の 2 回が戻ります。何も変わらないフレームを何度回しても、`Paint` は呼ばれません。

## なぜそうなるか

1フレームの仕事は、作り直しや描き直しが要ると印を付けられたものだけを処理します。`BuildOwner.BuildScope` は、作り直しの待ちリストが空なら何もせずに戻ります。`RenderPipeline.FlushLayout` と `FlushPaint` も、レイアウトと描き直しの待ちリストにあるものだけを処理します。何も変わらないフレームでは、どのリストも空なので、`Paint` は呼ばれません。アプリを動かすときは、`WidgetBinding.DrawFrame` が、描き直しの予約がないフレームでレイアウトと描画を飛ばします。

`SetState` を呼ぶと、その `State` の Element が作り直しの待ちリストに入ります。作り直しで `Square` の色が変わると、`UpdateRenderObject` が `MarkNeedsPaint` を呼び、`RenderSquare` に描き直しの印が付きます。印は、いちばん近い再描画の境界(この実験ではルートの `RenderView`)まで上がります。そのため、境界の中にある左右の四角が両方とも描き直されます。この差分更新の流れは、[フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) の差分更新の節で説明します。

## 関連

- [フレームワーク(三つのツリー): 仕組みと理由](../framework/trees/Explanation.md) — 差分ビルドと差分更新
- [1フレームで、どこが描き直されているの?](RepaintRegion.md) — 描き直す範囲を `RepaintBoundary` で狭める実験
- [State](../../concepts/State.md) — 利用者から見た `SetState`
- [実験の一覧](Home.md)
