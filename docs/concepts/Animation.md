# Animation — 時間で値を変える

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation
- 読み手: FloatSoda でオーバーレイを作る人と、本体を変える人の両方 / C# と、Unity か Flutter のどちらかを知っている / 時間に沿って値を変え、画面を動かす仕組みを理解したい
- テーマ: 時間に沿って値を変え、画面を動かす仕組み
- Why: 値がいつ、どう進むかを知らないと、動かない・止まらないといった不具合の原因が分からないため
- What: `AnimationController`、`Ticker`、`Curve`、`FadeTransition`
- How: Concept テンプレートに沿う
- 要素:
  - `Ticker` がフレームごとに値を進め、`Curve` が進み方を変える(Animation)
  - `AnimationController` の開始・停止・破棄(Animation)
  - `FadeTransition` で値を見た目に結び付ける(Animation)
  - 制約: Tween と暗黙のアニメーションはまだ使えない
- 見出し(案):
  - 概要
  - いつ使うか
  - 基本モデル
  - 最小例
  - Unity / uGUI からの読み替え
  - 制約と未実装
  - 関連ページ
- ※要確認: Tween と暗黙のアニメーションを追跡する Issue 番号
