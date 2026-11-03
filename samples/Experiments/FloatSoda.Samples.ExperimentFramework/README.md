# ExperimentFramework

## これは何か

`SetState` で画面の一部の色を変えたとき、どの再描画境界が描画待ちリストに登録されるかを、ログで確かめる実験です。確かめる対象は `RenderPipeline.NodesNeedingPaint` で、次の `FlushPaint` で描き直す RenderObject の待ちリストです。最終画像の部分再描画は調べません。

HMD も SteamVR も使いません。`RenderPipeline`、`BuildOwner`、`RenderObjectToWidgetAdapter` を自分で組み、`BuildScope`、`FlushLayout`、`FlushPaint` を順に呼びます。`FloatSodaApp` は使いません。

コントリビューター向け docs「実験: フレームワーク層」のコードの実体です。

## 使い方

`Program.cs` が、次の4つのタイミングで `NodesNeedingPaint` を出力します。

1. 初回フレームのあと
2. `SetState` の直後
3. `BuildScope` のあと
4. `FlushPaint` のあと

`TogglePanel` は、`Column` に四角を2つ並べ、上の四角の色を `SetState` で切り替えます。引数なしのときは、それぞれの四角を `RepaintBoundary` で包みます。引数 `no-boundary` を付けると包みません。

境界ありの出力は次のとおりです。

```text
初回フレーム後: NodesNeedingPaint = []
SetState 直後: NodesNeedingPaint = []
BuildScope 後: NodesNeedingPaint = [RenderRepaintBoundary@depth2]
FlushPaint 後: NodesNeedingPaint = []
```

`no-boundary` では、`BuildScope` 後の行が `[RenderView@depth0]` になります。描き直しの範囲が、境界から根の `RenderView` まで広がります。

## Flutterとの違い

対象外です。Flutter のウィジェットを移植したものではなく、FloatSoda のフレームワーク層の挙動を確かめる実験です。

## 実行

SteamVR も HMD も要りません。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentFramework
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentFramework -- no-boundary
```

## 関連

- コントリビューター向け docs「実験: フレームワーク層」
- `samples/Experiments/FloatSoda.Samples.ExperimentRendering`: 描画層の実験
