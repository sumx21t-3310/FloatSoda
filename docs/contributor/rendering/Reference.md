# 描画: 守ること

このページは、描画の層(`FloatSoda.Rendering`)を変えても維持する条件の一覧です。変更が何を壊しうるかを確かめるときと、レビューで確認するときに使います。条件がなぜ要るかは [描画: 仕組みと理由](Explanation.md) にあります。

## 適用範囲

- `ILayer` を実装するすべての型(一覧の一次情報は [`src/FloatSoda.Rendering/Layers/`](../../../src/FloatSoda.Rendering/Layers))
- Layer ツリーを描く `LayerRenderer` と `LayerBitmapRenderer`
- Layer ツリーを組み立てる側(`FloatSoda` の `PaintingContext`)のうち、Layer と共有するオブジェクトの扱い

## 必須条件

Layer の Clone についての条件は、[REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」の「Layer」が一次情報です。ここには複製せず、要点だけを示します。

- Layer 型の `Clone()` は、その型に足したプロパティをすべて写すこと(REVIEW.md の「Layer」)
- Clone 後に、メインスレッド側と可変オブジェクトを共有しないこと(REVIEW.md の「Layer」)

REVIEW.md は、共有しないオブジェクトの例に `SKPicture` を挙げています。一方、`PictureLayer.Clone()` は `SKPicture` の参照を共有する実装で、XML ドキュメントコメントにもそう書かれています。どちらに合わせるかは ※要確認 です。このページの以下の条件は、いまの実装(`SKPicture` を共有する)に合わせて書いています。

REVIEW.md にない条件は次のとおりです。

- `ContainerLayer` を継承する Layer の `Clone()` は、`Children` の各要素の `Clone()` を同じ順で新しい Layer に加えること
- Clone で参照を共有するオブジェクト(`SKPicture`、`SKPath`、`SKRoundRect`)は、所有権が Layer へ移らないため(各プロパティの XML ドキュメントコメント)、Layer に渡したあと、渡した側が書き換えも `Dispose` もしないこと
- 合成効果をかける Layer の `Paint` は、`Save` か `SaveLayer` の数だけ `Restore` を呼んでから戻ること
- レンダースレッドで描かれる Layer がメインスレッドの RenderObject や Widget に触れられないよう、`FloatSoda.Rendering` は SkiaSharp 以外を参照しないこと

## 不変条件

- Clone した Layer ツリーが元の Layer ツリーと共有するのは、上の必須条件で挙げた描画リソースだけである
- `LayerRenderer.Render` は、ルートの `Layout` を終えてから `Paint` を呼ぶので、`Paint` の時点ですべての Layer の `PaintBounds` が計算済みである
- 描画先のクリア・フラッシュ・破棄は、描画の層ではなく呼び出し元(`FloatSoda.Engine` の `Renderer`)が行う

## Observable behavior

テストから観測できる契約です。型とメンバーごとの契約(引数、例外)は XML ドキュメントコメントにあります。

- `ContainerLayer` は、`Children` の先頭から順に描き、後の子を前の子の上に重ねる
- 合成効果の Layer が描いたあと、クリップ・変換・不透明度は後続の Layer へ引き継がれない
- Clone した Layer は、元の Layer と同じ描画結果になる
- `LayerBitmapRenderer.Render` は、透明で初期化した新しい `SKBitmap` を返し、その破棄は呼び出し元が行う

## 破ったときの failure mode

| 破った条件 | 起きること |
|---|---|
| `Clone()` が新しいプロパティを写さない | レンダースレッドの Layer が既定値で描かれる。たとえば `OpacityLayer.Alpha` を写さないと、半透明のはずの子が不透明で表示される |
| Clone 後に子のリストを共有する | メインスレッドの次のフレームの `Children.Clear()` と、レンダースレッドの描画が重なる。例外でフレームが失われるか、書き換え途中のツリーが描かれる。REVIEW.md のとおり、テストではまず落ちない |
| 共有した `SKPicture` や `SKPath` を、渡した側が `Dispose` する | レンダースレッドが破棄済みのネイティブオブジェクトを描こうとする。どう壊れるかは ※要確認 |
| `Paint` の `Save` と `Restore` の数が合わない | 後続の Layer にクリップや変換が残り、兄弟の Layer が切り取られたりずれたりする |

## 検証方法

| 条件 | 固定しているテスト |
|---|---|
| 各 Layer の描画結果 | [`tests/FloatSoda.Rendering.Test/Layers/LayerTest.cs`](../../../tests/FloatSoda.Rendering.Test/Layers/LayerTest.cs) の `PictureLayer_PaintsColor`、`ContainerLayer_PaintsAllChildren`、`ClipRectLayer_ClipsToRect` など |
| `OpacityLayer.Clone()` が `Alpha` を写す | 同じファイルの `OpacityLayer_Clone_PreservesAlpha`(回帰テスト) |
| ほかの Layer の `Clone()` がプロパティを写す | 未設定 |
| Clone したツリーが元のツリーとオブジェクトを共有しない | 未設定 |
| 合成効果の後で、キャンバスの状態が後続へ引き継がれない | 未設定 |

未設定の条件は、レビューで確認します(REVIEW.md の「Layer」)。

## Flutter との関係

Layer の種類と名前は、Flutter の `layer.dart` に合わせています。Flutter には `Clone()` がなく、`addToScene` でエンジン側のシーンを組み立てて渡します。Clone に関する条件は FloatSoda 独自のもので、[`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) のエントリはありません。

## 関連

- [描画: 仕組みと理由](Explanation.md) — Clone してから渡す理由
- [描画: 作業の手順](HowTo.md) — Layer を足すときの手順
- [REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」
