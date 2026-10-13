# フレームワーク(アニメーション): 貢献までの流れ

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial(WritingDocumentation 5 章の「貢献までの流れ」)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(アニメーション)の層(`FloatSoda` の `Animation`)を触ろうとしている / その層で最初の PR を出したい
- テーマ: Flutter の `Curve` を1つ移植して PR を出すまで
- Why: その層で最初の PR を出すまでの道筋がないと、どこから手を付けてよいか分からないため
- What: Flutter 本家の `Curve` を1つ移植し、テストを書いて PR を出す
- How: 一本道の手順で書き、規約は CONTRIBUTING.md の節へリンクする
- 要素:
  - 題材: Flutter の `Curve` を1つ移植する
  - Flutter 本家の実装を読む(flutter-widget-source スキル)
  - テストを書く(CONTRIBUTING.md へリンク)
  - PR を出す(CONTRIBUTING.md へリンク)
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 …
  - PR を出す
  - 次に読むもの
- ※要確認:
  - まだ移植していない `Curve`
