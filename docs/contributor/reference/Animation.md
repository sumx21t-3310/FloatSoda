# アニメーションで守ること

アニメーションの層で、実装を変えても維持する条件をまとめます。仕組みの説明は [アニメーションの仕組みと理由](../explanation/Animation.md) にあります。

> ※要確認 — このページの条件は、現在の実装と、それを固定しているテストから読み取ったものです。`WidgetTicker` の契約(コールバックのスレッド、マウント前の `Start`、破棄し忘れの扱い)は、追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282) で明文化する途中です。#282 で決まったら、このページを合わせます。

## 適用範囲

| 対象 | ソース |
|---|---|
| `WidgetTicker`、`TickerProvider`、`TickerProviderState<T>` | `src/FloatSoda/Animation/Ticker.cs`、`TickerProviderState.cs` |
| `AnimationController` | `src/FloatSoda/Animation/AnimationController.cs` |
| `Curve` を継承する型 | `src/FloatSoda/Animation/Curve.cs` |
| `IAnimation<T>` を購読する RenderObject | `src/FloatSoda/RenderObjects/Animation/` |

## 必須条件

- `Curve` を継承する型は、`TransformInternal` だけを実装し、端点の扱いを基底の `Transform` に任せること
- 自分と同じ系統の `Curve` を返す計算プロパティを持たせるときは、`PrintMembers` を書くこと(基底の `Curve` は `Flipped` を持つため、`protected virtual bool PrintMembers` を用意している)
- `IAnimation<T>` を購読する RenderObject は、`Attach` で購読し、`Detach` で解除すること
- 購読先を差し替えるプロパティは、古い購読を外してから新しい購読を付けること
- `IAnimation<T>` を購読する RenderObject は、描画に使う値が変わったときだけ `MarkNeedsPaint` を呼ぶこと
- `AnimationController` を持つ `State` は、`Dispose` で `AnimationController.Dispose` を呼ぶこと

## 不変条件

- `AnimationController.Value` は、`Curve` が範囲を越える値を返しても、常に `LowerBound` 以上 `UpperBound` 以下である
- `Curve.Transform` は、`t` が 0 なら 0 を、1 なら 1 を返す
- 1つの `WidgetTicker` は、1フレームに多くても1回しか tick しない
- `WidgetTicker` が届ける経過時間は、`Start` のあとの最初の tick で 0 になる

## Observable behavior

| 振る舞い | 内容 |
|---|---|
| `Changed` | `Value` が変わったときだけ発火する |
| `StatusChanged` | `Status` が別の値に変わったときだけ、新しい値を伴って発火する |
| `Forward()` | `UpperBound` へ進み、着いたら `Completed` になる |
| `Reverse()` | `LowerBound` へ進み、着いたら `Dismissed` になる |
| `Forward(from:)` | 指定した値から始め、残りの割合だけ `Duration` を縮める |
| すでに終端にいるときの `Forward()` | フレームを待たずに、その場で `Completed` になる |
| `Stop()` | 進行を止め、`Value` をそのときの値のまま保つ |
| `WidgetTicker.Muted` | `true` の間は tick しない。`false` に戻すと、基準点を保ったまま再開する |
| `WidgetTicker.Stop()` のあとの `Start()` | 基準点を作り直し、経過時間が 0 から始まる |
| `AnimationController.Dispose()` | フレームコールバックの登録を取り消し、`Changed` と `StatusChanged` の購読をすべて外す |

## 破ったときの failure mode

