← [コントリビューター](../Home.md)

# 実験: OVR 層

## 問い: OVR 層だけで、オーバーレイと入力が扱えるか

`FloatSoda.OVR` だけを参照するコンソールアプリで、オーバーレイを出し、レーザーポインターの入力を受けられるかを、HMD の中で確かめる実験です。[OVR 層](ovr-layer.md) を読んだあとに進みます。SteamVR と HMD が要ります。

完成形は次のとおりです。

- HMD の 1 m 前に付いたオーバーレイに、画像が出る
- レーザーポインターを当てると、コントローラーが振動する
- ホバー、クリック、移動のイベントが、コンソールに出る

先に予想を立てます。FloatSoda のほかの層なしで、ここまで動くかを考えてください。

## 用意するもの

SteamVR と HMD を使える状態にします。FloatSoda を clone したフォルダーで、オーバーレイに出す画像 `layer_tree_output.png` を作ります。サンプルの `PaintingSample` を1回動かすと、デスクトップに `layer_tree_output.png` ができます。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.PaintingSample
```

**できた?** デスクトップに `layer_tree_output.png` ができた。

<details>
<summary>できなかったら</summary>

ビルドに失敗する場合は、[開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md) で SDK と clone の状態を確かめます。HMD を持っていない場合は、この実験を飛ばして [実験: 描画層](experiment-rendering.md) に進みます。

</details>

## サンプルを動かす

実験には、[samples/Showcase/FloatSoda.Samples.PrimitiveOverlay](../../../samples/Showcase/FloatSoda.Samples.PrimitiveOverlay/) を使います。プロジェクトが参照するのは `FloatSoda.OVR` だけです。`Program.cs` の流れは次のとおりです。

1. `OVRApplication` を作って、SteamVR に接続し、`Identify()` を呼ぶ
2. `VRSystemEventDispatcher` に、終了を知らせる `VREvent_Quit` の処理を登録する
3. `OverlayIdentity` と `DeviceTrackedOverlay` を作り、位置を `(0, 0, -1)` にする
4. デスクトップの `layer_tree_output.png` を `Texture.FromFile` で読み、`Visibility.Show()` で出す
5. `Input.Method` をマウスにして、`MakeOverlaysInteractiveIfVisible` を有効にする
6. `FocusEnter`、`FocusLeave`、`MouseButtonDown`、`MouseButtonUp`、`MouseMove` の処理を登録する
7. ループでホバー状態の変化の表示と、2つのディスパッチャーの `PollEvents` を行い、各周の終わりに `1000 / 72` ミリ秒待機する

画像のパスは、第1引数で渡せます。省略すると、デスクトップの `layer_tree_output.png` を使います。SteamVR を起動してから、サンプルを動かします。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.PrimitiveOverlay
```

**できた?** 初期化エラーで終了せず、動き続けている。

<details>
<summary>できなかったら</summary>

`Init_HmdNotFound` の `VRInitializeException` で止まる場合は、SteamVR の状態と HMD の接続を確かめます。`NullReferenceException` で止まる場合は、原因が確かめられていません。スタックトレースの全文を添えて、[OVR 層](ovr-layer.md) の「つまずきやすい点」に従って報告します。`OpenVR.IsHmdPresent()` が `True` でも、初期化が `Init_HmdNotFound` で失敗する場合があります。詳細は [OVR 層](ovr-layer.md) の「つまずきやすい点」にあります。デスクトップに png がない場合は、前の手順に戻って `PaintingSample` を動かします。

</details>

<!-- ※要確認: png がないときの `FromFile` の挙動(例外になるか) / デスクトップの png を消して動かし、出力を確かめる -->

## HMD の中で操作する

ソースの読みでは、オーバーレイは HMD の正面、1 m 先に付きます。実機での見え方は未確認です。`DeviceTrackedOverlay` の追従先は、既定で HMD で、位置は追従先から見た `(0, 0, -1)` です。頭を動かしても、同じ位置に付いてきます。

正面の 1 m 先でオーバーレイを探し、レーザーを当てて、クリックします。

**できた?** 画像が見え、ホバーとクリックで振動した。

<details>
<summary>できなかったら</summary>

オーバーレイが見えない場合は、位置(`Transform.Position`)と、追従先のデバイス(`Transform.Target`)を確かめます。ホバーしない場合は、`Input.Method` がマウスになっているか、`MakeOverlaysInteractiveIfVisible` が有効かを確かめます。

</details>

<!-- ※要確認: 絵、入力、振動が実機で出るか / 追従先の既定が HMD で、位置が HMD の 1 m 前になること(ソースの読みだけで、実機で見え方を確かめていない) / HMD でサンプルを動かして確かめる -->

## ログと振動を読む

コンソールには、イベントに応じて次の行が出ます。

| 出力 | イベント |
| :-- | :-- |
| `クリック: (x, y) ボタン: Left` | `MouseButtonDown`。座標とボタンを出す |
| `クリック離した` | `MouseButtonUp` |
| `移動: (x, y)` | `MouseMove`。座標を出す |
| `hovered` | ホバーが始まったときに出る |
| `unhovered` | ホバーが終わったときに出る |

振動は、`FocusEnter` で 25 ミリ秒、`FocusLeave` で振幅を下げた 25 ミリ秒、`MouseButtonDown` で 25 ミリ秒(周波数 1500、振幅 0.5)に設定されています。

見る点は、クリックした位置の座標と、イベントの順序です。

**できた?** クリックした位置の座標が出た。

<details>
<summary>できなかったら</summary>

座標が出ない場合は、`overlay.EventDispatcher.PollEvents()` がループの中で呼ばれているかを確かめます。イベントは、`PollEvents` を呼んだスレッドで、ハンドラーが呼ばれます。

</details>

<!-- ※要確認: 座標の範囲 / サンプルは SetMouseScale を呼ばないので、x と y の範囲は OpenVR の既定になる。実機でクリックして、値の範囲を確かめる(フレームワークは、ピクセルサイズで SetMouseScale を呼んでいる) -->
<!-- ※要確認: FocusLeave の処理が MakeOverlaysInteractiveIfVisible を false にするので、2回目以降のホバーが動くか / 実機で、レーザーを外して当て直して確かめる -->

## なぜそうなるか

OVR 層だけで、画像の提出と入力の受信ができます。画像は `Texture.FromFile` が OpenVR にファイルのパスを渡し、OpenVR が画像ファイルを読みます。入力は `VREvent_*` のイベントとして、`PollEvents` を呼んだときに受け取ります。

OVR 層が持たないのは、描画とレイアウトです。描画層が Layer ツリーを絵にし、エンジン層が GL テクスチャとして OpenVR に提出し、フレームワーク層がウィジェットから Layer ツリーを作ります。ポインターの扱いも上の層が足します。エンジン層は、OpenVR のマウスイベントを `OverlayMouseRawPointerSource` で包んで、フレームワークへ渡します。OVR 層の型の詳細は [OVR 層](ovr-layer.md) にあります。

上の層が足すものは、[描画が届くまで(4段)](tutorial-render-path.md) で、層を1つずつ足して確かめられます。

## 次に読むもの

- [OVR 層](ovr-layer.md): OpenVR を包む型と、つまずきやすい点
- [描画が届くまで(4段)](tutorial-render-path.md): 層を足して、描画が届くまでを作る
- [実験: 描画層](experiment-rendering.md): HMD なしで確かめられる実験

