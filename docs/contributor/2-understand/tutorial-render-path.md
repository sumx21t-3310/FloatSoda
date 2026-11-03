← [コントリビューター](../Home.md)

# 描画が届くまで(4段)

## このチュートリアルで作るもの

層を1つずつ足しながら、描画が SteamVR のオーバーレイに届くまでを、手を動かして確かめます。OVR 層だけでオーバーレイを出すところから始め、描画層、エンジン層、フレームワーク層の順に足します。OVR 層と描画層は互いを参照しないので、それぞれを試してから、両方を使うエンジン層へ進みます。最後に、フレームワーク層で同じ描画の流れを組みます。

| 段 | 足す層 | 目に見える結果 |
| :-- | :-- | :-- |
| 1 | OVR 層 | HMD の 1 m 前のオーバーレイに、png の絵が出る |
| 2 | 描画層 | 自分で組んだ Layer ツリーが、png の画像になる |
| 3 | エンジン層 | 同じ Layer ツリーが、GL テクスチャ経由でオーバーレイに出る |
| 4 | フレームワーク層 | ウィジェットで書いた色の四角が、ダッシュボードに出る |

最後の段で、フレームワークの `FloatSodaApp` が、段3で自分が書いた仕事と同じことをしているのが分かります。

```text
段1  OVR            OpenVR を呼ぶ                         参照: なし
段2  Rendering      Layer ツリーを絵にする                 参照: なし(SkiaSharp のみ)
段3  Engine         GL テクスチャで SteamVR に出す         参照: Abstractions、OVR、Rendering
段4  FloatSoda      ウィジェットから Layer ツリーを作る     参照: Abstractions、Engine、Rendering
```

各段の終わりに、確認の項目と、選択式のクイズが1問あります。仕組みの説明は、層ごとの解説に分けています。

## 始める前に用意するもの

次の3つを用意します。

- .NET の SDK
- FloatSoda を clone したフォルダー
- SteamVR と HMD(段2だけは、HMD なしで進められる)

4段のコードは、1つの作例 [samples/Experiments/FloatSoda.Samples.RenderPath](../../../samples/Experiments/FloatSoda.Samples.RenderPath/) にまとまっています。コードの全文は、このページに載せません。作例のファイルを読みながら進めます。リポジトリのルートで、`--step N` を付けて段を選びます。準備の詳細は [開発環境を用意する](../3-prepare/dev-environment.md) にあります。SteamVR が起動できない、HMD がつながらない場合の直し方は、ここでは扱いません。

<!-- ※要確認: 作例は #313 で main に入った。--step N と png の引数は #313 の実際の引数に合わせた。各段の出力は、移す前のコードに基づくので、#313 がマージされたら実物と照らす -->

## 1. OVR 層だけでオーバーレイを出す

段1は、FloatSoda のほかの層を使わず、`FloatSoda.OVR` だけで、png の画像をオーバーレイとして HMD の前に出します。出す png は段2が作るので、先に次のコマンドを1回動かします。png は、カレントディレクトリの `my_layer_tree.png` に保存されます。段2の内容は、次の節で説明します。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 2
```

SteamVR を起動してから、動かします。HMD の中で、オーバーレイを探します。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 1
```

別の png を出したいときは、`-- --step 1 <png のパス>` の形でパスを渡します。省略すると、カレントディレクトリの `my_layer_tree.png` を使います。

作例は、`DeviceTrackedOverlay` を HMD から 1 m 前の位置に置き、`overlay.Texture.FromFile` で png を渡します。

**できた?** HMD の 1 m 前に付いたオーバーレイに、段2で作った png の絵が出た。

<details>
<summary>できなかったら</summary>

`Init_HmdNotFound` の例外が出る場合や、オーバーレイが見つからない場合は、SteamVR の状態と HMD の接続を確かめます。原因の見分け方は、[OVR 層](ovr-layer.md) の「つまずきやすい点」にあります。段2の png がない場合は、段2に戻ります。

</details>

<!-- ※要確認: オーバーレイが HMD の 1 m 前に出ること / DeviceTrackedOverlay の追従先の既定は HMD(ソースで確認)で、位置 (0, 0, -1) の見え方は実機で確かめていない。HMD で確かめる -->
<!-- ※要確認: 段1のコードが実機で動くか / コードのコンパイルは確かめたが、HMD のある環境で実行していない -->

