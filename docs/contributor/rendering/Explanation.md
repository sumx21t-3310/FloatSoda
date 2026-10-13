# 描画: 仕組みと理由

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation(WritingDocumentation 5 章の Architecture と Design)
- 読み手: FloatSoda 本体を変えるコントリビュータと Coding Agent / 描画の層(`FloatSoda.Rendering`)を触ろうとしている / いまどう動き、なぜそう設計したかを理解したい
- テーマ: 描画層がどう動き、なぜその設計にしたか
- Why: いまどう動き、なぜそう設計したかを知らないと、変更の影響を見積もれないため
- What: Layer ツリー、PaintingContext との境目、Clone して渡す理由
- How: 今のページの節を移し、足りない節を新しく書く
- 要素:
  - Layer ツリー(Architecture の Layer の部分)
  - PaintingContext と Layer ツリー(RenderObjects の「PaintingContext とレイヤーツリー」)
  - Clone してからレンダースレッドへ渡す理由(新しく書く)
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
