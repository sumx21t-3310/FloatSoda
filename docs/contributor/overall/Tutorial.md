# 全体: 貢献までの流れ

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial(WritingDocumentation 5 章の「貢献までの流れ」)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / 全体の層(層をまたぐ話(API 設計、ローカライズ、docs、テスト方針、リリース、入力の流れ))を触ろうとしている / その層で最初の PR を出したい
- テーマ: docs の小さな修正で、1つの PR を出し切るまで
- Why: その層で最初の PR を出すまでの道筋がないと、どこから手を付けてよいか分からないため
- What: Issue → ブランチ → docs の修正 → サイトのビルドで確かめる → PR → レビュー → マージ
- How: 一本道の手順で書き、規約は CONTRIBUTING.md の節へリンクする
- 要素:
  - 題材: docs の小さな修正(コードを触らない)
  - Issue を選ぶ・書く(CONTRIBUTING.md の「Issue とラベル」へリンク)
  - ブランチを切る(CONTRIBUTING.md の「ブランチ名」へリンク)
  - サイトのビルドと検査(`website/` の `npm run build` と `npm run verify`)
  - PR を出し、レビューを受ける(CONTRIBUTING.md の「ブランチ・PRフロー」、REVIEW.md へリンク)
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 …
  - PR を出す
  - 次に読むもの
- ※要確認: なし
