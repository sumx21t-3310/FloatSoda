# フレームワーク(三つのツリー): 作業の手順

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / how-to(WritingDocumentation 5 章の Development)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(三つのツリー)の層(`FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture`。`FloatSoda.Hooks`、`FloatSoda.Testing`)を触ろうとしている / 決まった作業を規約どおりに終えたい
- テーマ: 三つのツリーの作業の手順
- Why: 決まった作業の手順と規約を知らないと、レビューで何度も直すことになるため
- What: ウィジェットを移植する、カスタムクリッパーを実装する、ヘッドレスでテストする
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - ウィジェットを移植する。Flutter 本家の確認、テスト観点、層1サンプル、差異の記録(新しく書く)
  - カスタムクリッパーを実装する(RenderObjects の「カスタムクリッパーの実装」)
  - ヘッドレスでテストする(`FloatSoda.Testing`、新しく書く)
- 見出し(案):
  - 規約
  - 手順
  - 規約が守られる場所
  - 関連
- ※要確認: なし
