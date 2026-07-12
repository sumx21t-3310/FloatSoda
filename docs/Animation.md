← [Home](Home.md)

# アニメーション

> **実装状況:**
> - **実装済み:** `AnimationController`(Forward / Reverse / Stop / AnimateWith)、`WidgetTicker` / `ITickerProvider` / `TickerProviderState<T>`、`Curve` / `Curves`(標準イージング一式)、`InterpolationSimulation`、`FadeTransition`(`RenderAnimatedOpacity` 経由のペイントのみ更新)が動作します。
> - **未実装:** `Tween<T>` / `CurvedAnimation` などのアニメーション合成、`AnimatedContainer` 系の暗黙的アニメーション、スプリング等の物理シミュレーションは未実装です(`ISimulation` を実装すれば `AnimateWith` で駆動は可能)。

FloatSoda のアニメーションは Flutter のアニメーション基盤を踏襲しています。**Ticker がフレームごとに経過時間を供給し、AnimationController がそれを 0.0〜1.0 の値に変換し、値の変化を購読したものだけが再描画される**という構造です。`SetState()` によるリビルドを介さずにペイントだけを更新できるため、毎フレームのアニメーションでも Widget ツリーの再ビルドコストがかかりません。

---

## 全体像

```
WidgetBinding (IFrameScheduler)
  │  ScheduleFrameCallback / フレームごとのタイムスタンプ
  ▼
WidgetTicker                       ← 開始からの相対時間を通知
  │  onTick(elapsed)
  ▼
AnimationController : IAnimation<double>
  │  ISimulation.X(t) で値を計算(Curve 適用)
  │  Changed / StatusChanged イベント
  ▼
FadeTransition → RenderAnimatedOpacity
                    └─ Changed 購読 → MarkNeedsPaint() → 再ペイントのみ
```

| クラス | 役割 |
|---|---|
| `IFrameScheduler` | フレームコールバックの登録・キャンセル。通常は `WidgetBinding` が実装(テストでは Fake に差し替え可能) |
| `WidgetTicker` | フレームごとに「開始からの相対時間」を通知。Flutter の `Ticker` 相当 |
| `ITickerProvider` / `TickerProvider` | Ticker の生成・追跡。`AnimationController.Vsync` に渡す |
| `TickerProviderState<T>` | `ITickerProvider` を提供する `State<T>` 基底。Flutter の `TickerProviderStateMixin` 相当 |
| `IAnimation<T>` | 時間経過で変化する値。`Value` / `Status` / `Changed` / `StatusChanged` を公開。Flutter の `Animation<T>` 相当 |
| `AnimationController` | `IAnimation<double>` の駆動役。Forward / Reverse / Stop / AnimateWith |
| `ISimulation` | 時間→値の関数。標準実装は `InterpolationSimulation`(begin→end を Duration と Curve で補間) |
| `ICurve` / `Curve` / `Curves` | イージング曲線。`Curves` に標準インスタンス一式 |

---

## AnimationController

`IAnimation<double>` の標準実装で、`LowerBound`(既定 0.0)〜 `UpperBound`(既定 1.0)の間を `Duration` かけて往復させます。

```csharp
var controller = new AnimationController
{
    Vsync = this,                          // TickerProviderState<T> を継承した State
    Duration = TimeSpan.FromSeconds(1.5),
    Curve = Curves.EaseInOut,              // 省略時は Linear
};

controller.Forward();          // UpperBound へ再生
controller.Reverse();          // LowerBound へ再生
controller.Forward(from: 0.5); // 指定値から再生(残り割合で Duration をスケール)
controller.Stop();             // 停止(Value は現在値のまま)
```

### AnimationStatus

`Status` は値がどちら側にあるか・どちらへ進行中かを表します。

| Status | 意味 |
|---|---|
| `Dismissed` | 停止・先頭(`LowerBound`) |
| `Forward` | 順方向に進行中 |
| `Reverse` | 逆方向に進行中 |
| `Completed` | 停止・末尾(`UpperBound`) |

`StatusChanged` を購読すると完了・折り返しを検知できます。往復アニメーションは `Completed` で `Reverse()`、`Dismissed` で `Forward()` を呼ぶのが定石です(`samples/FloatSoda.Samples.OverlayApp/PulseWidget.cs` 参照)。

### AnimateWith(カスタムシミュレーション)

`Forward` / `Reverse` は内部で `InterpolationSimulation` を使いますが、`AnimateWith(ISimulation)` に任意の `ISimulation` 実装を渡せば、スプリングなど時間関数が非線形なアニメーションも駆動できます。

---

## Ticker と TickerProvider

`AnimationController` は自分では時計を持ちません。`Vsync` に渡された `ITickerProvider` から `WidgetTicker` を生成し、フレームコールバック(`IFrameScheduler`、通常は `WidgetBinding`)経由でタイムスタンプを受け取ります。

`State` でコントローラを使う場合は `TickerProviderState<T>` を継承するのが最も簡単です:

