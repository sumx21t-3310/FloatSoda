# プラットフォーム: 守ること

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / reference(WritingDocumentation 5 章の Requirements)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / プラットフォームの層(`FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR`)を触ろうとしている / 変更の前後で何を守るかを調べたい
- テーマ: プラットフォーム層で維持する条件
- Why: 変更後も維持する条件を知らないと、変更が何を壊すかを判断できないため
- What: `internal` の契約。`public` の契約は API Reference に置く
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - OVRIntegration の「例外体系」「オーバーレイプロパティ」のうち `internal` の契約
- 見出し(案):
  - 適用範囲
  - 必須条件
  - 不変条件
  - Observable behavior
  - 破ったときの failure mode
  - 検証方法
  - Flutter との関係
  - 関連
- ※要確認:
  - `internal` の契約として書ける条件があるか
