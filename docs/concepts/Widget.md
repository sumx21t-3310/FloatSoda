# Widget — 宣言的 UI と Widget ツリー

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation
- 読み手: FloatSoda でオーバーレイを作る人と、本体を変える人の両方 / C# と、Unity か Flutter のどちらかを知っている / Widget とは何で、どんな種類があり、どう組み合わせて画面を作るかを理解したい
- テーマ: Widget とは何で、どんな種類があり、どう組み合わせて画面を作るか
- Why: Widget を組み合わせる考え方が分からないと、LLM が書いたコードの意味も直し方も分からないため
- What: Widget は不変の設計図であること、StatelessWidget と StatefulWidget の違い、組み込み Widget の種類、色の指定
- How: Concept テンプレートに沿い、最小例は Widget カタログへリンクする
- 要素:
  - Widget は不変の `record` で、作り直しても安い(WidgetSystem)
  - `StatelessWidget` と `StatefulWidget` の違い(WidgetSystem)
  - `Key` と、作り直しても同じ部品が使い続けられる条件(WidgetSystem)
  - 組み込み Widget の種類の地図(レイアウト・描画・入力)。一覧は Widget カタログと API Reference へリンクする(WidgetSystem の組み込みウィジェット一覧)
  - 色の節: FloatSoda の `Color` と SkiaSharp の `SKColor` の関係と暗黙の変換(#251)
  - 制約: スクロール系の Widget は公開 API として使えない(WidgetSystem)
- 見出し(案):
  - 概要
  - いつ使うか
  - 基本モデル
  - 組み込み Widget の種類
  - 色
  - 最小例
  - Unity / uGUI からの読み替え
  - 制約と未実装
  - 関連ページ
- ※要確認: スクロール系の制約を追跡する Issue 番号
