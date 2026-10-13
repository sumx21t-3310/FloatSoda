# UI: 仕組みと理由

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation(WritingDocumentation 5 章の Architecture と Design)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / UIの層(`FloatSoda.UI`、`FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop`)を触ろうとしている / いまどう動き、なぜそう設計したかを理解したい
- テーマ: UI の層がどう分かれ、なぜ3層にしたか
- Why: いまどう動き、なぜそう設計したかを知らないと、変更の影響を見積もれないため
- What: 3層の役割と、3層に分けた理由
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - 3層の役割(UILayering)
  - 3層に分けた理由(UILayering の「Material ロックイン」)
- 見出し(案):
  - 実装状況
  - 責務と境界
  - 関係するコンポーネント
  - 処理の流れ
  - 所有権・ライフサイクル
  - なぜそう設計したか
  - Flutter との対応
  - テストでの駆動
  - 関連
- ※要確認: なし