```csharp
public record PulseState : TickerProviderState<PulseWidget>
{
    private AnimationController? _opacity;

    public override void InitState()
    {
        _opacity = new AnimationController
        {
            Vsync = this,
            Duration = TimeSpan.FromSeconds(1.5),
        };
        _opacity.Forward();
    }
}
```

`WidgetTicker` は `Muted = true` で(経過時間の基準を保ったまま)一時停止できます。`Dispose()` で Provider の追跡から外れます。

---

## Curve と Curves

`ICurve.Transform(t)` は正規化された時刻 t∈[0,1] を写像します。抽象基底 `Curve` は Flutter と同じ契約(端点 t==0 / t==1 はそのまま返し、間だけ `TransformInternal` に委譲)を保証し、`Flipped` で時間・値軸を反転したカーブを得られます。

### カーブ型

| 型 | 説明 |
|---|---|
| `LinearCurve` | 恒等写像 |
| `Cubic(a, b, c, d)` | 3次ベジェ。CSS の `cubic-bezier` 相当 |
| `ThreePointCubic` | 2つの `Cubic` を中点で連結(強調イージング用) |
| `SawTooth(count)` | 0→1 を count 回繰り返すノコギリ波 |
| `Interval(begin, end, curve)` | 指定区間だけ curve を適用、区間外はクランプ |
| `Threshold(t)` | t 未満は 0、以上は 1 のステップ |
| `FlippedCurve(curve)` | `1 - curve(1 - t)` |
| `DecelerateCurve` | 放物線状の減速 |
| `ElasticIn/Out/InOutCurve(period)` | ゴムひものような弾性振動 |
| `BounceIn/Out/InOutCurve` | ボールが跳ねるようなバウンス |

### 標準インスタンス(`Curves`)

Flutter の `Curves` と同じ係数で、命名は C# 規約(PascalCase)です。

| グループ | メンバ |
|---|---|
| 基本 | `Linear`, `Decelerate`, `FastOutSlowIn`, `SlowMiddle` |
| Ease | `Ease`, `EaseIn`, `EaseOut`, `EaseInOut`, `EaseInToLinear`, `LinearToEaseOut` |
| Ease バリエーション | `EaseIn/Out/InOut` × `Sine` / `Quad` / `Cubic` / `Quart` / `Quint` / `Expo` / `Circ` / `Back`(例: `EaseInOutCubic`) |
| 強調・複合 | `EaseInOutCubicEmphasized`, `FastLinearToSlowEaseIn`, `FastEaseInToSlowEaseOut` |
| バウンス | `BounceIn`, `BounceOut`, `BounceInOut` |
| 弾性 | `ElasticIn`, `ElasticOut`, `ElasticInOut` |

`Back` 系と `Elastic` 系は 0〜1 の範囲を行き過ぎる(オーバーシュートする)値を返します。`AnimationController` は `LowerBound`〜`UpperBound` で値をクランプするため、オーバーシュートをそのまま使いたい場合は注意してください。

---

## FadeTransition と RenderAnimatedOpacity

`FadeTransition` は `IAnimation<double>` で子の不透明度を駆動するウィジェットです。

```csharp
new FadeTransition
{
    Opacity = _controller,   // IAnimation<double>
    Child = ...,
}
```

ポイントは更新経路です。`RenderAnimatedOpacity` が `Changed` を購読し、値が変わったフレームだけ `MarkNeedsPaint()` を呼びます([RenderObjects](RenderObjects.md) の差分更新参照)。つまり:

- **Widget のリビルドは発生しない** — `SetState()` は不要
- **レイアウトも走らない** — 再ペイントのみ(`OpacityLayer` の差し替え)
- 不透明度が 0 のフレームは子のペイント自体をスキップ

毎フレーム値が変わるアニメーションで `SetState()` を使うとフレームごとに Widget ツリーの差分ビルドが走るため、アニメーション値は `*Transition` 系ウィジェット(現状は `FadeTransition`)で RenderObject に直結させるのが推奨パターンです。

---

## テストでの駆動

`IFrameScheduler` を Fake に差し替えると、実時間なしでアニメーションを進められます(`tests/FloatSoda.Test/Animation/` の `FakeFrameScheduler` 参照):

```csharp
var scheduler = new FakeFrameScheduler();
var provider = new TickerProvider { ResolveScheduler = () => scheduler };
var controller = new AnimationController { Vsync = provider, Duration = TimeSpan.FromSeconds(1) };

controller.Forward();
scheduler.Pump(TimeSpan.Zero);                 // 基準点
scheduler.Pump(TimeSpan.FromSeconds(0.5));     // Value == 0.5
```

---

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — `StatefulWidget` / `State` のライフサイクル
- [BuildPipeline](BuildPipeline.md) — フレームパイプラインと `WidgetBinding`
- [RenderObjects](RenderObjects.md) — `MarkNeedsPaint` による差分ペイント
- [APIDesign](APIDesign.md) — `Changed` / `StatusChanged` などイベント通知の規約
