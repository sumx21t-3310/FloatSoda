# PaintingSample

## これは何か

FloatSoda の3つのツリー(Layer、RenderObject、ウィジェット)を、それぞれ直接組み立てて PNG の画像に書き出す作例です。画面は出さず、デスクトップに画像を3枚保存して終わります。SteamVR も HMD も要りません。

このサンプルは、描画の仕組みを実例で確かめるための作例です。主な読者は、コントリビューターです。利用者向けの公開 API だけでなく、`FloatSoda.Rendering` の Layer や RenderObject を直接使います。

## 使い方

`Program.cs` は、3つの層でそれぞれ図を組み立て、層ごとの描画器で PNG にします。

| 書き出すファイル | 組み立てるもの | 使う描画器 |
|---|---|---|
| `layer_tree_output.png` | `ContainerLayer` と、Skia の描画命令を記録した `PictureLayer` | `LayerBitmapRenderer` |
| `render_tree_output.png` | `RenderPositionedBox` や `RenderFlex` を組んだ RenderObject ツリー | `RenderObjectBitmapRenderer` |
| `widget_tree_output.png` | `Align` や `Flex` を組んだウィジェットツリー | `WidgetBitmapRenderer` |

RenderObject ツリーとウィジェットツリーは、どちらも3つの角丸の四角を横に並べた図を描きます。

`RenderObjectBitmapRenderer` と `WidgetBitmapRenderer` は、`FloatSoda.Testing` にあるヘッドレスの描画器です。テストでも同じものを使います。

## Flutterとの違い

対象外です。Flutter のウィジェットではなく、FloatSoda の内部の層を直接扱う作例です。

## 実行

リポジトリルートで実行します。SteamVR を起動する必要はありません。`--desktop` は持ちません。

```powershell
dotnet run --project samples/Showcase/FloatSoda.Samples.PaintingSample
```

デスクトップに、`layer_tree_output.png`、`render_tree_output.png`、`widget_tree_output.png` の3つのファイルができます。`layer_tree_output.png` は、[PrimitiveOverlay](../FloatSoda.Samples.PrimitiveOverlay) がオーバーレイに貼る画像として使います。

## 関連

- [実験: 描画層](../../../docs/contributor/2-understand/experiment-rendering.md) — Layer ツリーを組んで PNG にする実験
- [描画層](../../../docs/contributor/2-understand/rendering-layer.md) — Layer ツリーの仕組み
- [PrimitiveOverlay](../FloatSoda.Samples.PrimitiveOverlay) — 書き出した画像をオーバーレイに出す
