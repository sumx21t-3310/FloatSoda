# 全体: 守ること

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / reference(WritingDocumentation 5 章の Requirements)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / 全体の層(層をまたぐ話(API 設計、ローカライズ、docs、テスト方針、リリース、入力の流れ))を触ろうとしている / 変更の前後で何を守るかを調べたい
- テーマ: 層をまたいで維持する条件
- Why: 変更後も維持する条件を知らないと、変更が何を壊すかを判断できないため
- What: ツリーの不変条件、API 設計の規約、Flutter との差異の台帳
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - ツリーの不変条件は REVIEW.md の「4. FloatSoda 固有の不変条件」へリンクする(Requirements)
  - コンポーネント API の規約(APIDesign の 2 章)
  - 確認済みの Flutter との差異(known-divergences.md)
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
