# Layer を足す

このページは、描画の層(`FloatSoda.Rendering`)でよく行う2つの作業、Layer を足す作業と、描画結果をビットマップで確かめる作業の手順です。Layer がどう動くかは [描画の仕組みと理由](../explanation/Rendering.md) に、変更の前後で守る条件は [描画で守ること](../reference/Rendering.md) にあります。

| 知りたいこと | 置き場所 |
|---|---|
| ブランチ名、PR のスコープ、テストの命名と観点 | [CONTRIBUTING.md](../../../CONTRIBUTING.md) |
| Layer の不変条件(Clone) | [REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」 |
| XML ドキュメントコメントの書き方 | [DocumentationComments](../DocumentationComments.md) |
| Flutter に合わせるか、差異にするかの判断 | [APIDesign](../APIDesign.md) |
| 初めて Layer を足すときの一本道 | [Layer を1つ足して PR を出す](../tutorials/ShaderMaskLayer.md) |

## Layer を足す

### 規約

- 新しい Layer は `src/FloatSoda.Rendering/Layers/` に置き、名前空間を `FloatSoda.Rendering.Layers` にすること(CONTRIBUTING.md の「namespace とディレクトリ」)
- 子に効果をかける Layer は `ContainerLayer` を継承し、`Paint` で `base.Paint(context)` を呼んで子を描くこと
- `Paint` で `Save` か `SaveLayer` を呼んだら、戻る前に同じ数だけ `Restore` を呼ぶこと
- `Clone()` を override し、足したプロパティをすべて写し、`Children` の各要素の `Clone()` を同じ順で加えること(REVIEW.md の「Layer」)
- `SKPath` や `SKShader` のようなネイティブの描画リソースを持たせるときは、所有権が Layer へ移らないことを XML ドキュメントコメントに書くこと(既存の `ClipPathLayer.ClipPath` と `PictureLayer.Picture` と同じ形)
- Flutter に同じ役割の Layer があれば、名前とプロパティを Flutter の `layer.dart` に合わせ、違えるときは [APIDesign](../APIDesign.md) の記録ルールに従うこと
- テストは `tests/FloatSoda.Rendering.Test/Layers/` に置き、描画結果と `Clone()` の両方を確かめること

### 手順

1. Flutter の `packages/flutter/lib/src/rendering/layer.dart` で、同じ役割の Layer のプロパティと `addToScene` を読む(ローカルの参照用クローンは CONTRIBUTING.md の「テスト観点」)
2. `src/FloatSoda.Rendering/Layers/` に Layer のファイルを作り、`ContainerLayer` を継承する
3. Flutter の対応する Widget の既定値を確かめてから、プロパティを足す
4. 描画範囲が子と変わる Layer だけ、`Layout` を override して `PaintBounds` を設定する
5. `Paint` を override し、`Save` か `SaveLayer` で効果をかけ、`base.Paint(context)` で子を描き、`Restore` で戻す
6. `Clone()` を override し、新しいインスタンスに全プロパティを写して、子を1つずつ `Clone()` して加える
7. 描画結果のテスト(下の「描画結果をビットマップで確かめる」)と、`Clone()` のプロパティが元と等しく子が別のインスタンスであることのテストを書く
8. `Clone()` から1つのプロパティの代入を消し、7のテストが落ちることを確かめてから戻す
9. RenderObject からこの Layer を使うなら、フレームワークの層の `PaintingContext` に、`oldLayer` 引数で前回の Layer を使い回せる `PushXxx` メソッドを足す(`PushOpacity` と同じ形)
10. CONTRIBUTING.md の「ブランチ・PRフロー」にある、CI と同じコマンドを実行する

成功すると、手順10のすべてのコマンドが失敗 0 で終わります。

## 描画結果をビットマップで確かめる

### 規約

- 描画の層のテストでは、OpenGL と SteamVR を使わずに、`LayerBitmapRenderer` で CPU 上のビットマップへ描いて確かめること
- 判定は、ピクセルの色か不透明度を `GetPixel` で読んで行うこと
- アンチエイリアスや半透明で値が揺れる箇所は、`Assert.InRange` で幅を持たせることが望ましい(既存の `OpacityLayer_AppliesAlpha` と同じ形)
- `LayerBitmapRenderer.Render` が返す `SKBitmap` は、呼び出し元が `using` で破棄すること

### 手順

1. `SKPictureRecorder` で四角を描き、`EndRecording()` の戻り値を `Picture` に入れた `PictureLayer` を作る
2. 確かめたい Layer を作り、1の `PictureLayer` を `Children` に加える
3. `new LayerBitmapRenderer().Render(layer, new SKSizeI(100, 100))` で、ビットマップに描く
4. `bitmap.GetPixel(x, y)` で、効果がかかるはずの位置と、かからないはずの位置の両方を読み、`Assert` で判定する
5. 目で見て確かめたいときは、親ディレクトリのある保存先へ `bitmap.Save("out.png")`(`SKBitmapExtensions.Save`)で PNG に書き出す

成功すると、手順4の判定が通り、手順5の PNG に期待どおりの絵が出ます。手順1から4の実例は [`tests/FloatSoda.Rendering.Test/Layers/LayerTest.cs`](../../../tests/FloatSoda.Rendering.Test/Layers/LayerTest.cs) にあります。Layer、RenderObject、Widget の3段階をそれぞれ PNG に書き出す例は、[`samples/FloatSoda.Samples.PaintingSample/Program.cs`](../../../samples/FloatSoda.Samples.PaintingSample/Program.cs) にあります。

RenderObject や Widget から描いて確かめるときは、`FloatSoda.Testing` の `RenderObjectBitmapRenderer` と `WidgetBitmapRenderer` を使います。こちらはフレームワーク(三つのツリー)の層で扱います。

## 規約が守られる場所

- 描画結果と `OpacityLayer.Clone()` の回帰は、CI(`.github/workflows/ci.yml`)の `FloatSoda.Rendering.Test` で検出する
- ほかの Layer の `Clone()` の写し漏れと、Clone 後の可変オブジェクトの共有は、固定するテストがないため、レビューで REVIEW.md の「Layer」に沿って確認する
- 名前空間とディレクトリの一致の一部(ファイルスコープ名前空間)は、`.editorconfig` の `IDE0161` でビルド時に検査する

## 関連

- [Layer を1つ足して PR を出す](../tutorials/ShaderMaskLayer.md) — 実在の Layer を1つ足して PR を出すまで
- [描画で守ること](../reference/Rendering.md) — 検証方法と、未設定の条件
- [描画の仕組みと理由](../explanation/Rendering.md) — Clone してから渡す理由
