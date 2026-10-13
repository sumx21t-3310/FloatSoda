# フレームワーク(アニメーション): 仕組みと理由

FloatSoda のアニメーションは、Flutter のアニメーション基盤と同じ3段の仕組みで動きます。`WidgetTicker` がフレームごとに経過時間を届け、`AnimationController` がその時間を値に変え、値の変化を購読した RenderObject だけが描き直されます。このページでは、値が画面に届くまでの流れ、`WidgetTicker` とフレームの関係、持ち主と寿命、設計の理由を説明します。

## 実装状況

| 対象 | 現状 |
|---|---|
| `AnimationController`(`Forward`、`Reverse`、`Stop`、`AnimateWith`) | 使える |
| `WidgetTicker`、`ITickerProvider`、`TickerProvider`、`TickerProviderState<T>` | 使える。破棄し忘れた `WidgetTicker` は検出しない(追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282)) |
| `Curve` の系統と `Curves` の標準インスタンス | 使える。一覧は `src/FloatSoda/Animation/Curves.cs` にある |
| `InterpolationSimulation` | 使える。`ISimulation` を実装すれば、ほかの時間関数でも `AnimateWith` で動かせる |
| `FadeTransition`(`RenderAnimatedOpacity`) | 使える。値を受け取って描画だけを更新する、いまある唯一の Widget |
| `Tween`、`CurvedAnimation`、繰り返し、`AnimatedBuilder` | ない(追跡: [#137](https://github.com/sumx21t-3310/FloatSoda/issues/137)) |
| `AnimatedContainer` などの暗黙的アニメーション | ない(追跡: [#163](https://github.com/sumx21t-3310/FloatSoda/issues/163)) |

## 責務と境界

アニメーションの層は、時間を値に変えるところまでを担います。

| 担う | 担わない(担う層) |
|---|---|
| 経過時間を、範囲の中の値と進行状態(`AnimationStatus`)に変える | 毎フレームの時刻を作ること(コアの `WidgetBinding` と `FloatSodaApp`) |
| 値と状態の変化を `event` で知らせる | 値を受け取って描き直すこと(三つのツリーの `RenderAnimatedOpacity`) |
| 時刻の正規化された進み方を変える(`Curve`) | 状態を持つ Widget の寿命(三つのツリーの `State<T>`) |

## 関係するコンポーネント

| 型 | ソース | 役割 |
|---|---|---|
| `IFrameScheduler` | `src/FloatSoda/Core/IFrameScheduler.cs` | 次のフレームで呼ぶ処理を登録する口。通常は `WidgetBinding` が実装する |
| `WidgetTicker` | `src/FloatSoda/Animation/Ticker.cs` | 最初の tick からの経過時間を、フレームごとに届ける |
| `ITickerProvider`、`TickerProvider` | 同上 | `WidgetTicker` を作り、追跡する |
| `TickerProviderState<T>` | `src/FloatSoda/Animation/TickerProviderState.cs` | `ITickerProvider` を兼ねる `State<T>` の基底 |
| `AnimationController` | `src/FloatSoda/Animation/AnimationController.cs` | `IAnimation<double>` の実装。経過時間を `ISimulation` で値に変える |
| `ISimulation`、`InterpolationSimulation` | `src/FloatSoda/Animation/Simulation.cs` | 経過秒数から値を返す関数 |
| `ICurve`、`Curve` | `src/FloatSoda/Animation/Curve.cs` | 0〜1 の時刻を写す関数 |
| `RenderAnimatedOpacity` | `src/FloatSoda/RenderObjects/Animation/` | `IAnimation<double>` を購読し、値が変わったフレームだけ描き直す |

## 処理の流れ

### 値が画面に届くまで

`FadeTransition` で不透明度を動かす場合、1フレームの中で次の順に進みます。

```text
WidgetBinding.BeginFrame(elapsed)
  └─ フレームコールバックを呼ぶ
       └─ WidgetTicker.Tick(elapsed)                 開始からの経過時間に直す
            └─ AnimationController.OnTick(経過時間)
                 └─ ISimulation.X(秒) で値を計算し、範囲に収める
                      └─ 値が変わったら Changed を発火
                           └─ RenderAnimatedOpacity が MarkNeedsPaint
  └─ DrawFrame()
       └─ BuildScope(再ビルドする Element はない)
       └─ FlushLayout(dirty なノードはない)
       └─ FlushPaint(RenderAnimatedOpacity の境界だけを描き直す)
```

`AnimationController` は、`Forward` と `Reverse` で `InterpolationSimulation` を作ります。`InterpolationSimulation` は、経過秒数を `Duration` で割って 0〜1 に収め、`Curve.Transform` を通してから開始値と終了値のあいだを補間します。値が `Duration` を越えると `IsDone` が `true` になり、`AnimationController` は `WidgetTicker` を止めて、状態を `Completed` か `Dismissed` にします。

この流れでは、Widget の再ビルドもレイアウトも起きません。`State.SetState` で値を Widget に渡す書き方にすると、毎フレーム `BuildScope` で差分ビルドが走ります。

### WidgetTicker とフレームの関係

`WidgetTicker` は自分で時計を持ちません。時刻は、`IFrameScheduler.ScheduleFrameCallback` で登録した処理を通して、フレームごとに受け取ります。`WidgetBinding` での仕組みは次のとおりです。

- 登録した処理は次のフレームの `BeginFrame` で1回だけ呼ばれて消えるので、`WidgetTicker` は呼ばれるたびに次のフレームの分を登録し直す
- `BeginFrame` は登録の一覧を写し取って消してから呼ぶので、呼ばれている最中に登録し直した処理は次のフレームで呼ばれる
- 1つの `WidgetTicker` は、1フレームに多くても1回しか tick しない
- フレームの中の順序はポインタ入力 → フレームコールバック → `DrawFrame` なので、tick の中で変えた値は同じフレームの描画に反映される
- 渡される時刻は、アプリの開始からの単調な経過時間で、1フレームの中ではすべてのウィンドウで同じ値になる([コアの Explanation](../core/Explanation.md))
- フレームコールバックは、メインループのスレッドで呼ばれる(契約として明文化するかは、追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282))

`WidgetTicker` は、最初の tick で受け取った時刻を基準点(`StartTime`)にし、それ以降は基準点からの差を届けます。`Start` したフレームではなく、次の tick が経過時間 0 になります。

| 操作 | 基準点 | 登録 |
|---|---|---|
| `Start()` | 次の tick で決まる | 次のフレームの分を登録する |
| `Muted = true` | 保つ | 登録を取り消す |
| `Muted = false` | 保つ。止めていたあいだの時間も、経過時間に含まれる | 登録し直す |
| `Stop()` | 消す | 登録を取り消す |
| `Dispose()` | — | 登録を取り消し、`TickerProvider` の追跡から外れる |

`AnimationController` は、`Forward`、`Reverse`、`AnimateWith` のたびに `Stop` と `Start` を続けて呼び、基準点を作り直します。

tick の間隔は、メインループの1周の間隔と同じです。メインループは `FloatSodaOptions.TargetFrameRate` に合わせて `FramePacer` で待ちます。

### スケジューラの解決

`TickerProviderState<T>` が作る `WidgetTicker` は、`Element.Owner.FrameScheduler`(その Element が属するウィンドウの `WidgetBinding`)から時刻を受け取ります。スケジューラは登録のたびに解決します。解決できない間(Element がマウントされる前)に `Start` すると、登録されずに終わります。現在の実装では、`InitState` の時点で Element が `Owner` を持つため、`InitState` から `Forward` を呼べば動きます。これを契約とするかは、追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282) で決めます。

## 所有権・ライフサイクル

| もの | 持ち主 | 作られるとき | 捨てられるとき |
|---|---|---|---|
| `AnimationController` | 利用者の `State`(フィールド) | 利用者が `new` したとき。多くは `InitState` | 利用者が `Dispose` を呼んだとき |
| `WidgetTicker` | `AnimationController`。追跡は `TickerProvider` | 最初の `Forward`、`Reverse`、`AnimateWith` | `AnimationController.Dispose` |
| `Changed` と `StatusChanged` の購読 | 購読した側 | 購読したとき | `AnimationController.Dispose` がすべて外す |
| `RenderAnimatedOpacity` の購読 | `RenderAnimatedOpacity` | `Attach` | `Detach` |

`TickerProviderState<T>` は、`State` の `Dispose` で自分が作った `WidgetTicker` を破棄しません。利用者が `AnimationController.Dispose` を書き忘れると、ウィジェットがツリーから外れたあとも、`WidgetTicker` がフレームコールバックを登録し続けます。検出か自動の破棄を足すかは、追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282) で決めます。

## なぜそう設計したか

| 判断 | 理由 | 出典 |
|---|---|---|
| 値の変化を RenderObject が直接購読し、描画だけを更新する | 毎フレーム値が変わっても、Widget の再ビルドとレイアウトを走らせないため | 現行の [Animation](../../Animation.md) |
| `WidgetTicker` に時計を持たせず、`IFrameScheduler` から時刻を受け取る | フレームごとのタイムスタンプを受け取るため。テストでは `IFrameScheduler` を偽物に差し替えられる | `IFrameScheduler` の XML ドキュメントコメント |
| `Forward(from:)` で途中から始めるとき、残りの割合で `Duration` を縮める | 途中から再開しても、進む速さを変えないため | `AnimationController.AnimateTo` のコメント |
| `Curve` は端点(`t` が 0 と 1)をそのまま返す | 計算の誤差で端点の値がずれないようにするため。Flutter と同じ | `Curve.Transform` のコメント |
| `TickerProviderState<T>` は継承ではなく `TickerProvider` への委譲で作る | スケジューラがマウントのあとでないと決まらず、遅れて解決する必要があるため | `TickerProviderState<T>` のコメント |

## Flutter との対応

| FloatSoda | Flutter |
|---|---|
| `WidgetTicker` | `Ticker` |
| `ITickerProvider`、`TickerProviderState<T>` | `TickerProvider`、`TickerProviderStateMixin` |
| `IAnimation<T>`、`IListenable` | `Animation<T>`、`Listenable` |
| `AnimationController` | `AnimationController` |
| `ISimulation`(`X`、`Dx`、`IsDone`) | `Simulation`(`x`、`dx`、`isDone`) |
| `ICurve`、`Curve`、`Curves` | `ParametricCurve<double>`、`Curve`、`Curves`。`Curves` の係数は Flutter と同じ |
| `FadeTransition`、`RenderAnimatedOpacity` | `FadeTransition`、`RenderAnimatedOpacity` |

通知を `addListener` ではなく C# の `event` で受け取るのは、表現の違いで、挙動の差異ではありません([APIDesign](../../APIDesign.md) の「3.6 リスナーパターンを実装せず `event` を使う」)。Flutter にあって FloatSoda にないものは次のとおりです。

- `State` の破棄時に動いている `Ticker` を見つけてエラーにする仕組み(追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282))
- `TickerMode`、`TickerFuture`、`absorbTicker`(#282 では、需要が出てから扱うとしている)
- `Tween`、`CurvedAnimation`、`repeat`(追跡: [#137](https://github.com/sumx21t-3310/FloatSoda/issues/137))

## テストでの駆動

`IFrameScheduler` を偽物に差し替えると、実時間を待たずに好きな時刻でアニメーションを進められます。`tests/FloatSoda.Test/Animation/FakeFrameScheduler.cs` の `Pump(TimeSpan)` は、登録された処理を写し取ってから呼ぶので、`WidgetBinding.BeginFrame` と同じ順序で動きます。

```csharp
var scheduler = new FakeFrameScheduler();
var provider = new TickerProvider { ResolveScheduler = () => scheduler };
var controller = new AnimationController { Vsync = provider, Duration = TimeSpan.FromSeconds(1) };

controller.Forward();
scheduler.Pump(TimeSpan.Zero);                 // 最初の tick が基準点になる
scheduler.Pump(TimeSpan.FromSeconds(0.5));     // Value は 0.5
```

ほかの実例は次のとおりです。

- `tests/FloatSoda.Test/Animation/WidgetTickerTest.cs` — 基準点、`Muted`、`Stop` と `Start`
- `tests/FloatSoda.Test/Animation/AnimationControllerTest.cs` — 値と状態の進み方
- `tests/FloatSoda.Test/RenderObjects/RenderAnimatedOpacityTest.cs` — 値の変化が描画に届くこと
- `tests/FloatSoda.Test/Core/WidgetBindingTest.cs` — `BeginFrame` がフレームコールバックに時刻を渡すこと

## 関連

- [アニメーションの Reference](Reference.md) — アニメーションで守る条件
- [アニメーションの HowTo](HowTo.md) — アニメーションの部品を移植する手順
- [コアの Explanation](../core/Explanation.md) — フレームの1周と時刻の作り方
- [Animation](../../Animation.md) — 元になった現行ページ