**クイズ** 段1のオーバーレイの絵は、だれが描いたでしょうか。

- (a) Skia
- (b) OpenVR が png を読んで表示した
- (c) OpenGL

<details>
<summary>答え</summary>

(b) です。`overlay.Texture.FromFile` が、OpenVR の `SetOverlayFromFile` を呼びます。FloatSoda は画像のパスを渡すだけで、描いているのは OpenVR です。

</details>

OVR 層の型の詳細は、[OVR 層](ovr-layer.md) にあります。

## 2. Layer ツリーを組んで png に描く

段2は、HMD も SteamVR も使わずに、自分で組んだ Layer ツリーを png にします。`FloatSoda.Rendering` だけを使います。Layer ツリーを組んで png にする基本は、[samples/Showcase/FloatSoda.Samples.PaintingSample](../../../samples/Showcase/FloatSoda.Samples.PaintingSample/) のコードで読めます。このページでは、同じ説明を繰り返しません。段2の作例は、その流れを最小にしたものです。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 2
```

コンソールに、png の保存先のパスが出ます。保存先を変えたいときは、`-- --step 2 <png の保存先>` の形で渡します。省略すると、カレントディレクトリの `my_layer_tree.png` です。保存される絵は、白っぽい地に緑の四角です。

**できた?** 保存先のパスが出て、その png を画像ビューアーで開くと、白っぽい地に緑の四角が見えた。

<details>
<summary>できなかったら</summary>

png が空になる、真っ黒になる場合は、作例の中で `PictureLayer` に `Picture` を設定したか、描画の範囲(`BeginRecording` に渡した矩形)がサイズと合っているかを確かめます。仕組みは [描画層](rendering-layer.md) の「CPU の Bitmap に描く」にあります。

</details>

**クイズ** `LayerBitmapRenderer.Render` が、`root` に対して最初に呼ぶのは、どれでしょうか。

- (a) `Paint`
- (b) `Layout`
- (c) `Clone`

<details>
<summary>答え</summary>

(b) です。内部の `LayerRenderer.Render` が、`root.Layout(context)` を呼んでから `root.Paint(context)` を呼びます。

</details>

Layer の種類と `Clone` の共有範囲は、[描画層](rendering-layer.md) にあります。合成の Layer を同じ絵に使って見比べる実験は、[実験: 描画層](experiment-rendering.md) です。

## 3. Layer ツリーをエンジン層に差し込んで SteamVR に出す

段3は、段2で組んだ Layer ツリーを、png ではなく GL テクスチャとしてオーバーレイに出します。段1の `FromFile` の代わりに、エンジン層の `RenderPostTaskRunner`、`GLView`、`Renderer`、`OverlayWindow` を使います。`FloatSoda.Engine` は、フレームワーク(`FloatSoda`)を参照しないので、フレームワークなしで書けます。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 3
```

作例のコードの要点は3つです。

- `RenderPostTaskRunner` が、描画スレッドを作る。`Start` のあと、`PostTask` に渡した処理が、描画スレッドで実行される
- `GLView`、`Renderer`、`OverlayWindow` は、描画スレッドの中で作る。`OverlayWindow` は、`Present` のたびに GL テクスチャを OpenVR に渡す
- `FramePacer(60)` が、描画ループの周期を 60 回/秒にそろえる

作例は、GL のリソースの後始末を省いた最小のコードです。

`OverlayWindow` は、エンジン層の型です。フレームワーク(`FloatSoda`)にも、同じ名前の `FloatSoda.Widgets.OverlayWindow` があります。同じプロジェクトに両方を入れるときは、フレームワークと同じように、`using EngineOverlayWindow = FloatSoda.Engine.OverlayWindow;` と別名を付けます。

**できた?** 段2と同じ絵が、`FromFile` を呼ばずに、オーバーレイに出た。

<details>
<summary>できなかったら</summary>

絵が出ない、例外が出る場合は、`GLView` を描画スレッド以外で作っていないかを、作例のコードで確かめます。GL を使う処理は、`PostTask` に入れます。スレッドの制約は、[エンジン層](engine-layer.md) の「描画スレッドは、キューのタスクを実行する」にあります。

</details>

