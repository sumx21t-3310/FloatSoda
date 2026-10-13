# プラットフォーム: 仕組みと理由

プラットフォームの層は、FloatSoda が描いた絵を SteamVR のオーバーレイとして表示し、SteamVR からのイベントと入力を受け取る層です。対象は `FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR` の3つのプロジェクトです。このページでは、OpenVR の初期化、オーバーレイ、イベントと入力、2つのスレッド、テクスチャ転送と `FramePacer` が、いまどう動き、なぜそうしたかを説明します。変更の前後で守る条件は [プラットフォーム: 守ること](Reference.md) にあります。

## 実装状況

利用者から観測できる制約のうち、この層に原因があるものは次のとおりです。

- ポインタ入力が届くのはダッシュボードのオーバーレイだけで、それ以外のオーバーレイでは `PointerSource` が `null` になる(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))
- サイズを省略したウィンドウでは、SteamVR 上の表示範囲が起動時の大きさに固定される(追跡: [#285](https://github.com/sumx21t-3310/FloatSoda/issues/285))
- レンダースレッドは、描くものがないときも一定の間隔で起きる(追跡: [#212](https://github.com/sumx21t-3310/FloatSoda/issues/212))
- OpenVR の初期化に失敗するとアプリは起動せず、あとから接続する仕組みも再接続する仕組みもない(追跡: [#140](https://github.com/sumx21t-3310/FloatSoda/issues/140))

## 責務と境界

プラットフォームの層は、OpenVR と OpenGL を FloatSoda の型で包み、フレームワークの層へ「描画先」と「入力源」を渡します。Widget、RenderObject、Layer ツリーの組み立ては担いません。

| プロジェクト | 担うもの | 参照するもの |
|---|---|---|
| `FloatSoda.OVR` | OpenVR の初期化(`OVRApplication`)、オーバーレイと能力オブジェクト、イベントの配送(`VREventDispatcher`)、アクション入力、型付きの例外。生バインディング `openvr_api.cs` を含む | なし |
| `FloatSoda.Abstractions` | 描画先の契約(`IEngineWindow`)、生ポインタ入力の契約と変換(`IRawPointerSource`、`PointerController`)、フレーム待機の契約(`IFramePacer`)、`Offset` や `Dpm` などの値型 | `FloatSoda.Rendering` |
| `FloatSoda.Engine` | OpenGL と Skia の描画先(`GLView`、`Renderer`)、オーバーレイとデスクトップのウィンドウ(`OverlayWindow`、`DesktopWindow`)、専用スレッド(`RenderPostTaskRunner`、`IOTaskRunner`)、`FramePacer` | `FloatSoda.Abstractions`、`FloatSoda.OVR`、`FloatSoda.Rendering` |

参照関係は各プロジェクトの `.csproj` が一次情報です。`FloatSoda.OVR` はほかのプロジェクトを参照しないため、FloatSoda の UI を使わずに OpenVR だけを扱うこともできます([`samples/FloatSoda.Samples.PrimitiveOverlay`](../../../samples/FloatSoda.Samples.PrimitiveOverlay/Program.cs) がその例です)。

## 関係するコンポーネント

| 型 | ソース | 役割 |
|---|---|---|
| `OVRApplication` | [`FloatSoda.OVR/OVRApplication.cs`](../../../src/FloatSoda.OVR/OVRApplication.cs) | コンストラクタで `OpenVR.Init`、`Dispose` で `OpenVR.Shutdown` を呼ぶ |
| `IOverlay` と実装 | [`FloatSoda.OVR/Overlay/Overlay.cs`](../../../src/FloatSoda.OVR/Overlay/Overlay.cs) | オーバーレイの種類。能力オブジェクトをプロパティとして持つ |
| 能力オブジェクト | [`FloatSoda.OVR/Overlay/Capability.cs`](../../../src/FloatSoda.OVR/Overlay/Capability.cs) | `OverlayOpacity` など。プロパティの読み書きを OpenVR の呼び出しに変える |
| `OverlayIdentity`、`DashboardOverlayIdentity` | [`FloatSoda.OVR/Overlay/Identity.cs`](../../../src/FloatSoda.OVR/Overlay/Identity.cs) | オーバーレイのハンドルを作り、`Dispose` で破棄する |
| `VREventDispatcher` | [`FloatSoda.OVR/SystemEventDispatcher.cs`](../../../src/FloatSoda.OVR/SystemEventDispatcher.cs) | OpenVR のイベントキューを読み、種類ごとのハンドラへ配る |
| `ActionManifestWriter`、`VRInputUpdater` | [`FloatSoda.OVR/Input/`](../../../src/FloatSoda.OVR/Input) | アクションマニフェストの生成と、毎フレームの入力の更新 |
| 例外 | [`FloatSoda.OVR/Exceptions/`](../../../src/FloatSoda.OVR/Exceptions) | OpenVR のエラー値を型付きの例外に変える |
| `GLView`、`Renderer` | [`FloatSoda.Engine/GLView.cs`](../../../src/FloatSoda.Engine/GLView.cs)、[`Renderer.cs`](../../../src/FloatSoda.Engine/Renderer.cs) | GL コンテキスト、GL テクスチャ、Skia のサーフェスを持ち、Layer ツリーを描く |
| `OverlayWindow` | [`FloatSoda.Engine/OverlayWindow.cs`](../../../src/FloatSoda.Engine/OverlayWindow.cs) | 描いたテクスチャをオーバーレイへ渡す `IEngineWindow` |
| `RenderPostTaskRunner`、`IOTaskRunner` | [`FloatSoda.Engine/TaskRunner.cs`](../../../src/FloatSoda.Engine/TaskRunner.cs) | レンダースレッドと I/O スレッド |
| `FramePacer` | [`FloatSoda.Engine/FramePacer.cs`](../../../src/FloatSoda.Engine/FramePacer.cs) | ループを目標フレームレートに合わせて待たせる |

この層を使う側は、フレームワークの `FloatSodaApp`([`src/FloatSoda/FloatSodaApp.cs`](../../../src/FloatSoda/FloatSodaApp.cs))と `WidgetBinding`([`src/FloatSoda/Core/WidgetBinding.cs`](../../../src/FloatSoda/Core/WidgetBinding.cs))です。

## 処理の流れ

### 起動と終了

`FloatSodaApp.Run()` は、メインスレッドで OpenVR を初期化してから、2つの専用スレッドを開始します。順序は次のとおりです。

1. `new OVRApplication(appInfo)` で `OpenVR.Init` を呼び、失敗すれば `VRInitializeException` で起動を止める
2. `OVRApplication.Identify()` でプロセスを AppKey に結び付け、失敗しても警告をログに出して起動を続ける
3. `VRSystemEventDispatcher` を作り、`VREvent_Quit` と `VREvent_ProcessQuit` で停止を要求するハンドラと、トラッキングデバイスの接続でオーバーレイの位置を再適用するハンドラを登録する
4. アクション入力の定義があればマニフェストを書き出して `VRInputUpdater` を作り、失敗しても警告をログに出して起動を続ける
5. `IOTaskRunner` と `RenderPostTaskRunner` を開始する

停止が要求されると、メインループを抜け、`FloatSodaApp.Dispose` で資源を解放します。

### オーバーレイと能力オブジェクト

オーバーレイは、位置を SteamVR が決めるダッシュボード(`IDashboardOverlay`)と、位置をアプリが決めるもの(`IMovableOverlay`)に分かれます。種類の一覧は [`Overlay.cs`](../../../src/FloatSoda.OVR/Overlay/Overlay.cs) にあります。

どのオーバーレイも、OpenVR のハンドルを持つ Identity と、能力オブジェクトのプロパティでできています。能力オブジェクト(`OverlayOpacity`、`OverlayWidthInMeters` など)は、ハンドルだけを持ちます。値を読むたび、書くたびに OpenVR の `GetOverlayXxx` と `SetOverlayXxx` を呼び、値を手元に保存しません。戻り値のエラーは `ThrowIfError()` で型付きの例外に変えます。

### ウィンドウの作成とテクスチャ転送

1つのウィンドウは、1つの GL コンテキストと、1枚の GL テクスチャを持ちます。`WidgetBinding.EnsureInitialized` が、レンダースレッドへ次の処理を投入します。

1. GLFW の隠しウィンドウの GL コンテキスト、RGBA8 の GL テクスチャ、そのテクスチャを描画先とする Skia のサーフェス(`GRSurfaceOrigin.BottomLeft`)を持つ `GLView` を作る
2. `OverlayWindow` を作り、OpenVR のオーバーレイもこのとき作る

ウィンドウのサイズを省略すると、`GLView` は 1000×1000 で作られます。ダッシュボードのオーバーレイでは、`OverlayWindow` が入力方式をマウスにし、マウス座標の範囲をテクスチャのピクセル数に合わせます。

毎フレームの転送は、次の順で進みます。

1. メインスレッドの `WidgetBinding.DrawFrame()` が、Clone した Layer ツリーを `RenderPostTaskRunner.PostRender` でキューに積み、描画を待たずに戻る
2. レンダースレッドのループは、`PreUpdate` でキューのタスクをすべて実行し、`Update` で `GLFW.PollEvents()` を呼び、`FramePacer` で次のフレームまで待つ
3. 描画のタスクが呼ぶ `OverlayWindow.Present` の中で、`Renderer.Render` が GL コンテキストをスレッドに結び付け、描画先のクリア、Layer ツリーの描画、Skia と GL のフラッシュを行う
4. `OverlayWindow.Present` が、GL テクスチャのハンドルを `Texture_t`(`ETextureType.OpenGL`、`EColorSpace.Auto`)に入れて `SetOverlayTexture` で SteamVR に渡す

レンダースレッドが起きるまでにメインスレッドが2フレーム分のタスクを積んだ場合、2つとも順に描かれます。古いフレームを飛ばす仕組みはありません。

レイアウトの結果でウィンドウのサイズが変わると、メインスレッドは `IEngineWindow.Resize` をレンダースレッドへ投入します。`OverlayWindow.Resize` は、GL テクスチャとサーフェスを作り直し、オーバーレイの物理幅を「ピクセル幅 ÷ `Dpm`」に設定します。デスクトップ表示(`DesktopWindow`)では、`SetOverlayTexture` の代わりに、GL テクスチャを GLFW ウィンドウの画面へコピーして表示します。

この流れは、[実験: SteamVR へのテクスチャ転送はどうなっているの?](../experiments/TextureTransfer.md) で動かして確かめられます。Layer ツリーの描き方は [描画: 仕組みと理由](../rendering/Explanation.md) にあります。

### FramePacer

`FramePacer` は、メインループとレンダースレッドのループを、それぞれ目標フレームレートに合わせて待たせます。2つのループは別々の `FramePacer` を持ちます。どちらの目標も `FloatSodaOptions.TargetFrameRate`(既定値 60)です。

`WaitForNextFrame` は、前回の待機が終わってからの経過時間を `Stopwatch` で測り、1フレームの目標時間から引いた残りだけ待ちます。待ち方は2段です。

1. 残りがあれば、残りより 1 ミリ秒短い時間を、キャンセルで中断できる `WaitHandle.WaitOne` で眠る
2. 目標時間に達するまで、`Thread.Yield()` を挟みながら `Stopwatch` を見続ける

最後に `Stopwatch` を再スタートします。処理が目標時間を超えたフレームでは待たずに戻ります。`FramePacer` は SteamVR の表示のタイミングとは同期していません。

眠る待機には誤差があるため、1 ミリ秒手前で起きて、残りを細かく待つ形にしています(`FramePacer.cs` のコメント)。

### イベントと入力

メインループは、毎フレーム次の順でイベントと入力を処理します。

1. `VRSystemEventDispatcher.PollEvents()` が、OpenVR のシステムのイベントキューを空になるまで読み、登録されたハンドラを呼ぶ
2. `VRInputUpdater.Update()` が、有効なアクションセットだけを `UpdateActionState` で更新し、各アクションの値とイベント(`OnPerformed`、`OnReleased`)を更新する
3. 各ウィンドウの `WidgetBinding.BeginFrame` が `OverlayWindow.PollInputEvents()` でオーバーレイのイベントキューを読み、`OverlayMouseRawPointerSource` が変えた `RawPointerEvent` を `PointerController` がキューに積む
4. `PointerController.Flush()` が、キューの生イベントを `PointerEvent` に変えて、ヒットテストへ渡す

手順3で生イベントを作るのは、ダッシュボードのオーバーレイだけです。手順2で使う有効なアクションセットの配列は、`Enabled` の組み合わせが変わったフレームでだけ作り直します。

アクション入力のマニフェストは、起動時に C# の定義(`InputActionMap`、`InputAction<T>`)から生成します。`ActionManifestWriter.Write` が、`actions.json` と、コントローラーの種類ごとのバインディングの JSON を `%TEMP%/FloatSoda/{AppKey}/input/` に書き出します。`VRInputUpdater` が `SetActionManifestPath` でマニフェストを登録し、アクションセットとアクションのハンドルを解決します。ヒットテストから先は、フレームワーク(三つのツリー)の層で扱います。

### 所有権とスレッド

| スレッド | 持つもの | ほかのスレッドとのやりとり |
|---|---|---|
| メインスレッド | `OVRApplication`、`VRSystemEventDispatcher`、`VRInputUpdater`、Widget・RenderObject・Layer のツリー、`PointerController` | `RenderPostTaskRunner.PostTask` / `PostRender` でレンダースレッドへタスクを積む |
| レンダースレッド | GLFW(`GLFW.Init` から `GLFW.Terminate` まで)、`GLView`、`Renderer`、`IEngineWindow` の実装 | `ConcurrentQueue<Action>` に積まれたタスクを、`PreUpdate` で取り出して実行する |
| I/O スレッド | `IOTaskRunner.RunAsync` に投入された処理 | 投入順に1件ずつ実行し、`Task` で結果を返す |

OpenVR の呼び出しは、2つのスレッドに分かれています。オーバーレイの作成、`SetOverlayTexture`、物理幅とマウス座標の範囲の設定、トラッキングデバイスへの位置の再適用はレンダースレッドで行います。システムとオーバーレイのイベントの読み出しと、アクション入力の更新はメインスレッドで行います。

`Identity` はオーバーレイのハンドルを持ち、`Dispose` で `DestroyOverlay` を呼びます。`OverlayWindow.Dispose` は、ポインタ入力源とオーバーレイを破棄します。

## なぜそう設計したか

### GL の資源をレンダースレッドだけに置く

GL コンテキストは、`GLFW.MakeContextCurrent` を呼んだスレッドで使います。FloatSoda では、GLFW の初期化から終了まで、GL コンテキストとテクスチャの作成・描画・破棄を、すべてレンダースレッドで行います。ウィンドウの作成もタスクとしてレンダースレッドへ投入するのは、このためです。`GLView` の XML ドキュメントコメントも、レンダースレッドで作成・使用・破棄するよう求めています。

レンダースレッドで1つのタスクが例外を投げても、`DrainPendingTasks` が例外をログに出して次のタスクへ進みます。例外を拾わないと、ループの外側の `catch` まで抜けて `OnStop` が走り、以後すべての描画が止まるためです(`PostTaskRunner.DrainPendingTasks` のコメント)。

### マウス座標を反転しない

OpenVR の仕様では、オーバーレイのマウス座標の原点は左下です。一方、FloatSoda の GL テクスチャは下から上へ格納され、SteamVR が表示するときに上下が反転します。2つの反転が打ち消し合い、マウス座標は左上を原点として届きます。実測で確かめたうえで、`OverlayMouseRawPointerSource` は座標を反転せずに使います(同クラスの XML ドキュメントコメント)。y を反転すると、上下が逆になります。

### 起動を止めない失敗と、止める失敗

`OpenVR.Init` の失敗では、例外を投げ直して起動を止めます。一方、`Identify()` の失敗と、アクション入力の初期化の失敗は、警告をログに出して起動を続けます(`FloatSodaApp.Initialize` のコメント)。

`Identify()` の失敗で起動を止めないのは、マニフェストに登録していない AppKey でも起動できるようにするためです。動かなくなるのは、自動起動やシーンの切り替えのように、マニフェストと連携する機能だけです。この扱いの見直しは [#147](https://github.com/sumx21t-3310/FloatSoda/issues/147) で追跡しています。アクション入力の初期化の失敗で起動を止めないのは、入力が使えなくても、オーバーレイの表示は続けられるためです。

### アクション入力を C# だけで定義する

アクション入力の定義は C# のコードだけで完結し、利用者は JSON を書きません。FloatSoda の利用者は、UI をコードだけで書けることを求めています([TargetUsers](../../TargetUsers.md))。マニフェストとバインディングの JSON は、起動のたびに C# の定義から生成します。

バインディングを実行時に変える API は、意図して用意していません。実際のバインディングは SteamVR が管理し、利用者は SteamVR のバインディングの画面からいつでも変えられるためです。C# で書く `SuggestedPath` と `DefaultBindings` は、最初の割り当ての提案です。

### デバイスが未接続でも例外にしない

`DeviceTrackedOverlayTransform.Apply()` は、追従先のコントローラーが未接続のとき、例外を投げずに `PendingReapply` を `true` にして戻ります。起動時にコントローラーの電源が入っていないことは普通にあるためです。`FloatSodaApp` は、`VREvent_TrackedDeviceActivated` と `VREvent_TrackedDeviceRoleChanged` を受けて、位置を再適用します。

### コントローラーのレイを自前で計算する部品

`ControllerPointerSystem` は、`VROverlayFlags.MakeOverlaysInteractiveIfVisible` を使わずに、コントローラーのレイとオーバーレイの交差を毎フレーム自前で計算します。このフラグを使うと SteamVR が入力を横取りし、VRChat の AFK 判定にかかるためです(同クラスの XML ドキュメントコメント)。この部品は、まだウィンドウに接続していません(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182))。

## Flutter との対応

この層は、Flutter のエンジンと embedder(各プラットフォームへの組み込み部分)が担う範囲に当たります。Flutter のフレームワーク(Dart)には対応する型がないため、Flutter の API には合わせていません。メインスレッドで Layer ツリーを作り、別のスレッドで描く分担は、Flutter の UI スレッドとラスタースレッドの分担と同じです。

ポインタ入力は、メインループがフレームごとに1回まとめて読み出します。Flutter はフレームと関係なくポインタイベントを配るので、この点は Flutter と違います([`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) のエントリ 7)。違いは [描画: 仕組みと理由](../rendering/Explanation.md) の「Flutter との対応」にあります。

## テストでの駆動

OpenVR と GL を呼ぶ部分は、SteamVR と HMD がないと動きません。実機での確かめ方は、[`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) スキルにあります。

OpenVR と GL を呼ばない部分は、ヘッドレスのテストで確かめられます。

- アクションマニフェストの生成は、`ActionManifestWriter.Write` に出力先を渡して確かめる([`tests/FloatSoda.Test/Input/ActionManifestWriterTest.cs`](../../../tests/FloatSoda.Test/Input/ActionManifestWriterTest.cs))
- レンダースレッドのタスクの順序と例外の扱いは、`PostTaskRunner` を継承したテスト用の型で確かめる([`tests/FloatSoda.Test/Engine/PostTaskRunnerTest.cs`](../../../tests/FloatSoda.Test/Engine/PostTaskRunnerTest.cs))
- `FloatSodaApp` は `IFramePacer` と `TimeProvider` を受け取るので、テストでは差し替えられる

## 関連

- [プラットフォーム: 守ること](Reference.md) — `internal` の契約
- [プラットフォーム: 作業の手順](HowTo.md) — OpenVR の API を包む、例外を足す
- [描画: 仕組みと理由](../rendering/Explanation.md) — Layer ツリーと Clone
- [実験: オーバーレイの大きさは何で決まるの?](../experiments/OverlaySize.md)
- [OVRIntegration](../OVRIntegration.md) — 再編前のページ
