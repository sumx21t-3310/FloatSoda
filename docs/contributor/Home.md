# FloatSoda を変える

FloatSoda 本体を実装・変更・レビューする人向けのページです。ページは、貢献までの流れ・作業の手順・守ること・仕組みと理由の4種類に分かれています。どの種類も、書くことがあるトピックだけをページにしています。再編の途中なので、まだ新しいページに移していない内容は、下の「今のページ」にあります(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。

## 読む順番

1. 面白がる — [実験](experiments/Home.md)で、FloatSoda の動作原理を動かして確かめる
2. 貢献する — [docs の小さな修正で PR を出す](tutorials/FirstPullRequest.md)で、最初の PR を出す
3. 調べる — 触る部分の「仕組みと理由」と「守ること」を読む
4. 提案する — Issue や ADR で、設計の変更を出す

## 種類ごとのページ

**貢献までの流れ** — 題材を1つ決め、PR を出すまでを一本道でたどります。

- [docs の小さな修正で PR を出す](tutorials/FirstPullRequest.md)
- [Layer を1つ足して PR を出す](tutorials/ShaderMaskLayer.md)
- [OpenVR の API を1つ包んで PR を出す](tutorials/OverlayPreCurvePitch.md)

**作業の手順** — 特定の作業を終えるための手順です。共通の規約への案内は、入口の [作業の手順](how-to/Home.md) にあります。

- [OpenVR の API を包む](how-to/WrapOpenVRApi.md)
- [Layer を足す](how-to/AddLayer.md)
- [ジオメトリの値型を足す](how-to/AddValueType.md)
- [Flutter のウィジェットを移植する](how-to/PortWidget.md)
- [アニメーションの部品を移植する](how-to/PortAnimation.md)

**守ること** — 実装を変えても維持する条件です。共通の規約への案内は、入口の [守ること](reference/Home.md) にあります。

- [プラットフォームで守ること](reference/Platform.md)
- [描画で守ること](reference/Rendering.md)
- [コアで守ること](reference/Core.md)
- [三つのツリーで守ること](reference/Trees.md)
- [アニメーションで守ること](reference/Animation.md)

**仕組みと理由** — いまどう動くか、なぜそう設計したかです。

- [全体の仕組みと理由](explanation/Overall.md)
- [プラットフォームの仕組みと理由](explanation/Platform.md)
- [描画の仕組みと理由](explanation/Rendering.md)
- [コアの仕組みと理由](explanation/Core.md)
- [三つのツリーの仕組みと理由](explanation/Trees.md)
- [アニメーションの仕組みと理由](explanation/Animation.md)

## 今のページ

再編で、上の種類ごとのページへ移します。行き先は [WritingDocumentation の再編の地図](WritingDocumentation.md#再編の地図) にあります。

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
