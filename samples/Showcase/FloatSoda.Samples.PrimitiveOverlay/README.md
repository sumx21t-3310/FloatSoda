# PrimitiveOverlay

## これは何か

`FloatSoda.OVR` だけを参照するコンソールアプリで、オーバーレイを出し、レーザーポインターの入力を受ける作例です。コントリビューター向け docs「実験: OVR 層」の実験に使います。FloatSoda のほかの層は使いません。

このサンプルは、低レベル API と `FloatSoda.OVR` の境界を理解するための作例です。主な読者は、コントリビューターと、ウィジェットを使わずに OpenVR のオーバーレイを直接扱いたい利用者です。

## 使い方

`Program.cs` の流れは次のとおりです。

1. `OVRApplication` を作って SteamVR に接続し、`Identify()` で SteamVR に識別させる。マニフェスト未登録で失敗しても続行する
2. `VRSystemEventDispatcher` に、`VREvent_Quit` の処理を登録する
3. `DeviceTrackedOverlay` を作り、位置を `(0, 0, -1)` にする
4. png を `Texture.FromFile` で読み、`Visibility.Show()` で出す
5. `Input.Method` をマウスにして、`MakeOverlaysInteractiveIfVisible` を有効にする
6. `FocusEnter`、`FocusLeave`、`MouseButtonDown`、`MouseButtonUp`、`MouseMove` の処理を登録する
7. ループでホバー状態の変化と、2つのディスパッチャーの `PollEvents` を処理する

コンソールには、`クリック: (x, y) ボタン: Left`、`クリック離した`、`移動: (x, y)` が出ます。ホバー状態が変わったときだけ、`hovered` か `unhovered` が出ます。振動は、`FocusEnter`、`FocusLeave`、`MouseButtonDown` で鳴ります。

## Flutterとの違い

対象外です。Flutter のウィジェットではなく、OpenVR のオーバーレイを直接扱う作例です。

## 実行

SteamVR と HMD が要ります。`--desktop` は持ちません。png のパスは第1引数で指定します。省略すると、デスクトップの `layer_tree_output.png`(`PaintingSample` が出力する)を使います。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.PrimitiveOverlay -- <png のパス>
```

## 関連

- コントリビューター向け docs「実験: OVR 層」
- `samples/Showcase/FloatSoda.Samples.PaintingSample`: 表示する png を作る
- `samples/Experiments/FloatSoda.Samples.RenderPath`: 層を足して、描画が届くまでを確かめる
