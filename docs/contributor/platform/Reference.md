# プラットフォーム: 守ること

このページは、プラットフォームの層(`FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR`)を変えても維持する条件のうち、`internal` の契約と、1つの型に収まらない条件の一覧です。`public` の型とメンバーの契約(引数、戻り値、例外、スレッドの制約)は XML ドキュメントコメントにあり、API Reference として公開されます。条件がなぜ要るかは [プラットフォーム: 仕組みと理由](Explanation.md) にあります。

## 適用範囲

| 対象 | ソース |
|---|---|
| アクション入力の `internal` メンバー(`InputAction.Handle`、`ManifestType`、`Update`、`BindingPathsFor`、`InputActionMap.Handle`) | [`src/FloatSoda.OVR/Input/`](../../../src/FloatSoda.OVR/Input) |
| マニフェストのパスの命名(`InputPaths`)と、JSON の組み立て(`ActionManifestWriter.BuildManifest`、`BuildBindings`) | 同上 |
| 例外メッセージのアクセサ(`ExceptionMessages`) | [`src/FloatSoda.OVR/Exceptions/Resources/`](../../../src/FloatSoda.OVR/Exceptions/Resources) |
| GLFW ウィンドウのハンドル(`GLView.WindowHandle`) | [`src/FloatSoda.Engine/GLView.cs`](../../../src/FloatSoda.Engine/GLView.cs) |
| GL の資源と OpenVR の呼び出しを、どのスレッドで行うか | `FloatSoda.Engine` と、それを使う `FloatSoda` の `FloatSodaApp`・`WidgetBinding` |

`FloatSoda.OVR` には `InternalsVisibleTo` がありません。上の `internal` メンバーは、テストからは `public` の `ActionManifestWriter.Write` などを通して確かめます。

## 必須条件

### アクション入力

- マニフェストに書くパスと、ハンドルを解決するパスは、どちらも `InputPaths` から作ること(アクションセットは `/actions/{map}`、アクションは `/actions/{map}/in/{action}`)
- `InputAction.ManifestType` は、値の型 `bool`、`float`、`Vector2` に対して、それぞれ `boolean`、`vector1`、`vector2` を返すこと
- `InputAction.BindingPathsFor(controller)` は、`DefaultBindings` のうち指定したコントローラーのパスだけを返し、`DefaultBindings` が空のときに限り `SuggestedPath` を返すこと
- `InputPaths.ControllerName` が返すコントローラーの名前は、`ActionManifestWriter` が書き出すバインディングのファイル名(`bindings_{name}.json`)と、マニフェストの `controller_type` の両方に使うこと
- `InputAction.Handle` と `InputActionMap.Handle` は、`VRInputUpdater` のコンストラクタだけが設定すること
- `InputAction.Update()` は、`VRInputUpdater.Update()` から、`UpdateActionState` が成功したフレームに、`Enabled` が `true` のマップのアクションに対してだけ呼ぶこと

### 例外メッセージ

- `ExceptionMessages` の各プロパティのキーは `{例外クラス名}_{OpenVR のエラー列挙子}` とし、`ExceptionMessages.resx` と `ExceptionMessages.en.resx` の両方に同じキーで置くこと(手順は [Localization](../Localization.md) の「3. 例外メッセージ」)

### スレッド

- GLFW の初期化と終了、`GLView` の作成・使用・破棄は、ほかのスレッドから `PostTask` か `PostRender` で投入し、レンダースレッド(`RenderPostTaskRunner` のスレッド)で行うこと
- `GLView.WindowHandle` は、レンダースレッドでだけ使うこと
- `IEngineWindow.Present` と `Resize` は、レンダースレッドで呼ぶこと(`IEngineWindow` の XML ドキュメントコメント)
- `PostTaskRunner` の派生クラスは、`OnStart` で作ったスレッド所属の資源を `OnStop` で解放すること(`PostTaskRunner.OnStop` の XML ドキュメントコメント)

## 不変条件