| 破った条件 | 起きること |
|---|---|
| `State.Dispose` で `AnimationController.Dispose` を呼ばない | ウィジェットがツリーから外れたあとも、`WidgetTicker` がフレームコールバックを登録し続ける。値の変化を `SetState` に渡していれば、破棄した `State` に対して `SetState` が呼ばれる(#282 の背景) |
| `Changed` を、値が変わらないフレームでも発火する | 購読側が、変化のないフレームでも再ビルドか再描画をする |
| 購読する RenderObject が `Detach` で購読を外さない | ツリーから外した RenderObject に通知が届き続け、`MarkNeedsPaint` が呼ばれる。`AnimationController` が生きているあいだ、RenderObject が回収されない |
| `Curve` の派生型で `PrintMembers` を書かずに、同じ系統の `Curve` を返すプロパティを足す | `ToString()` が終わらず、スタックオーバーフローになる |
| `Value` を範囲に収めない | `Back` 系や `Elastic` 系の `Curve` を使ったとき、範囲の外の値が購読側に渡る。`RenderAnimatedOpacity` では、不透明度を `byte` へ変換するときに範囲を外れる |

## 検証方法

| 条件 | 固定しているテスト |
|---|---|
| `Forward` と `Reverse` の進み方と終わり方 | `tests/FloatSoda.Test/Animation/AnimationControllerTest.cs` の `Forward_ProgressesLinearlyAndCompletes`、`Reverse_ProgressesToLowerBoundAndDismisses` |
| `Changed` の発火 | 同じファイルの `Forward_RaisesChangedWhenValueChanges` |
| `Forward(from:)` の `Duration` | 同じファイルの `Forward_FromMidpoint_ScalesRemainingDuration` |
| 終端にいるときの `Forward()` | 同じファイルの `Forward_WhenAlreadyAtUpperBound_CompletesImmediately` |
| `Stop()` で値を保つ | 同じファイルの `Stop_RetainsCurrentValue` |
| `AnimateWith` | 同じファイルの `AnimateWith_DrivesValueFromCustomSimulation` |
| `WidgetTicker` の基準点、`Muted`、`Stop` | `tests/FloatSoda.Test/Animation/WidgetTickerTest.cs` の `Start_DeliversElapsedTimeRelativeToFirstTick`、`Muted_PausesAndResumesTicking`、`Stop_ThenStart_ResetsElapsedTimeBase`、`Stop_CancelsScheduledCallback` |
| `Curve` の端点 | `tests/FloatSoda.Test/Animation/CurveTest.cs` の `Transform_PreservesEndpoints` |
| `Curve` の `ToString()` が終わる | `tests/FloatSoda.Test/RecordToStringTest.cs` の `ToString_FlippedCurve_元のカーブを含む文字列を返す` と、`PrintMembers_自分と同じ系統のrecordを返す計算プロパティを持つ_自動生成のままにしていない` |
| 購読する RenderObject の `Detach`、値が変わらないときの描画 | `tests/FloatSoda.Test/RenderObjects/RenderAnimatedOpacityTest.cs` の `Detach_UnsubscribesFromAnimation`、`OpacitySwap_ResubscribesToNewAnimation`、`UnchangedValue_DoesNotMarkNeedsPaint` |
| `Value` を範囲に収める | 未設定 |
| `StatusChanged` が変化のときだけ発火する | 未設定 |
| `AnimationController.Dispose` | 未設定 |
| `State.Dispose` で `AnimationController.Dispose` を呼ぶ | 未設定。検出の仕組みもない(追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282)) |

## Flutter との関係

| 条件 | Flutter |
|---|---|
| `Curve.Transform` の端点 | 同じ。Flutter の `Curve.transform` も、0 と 1 をそのまま返す |
| `Value` を範囲に収める | 同じ。Flutter の `AnimationController` も `lowerBound` と `upperBound` に収める |
| `Muted` のあいだも基準点を保つ | 同じ。Flutter の `Ticker.muted` も経過時間を進め続ける |
| 動いている `Ticker` を残したまま `State` を破棄したとき | Flutter はエラーにする。FloatSoda は検出しない(追跡: [#282](https://github.com/sumx21t-3310/FloatSoda/issues/282)) |
| 通知を C# の `event` で受け取る | 表現の違いで、挙動の差異ではない([APIDesign](../APIDesign.md) の「3.6」) |

台帳([`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md))に、アニメーションのエントリはまだありません。

## 関連

- [アニメーションの仕組みと理由](../explanation/Animation.md) — 条件の背景にある仕組み
- [アニメーションの部品を移植する](../how-to/PortAnimation.md) — アニメーションの部品を移植する手順
- [三つのツリーで守ること](Trees.md) — RenderObject の購読と attach / detach の条件
