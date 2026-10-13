# Layout — 制約・サイズ・配置

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation
- 読み手: FloatSoda でオーバーレイを作る人と、本体を変える人の両方 / C# と、Unity か Flutter のどちらかを知っている / 親子の間で大きさと位置がどう決まるかを理解したい
- テーマ: 親子の間で大きさと位置がどう決まるか
- Why: 制約の流れを知らないと、はみ出しや、思った大きさにならない原因が分からないため
- What: 制約は親から子へ下り、サイズは子から親へ上がること。Flex、Stack、制約を変える Widget
- How: Concept テンプレートに沿い、例は層1サンプル(レイアウト基本・制約変換系)へリンクする
- 要素:
  - 制約は親から子へ下り、サイズは子から親へ上がる(RenderObjects の「制約フロー」)
  - `Row` / `Column` と `Expanded` は余った幅を flex の比で分ける(WidgetSystem のレイアウト)
  - `Stack` と `Positioned` で重ねる(WidgetSystem のレイアウト)
  - 制約を変える Widget(`ConstrainedBox`、`FittedBox`、`OverflowBox` など)(WidgetSystem のレイアウト)
  - intrinsic 測定はコストが高い(RenderObjects の「intrinsic 測定」)
- 見出し(案):
  - 概要
  - いつ使うか
  - 基本モデル
  - 最小例
  - Unity / uGUI からの読み替え
  - 制約と未実装
  - 関連ページ
- ※要確認: なし
