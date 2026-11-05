# OverlayApp

## これは何か

レイアウト、時計、アニメーション、カウンター、ドラッグ、コントローラーのボタン入力を、1つのアプリにまとめた総合の作例です。ダッシュボード、コントローラーに付くオーバーレイ、空間に置くオーバーレイの3種類を同時に作ります。

このサンプルは、統合例としてチュートリアルを補う作例です。主な読者は、FloatSoda で UI を作る利用者です。ウィジェットを1つずつ学んだあとに、アプリ全体でどう組み合わせるかを見るときに使います。

## 使い方

`Program.cs` が複数のウィンドウを作り、それぞれの中身を別のファイルのウィジェットが受け持ちます。

| ウィンドウ | 種類 | 中身 | 見どころ |
|---|---|---|---|
| FloatSodaDashboard | `DashboardWindow` | `StackWidget.cs` | `Flex` で横に並べた3つの角丸の四角 |
| WatchDashBoard | `DashboardWindow` | `WatchWidget.cs` | 毎秒書き換わる時計 |
| Left Hand | `DeviceTrackedWindow` | `WatchWidget.cs` | 左のコントローラーに付く時計 |
| Pulse | `DashboardWindow` | `PulseWidget.cs` | `AnimationController` で約1.5秒周期のフェードを繰り返す |
| Counter | `DashboardWindow` | `CounterWidget.cs` | `StatefulWidget` と `SetState` のカウンター |
| Drag | `DashboardWindow` | `DragBoxWidget.cs` | `GestureDetector.OnPanUpdate` で箱を掴んで動かす |
| WorldSpace Watch | `WorldSpaceWindow` | `WatchWidget.cs` | プレイエリアの中央から前方1m・高さ1mに置く時計 |

コントローラーのボタン入力は、`InputAction<bool>` で受け取ります。右手のトリガーを引くと、コンソールに `grab: pressed` と `grab: released` が出ます。ボタンの割り当ては、SteamVR のコントローラーバインディングの画面から変えられます。

ポインターの座標が届くのはダッシュボードのオーバーレイだけです。Left Hand と WorldSpace Watch のウィンドウでは、タップやドラッグを受け取れません。詳しくは、[できないこと・まだ使えないもの](../../../docs/reference/limitations.md#置く場所と入力)を開いてください。

## Flutterとの違い

対象外です。ウィジェット1つではなく、複数のウィジェットとオーバーレイの種類を組み合わせたアプリ全体の作例です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.OverlayApp
```

## 関連

- [GettingStarted](../FloatSoda.Samples.GettingStarted) — 最小のアプリの形
- [アニメーションを付ける](../../../docs/guide/4-build/screen/add-animation.md) — `PulseWidget` の書き方
- [ボタンやトグルを組み立てる](../../../docs/guide/4-build/screen/build-buttons-toggles.md) — `DragBoxWidget` のドラッグ
- [作例のカタログ](../../../docs/reference/sample-catalog.md) — ほかの作例の一覧
