# フレームワーク(三つのツリー): 貢献までの流れ

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial(WritingDocumentation 5 章の「貢献までの流れ」)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / フレームワーク(三つのツリー)の層(`FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture`。`FloatSoda.Hooks`、`FloatSoda.Testing`)を触ろうとしている / その層で最初の PR を出したい
- テーマ: Flutter のウィジェットを1つ移植して PR を出すまで
- Why: その層で最初の PR を出すまでの道筋がないと、どこから手を付けてよいか分からないため
- What: Flutter 本家を読み、Widget と RenderObject とテストと層1サンプルを足して PR を出す
- How: 一本道の手順で書き、規約は CONTRIBUTING.md の節へリンクする
- 要素:
  - 題材: Flutter のウィジェットを1つ移植する
  - Flutter 本家の実装を読む(flutter-widget-source スキル)
  - Widget と RenderObject を書く
  - テストを書く(CONTRIBUTING.md の「テスト観点」へリンク)
  - 層1サンプルを足す(CONTRIBUTING.md の「サンプルを追加する場合の規約」へリンク)
  - PR を出す(CONTRIBUTING.md へリンク)
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 …
  - PR を出す
  - 次に読むもの
- ※要確認:
  - 題材に向くウィジェット
