# ExperimentRendering

## これは何か

同じ絵を `OpacityLayer`、`ClipRectLayer`、`TransformLayer` で1つずつ包み、png を並べて見比べる実験です。絵は、青い矩形と橙の矩形が重なった1枚です。包まない絵と合わせて、png が4枚できます。HMD も SteamVR も使いません。

コントリビューター向け docs「実験: 描画層」のコードの実体です。

| ファイル | 包み方 |
| :-- | :-- |
| `plain.png` | 包まない |
| `opacity.png` | `OpacityLayer`(`Alpha` は 128) |
| `clip.png` | `ClipRectLayer`(左上の 200×150 だけを残す) |
| `transform.png` | `TransformLayer`(中心まわりに 15 度回す) |

## 使い方

`Program.cs` の `Save` が、絵を包んでルートの `ContainerLayer` に入れ、`LayerBitmapRenderer.Render` で `SKBitmap` にして png に保存します。`PictureLayer` は `SKPicture` の所有権を持たないので、保存が終わるまで `Save` が絵を保持します。

出力の見方は次のとおりです。

- 3つの点(青、橙、重なり)の色が1行ずつ出る。`#80ffa500` は、先頭の 2 桁(`80`)が不透明度
- `opacity.png` は、3つの点がどれも不透明度 `80`。重なりは濃くならない
- `clip.png` は、橙の点 (300,230) が透明 (`00000000`) になる
- `transform.png` は、点の値が `plain.png` と同じ。回転は画像を開いて目で確かめる

```text
plain.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
opacity.png: 青(50,50)=#806495ed 橙(300,230)=#80ffa500 重なり(150,120)=#80ffa500
clip.png: 青(50,50)=#ff6495ed 橙(300,230)=#00000000 重なり(150,120)=#ffffa500
transform.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
```

## Flutterとの違い

対象外です。Flutter のウィジェットを移植したものではなく、FloatSoda の描画層の Layer を確かめる実験です。

## 実行

png の出力先は第1引数で指定します。省略すると、カレントディレクトリに出力します。SteamVR も HMD も要りません。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentRendering -- <出力先のフォルダー>
```

## 関連

- コントリビューター向け docs「実験: 描画層」
- `samples/Showcase/FloatSoda.Samples.PaintingSample`: Layer ツリー、RenderObject ツリー、ウィジェットのツリーを png に出す
- `samples/Experiments/FloatSoda.Samples.RenderPath`: 段2で Layer ツリーを png に出す
