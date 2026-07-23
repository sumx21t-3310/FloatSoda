# Changelog

このプロジェクトの特筆すべき変更はすべてこのファイルに記録されます。

フォーマットは [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に基づいており、
バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に準拠します。

## [Unreleased]

## [0.3.1] - 2026-07-24

### Added

- RenderObjects/Widgets具象クラスとCore/Animation/GeometricsにXMLドキュメントコメントを整備
- UI/Cream/FizzyPop/Testingの残りのXMLドキュメントコメントを整備

### Changed

- `RenderPadding` が未実装であることをドキュメントに明記

## [0.3.0] - 2026-07-23

### Added

- ポインタ入力とヒットテストを実装（GLFW / Dashboardオーバーレイのレーザー入力、`IVRInput` アクションマニフェスト対応）
- ジェスチャー層（`Listener` / `GestureDetector` / `GestureArena` / `PointerRouter` / `GestureRecognizer`、Tap・Pan認識器）を追加
- `AbsorbPointer` / `IgnorePointer` を追加
- 基盤層（Abstractions / Widget / Element / RenderObject / Rendering / Engine）とジェスチャーAPIにXMLドキュメントコメントを整備

## [0.2.0] - 2026-07-16

### Changed

- Generic Hostベースの起動モデルへ移行（`Host.CreateApplicationBuilder` + `AddFloatSoda()` でDIコンテナから `FloatSodaApp` を解決。`FloatSodaAppExtensions` は廃止）
- ランタイム層を分割し、`FloatSoda.Common` を `FloatSoda.Abstractions` に再編

### Removed

- 未使用の `NullableExtension` を削除

## [0.1.0] - 2026-07-14 — 宣言的UIコア完成・初回正式リリース

### Added

- 宣言的UIコアの完成（`StatelessWidget` / `StatefulWidget` + `SetState()` / `InheritedWidget`、`BuildOwner` による差分ビルド、RenderObjectツリーの差分レイアウト・差分ペイント）
- アニメーションシステム（`AnimationController` / `Ticker` / `FadeTransition` / `Curves`）
- 3種のオーバーレイの統一管理（`DashboardWindow` / `WorldSpaceWindow` / `DeviceTrackedWindow`）
- SteamVRへのプロセス登録（`OVRApplication.Identify()`）

### Changed

- 公開NuGetパッケージをコア4つ（`FloatSoda` / `FloatSoda.Common` / `FloatSoda.Engine` / `FloatSoda.OVR`）に整理。`FloatSoda.UI` 系（デザインシステム）と `FloatSoda.Hooks`（実験的）は完成まで公開を停止

### Fixed

- **重大**: レンダーツリーで子を動的に削除すると無限再帰（StackOverflow）していた `CleanChildRelayoutBoundary` の自己再帰バグを修正
- SteamVRにAppKey未登録でも起動できるように修正

### Removed (Breaking)

- `WithOpenVRFrameLimiter()` を削除。`WaitGetPoses` はシーンアプリ専用APIのためオーバーレイアプリでは必ずクラッシュしていた。フレームレート制御には `WithTargetFrameRate()`（既定30fps）を使用

## [0.0.3] - 2026-07-13

### Fixed

- `openvr_api.dll` が正しく配置されるように `FloatSoda.OVR.csproj` を変更

## [0.0.2] - 2026-07-10

### Added

- Flutter ライクな宣言的 UI システム（`StatelessWidget` / `Element` / `BuildOwner` による差分ビルド）
- RenderObject ツリーによる差分レイアウト・差分ペイント（`RenderPipeline`）
- Skia 描画コマンドを保持する Layer ツリー（`ContainerLayer` / `PictureLayer` / クリップ・変換レイヤー）
- SkiaSharp → OpenGL (GLFW/OpenTK) → OpenVR オーバーレイテクスチャのレンダリング経路
- 複数オーバーレイの統一管理（ダッシュボード・ワールド座標固定・デバイス追従）
- メインスレッドとレンダースレッドのレイヤークローンによる分離
- NuGetパッケージのメタデータ整備・リリース自動化(Directory.Build.props / CHANGELOG / Trusted Publishingによるリリースワークフロー)

[Unreleased]: https://github.com/sumx21t-3310/FloatSoda/compare/v0.3.0...main
[0.3.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.3.0
[0.2.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.2.0
[0.1.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.1.0
[0.0.3]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.3
[0.0.2]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.2
