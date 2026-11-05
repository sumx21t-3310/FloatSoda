← [コントリビューター](../Home.md)

# 実験: 描画層

## 問い: 合成の Layer は、同じ絵に何をするか

同じ絵を `OpacityLayer`、`ClipRectLayer`、`TransformLayer` で1つずつ包み、png を並べて見比べる実験です。[描画層](rendering-layer.md) の Layer の種類の表を読んだあとに進みます。HMD も SteamVR も使いません。

絵は、青い矩形と橙の矩形が重なった1枚です。包まない絵と合わせて、png が4枚できます。

| ファイル | 包み方 |
| :-- | :-- |
| `plain.png` | 包まない |
| `opacity.png` | `OpacityLayer`(`Alpha` は 128) |
| `clip.png` | `ClipRectLayer`(左上の 200×150 だけを残す) |
| `transform.png` | `TransformLayer`(中心まわりに 15 度回す) |

先に予想を立てます。3つの Layer が、それぞれ絵をどう変えるかを考えてください。特に、青と橙が重なる部分が、`OpacityLayer` でどう見えるかを予想します。

## 4通りに包んで png に出す

作例は [samples/Experiments/FloatSoda.Samples.ExperimentRendering](../../../samples/Experiments/FloatSoda.Samples.ExperimentRendering/) にあります。コードの全文は、このページに載せず、作例のファイルを読みます。作例は、同じ絵を4通りに包み、`LayerBitmapRenderer.Render` で `SKBitmap` にして、png で保存します。あわせて、3つの点の色を出力します。

Layer ツリーを組んで png にする基本は、[samples/Showcase/FloatSoda.Samples.PaintingSample](../../../samples/Showcase/FloatSoda.Samples.PaintingSample/) のコードで読めます。この実験は、その流れに、絵を包む Layer を1枚足したものです。

リポジトリのルートで、次のコマンドを実行します。.NET の SDK のバージョンなど、開発の準備は [開発環境を用意する](../3-prepare/dev-environment.md) にあります。

```powershell
dotnet run --project samples/Experiments/FloatSoda.Samples.ExperimentRendering -- <出力先フォルダー>
```

出力先フォルダーは省略できます。省略すると、カレントディレクトリに png を保存します。

**できた?** `plain.png`、`opacity.png`、`clip.png`、`transform.png` の4枚ができた。最後の行に出る保存先のフォルダーに、4枚がある。

<details>
<summary>できなかったら</summary>

SDK が見つからない、またはビルドで例外が出る場合は、[開発環境を用意する](../3-prepare/dev-environment.md) で SDK と clone の状態を確かめます。png が空になる、サイズが 0 になる場合は、作例の中で `LayerBitmapRenderer.Render` に渡した大きさと、ルートに子を入れたかを確かめます。仕組みは [描画層](rendering-layer.md) の「CPU の Bitmap に描く」にあります。

</details>

<!-- ※要確認: 作例は #313 で main に入った。引数は #313 の実際の引数に合わせた。保存先の行と点の色の出力は、移す前のコードに基づくので、#313 がマージされたら実物と照らす -->

## png を見比べる

画像ビューアーで4枚を並べて開き、違いを言葉にします。背景は透明なので、ビューアーによって白や黒に見えます。

見る点は3つです。

- 青と橙が重なる部分の濃さ
- 左上の 200×150 より外に、絵が残っているか
- 絵全体が傾いているか

コンソールの出力は、4枚の点の色を数値で示します(このあとに、保存先の行が続きます)。`#80ffa500` は、先頭の 2 桁(`80`)が不透明度で、残りが色です。

```text
plain.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
opacity.png: 青(50,50)=#806495ed 橙(300,230)=#80ffa500 重なり(150,120)=#80ffa500
clip.png: 青(50,50)=#ff6495ed 橙(300,230)=#00000000 重なり(150,120)=#ffffa500
transform.png: 青(50,50)=#ff6495ed 橙(300,230)=#ffffa500 重なり(150,120)=#ffffa500
```

`opacity.png` は、3つの点がどれも不透明度 `80`(10進で 128)です。重なりの点も `80` のままで、重なると濃くなる、ということはありません。`clip.png` は、橙の点(300,230)が透明(`00000000`)になり、(50,50)と(150,120)は残ります。`transform.png` は、3つの点の値が `plain.png` と同じです。回転は、点の値では分かりません。画像を開いて、絵全体が回っていることを目で確かめます。

**できた?** 「まとめた絵に不透明度を掛けると、この例の重なりは濃くならない」「指定した矩形の外は描かれない」「指定した回転が絵全体に掛かる」と言える。

<details>
<summary>できなかったら</summary>

`opacity.png` の重なりの不透明度が `80` より大きい場合は、作例の中で `OpacityLayer` が包んでいる対象が、2つの矩形をまとめた `PictureLayer` になっているかを確かめます。矩形ごとに別の `PictureLayer` にして、それぞれを `OpacityLayer` で包むと、重なりは濃くなります。

</details>

## なぜそうなるか

`OpacityLayer` は、`SaveLayer` で子を一時レイヤーにまとめて描きます。`Restore` のときに、`Alpha` を掛けて合成します。2つの矩形は、先に1枚の絵として描かれるので、重なりの部分は橙で上書きされたままになります。そのあとで、絵全体が半透明になります。クリップと変換は、`Save` と `Restore` の組で子を包みます。クリップは `ClipRect` で描画範囲を絞り、変換は `Concat` で座標系を変えてから、子を描きます。どちらも `Restore` で状態を戻すので、後ろに並ぶ Layer へは影響しません。

自分で Layer を足すときは、`Paint` が何を `Save` して何を `Restore` するかを先に決めます。詳細は、[描画層](rendering-layer.md) の「Layer の種類と、それぞれの役割」にあります。

<!-- ※要確認: transform.png の見た目(回転)は、画像を開いて目で確かめた。点の値では回転を確かめていない / 別の PC と別のビューアーでも同じように見えるか、確かめる -->

## 次に読むもの

- [描画層](rendering-layer.md): Layer の種類と、`Clone` の共有範囲
- [描画が届くまで(4段)](tutorial-render-path.md): 2段目で、自分で組んだ Layer ツリーを png に出す
- [実験: フレームワーク層](experiment-framework.md): `SetState` のあとに描き直される範囲を確かめる

