# RenderPath

## これは何か

描画が SteamVR のオーバーレイに届くまでを、層を1つずつ足しながら確かめるチュートリアルのコードです。1つのプロジェクトに4段を入れ、`--step 1` から `--step 4` で段を選びます。コントリビューター向け docs「描画が届くまで(4段)」のコードの実体です。

| 段 | 足す層 | 目に見える結果 | SteamVR と HMD |
| :-- | :-- | :-- | :-- |
| 1 | OVR 層 | HMD の 1 m 前のオーバーレイに、png の絵が出る | 要る |
| 2 | 描画層 | 自分で組んだ Layer ツリーが、png の画像になる | 要らない |
| 3 | エンジン層 | 同じ Layer ツリーが、GL テクスチャ経由でオーバーレイに出る | 要る |
| 4 | フレームワーク層 | ウィジェットで書いた色の四角が、ダッシュボードに出る | 要る |

## 使い方

各段のコードは、`Step1.cs` から `Step4.cs` にあります。`Program.cs` は、`--step` の値で段を選びます。

- 段1(`Step1.cs`): `OVRApplication`、`DeviceTrackedOverlay`、`Texture.FromFile` で、png を HMD の 1 m 前に出す。描いているのは OpenVR
- 段2(`Step2.cs`): `SKPictureRecorder` で描画を記録し、`PictureLayer` と `ContainerLayer` で Layer ツリーを組み、`LayerBitmapRenderer.Render` で png にする
- 段3(`Step3.cs`): `RenderPostTaskRunner`、`GLView`、`Renderer`、`OverlayWindow` で、同じ Layer ツリーを GL テクスチャとしてオーバーレイに出す。後始末は省いている
- 段4(`Step4.cs`): `FloatSodaApp` と `DashboardWindow` で、色の四角のウィジェットをダッシュボードに出す。段3で手書きした処理を、フレームワークが受け持つ

段1が出す png は、段2の出力をそのまま使えます。段1と段2の png のパスの既定は、カレントディレクトリの `my_layer_tree.png` で、第1引数で変えられます。

## Flutterとの違い

対象外です。Flutter のウィジェットを移植したものではなく、FloatSoda の層構造を確かめるチュートリアルです。

## 実行

段2を先に実行して、png を作ります。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 2 <png の保存先>
```

SteamVR を起動してから、段1、3、4を実行します。HMD の中で、オーバーレイまたはダッシュボードを探します。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 1 <段2が作った png のパス>
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 3
dotnet run --project samples/Experiments/FloatSoda.Samples.RenderPath -- --step 4
```

段1と段3は、Enter キーで終了します。

## 関連

- コントリビューター向け docs「描画が届くまで(4段)」
- `samples/Showcase/FloatSoda.Samples.PrimitiveOverlay`: OVR 層だけでオーバーレイと入力を扱う
- `samples/Showcase/FloatSoda.Samples.PaintingSample`: 段2と同じ仕組みで、3つのツリーを png に出す
