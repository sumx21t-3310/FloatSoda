# フレームワーク(コア): 貢献までの流れ

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial(WritingDocumentation 5 章の「貢献までの流れ」)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(コア)の層(`FloatSoda` の `FloatSodaApp` と Generic Host まわり、`Core`、`Geometrics`、`Painting`)を触ろうとしている / その層で最初の PR を出したい
- テーマ: ジオメトリの値型を1つ足して PR を出すまで
- Why: その層で最初の PR を出すまでの道筋がないと、どこから手を付けてよいか分からないため
- What: 値型を1つ足し、テストを書いて PR を出す
- How: 一本道の手順で書き、規約は CONTRIBUTING.md の節へリンクする
- 要素:
  - 題材: ジオメトリの値型を1つ足す
  - `record struct` で書く(APIDesign の 2 章)
  - テストを書く(CONTRIBUTING.md の「テスト方針」へリンク)
  - PR を出す(CONTRIBUTING.md へリンク)
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 …
  - PR を出す
  - 次に読むもの
- ※要確認:
  - 題材に向く値型
