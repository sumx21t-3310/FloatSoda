# コアで守ること

コアの層で、実装を変えても維持する条件をまとめます。対象は、dirty な RenderObject を消化する `RenderPipeline` と、`Geometrics` などのジオメトリの値型の2つです。仕組みの説明は [コアの仕組みと理由](../explanation/Core.md) にあります。

`MarkNeedsLayout` / `MarkNeedsPaint` の伝播と境界ノードの登録の条件は、[REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」の「差分更新」に置いてあります。このページでは複製せず、4 章にない条件だけを書きます。

> ※要確認 — `RenderPipeline` の条件は、現在の実装と XML ドキュメントコメントから読み取ったものです。固定するテストがなく、契約として採るかはまだ決まっていません。

## 適用範囲

| 対象 | 効く範囲 |
|---|---|
| `RenderPipeline` | `src/FloatSoda/Core/RenderPipeline.cs`。`FlushLayout`、`FlushPaint`、`RequestVisualUpdate` と、2つの dirty リスト |
| ジオメトリの値型 | 公開 API に現れる座標・サイズ・余白・角度などの値型。`src/FloatSoda/Geometrics/`、`src/FloatSoda.Abstractions/Geometries/` などに置く型 |

## 必須条件

### RenderPipeline

- `FlushLayout` は、`NodesNeedingLayout` を `Depth` の小さい順(親が先)に処理すること
- `FlushLayout` は、処理中に登録されたノードも、リストが空になるまで同じ呼び出しの中で処理すること
- `FlushPaint` は、開始時点の `NodesNeedingPaint` だけを処理し、処理中に登録されたノードは次の `FlushPaint` に回すこと
- `FlushLayout` と `FlushPaint` は、もう dirty でないノードと、`Owner` がこのパイプラインでないノードを処理しないこと
- `RequestVisualUpdate` は、次のフレームの更新を予約するだけで、レイアウトも描画も実行しないこと
- `RenderView` を設定したら、その `RenderView` をこのパイプラインへ接続(`Attach`)すること

### ジオメトリの値型

- `readonly record struct` で定義すること([APIDesign](../APIDesign.md) の「8. ジオメトリオブジェクトには `record struct` を使う」)
- 公開 API の実数は `double` にし、`float` は SkiaSharp・OpenVR・OpenGL へ渡す境界でだけ使うこと(APIDesign の「8.5」)
- SkiaSharp の型を公開 API に出さないこと(APIDesign の「8.5」)
- 自分と同じ型を返す計算プロパティを持つ型には、`PrintMembers` を書くこと(APIDesign の「自分と同じ型を返す計算プロパティには `PrintMembers` を書く」)
- プライマリコンストラクタ以外の作り方は、静的ファクトリメソッドで用意し、APIDesign の「7.1 命名規則」に従うこと
- 単位付きの拡張プロパティを足すときは、レシーバーを `double` と `int` の両方にすること(APIDesign の「7.5」)

既存の型には、`float` を公開している型(`Alignment`、`Angle`、`Dpm`)が残っています。解消は追跡: [#131](https://github.com/sumx21t-3310/FloatSoda/issues/131) で進めます。新しく足す型は上の条件に従います。

## 不変条件

- `FlushLayout` から戻った時点で、`NodesNeedingLayout` は空である
- 同じ値を持つジオメトリの値型の2つのインスタンスは、`==` で等しい
- ジオメトリの値型の `ToString()` は、どの値に対しても終了する

## Observable behavior

- Widget にも RenderObject にも変更がないフレームでは、レイアウトも描画も実行されない([BuildPipeline](../BuildPipeline.md) の「WidgetBinding.DrawFrame」)
- 最初のフレームでは、`RenderView.PrepareInitialFrame` によって、ツリー全体がレイアウトされ描画される
- ツリーから外したノードは、外す前に dirty リストへ登録されていても、レイアウトも描画もされない
- ジオメトリの値型は、`with` 式で一部だけ変えた新しい値を作れる

## 破ったときの failure mode

| 破った条件 | 起きること |
|---|---|
| `Owner` が別のノードを飛ばさない | ツリーから外したノードのレイアウトと描画が走る。REVIEW.md の「attach / detach」が見る「detach 済みのノードが dirty リストに残る」状態が、そのまま描画に出る |
| `RequestVisualUpdate` を呼ばない | dirty リストには登録されるが、`NeedsVisualUpdate` が立たず、`DrawFrame` が `FlushLayout` / `FlushPaint` を呼ばない。ほかの変更が来るまで画面が更新されない |
| ジオメトリの値型を `class` にする | 等しい値を新しく作るたびに、参照が違うため `==` が `false` になる。Widget の record 等値比較の高速パス(`Element.UpdateChild`)が一致せず、変わっていない Widget でも `UpdateRenderObject` が毎回走る |
| `PrintMembers` を書かずに、自分と同じ型を返す計算プロパティを足す | `ToString()` が無限に再帰する。`record struct` ではスタックオーバーフローでプロセスごと終了し、`catch` できない。`$"{padding}"` やログ出力の1行で起きる |

## 検証方法

| 条件 | 固定しているテスト |
|---|---|
| `PrintMembers` の書き忘れ | `tests/FloatSoda.Test/RecordToStringTest.cs` の `PrintMembers_自分と同じ系統のrecordを返す計算プロパティを持つ_自動生成のままにしていない` |
| 境界の内側だけが描き直される | `tests/FloatSoda.Test/Widgets/RepaintBoundaryTest.cs` の `MarkNeedsPaint_境界内の子がDirty_境界内だけを再描画する` |
| 子のサイズ変更が `MarkNeedsLayout` 経由でルートまで届く | `tests/FloatSoda.Test/RenderObjects/RenderViewTest.cs` の `Size_FollowsChildSizeChange_ViaMarkNeedsLayout` |
| `FlushLayout` / `FlushPaint` の処理順と、処理中に登録されたノードの扱い | 未設定 |
| `Owner` が別のノードを飛ばす | 未設定 |
| ジオメトリの値型が `readonly record struct` で、`double` を使い、Skia 型を出さない | 未設定。レビューで確かめる |

## Flutter との関係

| 条件 | Flutter |
|---|---|
| `FlushLayout` の `Depth` 順と、空になるまでの繰り返し | `PipelineOwner.flushLayout` と同じく、浅い順に処理し、空になるまで繰り返す。Flutter は、処理の途中で新しいノードが登録されると、残りと合わせて並べ直す点が違う |
| `FlushPaint` の処理順 | Flutter の `PipelineOwner.flushPaint` は深い順(子が先)に処理する。FloatSoda は浅い順で、逆になっている。観測できる違いがあるかは ※要確認 で、台帳には未記載 |
| `FlushPaint` 中に登録されたノード | Flutter は、`flushPaint` の終了時にリストが空であることを assert で確かめる。FloatSoda は次の `FlushPaint` に回す |
| ジオメトリの値型を `record struct` で書くこと | C# の表現の違いで、挙動の差異ではない([REVIEW.md](../../../REVIEW.md) の「6. Flutter parity」) |

Flutter の参照先は `packages/flutter/lib/src/rendering/object.dart` の `flushLayout` と `flushPaint` です(ローカル参照クローンの `8a9f61cfd67`)。

## 関連

- [コアの仕組みと理由](../explanation/Core.md) — `RenderPipeline` と `WidgetBinding` がどう動くか
- [ジオメトリの値型を足す](../how-to/AddValueType.md) — 値型を足す手順
- [REVIEW.md](../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」 — ツリーの所有権・ライフサイクル・差分更新・Layer の条件
- [APIDesign](../APIDesign.md) — 値型の規約の理由と例