- `InputAction.Handle` が `OpenVR.k_ulInvalidActionHandle` のあいだ、`InputAction.Update()` は値もイベントも変えない
- `ExceptionMessages` は、キーに対応する文字列がリソースにないとき、例外を投げずにキーの文字列そのものを返す
- レンダースレッドのタスクが例外を投げても、後続のタスクは実行され、レンダースレッドは止まらない

## Observable behavior

`internal` の契約のうち、利用者とテストから観測できるものです。

- `ActionManifestWriter.Write` は、`actions.json` と、Index・Vive Wand・Oculus Touch の3種類のバインディングの JSON を、出力先のディレクトリに書き出す
- 入力パスの末尾が `click`、`touch`、`pull`、`position` のどれでもなければ、入力の種類を値の型から補う(`bool` は `click`、`float` は `pull`、`Vector2` は `position`)
- `bool` のアクションは押した瞬間に `OnPerformed`、離した瞬間に `OnReleased` を発火し、`float` と `Vector2` のアクションは値が変わったフレームに `OnPerformed` を発火する
- レンダースレッドに投入したタスクは、投入した順に実行される

## 破ったときの failure mode

| 破った条件 | 起きること |
|---|---|
| マニフェストのパスと、ハンドルを解決するパスを別の書き方で作る | マニフェストにないアクションのハンドルを引くことになり、そのアクションの値とイベントが更新されない。`GetActionHandle` がエラーを返して `VRInputException` になるか、エラーにならずに値が届かないだけかは ※要確認 |
| `ManifestType` と値の型が合わない | SteamVR がアクションの値を別の型として返し、`GetDigitalActionData` か `GetAnalogActionData` がエラーになる。エラーのフレームは無視されるので、値が更新されないまま残る ※要確認(実機で確かめていない) |
| resx の片方にだけキーを足す | その言語の環境で、例外メッセージがキーの文字列のまま表示される |
| `GLView` をレンダースレッドの外で使う | GL コンテキストが別のスレッドに結び付いたまま GL を呼ぶことになり、描画が失敗するか、プロセスが落ちる ※要確認(どちらになるか) |
| レンダースレッドのタスクの例外を拾わずにループの外へ出す | `OnStop` が走って GLFW が終了し、以後すべての描画が止まる |

## 検証方法

| 条件 | 固定しているテスト |
|---|---|
| マニフェストのパス、値の型、コントローラーの名前、`SuggestedPath` の複製、`DefaultBindings` の絞り込み | [`tests/FloatSoda.Test/Input/ActionManifestWriterTest.cs`](../../../tests/FloatSoda.Test/Input/ActionManifestWriterTest.cs) |
| 対応していない値の型を拒否する | 同じファイルの `InputAction_RejectsUnsupportedValueType` |
| レンダースレッドのタスクの例外と順序 | [`tests/FloatSoda.Test/Engine/PostTaskRunnerTest.cs`](../../../tests/FloatSoda.Test/Engine/PostTaskRunnerTest.cs) の `DrainPendingTasks_タスクが例外を投げる_後続タスクは実行される`、`DrainPendingTasks_複数タスクを投稿_FIFO順に実行される` |
| マニフェストのパスと、ハンドルを解決するパスが一致する | 未設定(SteamVR が要る) |
| `InputAction.Update()` のイベントの発火 | 未設定(SteamVR が要る) |
| resx の2つのファイルのキーが一致する | 未設定 |
| GL の資源をレンダースレッドの外で使わない | 未設定。レビューで確認する |

SteamVR が要る条件は、[`floatsoda-device-test-gen`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルでハーネスのシナリオにできます。

## Flutter との関係

この層の契約は、Flutter のエンジンと embedder の範囲に当たり、Flutter のフレームワークの API とは対応しません。[`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) のうち、この層の入力に関わるのは、エントリ 7(ポインタ入力がフレームの境目でまとめて処理される)と、エントリ 8(フレームレートが落ちたときの入力の遅れ。未確認)です。

## 関連

- [プラットフォーム: 仕組みと理由](Explanation.md) — スレッドの分担と、入力の流れ
- [プラットフォーム: 作業の手順](HowTo.md) — OpenVR の API を包む、例外を足す
- [Localization](../Localization.md) — 例外メッセージの追加手順
