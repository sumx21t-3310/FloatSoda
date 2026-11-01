# Changelog

このプロジェクトの特筆すべき変更はすべてこのファイルに記録されます。

フォーマットは [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/) に基づいており、
バージョニングは [Semantic Versioning](https://semver.org/lang/ja/) に準拠します。

## [Unreleased]

## [0.5.0] - 2026-10-30

### Added

- `Color.FromArgb(uint)` / `Color.FromArgb(a, r, g, b)` / `Color.FromRgbo(r, g, b, opacity)` を追加。Flutter の `Color(int)` / `Color.fromARGB` / `Color.fromRGBO` に当たる。`FromArgb(uint)` は `0xAARRGGBB` の並び順で読む。`FromRgbo` は `opacity` が 0.0〜1.0 の範囲の外か `NaN` のとき `ArgumentOutOfRangeException` を投げる(#367)
- 名前付きの色 `Colors` を追加(名前空間 `FloatSoda.Geometrics`)。CSS の名前付きの色から Grey の綴りの別名7つと `RebeccaPurple` を除き、`Transparent` を足した 141 色で、名前は SkiaSharp の `SKColors` と同じ。値は CSS に揃えたため、`DarkSeaGreen`(`#8FBC8F`)と `Transparent`(透明な黒)は `SKColors` と値が違う(#367)
- `Color.Parse` が、透明度を含む8桁の `#RRGGBBAA` と4桁の `#RGBA` を読めるようにした。CSS と同じく透明度は末尾に置くため、`FromArgb(uint)` の `0xAARRGGBB` とは並び順が逆になる(#367)
- カタログ型サンプル `Icon` / `DefaultTextStyle` / `Visibility`(`Offstage`・`IndexedStack`・`RotatedBox` を含む)/ `Builder`(`KeyedSubtree`・`RepaintBoundary` を含む)/ `IntrinsicWidth`(`IntrinsicHeight` を含む)/ `ListenableBuilder` を追加。`GettingStarted` / `OverlayApp` / `PaintingSample` に README を足し、既存の6サンプルの README に位置づけと主な読者を書いた(#366)

### Changed (Breaking)

- `Flex` / `Row` / `Column` の既定の配置を、`MainAxisAlignment.Center` / `CrossAxisAlignment.Start` から、Flutter と同じ `MainAxisAlignment.Start` / `CrossAxisAlignment.Center` へ変更。従来の配置が必要なら、両プロパティを指定する(#359)
- `ClipRect` の `ClipBehavior` の既定値を、`Clip.Antialias` から、Flutter と同じ `Clip.HardEdge` へ変更(#359)
- `Image` / `RenderImage` の `Fit` の既定値を、`BoxFit.Contain` から、Flutter と同じ `BoxFit.ScaleDown` へ変更。領域より小さい画像は、原寸のまま表示される。従来どおり拡大するなら `Fit = BoxFit.Contain` を指定する(#359)
- `IgnorePointer` / `RenderIgnorePointer` の `Ignoring` の既定値を、`false` から、Flutter と同じ `true` へ変更。`Ignoring` を省略した `IgnorePointer` は、子への入力を無視する(#359)
- `ColoredBox.Color` を `required` に変更。`Color` を省略した `ColoredBox` はコンパイルエラーになるため、色を指定する(#359)
- `Align` / `RenderPositionedBox` の `WidthFactor` / `HeightFactor` に負数か NaN を渡すと、`ArgumentOutOfRangeException` を投げるようにした(#359)
- 親の再ビルドで、値が等しい別インスタンスの Widget を渡したときも、子の Element を更新するようにした。そのため `Builder` などの `Build` が呼び直される。更新を省くのは、Flutter と同じく同じインスタンスを渡したときだけになる(#362)
- `Color` のコンストラクターで、`Red` / `Green` / `Blue` を必須にした。`new Color(Red: 255)` のように成分を省いた呼び出しは、白ではなくコンパイルエラーになる。`Alpha` の既定値 255 は変わらない(#367)
- `HSVColor` / `HSLColor` を公開 API から外した(`internal` にした)。どちらも変換が `NotImplementedException` を投げるだけだった(#367)
- `RenderColoredBox.Color` と、`FloatSoda.UI.Cream.ButtonStyle` / `FloatSoda.UI.FizzyPop.ButtonStyle` の `BackgroundColor` / `PressedBackgroundColor` / `DisabledBackgroundColor` の型を、`SKColor` から `Color` に変えた。`SKColor` からの暗黙変換は残っているため、`SKColors.Red` の代入はそのまま通る(#367)
- `Container` の `Decoration` に `Border` があるとき、ボーダーの太さを `Padding` に加算するようにした。`Padding` を省略しても、子がボーダーの内側に収まる。Flutter の `Container` と同じ挙動で、`Border` 付きの `Container` では子のレイアウト領域がボーダーの太さぶん狭くなる(#267)

### Changed

- Flutter と同じく、`IntrinsicWidth.StepWidth` に 0 を指定できるようにした。0 は `null` と同じく、幅を丸めないことを表す(#359)

### Fixed

- `ClipBehavior` に `Clip.None` を指定した `ClipRect` / `ClipRoundRect` / `ClipOval` / `ClipCustomPath` が、子を切り抜いていた問題を修正。Flutter と同じく、切り抜かずに描画する(#359)
- `ClipRoundRect` の `BorderRadius` を変えても、角丸のクリップが描き直されない問題を修正(#359)
- `Image` を包んだ `GestureDetector` / `Listener` で、画像を押しても反応しない問題を修正。Flutter と同じく、画像の領域内なら常にヒットする(#359)
- 祖先と子孫が同じフレームで再ビルドを予約すると、子孫の `Build` が2回呼ばれる問題を修正(#362)
- `PostTaskRunner` を `Stop()` で正常に止めたときも、`ObjectDisposedException` のエラーログが残る問題を修正(#216)
- `PostTaskRunner` の `Stop()` で終了待ちがタイムアウトしたあと、`Start()` で再開すると、新しいスレッドの更新ループまで止まる問題を修正(#216)
- `VRSettingsException` / `VRNotificationException` / `VRScreenshotException` / `VRCompositorException` / `TrackedPropertyException` の `ThrowIfError` が、対応表にないエラー値で `ArgumentOutOfRangeException` を投げていた問題を修正。対応表にない値でも、各クラスの型付き例外を「予期しない〜エラー」のメッセージで投げる。`EVRSettingsError.AccessDenied` と `EVRNotificationError.ServiceUnavailable` には専用のメッセージを追加した(#295)
- 指やポインタを押している最中に `GestureDetector` をツリーから外すと、離したときに外したウィジェットの `OnTap` が呼ばれる問題を修正。入れ子の `GestureDetector` で内側だけを外したとき、外側のタップが失われる問題もあわせて修正。破棄した認識器がジェスチャアリーナに残り、後続のイベントも受け取り続けていた(#297)
- `Transform` で拡大や回転をかけた子の、レイアウト上の大きさからはみ出した見た目の部分を押しても反応しない問題を修正。子へ届く `PointerEvent.Transform` に拡大や回転が入らない問題もあわせて修正。`RotatedBox` の `PointerEvent.Transform` も回転を含むようにした(#298)
- `ClipOval` の楕円の外や、`Clipper` で切り落とした `ClipRect` / `ClipRoundRect` / `ClipCustomPath` の領域を押しても、中の子が反応する問題を修正。`Clipper` を指定しない `ClipRoundRect` の角は、Flutter と同じく引き続き反応する(#299)

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

[Unreleased]: https://github.com/sumx21t-3310/FloatSoda/compare/v0.5.0...main
[0.5.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.5.0
[0.4.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.4.0
[0.3.1]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.3.1
[0.3.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.3.0
[0.2.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.2.0
[0.1.0]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.1.0
[0.0.3]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.3
[0.0.2]: https://github.com/sumx21t-3310/FloatSoda/releases/tag/v0.0.2
