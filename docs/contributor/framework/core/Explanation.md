# フレームワーク(コア): 仕組みと理由

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation(WritingDocumentation 5 章の Architecture と Design)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(コア)の層(`FloatSoda` の `FloatSodaApp` と Generic Host まわり、`Core`、`Geometrics`、`Painting`)を触ろうとしている / いまどう動き、なぜそう設計したかを理解したい
- テーマ: コアがどう動き、なぜその設計にしたか
- Why: いまどう動き、なぜそう設計したかを知らないと、変更の影響を見積もれないため
- What: `WidgetBinding` と `RenderPipeline`、アプリの起動と Generic Host
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - `WidgetBinding` と `RenderPipeline`(Architecture のフレームパイプラインの部分)
  - アプリの起動と Generic Host(新しく書く)
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
