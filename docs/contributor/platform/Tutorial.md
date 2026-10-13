# プラットフォーム: 貢献までの流れ

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial(WritingDocumentation 5 章の「貢献までの流れ」)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / プラットフォームの層(`FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR`)を触ろうとしている / その層で最初の PR を出したい
- テーマ: OpenVR の API を1つ包んで PR を出すまで
- Why: その層で最初の PR を出すまでの道筋がないと、どこから手を付けてよいか分からないため
- What: 生バインディングの API を1つ選び、ラッパーとテストを足して PR を出す
- How: 一本道の手順で書き、規約は CONTRIBUTING.md の節へリンクする
- 要素:
  - 題材: OpenVR の API を1つ包む
  - API を調べる(openvr-api-reference スキル)
  - ラッパーを書く(OVRIntegration)
  - 実機で確かめる(floatsoda-device-test-run スキル)
  - PR を出す(CONTRIBUTING.md へリンク)
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 …
  - PR を出す
  - 次に読むもの
- ※要確認:
  - 題材に向く小さな API
