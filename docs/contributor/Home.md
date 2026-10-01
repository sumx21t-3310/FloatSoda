# FloatSoda を変える

FloatSoda 本体を実装・変更・レビューする人向けのページです。現行のドキュメントはすべてここにあります。利用者向けの部分は、[FloatSoda を使う](../user/Home.md) へ書き直していきます([Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) 手順4)。

## Architecture — いまどう動いているか

- [Architecture](Architecture.md) — アセンブリ構成・ツリー構造・スレッドモデル
- [BuildPipeline](BuildPipeline.md) — BuildOwner による差分ビルドとフレームパイプライン
- [RenderObjects](RenderObjects.md) — RenderObject ツリー(レイアウト・描画)
- [WidgetSystem](WidgetSystem.md) — Widget / Element システムと組み込みウィジェット一覧
- [Animation](Animation.md) — AnimationController・Ticker・Curves
- [OVRIntegration](OVRIntegration.md) — OpenVR ラッパー・オーバーレイ種別・イベント処理
- [Input](Input.md) — アクション入力(コントローラーのボタン・トリガー・スティック)
- [GettingStarted](GettingStarted.md) — 環境構築・サンプル実行・最初のアプリ作成。利用者向けの版は [user/GettingStarted](../user/GettingStarted.md) に書き直す

## Design — なぜそう設計したか

- [UILayering](UILayering.md) — UI 層の3層パッケージ構成。**設計方針であり未提供**
- [APIDesign](APIDesign.md) — API 設計規約と Flutter parity の判断基準
- [Localization](Localization.md) — ローカライゼーション方針(日本語デフォルト)

## Requirements — 変更後も何を維持するか

- [Requirements](Requirements.md) — 骨組み。現行ページの契約の部分を分けて書く
- ツリーの不変条件は [REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」

## Development — どう変更・レビュー・公開するか

- [DocumentationComments](DocumentationComments.md) — ドキュメントコメント規約
- [WritingDocumentation](WritingDocumentation.md) — ドキュメント執筆ガイド
- [TestStrategy](TestStrategy.md) — テスト戦略

## 規約の置き場所

| 知りたいこと | 置き場所 |
|---|---|
| ブランチ名、名前空間、PR の流れ、テストの観点 | [CONTRIBUTING.md](../../CONTRIBUTING.md) |
| コードレビューの基準、ツリーの不変条件 | [REVIEW.md](../../REVIEW.md) |
| リリース手順 | [RELEASING.md](../../RELEASING.md) |
