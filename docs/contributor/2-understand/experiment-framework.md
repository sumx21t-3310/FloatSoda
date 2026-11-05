← [コントリビューター](../Home.md)

# 実験: フレームワーク層

## 問い: 一部の色を変えると、どこが描き直されるか

`SetState` で画面の一部の色を変えたとき、どの再描画境界が描画待ちリストに登録されるかを、ログで確かめる実験です。最終画像の部分再描画を調べるものではありません。[フレームワーク層](framework-layer.md) を読んだあとに進みます。HMD も SteamVR も使いません。

確かめる対象は `RenderPipeline.NodesNeedingPaint` です。次の `FlushPaint` で描き直す RenderObject の待ちリストで、空ならその時点で描き直すものはありません。このリストを、4つのタイミングで出力します。

1. 初回フレームのあと
2. `SetState` の直後
3. `BuildScope` のあと
4. `FlushPaint` のあと

先に予想を立てます。2 と 3 で、リストに何が入るかを考えてください。2つの四角を縦に並べて、片方の色だけを変えます。それぞれを `RepaintBoundary` で包みます。

## 作例を動かす

作例は [samples/Experiments/FloatSoda.Samples.ExperimentFramework](../../../samples/Experiments/FloatSoda.Samples.ExperimentFramework/) にあります。コードの全文は、このページに載せず、作例のファイルを読みます。作例は、パイプラインを自分で組み、`BuildScope`、`FlushLayout`、`FlushPaint` を順に呼びます。2つの四角を `Column` に並べ、上の四角の色を `SetState` で切り替えます。それぞれの四角は `RepaintBoundary` で包みます。

リポジトリのルートで、次のコマンドを実行します。.NET の SDK のバージョンなど、開発の準備は [開発環境を用意する](../3-prepare/dev-environment.md) にあります。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentFramework
```

**できた?** 「初回フレーム後」「SetState 直後」「BuildScope 後」「FlushPaint 後」の4つの行が出た。

<details>
<summary>できなかったら</summary>

SDK が見つからない、またはビルドで例外が出る場合は、[開発環境を用意する](../3-prepare/dev-environment.md) で SDK と clone の状態を確かめます。コンパイルが通らない場合は、公開 API が変わった可能性があります。作例が使っている型(`RenderPipeline`、`BuildOwner`、`RenderObjectToWidgetAdapter`、`StatefulWidget<T>`、`State<T>`)の名前と使い方を、[フレームワーク層](framework-layer.md) と、`src/FloatSoda` の現在のソースで照らします。

</details>

## ログを読む

出力は次の4行になります。

```text
初回フレーム後: NodesNeedingPaint = []
SetState 直後: NodesNeedingPaint = []
BuildScope 後: NodesNeedingPaint = [RenderRepaintBoundary@depth2]
FlushPaint 後: NodesNeedingPaint = []
```

見る場所は、リストの中身です。

- `SetState` 直後: 空
- `BuildScope` 後: `RenderRepaintBoundary` が1件
- `FlushPaint` 後: 空

`SetState` が行うのは再ビルドの予約までなので、直後は RenderObject がまだ変わらず、リストは空です。`BuildScope` の後に境界が1件入るのは、色を変えた四角の `RenderColoredBox` が `MarkNeedsPaint` を呼び、直近の境界で要求が止まるためです。`@depth2` は木の深さで、`RenderView`(0)、`Column` の `RenderFlex`(1)、境界(2)の順です。

このログで確認できるのは、深さ2の境界が1件だけ登録されたことです。2つの境界は型も深さも同じなので、どちらの境界かはこの出力から区別できません。

**できた?** 「`BuildScope` のあとだけ、深さ2の `RenderRepaintBoundary` が1件入った」と言える。

<details>
<summary>できなかったら</summary>

`BuildScope` のあとのリストが空の場合は、作例の中で `Toggle()`、`BuildScope()`、各行を出す処理の呼び出し順を確かめます。`[RenderRepaintBoundary@depth2]` ではなく `[RenderView@depth0]` が出た場合は、実行コマンドに `no-boundary` を付けていないかを確かめます。

</details>

## 壊してみる: RepaintBoundary を外す

境界を外すと、結果がどう変わるかを予想してから実行します。引数 `no-boundary` を付けると、作例は四角を `RepaintBoundary` で包まずに、そのまま並べます。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentFramework -- no-boundary
```

<!-- ※要確認: 作例は #313 で main に入った。引数 no-boundary は #313 の実際の引数に合わせた。出力の4行の形は、移す前のコードに基づくので、#313 がマージされたら実物と照らす -->

**できた?** 「`BuildScope` 後」の行が `[RenderView@depth0]` になった。描き直しの範囲が、境界から根の `RenderView` まで広がっている。

<details>
<summary>できなかったら</summary>

行が `[RenderRepaintBoundary@depth2]` のままなら、引数が渡っていません。`--` の位置と綴りを確かめます。

</details>

## なぜそうなるか

この例では、`RenderColoredBox.Color` の値が変わると、`MarkNeedsPaint` が呼ばれます。`MarkNeedsPaint` は、自分が再描画境界ならそこで止まって `NodesNeedingPaint` に登録し、そうでなければ親へ伝わります。`RepaintBoundary` があると、色を変えた `RenderColoredBox` から親へ伝わった要求が、最初の境界で止まります。境界がなければ、要求は根の `RenderView` まで伝わります。`RenderView` も再描画境界なので、そこで登録されます。

つまり、描き直す範囲は、変えたノードの祖先をたどって、最初に見つかる境界までです。自分の変更で境界をどこに置くかは、この範囲の広さで考えます。範囲が根まで広がる場所に、頻繁に変わる部品を置かないようにします。伝播の細部は、[フレームワーク層](framework-layer.md) の「レイアウトと描画は、境界までを描き直す」にあります。

<!-- ※要確認: HMD も SteamVR も要らないことは、OVRApplication を作らないコードで実行して確かめた(この実験の実行は、SteamVR が入った PC だった) / SteamVR が入っていない PC でも同じ出力になるかを確かめる -->

## 次に読むもの

- [フレームワーク層](framework-layer.md): ツリーの役割と、差分更新の仕組み
- [実験: 描画層](experiment-rendering.md): Layer の合成を png で確かめる
- [1フレームが SteamVR に届くまで](frame-to-steamvr.md): 描き直した結果が、SteamVR に届くまで

