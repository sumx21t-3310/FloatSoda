# フレームワーク(三つのツリー): 仕組みと理由

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation(WritingDocumentation 5 章の Architecture と Design)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(三つのツリー)の層(`FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture`。`FloatSoda.Hooks`、`FloatSoda.Testing`)を触ろうとしている / いまどう動き、なぜそう設計したかを理解したい
- テーマ: 三つのツリーがどう連携し、なぜその設計にしたか
- Why: いまどう動き、なぜそう設計したかを知らないと、変更の影響を見積もれないため
- What: Widget と Element、差分ビルド、制約とレイアウト、差分更新、ヒットテストとジェスチャ、Hooks
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - Widget と Element の役割(WidgetSystem)
  - 差分ビルドと BuildOwner(BuildPipeline)
  - 制約フロー、差分更新、intrinsic 測定(RenderObjects の各節)
  - ヒットテストとジェスチャ(新しく書く)
  - Hooks(WidgetSystem の Hooks の節)
- 見出し(案):
  - 実装状況
  - 責務と境界
  - 関係するコンポーネント
  - 処理の流れ
  - 所有権・ライフサイクル
  - なぜそう設計したか
  - Flutter との対応
  - テストでの駆動
  - 関連
- ※要確認: なし
