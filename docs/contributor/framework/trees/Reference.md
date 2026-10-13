# フレームワーク(三つのツリー): 守ること

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / reference(WritingDocumentation 5 章の Requirements)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(三つのツリー)の層(`FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture`。`FloatSoda.Hooks`、`FloatSoda.Testing`)を触ろうとしている / 変更の前後で何を守るかを調べたい
- テーマ: 三つのツリーで維持する条件
- Why: 変更後も維持する条件を知らないと、変更が何を壊すかを判断できないため
- What: 所有権とライフサイクル、差分更新、RenderObject の基本契約
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - REVIEW.md の「ツリーの所有権とライフサイクル」「差分更新」へリンクする
  - RenderObject の基本契約(RenderObjects の「基本契約」)
  - 組み込み RenderObject の一覧は書かず、コードへリンクする
- 見出し(案):
  - 適用範囲
  - 必須条件
  - 不変条件
  - Observable behavior
  - 破ったときの failure mode
  - 検証方法
  - Flutter との関係
  - 関連
- ※要確認: なし
