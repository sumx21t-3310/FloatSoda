# Changelog

このプロジェクトの特筆すべき変更はすべてこのファイルに記録されます。

フォーマットは [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に基づいており、
バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に準拠します。

## [Unreleased]

## [0.4.0] - 2026-09-22

### Added

- レイアウトウィジェットを追加: `Padding` / `Container`(`Padding` の合成を含む)/ `Stack` / `Positioned` / `IndexedStack` / `Wrap` / `Expanded` / `Flexible` / `Spacer` / `AspectRatio` / `FittedBox` / `LimitedBox` / `FractionallySizedBox` / `OverflowBox` / `SizedOverflowBox` / `ConstraintsTransformBox` / `UnconstrainedBox` / `IntrinsicWidth` / `IntrinsicHeight`
- 描画・表示制御ウィジェットを追加: `DecoratedBox` / `Opacity` / `Transform` / `RotatedBox` / `Visibility` / `Offstage` / `RepaintBoundary`
- `Icon` を追加(名前空間 `FloatSoda.Widgets.Paint`)。`IconData` と `FontProvider` で指定したアイコンフォントのグリフを表示する
- `FontProvider`(`SystemFontProvider` / `FileFontProvider`)を追加。`TextStyle.Font` と `IconData` でフォントを指定できる。読み込んだフォントはアプリケーションの終了まで保持される
- `Image` に `Fit`(既定 `BoxFit.Contain`)と `Alignment` を追加
- `Image` に `LoadingBuilder` / `ErrorBuilder` を追加。読み込み中や読み込み失敗時に、画像の代わりに表示するウィジェットを指定できる。`LoadingBuilder` は読み込みの進み具合(`ImageLoadingProgress?`)を受け取る(現在は常に `null`)
- `ImageProvider.ResolveAsync()` と `ImageHandle` を追加。等しい `ImageProvider`(record の値の等価性)から借りた画像は共有され、最後のハンドルを破棄した時点で解放される
- 画像の読み込みを非同期化。読み込みとデコードはスレッドプールで並行して進み、フレームを処理するスレッドを止めない
- `TaskBuilder<T>` を追加。`Task<T>` の進行状況(読み込み中 / 完了 / 失敗)に応じてビルド結果を切り替える
- ビルド補助ウィジェットを追加: `Builder` / `KeyedSubtree` / `ListenableBuilder`
- `PointerRegion` を追加。ポインタが領域へ入ったことや出たことを `OnPointerEnter` / `OnPointerExit` で受け取れる(ホバー入力)
- `TextStyle` を追加(`FontSize` / `Color` / `Font` / `FontWeight` / `IsItalic`)。各プロパティでは `null` が「未指定(継承対象)」を表し、既定値(30 / 黒 / Arial / 400 / 非斜体)は描画時に適用される
- `Text` に `Style`(`TextStyle`)を追加
- `DefaultTextStyle` を追加。配下の `Text` が明示しなかった書式プロパティを祖先から継承する(明示指定 > `DefaultTextStyle` > 既定値)。`Style` を変更すると、`InheritedWidget` の依存追跡により配下の `Text` が再ビルドされる
- `TextStyle.Merge` と `TextStyle.Inherit` を追加(Flutter の `TextStyle.merge` / `inherit` 相当)
- 自作ウィジェット向けの基底型を追加: `LeafRenderObjectWidget<T>`(子を持たない RenderObject 用)と `ParentDataWidget<T>`(親の RenderObject へ配置情報を渡す用)
- RenderObject に intrinsic サイズの測定を追加(`IntrinsicWidth` / `IntrinsicHeight` の基盤)
- ドキュメントサイト <https://floatsoda.sumx21t.com> を公開。LLM 向けに `llms.txt` / `llms-full.txt` を配信する
- Widget ごとのカタログ型サンプル(`samples/FloatSoda.Samples.*`)と `GettingStarted` サンプルを追加

### Changed (Breaking)

- `Image` を刷新。基底型を `StatefulWidget<Image>` に変更し、`ImageProvider` プロパティを `Provider` へ改名。`Child` を削除(読み込み中や失敗時の表示は `LoadingBuilder` / `ErrorBuilder` へ、画像の上への重ね描きは `Stack` へ置き換える)。画像の収め方の既定を、領域いっぱいへの引き伸ばしから `BoxFit.Contain` へ変更(従来の表示が必要なら `Fit = BoxFit.Fill` を指定する)
- `ImageProvider` / `FileImageProvider` の名前空間を `FloatSoda.Core` から `FloatSoda.Core.Providers` へ移動。公開されていた同期の `Load()` を削除し、派生型が実装する `protected abstract ValueTask<SKImage> LoadAsync(CancellationToken)` へ変更。呼び出し側は `ResolveAsync()` で `ImageHandle` を借り、`Dispose()` で返す
- `RenderImage` の名前空間を `FloatSoda.RenderObjects` から `FloatSoda.RenderObjects.Painting` へ移動。子を持たない RenderObject に変更し(基底型は `RenderBox`)、`Child` を削除
- `Text` と `RichText` の名前空間を `FloatSoda.Widgets.Components` から `FloatSoda.Widgets` へ移動。`using` の書き換えが必要
- `TapGestureRecognizer` と `PanGestureRecognizer` の名前空間を `FloatSoda.Gesture` から `FloatSoda.Gesture.Recognizers` へ移動
- `RenderPadding` の名前空間を `FloatSoda.Widgets.Layout` から `FloatSoda.RenderObjects.Layout` へ移動し、`Spacing` プロパティを `Padding` へ改名。`RenderSiftedBox` を削除
- `FloatSoda.Engine` のスレッド実行基盤を再編。`IThreadRunner` → `ITaskRunner`、`ThreadRunner` → `PostTaskRunner`、`RenderThreadRunner` → `RenderPostTaskRunner` へ改名し、投入順に1件ずつ処理する `IOTaskRunner` を追加
- `IHasMultiChildrenRenderObject` に `InsertChild(child, after)` と `MoveChild(child, after)` を追加。複数の子を持つ RenderObject を自作している場合は、両メソッドの実装が必要(`MultiChildrenCollection<T>.Insert` / `Move` へ委譲すればよい)
- `TextSpan.Style` の型を、Topten.RichTextKit の `Style` から FloatSoda の `TextStyle` へ変更
- `TextSpan.Build(TextBlock, Style)` を削除。`TextSpan` の描画は `TextPainter` / `RenderParagraph` が担う
- `ConstrainedBox.Constraints` を削除し、必須の `AdditionalConstraints` へ置き換え(Flutter の `ConstrainedBox.constraints` と同じく、親の制約に追加で課す制約を指定する)。`Constraints = …` は `AdditionalConstraints = …` へ書き換える
- `PointerEvent.Transform` と `HitTestEntry.Transform` の型を `Offset?` から `Matrix3x2?` へ、`HitTestResult.LastTransform` の型を `Offset` から `Matrix3x2` へ変更。`FittedBox` や `Transform` の拡縮と回転を、ヒットテストの座標変換に反映するため。平行移動だけを渡していたコードは `Matrix3x2.CreateTranslation(x, y)` へ書き換える
- 非ジェネリックの `LeafRenderObjectElement` を削除し、`LeafRenderObjectElement<T>` へ置き換え。子を持たない RenderObject のウィジェットは、`LeafRenderObjectWidget<T>` を継承すれば Element を自作せずに済む

### Fixed

- `EdgeInsets.ToString()` が無限再帰し、スタックオーバーフローでプロセスごと異常終了する問題を修正。`$"{padding}"` のような文字列補間やログ出力でも起きていた。`Curve`(`Curves.Linear` など)の `ToString()` が `InsufficientExecutionStackException` を投げる問題も同じ原因で、あわせて修正(#279)
- `Row` / `Column` などの複数の子を持つウィジェットで、子の RenderObject の順番が Widget の並びと食い違う問題を修正。途中の子の RenderObject が別の型へ差し替わったときや、子リストの途中へ Widget を追加したとき、`Key` つきの子を並べ替えたときに、対象の RenderObject が末尾へ入ったり元の位置に残ったりして、並び順が崩れていた(#276)

### Removed

- `FloatSoda` と `FloatSoda.Engine` から未使用だった `Polly` への依存を削除

### Known issues

- `Size` を指定していないウィンドウは、起動後に中身の大きさが変わっても、SteamVR 上の表示が最初の大きさのまま更新されない。中身の大きさが変わるウィンドウには `Size` を指定する(#285)

## [0.3.1] - 2026-07-24

### Added

- RenderObjects/Widgets具象クラスとCore/Animation/GeometricsにXMLドキュメントコメントを整備
- UI/Cream/FizzyPop/Testingの残りのXMLドキュメントコメントを整備

### Changed

- `RenderPadding` が未実装であることをドキュメントに明記

## [0.3.0] - 2026-07-23

> **NuGet には未公開(欠番)。** タグ `v0.3.0` を打ったコミットでは `Directory.Build.props` の `<Version>` が 0.2.0 のままだったため、Release ワークフローがバージョンの照合で停止した。この節の変更は、すべて 0.3.1 に含まれる。0.3.1 以降を使うこと

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

[Unreleased]: https://github.com/sumx21t-3310/FloatSoda/compare/v0.4.0...main
[0.4.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.4.0
[0.3.1]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.3.1
[0.3.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.3.0
[0.2.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.2.0
[0.1.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.1.0
[0.0.3]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.3
[0.0.2]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.2
