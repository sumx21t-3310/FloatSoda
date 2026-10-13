# 描画: 作業の手順

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / how-to(WritingDocumentation 5 章の Development)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / 描画の層(`FloatSoda.Rendering`)を触ろうとしている / 決まった作業を規約どおりに終えたい
- テーマ: 描画層の作業の手順
- Why: 決まった作業の手順と規約を知らないと、レビューで何度も直すことになるため
- What: Layer を足す、描画結果をビットマップで確かめる
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - Layer を足す。Clone の実装を含む(新しく書く)
  - 描画結果をビットマップで確かめる(`LayerBitmapRenderer`、新しく書く)
- 見出し(案):
  - 規約
  - 手順
  - 規約が守られる場所
  - 関連
- ※要確認: なし