<!-- ※要確認: 段3のコードが実機で動くか / コンパイルは確かめたが、HMD のある環境で実行していない。GL テクスチャの提出を1回だけ行うコードで、絵が出続けるか、確かめる -->
<!-- ※要確認: オーバーレイを作るスレッド / このコードはオーバーレイをメインスレッドで作り、テクスチャを描画スレッドで設定する。フレームワークは、オーバーレイの作成を描画スレッドで行う。どちらでも動くかを、実機で確かめる -->

**クイズ** GL テクスチャのハンドルを `Texture_t` に包んで、オーバーレイに渡すのは、どの型でしょうか。

- (a) `GLView`
- (b) `Renderer`
- (c) `OverlayWindow`

<details>
<summary>答え</summary>

(c) です。`OverlayWindow.Present` が、`Renderer.Render` のあとに、テクスチャのハンドルを `Texture_t` に包み、`Overlay.Texture.FromTexture_t` で OpenVR に渡します。

</details>

エンジン層の詳細は、[エンジン層](engine-layer.md) にあります。

## 4. ウィジェットで描く

段4は、段3で手書きしたものを、フレームワークに任せます。ウィジェットで色の四角を書き、ダッシュボードに出します。

SteamVR を起動してから、動かします。HMD の中でダッシュボードを開き、`RenderPath` のタブを選びます。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 4
```

段3で書いた処理は、フレームワークの次の箇所が受け持っています。

| 段3で自分が書いたもの | フレームワークの担当 |
| :-- | :-- |
| `OVRApplication` を作る | `FloatSodaApp` の起動処理 |
| `OverlayIdentity` と `DeviceTrackedOverlay` を作る | `CreateWindow` が作成を予約し、初期化処理が描画スレッドでダッシュボード用のオーバーレイを作る |
| `RenderPostTaskRunner` と `FramePacer` を作って `Start` する | `AddFloatSoda` が登録し、`FloatSodaApp` が持って開始する |
| `PostTask` で `GLView`、`Renderer`、`OverlayWindow` を作る | `WidgetBinding.EnsureInitialized` |
| Layer ツリーを手で組む | `BuildOwner` がビルドを進め、`RenderPipeline` がレイアウトと描画を処理して Layer ツリーを更新する |
| `window.Resize(size)` | `WidgetBinding.DrawFrame` が、レイアウト結果のサイズが変わったときに投入する |
| `PostRender(window, layer)` | `WidgetBinding.DrawFrame` が、`Layer` を複製して `PostRender` に渡す |
| Enter キーで待ち、`Stop` を呼ぶ | ホスト(`host.RunAsync`)と、`FloatSodaApp` の終了処理 |

**できた?** ダッシュボードに、色の四角が出た。

<details>
<summary>できなかったら</summary>

起動で例外が出る場合は、利用者向けのガイドの [エラーを読んで直す](../../guide/5-troubleshoot/read-errors.md) を確かめます。四角が出ない場合は、[反応しないときに切り分ける](../../guide/5-troubleshoot/not-responding.md) を確かめます。

</details>

<!-- ※要確認: 段4のコードが実機で動くか / 色の四角を出す GettingStarted サンプルと同じ構成で、コンパイルは確かめた。HMD のある環境で、ダッシュボードにタブと四角が出るかを確かめる -->

**クイズ** 段3で自分が書いた `PostRender` の呼び出しを、フレームワークでは、何が呼んでいるでしょうか。

- (a) `FloatSodaApp.CreateWindow`
- (b) `WidgetBinding.DrawFrame`
- (c) `FramePacer`

<details>
<summary>答え</summary>

(b) です。`WidgetBinding.DrawFrame` が、`RenderView.Layer` を複製して、`PostRender` で描画スレッドに渡します。

</details>

フレームワークの詳細は、[フレームワーク層](framework-layer.md) にあります。

## 次に読むページ

目的に合わせて、続きを選びます。

- 層をまたぐ流れを読む: [1フレームが SteamVR に届くまで](frame-to-steamvr.md)
- 層ごとの解説を読む: [OVR 層](ovr-layer.md)、[描画層](rendering-layer.md)、[エンジン層](engine-layer.md)、[フレームワーク層](framework-layer.md)、[UI 層](ui-layer.md)
- 実験で確かめる: [実験: OVR 層](experiment-ovr.md)、[実験: 描画層](experiment-rendering.md)、[実験: フレームワーク層](experiment-framework.md)
- 開発の準備に進む: [開発環境を用意する(コントリビューター)](../3-prepare/dev-environment.md)
