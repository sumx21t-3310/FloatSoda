# Changelog

このプロジェクトの特筆すべき変更はすべてこのファイルに記録されます。

フォーマットは [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に基づいており、
バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に準拠します。

## [Unreleased]

## [0.0.2] - 2026-07-10

### Added

- Flutter ライクな宣言的 UI システム（`StatelessWidget` / `Element` / `BuildOwner` による差分ビルド）
- RenderObject ツリーによる差分レイアウト・差分ペイント（`RenderPipeline`）
- Skia 描画コマンドを保持する Layer ツリー（`ContainerLayer` / `PictureLayer` / クリップ・変換レイヤー）
- SkiaSharp → OpenGL (GLFW/OpenTK) → OpenVR オーバーレイテクスチャのレンダリング経路
- 複数オーバーレイの統一管理（ダッシュボード・ワールド座標固定・デバイス追従）
- メインスレッドとレンダースレッドのレイヤークローンによる分離
- NuGetパッケージのメタデータ整備・リリース自動化(Directory.Build.props / CHANGELOG / Trusted Publishingによるリリースワークフロー)

[Unreleased]: https://github.com/sumx21t-3310/FloatSoda/compare/v0.0.2...main
[0.0.2]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.2


## [0.0.3] - 2026-07-13
- openvr_api.dllが正しく配置されるようにFloatSoda.OVR.csprojを変更