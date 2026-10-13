# State — 状態と作り直し

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation
- 読み手: FloatSoda でオーバーレイを作る人と、本体を変える人の両方 / C# と、Unity か Flutter のどちらかを知っている / 状態を持つ Widget と、状態が変わったときに何が作り直されるかを理解したい
- テーマ: 状態を持つ Widget と、状態が変わったときに何が作り直されるか
- Why: 作り直しの範囲を知らないと、状態が消える・画面が更新されないといった不具合の原因が分からないため
- What: `SetState` と作り直しの範囲、State の寿命、`InheritedWidget` による値の配布
- How: Concept テンプレートに沿う
- 要素:
  - `SetState` を呼ぶと、その Widget から下が作り直される(WidgetSystem、BuildPipeline のうち利用者から観測できる部分)
  - State の寿命は、ツリーの中の位置と `Key` で決まる(WidgetSystem)
  - `InheritedWidget` は上から下へ値を配り、依存した子だけを作り直す(WidgetSystem)
  - 変更を通知するオブジェクトと `ListenableBuilder`(WidgetSystem)
  - 別のスレッドから状態を変える場合は Threading のページへリンクする
- 見出し(案):
  - 概要
  - いつ使うか
  - 基本モデル
  - 最小例
  - Unity / uGUI からの読み替え
  - 制約と未実装
  - 関連ページ
- ※要確認: なし
