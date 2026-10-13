# FloatSoda を変える

FloatSoda 本体を実装・変更・レビューする人向けのページです。層ごとに、貢献までの流れ・作業の手順・守ること・仕組みと理由の4種類のページがあります。新しいページは設計シートだけがあり、本文はまだありません(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。本文が書けるまでは、下の「今のページ」を読んでください。

## 読む順番

1. 面白がる — [実験](experiments/Home.md)で、FloatSoda の動作原理を動かして確かめる
2. 貢献する — [全体: 貢献までの流れ](overall/Tutorial.md)で、最初の PR を出す
3. 調べる — 触る層の「仕組みと理由」と「守ること」を読む
4. 提案する — Issue や ADR で、設計の変更を出す

## 層ごとのページ

| 層 | 貢献までの流れ | 作業の手順 | 守ること | 仕組みと理由 |
|---|---|---|---|---|
| 全体 | [開く](overall/Tutorial.md) | [開く](overall/HowTo.md) | [開く](overall/Reference.md) | [開く](overall/Explanation.md) |
| プラットフォーム | [開く](platform/Tutorial.md) | [開く](platform/HowTo.md) | [開く](platform/Reference.md) | [開く](platform/Explanation.md) |
| 描画 | [開く](rendering/Tutorial.md) | [開く](rendering/HowTo.md) | [開く](rendering/Reference.md) | [開く](rendering/Explanation.md) |
| フレームワーク(コア) | [開く](framework/core/Tutorial.md) | [開く](framework/core/HowTo.md) | [開く](framework/core/Reference.md) | [開く](framework/core/Explanation.md) |
| フレームワーク(三つのツリー) | [開く](framework/trees/Tutorial.md) | [開く](framework/trees/HowTo.md) | [開く](framework/trees/Reference.md) | [開く](framework/trees/Explanation.md) |
| フレームワーク(アニメーション) | [開く](framework/animation/Tutorial.md) | [開く](framework/animation/HowTo.md) | [開く](framework/animation/Reference.md) | [開く](framework/animation/Explanation.md) |
| UI | [開く](ui/Tutorial.md) | [開く](ui/HowTo.md) | [開く](ui/Reference.md) | [開く](ui/Explanation.md) |

## 今のページ

再編で、上の層のページへ移します。行き先は [WritingDocumentation の再編の地図](WritingDocumentation.md#再編の地図) にあります。

- [Architecture](Architecture.md) — アセンブリ構成・ツリー構造・スレッドモデル
- [BuildPipeline](BuildPipeline.md) — BuildOwner による差分ビルドとフレームパイプライン
- [RenderObjects](RenderObjects.md) — RenderObject ツリー(レイアウト・描画)
- [WidgetSystem](WidgetSystem.md) — Widget / Element システムと組み込みウィジェット一覧
- [Animation](Animation.md) — AnimationController・Ticker・Curves
- [OVRIntegration](OVRIntegration.md) — OpenVR ラッパー・オーバーレイ種別・イベント処理
- [Input](Input.md) — アクション入力(コントローラーのボタン・トリガー・スティック)
- [GettingStarted](GettingStarted.md) — 環境構築・サンプル実行・最初のアプリ作成
- [UILayering](UILayering.md) — UI 層の3層パッケージ構成。**設計方針であり未提供**
- [APIDesign](APIDesign.md) — API 設計規約と Flutter parity の判断基準
- [Localization](Localization.md) — ローカライゼーション方針(日本語デフォルト)
- [Requirements](Requirements.md) — 骨組み。ツリーの不変条件は [REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」
- [DocumentationComments](DocumentationComments.md) — ドキュメントコメント規約
- [WritingDocumentation](WritingDocumentation.md) — ドキュメント執筆ガイド
- [TestStrategy](TestStrategy.md) — テスト戦略

## 規約の置き場所

| 知りたいこと | 置き場所 |
|---|---|
| ブランチ名、名前空間、PR の流れ、テストの観点 | [CONTRIBUTING.md](../../CONTRIBUTING.md) |
| コードレビューの基準、ツリーの不変条件 | [REVIEW.md](../../REVIEW.md) |
| リリース手順 | [RELEASING.md](../../RELEASING.md) |
