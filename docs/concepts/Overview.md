# FloatSoda の考え方

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / explanation
- 読み手: FloatSoda でオーバーレイを作る人と、本体を変える人の両方 / C# と、Unity か Flutter のどちらかを知っている / FloatSoda が UI をどう組み立て、オーバーレイに表示するかの全体像を理解したい
- テーマ: FloatSoda が UI をどう組み立て、オーバーレイに表示するかの全体像
- Why: 全体像がないと、各トピックのページがどうつながるかが分からないため
- What: 宣言的 UI、三つのツリーを利用者から見た姿、オーバーレイに表示されるまでの流れ、各トピックへの地図
- How: 知っている話(uGUI、Flutter)から、知らない話(三つのツリー)へ順に説明する
- 要素:
  - UI は C# のコードだけで書き、シーンやプレハブを持たない(TargetUsers の3つ目のペルソナ)
  - Widget は UI の設計図で、変更のたびに作り直す(Home の「全体像: 三つのツリー」)
  - 三つのツリーを利用者から見た姿(Home の「全体像: 三つのツリー」、Architecture の「ツリー構造」)
  - コードの変更がオーバーレイに表示されるまでの流れのうち、利用者から見える部分(Architecture の「レンダリングライフサイクル」)
  - uGUI の GameObject・Canvas との対応(新しく書く)
  - 各トピック(Widget、State、Layout、Input、Animation、Window、Threading)への地図
- 見出し(案):
  - 宣言的 UI とは
  - Widget は設計図
  - 三つのツリー
  - オーバーレイに表示されるまで
  - uGUI からの読み替え
  - 次に読むページ
- ※要確認: なし
