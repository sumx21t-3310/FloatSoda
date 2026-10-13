# フレームワーク(三つのツリー): 守ること

三つのツリーで、実装を変えても維持する条件をまとめます。仕組みの説明は [三つのツリーの Explanation](Explanation.md) にあります。

ツリーの所有権・ライフサイクル・差分更新の条件は、[REVIEW.md](../../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」に置いてあります。このページでは複製せず、4 章のどの条件がどこに効くかを示し、4 章にない条件だけを書きます。

| 4 章の節 | 効く対象 |
|---|---|
| ツリーの所有権とライフサイクル | 親子のつながり(adopt / drop)、パイプラインへの接続(attach / detach)、Element の `Mount` / `Update` / 破棄、`State.Dispose`、`Key` による使い回し |
| 差分更新 | `MarkNeedsBuild` と `BuildScope` の順序、`MarkNeedsLayout` / `MarkNeedsPaint` の伝播と境界の登録、不要な再ビルド・再レイアウト・再描画 |

## 適用範囲

`FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture` の名前空間にある型と、それを継承する型に効きます。組み込みの RenderObject は `src/FloatSoda/RenderObjects/` の下にあります。

## 必須条件

### RenderObject のレイアウト

- `Layout` をオーバーライドせず、サブクラスでは `PerformLayout` を実装すること
- `PerformLayout` は、`Constraints` の範囲に収まる `Size` を決めること(`Constraints.Constrain` を使う)
- 子のサイズを使って自分のサイズか子の位置を決めるときは、子の `Layout` に `parentUseSize: true` を渡すこと
- intrinsic 測定を受け付ける型は、`ComputeMinIntrinsicWidth` などの4つを実装すること

### RenderObject の描画

- `Paint(PaintingContext, Offset)` は、受け取った `offset` を原点にして `context.Canvas` へ記録すること
- クリップと不透明度は、`context.PushClipRect` や `context.PushOpacity` などで子の Layer に分けること
- Flutter から移植するときは、Semantics のフック(`markNeedsSemanticsUpdate` など)を削ること([APIDesign](../../APIDesign.md) の「実装しない API — Semantics」)

### RenderObject のヒットテスト

- 自分の `Size` の外の座標では、`false` を返すこと
- 子を調べるときは、描画で子に渡したのと同じ offset か変換を、`HitTestResult.AddWidthPaintOffset` か `AddWithPaintTransform` で座標に当てること
- 複数の子を持つときは、手前に描いた子から調べ、最初に当たった子で止めること
- 自分が当たったときは、子を足したあとで自分を `HitTestResult` に足すこと

### 外部への購読

- RenderObject が外部のイベントを購読するときは、`Attach` で購読し、`Detach` で解除すること(例: `RenderAnimatedOpacity` の `IAnimation<double>.Changed`)

## 不変条件

- `HitTestResult.Path` は、奥の RenderObject から手前の祖先へ向かう順に並び、末尾は `RenderView`(`WidgetBinding` を経由した場合は、さらにその後ろに `WidgetBinding`)である
- 1つの `State<T>` の `Dispose` は、多くても1回しか呼ばれない
- `Key` 付きの子を並べ替えたあと、子の RenderObject の並びは、子の Widget の並びと同じである
- 子の並びが変わらない更新では、RenderObject の移動もレイアウトの要求も起きない

## Observable behavior

- 指を押したまま領域の外へ出ても、`Move` と `Up` は押し始めた対象へ届く
- ポインタが止まっていても、下の Widget がレイアウトで動けば、`Listener` と `PointerRegion` に `Enter` と `Exit` が届く
- 認識器が1つだけのとき、`Down` の配送が終わった時点でその認識器が勝つ
- 誰も勝利を宣言しないまま `Up` が届くと、最初に参加した認識器が勝つ
- intrinsic 測定を実装していない `RenderBox` に問い合わせると、`NotSupportedException` が出る

## 破ったときの failure mode

| 破った条件 | 起きること |
|---|---|
| 子のサイズを使うのに `parentUseSize: true` を渡さない | 子が relayout boundary になる。子のサイズが変わっても `MarkNeedsLayout` が親へ伝わらず、親のレイアウトが古いまま残る |
| ヒットテストで子に当てる offset が、描画の offset と違う | 見えている位置と押せる位置がずれる。`Paint` だけを見るテストでは見つからない |
| 子を奥から調べる、または当たっても止めない | 重なった部分で、背後の子がイベントを受け取る |
| `Detach` で購読を解除しない | ツリーから外した RenderObject に通知が届き続け、`MarkNeedsPaint` が呼ばれる。購読元が生きているあいだ、RenderObject が回収されない |
| Semantics のフックを残す | 呼び出し先がないため、ビルドが通らないか、何もしないコードが残る |

## 検証方法

| 条件 | 固定しているテスト |
|---|---|
| ヒットの並びが奥から手前 | `tests/FloatSoda.Test/RenderObjects/HitTestTest.cs` の `HitPath_IsOrderedInnermostFirst` |
| 子が描画の offset の位置で当たる | 同じファイルの `FlexChild_IsHit_AtItsPaintOffset`、`FlexChild_IsNotHit_OutsideItsBounds` |
| 押したまま外へ出たあとも、次のタップが効く | `tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs` の `LeaveWhilePressed_DispatchesCancelAndExit_ThenNextTapStillWorks` |
| アリーナの勝敗の決まり方 | `tests/FloatSoda.Test/Gesture/GestureArenaTest.cs` の `SoleMember_WinsOnClose`、`FirstMember_WinsOnSweep_WhenUndecided` など |
| `State.Dispose` が1回だけ、ツリーから外したときに呼ばれる | `tests/FloatSoda.Test/Elements/StateDisposeTest.cs` の `Dispose_CalledOnce_PerState`、`Dispose_Called_WhenRemovedFromTree` |
| 子の RenderObject の並びと slot | `tests/FloatSoda.Test/Elements/RenderObjectSlotTest.cs` |
| intrinsic 測定を実装していない型が例外を出す | `tests/FloatSoda.Test/RenderObjects/IntrinsicLayoutTest.cs` の `GetIntrinsicDimension_未実装RenderBox_NotSupportedExceptionを投げる` |
| `parentUseSize` の渡し忘れ | 未設定 |
| `Detach` での購読の解除 | 未設定 |

## Flutter との関係

ここに挙げた条件は、Flutter の `RenderObject` と `RenderBox` の契約と同じ側に立ちます。Flutter と違う挙動は、台帳([`known-divergences.md`](../../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md))の次のエントリにあります。

| 条件に関わる差異 | 台帳 |
|---|---|
| 同じ位置の Widget の比べ方 | #1 |
| 外した Element を再利用しないため、`Key` 付きの部分木を別の親へ移すと `State` が作り直される | #2 |
| relayout boundary だけが変わったときの `Layout` の扱い | #5 |
| ポインタ入力をフレームの境目でまとめて配ること | #7 |

## 関連

- [REVIEW.md](../../../../REVIEW.md) の「4. FloatSoda 固有の不変条件」 — 所有権・ライフサイクル・差分更新・Layer の条件
- [三つのツリーの Explanation](Explanation.md) — 条件の背景にある仕組み
- [三つのツリーの HowTo](HowTo.md) — 移植とテストの手順
- [コアの Reference](../core/Reference.md) — `RenderPipeline` で守る条件
