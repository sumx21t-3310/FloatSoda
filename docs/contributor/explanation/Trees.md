# 三つのツリーの仕組みと理由

FloatSoda は、Flutter と同じ Widget・Element・RenderObject の三つのツリーで UI を組み立てます。Widget は毎回作り直す設計図、Element はフレームをまたいで残るノード、RenderObject はレイアウトと描画とヒットテストを担うノードです。このページでは、Widget の変更が RenderObject に届くまでの流れ、レイアウトと描画の差分更新、ポインタ入力がジェスチャになるまでの流れを説明します。フレームの1周の中でこれらがいつ呼ばれるかは [コアの仕組みと理由](Core.md) にあります。

## 実装状況

Widget・Element・RenderObject の基盤と、差分ビルド、差分レイアウト・差分ペイント、ヒットテストとジェスチャ認識は動いています。動いていない部分は次のとおりです。

| 対象 | 現状 |
|---|---|
| スクロール系の Widget(`ListView`、`GridView`、`SingleChildScrollView`) | `internal` で、公開 API から使えない(追跡: [#69](https://github.com/sumx21t-3310/FloatSoda/issues/69)) |
| `WorldSpaceWindow` と `DeviceTrackedWindow` のポインタ入力 | 入力源がつながっておらず、ヒットテストに座標が届かない(追跡: [#182](https://github.com/sumx21t-3310/FloatSoda/issues/182)) |
| `FloatSoda.Hooks` | `HookWidget` と `HookElement` だけがあり、ビルドループとつながっていない。`HookExtension` の各メソッドは `NotImplementedException` を投げる(追跡: [#72](https://github.com/sumx21t-3310/FloatSoda/issues/72)) |
| `GlobalKey` と、外した Element の再利用 | ない(台帳の #2、#3) |

台帳は、Flutter との差異を記録した [`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) です。

## 責務と境界

三つのツリーは、それぞれ次のことを担います。

| ツリー | 担うこと | 担わないこと |
|---|---|---|
| Widget | UI の設計図を、不変の値として表す | 状態を持つこと、レイアウトと描画 |
| Element | Widget と RenderObject を結び付け、フレームをまたいで残る。`State` の寿命を管理する | レイアウトと描画 |
| RenderObject | レイアウト(`PerformLayout`)、描画の記録(`Paint`)、ヒットテスト | Widget の差分の判定 |

この層が担わないのは、フレームの中で各段を呼ぶ順序(コア)と、記録された Layer ツリーを GL へ描くこと(描画)です。

## 関係するコンポーネント

| 型 | ソース | 役割 |
|---|---|---|
| `Widget` | `src/FloatSoda/Widgets/Widget.cs` | すべての Widget の基底。`abstract record` で、`CreateElement()` と `Key` を持つ |
| `Element` | `src/FloatSoda/Elements/Element.cs` | `Mount`、`UpdateChild`、`MarkNeedsBuild`、`Rebuild` を持つノード |
| `BuildOwner` | `src/FloatSoda/Elements/BuildOwner.cs` | dirty な Element のリストを持ち、`BuildScope` で再ビルドする |
| `RenderObjectElement<T>` | `src/FloatSoda/Elements/RenderObjectElement.cs` | RenderObject を作って祖先の RenderObject へ挿入し、子リストの差分を取る |
| `RenderObject`、`RenderBox` | `src/FloatSoda/RenderObjects/` | レイアウト・描画・ヒットテストの基底 |
| `HitTestResult`、`HitTestEntry` | `src/FloatSoda/Gesture/IHitTestTarget.cs` | ヒットした RenderObject の並びと、座標の変換行列 |
| `GestureRecognizer`、`GestureArenaManager`、`PointerRouter` | `src/FloatSoda/Gesture/` | ポインタイベントの列をタップやパンとして解釈する |
| `WidgetBitmapRenderer`、`RenderObjectBitmapRenderer` | `src/FloatSoda.Testing/` | ツリーをヘッドレスでビットマップへ描く |

Widget の基底クラスごとの対応する Element は、[WidgetSystem](../WidgetSystem.md) の「Widget の階層」にあります。組み込みの Widget と RenderObject は、`src/FloatSoda/Widgets/` と `src/FloatSoda/RenderObjects/` の下にあります。

## 処理の流れ

### Widget の変更が RenderObject に届くまで

再ビルドは、`Element.MarkNeedsBuild()` から始まります。`State.SetState` も、内部でこのメソッドを呼びます。

1. `MarkNeedsBuild()` が Element を dirty にして `BuildOwner.ScheduledBuildFor` で dirty リストへ登録し、空のリストへの最初の登録のときに `WidgetBinding.EnsureVisualUpdate` を呼ぶ
2. 次のフレームの `BuildOwner.BuildScope()` が、dirty リストを `Depth` の小さい順(親が先)に `Rebuild()` し、ビルド中に dirty になった Element も並べ直して同じフレームで処理する
3. `StatelessElement` などは、`Build()` が返した Widget を `UpdateChild` で子に適用する
4. RenderObject を持つ Element は、`UpdateRenderObject` で既存の RenderObject のプロパティを書き換える
5. プロパティの setter が `MarkNeedsLayout` か `MarkNeedsPaint` を呼び、RenderObject 側の差分更新につながる

`UpdateChild(child, newWidget)` は、次の順に判定します。

| 条件 | 動作 |
|---|---|
| `newWidget` が `null` | 子を外して破棄する(`DeactivateChild`) |
| 子がいない | `InflateWidget` で新しい Element を作って `Mount` する |
| `child.Widget == newWidget`(record の等値比較) | 何もしない |
| `Widget.CanUpdate(old, new)`(実行時型と `Key` が等しい) | `child.Update(newWidget)` で既存の Element を使い回す |
| それ以外 | 子を破棄して、新しく作る |

`==` の行は、プロパティがすべて等しい Widget を飛ばす高速パスです。Flutter の同じ位置の判定は同一性(同じインスタンスか)で、FloatSoda は構造の等しさで判定します(台帳の #1)。

### 子の RenderObject を挿入する位置(slot)

親の Element は、子の Element に slot を割り当てます。slot は、子の RenderObject を親の子リストのどこに挿入するかを決める値です。

| 親 | 子へ割り当てる slot |
|---|---|
| `MultiChildRenderObjectElement<T>`(`Row`、`Stack` など) | `IndexedSlot(Index, Previous)`。自分の位置と、直前の兄弟の Element |
| 子が1つの `RenderObjectElement` | `null` |
| `ComponentElement`(`StatelessElement`、`StatefulElement` など) | 自分の slot をそのまま渡す |

`MultiChildRenderObjectElement<T>` は、`Previous` の RenderObject の次に子を挿入します。`Previous` が RenderObject を持たない Element でも、`Element.RenderObject` が子孫から探します。slot が要るのは、途中の子の RenderObject が差し替わるとき、子リストの途中に Widget を足すとき、`Key` 付きの子を並べ替えるときの3つです。並べ替えでは、Element を使い回したうえで `UpdateSlot` → `MoveRenderObjectChild` で同じ RenderObject を移動します。

### ルートの接続

Widget ツリーのルートは、`RenderObjectToWidgetAdapter`(Widget)と `RenderObjectToWidgetElement<RenderView>`(Element)の組で `RenderView` につながります。`AttachToRenderTree(owner, element)` は、最初の呼び出しでは Element を作ってツリー全体を `Mount` します。2回目からは、既存の Element に新しいルートの Widget を渡して `MarkNeedsBuild()` を呼び、次の `BuildScope` で反映させます。

### レイアウト: 制約は下へ、サイズは上へ

レイアウトは、親が子へ `BoxConstraints` を渡し、子が決めた `Size` を親が受け取る一方通行です。フレームワークは非仮想の `Layout(BoxConstraints, parentUseSize)` を呼び、その中で必要なときだけ `PerformLayout()` を呼びます。

`MarkNeedsLayout()` は、自分の `RelayoutBoundary` まで親方向へ伝わります。次のどれかに当たるノードは、自分自身が relayout boundary になります。自分のサイズが変わっても、親のレイアウトに影響しないためです。

- 親が子のサイズを使わない(`parentUseSize` が `false`)
- 制約だけでサイズが決まる(`SizedByParent` が `true`)
- 受け取った制約が tight
- 親がいない(ルート)

伝播が止まった境界のノードが `RenderPipeline.NodesNeedingLayout` に登録され、`FlushLayout` が `LayoutWithoutResize()` を呼びます。`Layout` は、制約・dirty の状態・relayout boundary のどれも変わっていなければ何もしません。relayout boundary だけが変わった場合の扱いは Flutter と違います(台帳の #5)。

### 描画: repaint boundary で止める

`MarkNeedsPaint()` は、`IsRepaintBoundary` が `true` のノードまで親方向へ伝わり、そのノードが `RenderPipeline.NodesNeedingPaint` に登録されます。境界になるのは、ルートの `RenderView` と、`RepaintBoundary` Widget の実体の `RenderRepaintBoundary` です。

`FlushPaint` は、境界のノードごとに `PaintingContext.RepaintCompositedChild` を呼びます。`Paint(PaintingContext, Offset)` は Skia の描画コマンドを `PictureLayer` に記録し、クリップや不透明度は `PushClipRect` や `PushOpacity` で子の Layer に分けます。Layer ツリーのその先は [描画の仕組みと理由](Rendering.md) にあります。

### intrinsic 測定

intrinsic 測定は、制約を渡す前に「子が本来必要とするサイズ」を問い合わせる仕組みです。`RenderBox` は、`GetMinIntrinsicWidth(height)` など4つの問い合わせ口を持ちます。サブクラスは、対応する `ComputeMinIntrinsicWidth` などを実装します。`RenderProxyBox` は既定で子へ委譲し、実装していない型に問い合わせると `NotSupportedException` が出ます。intrinsic 測定は通常のレイアウトとは別にツリーをたどるため、入れ子にすると計算量が最悪 O(N²) になります。

### ヒットテスト

ヒットテストは、ポインタの座標から、そこにある RenderObject を手前から順に集める処理です。入口は `WidgetBinding.HandlePointerEvent` で、ポインタイベントの種類ごとに次のように扱います。

| イベント | 扱い |
|---|---|
| `Down` | ヒットテストを行い、結果をポインタごとに保存してから、その結果へ配送する |
| `Move`、`Up` | `Down` のときに保存した結果へ配送する。`Up` のあとで保存した結果を捨てる |
| `Cancel` | 保存した結果があればそこへ配送し、なければジェスチャの状態だけを取り消す |
| `Remove` | 押したまま入力源から外れた場合は、保存した結果へ `Cancel` を合成して配送する |

`Move` と `Up` を `Down` の結果へ届けるのは、指が領域の外へ出ても、押し始めた対象がイベントを受け取り続けるようにするためです(コードのコメントでは「ポインターキャプチャ」と呼んでいます)。

ヒットテストそのものは、`RenderView.HitTest` から子へ再帰します。`RenderBox.HitTest` の既定の動きは次のとおりです。

1. 座標が自分の `Size` の外なら、`false` を返す
2. `HitTestChildren` で子を調べ、当たらなければ `HitTestSelf` で自分を調べる
3. どちらかが当たれば、自分を `HitTestResult` に足して `true` を返す

子を先に足すため、結果の並びは奥の子から順になります。複数の子を持つ `RenderFlex` と `RenderStack` は、子を描画の逆順(手前に描いた子から)に調べ、最初に当たった子で止めます。子へ渡す座標は、`HitTestResult.AddWidthPaintOffset` と `AddWithPaintTransform` が子のローカル座標へ変換します。そのときの変換行列は各 `HitTestEntry` に残り、配送時に `PointerEvent.Transform` として渡されます。

`RenderView` は、子が当たらなくても自分を結果に足します。最後に `WidgetBinding` が自分のエントリを末尾に足すため、配送ではすべての RenderObject のあとに `WidgetBinding.HandleEvent` が呼ばれます。

ヒットテストの結果を変える RenderObject は、レイアウトと描画を子に任せる `RenderProxyBox` の派生です。

| RenderObject | ヒットテストでの動き |
|---|---|
| `RenderPointerListener` | `Behaviour` に従う。`Opaque` は自分の領域全体を当たりにする。`Translucent` は自分を結果に足すが、`false` を返して背後の兄弟の探索を続けさせる |
| `RenderAbsorbPointer` | 自分を当たりにして、子を調べない |
| `RenderIgnorePointer` | 自分も子も当たりにしない |

`Behaviour` の既定値と、その理由は [WidgetSystem](../WidgetSystem.md) の「ヒットテストの振る舞い」にあります。

### ホバー

ホバーの `Enter` と `Exit` は、入力源から届くイベントではなく、`PointerHoverTracker` が合成します。ポインタが動くたびにヒットテストを行い、前回の結果にあって今回ない `RenderPointerListener` へ `Exit` を、その逆へ `Enter` を送ります。この比較はレイアウトのあとにも行います。そのため、ポインタが止まっていても、下の Widget が動けば `Enter` と `Exit` が届きます。

### ジェスチャ認識

`GestureDetector` は、`RawGestureDetector` を組み立てるだけの `StatelessWidget` です。`RawGestureDetector` の `State` は、認識器(`GestureRecognizer`)を持ち、子を `Listener` で包みます。ジェスチャは次の順に決まります。

1. `Down` がヒットした `Listener` の `OnPointerDown` で、各認識器が `AddPointer` によって `PointerRouter` にポインタを購読し、`GestureArenaManager` のアリーナに参加する
2. ヒットした結果の末尾にある `WidgetBinding.HandleEvent` が、`PointerRouter.Route` でイベントを認識器へ届ける
3. `Down` の配送が終わるとアリーナの受け付けを締め切り(`Close`)、参加者が1つならその認識器が勝つ
4. 認識器は自分のジェスチャだと判断した時点で勝利を宣言し、締め切り前の宣言は締め切りの時点で確定する(たとえばパンは、指が一定距離を越えて動いた時点で宣言する)
5. 誰も宣言しないまま `Up` が届くと、アリーナの最初の参加者を勝者にする(`Sweep`)

認識器の作り方と、`RawGestureDetector` の使い方は [WidgetSystem](../WidgetSystem.md) の「認識器を自分で組む」にあります。

### Hooks

`FloatSoda.Hooks` は、R3 の `ReactiveProperty<T>` で値の変更を再ビルドにつなぐ `HookWidget` を目指しています。いまは `HookWidget` と `HookElement` の型があるだけで、`BuildOwner` のビルドループとはつながっていません。状態管理の中での Hooks の役割は、追跡: [#160](https://github.com/sumx21t-3310/FloatSoda/issues/160) で検討しています。

## 所有権・ライフサイクル

| もの | 持ち主 | 作られるとき | 捨てられるとき |
|---|---|---|---|
| Widget | 親の Widget(値として) | `Build` のたびに | 次の `Build` で置き換わったとき。参照がなくなれば GC が回収する |
| Element | 親の Element | `InflateWidget` → `Mount` | `DeactivateChild` → `Deactivate`。再利用はしない |
| `State<T>` | `StatefulElement` | Element の `Mount` | Element の `Deactivate` で `Dispose` が1回だけ呼ばれる |
| RenderObject | 対応する `RenderObjectElement` | Element の `Mount` で `CreateRenderObject` | 親の Element の `DeactivateChild` で、親の RenderObject から外れる |
| 認識器 | `RawGestureDetector` の `State` | 同じ型の認識器がまだないとき | `Gestures` から型が消えたとき、または `State` の `Dispose` |
| `GestureArenaManager`、`PointerRouter` | `WidgetBinding`(ウィンドウに1つ) | `WidgetBinding` の作成 | `WidgetBinding` と同じ |

FloatSoda には、外した Element を同じフレームの中で再利用する仕組み(Flutter の inactive element のプール)がありません。そのため `Deactivate` が終わりで、`Key` 付きの部分木を別の親へ移すと `State` は作り直されます(台帳の #2)。

RenderObject の親子のつながりは、`SingleChildContainer<T>` と `MultiChildrenCollection<T>` を通して張ります。この2つは、子の代入のたびに adopt と drop を、パイプラインへの接続のたびに attach と detach を子へ伝えます。外部の購読も attach と detach に合わせます。たとえば `RenderAnimatedOpacity` は、attach で `IAnimation<double>.Changed` を購読し、detach で解除します。守る条件は [三つのツリーで守ること](../reference/Trees.md) にあります。

## なぜそう設計したか

| 判断 | 理由 | 出典 |
|---|---|---|
| Widget を不変の `record` にする | 毎回作り直しても、`==` で前回との違いを判定できるため | [WidgetSystem](../WidgetSystem.md) の「三ツリーの役割」 |
| dirty な Element を親から先に再ビルドする | 親の再ビルドで子も更新される場合に、子を別に再ビルドする無駄を省くため。Flutter と同じ戦略 | [BuildPipeline](../BuildPipeline.md) |
| `Move` と `Up` を `Down` のヒットテスト結果へ届ける | 押し始めた対象が、指が外へ出てもイベントを受け取り続けるため | `WidgetBinding.HandlePointerEvent` のコメント |
| 認識器の生成と、コールバックの設定を別のデリゲートに分ける | Widget を作り直しても認識器を保ち、押している途中のジェスチャを途切れさせないため | [WidgetSystem](../WidgetSystem.md) の「認識器を自分で組む」 |
| `GestureDetector` と `Listener` の `Behaviour` の既定値を `DeferToChild`、`PointerRegion` を `Opaque` にする | 押せる範囲を子の描画範囲に合わせ、ホバーは隙間で点滅させないため | WidgetSystem の「ヒットテストの振る舞い」 |
| Semantics の dirty フラグとフックを持たない | 対象ユーザーとランタイムに受け皿がないため | [APIDesign](../APIDesign.md) の「実装しない API — Semantics」 |

## Flutter との対応

三つのツリーの役割分担、`BuildOwner` による親から先の再ビルド、`CanUpdate` の「実行時型と `Key`」の判定、relayout boundary と repaint boundary は Flutter と同じ戦略です。確認済みの差異は次のとおりです。

| 差異 | 台帳 |
|---|---|
| 同じ位置の Widget を、同一性ではなく構造の等しさで比べる | #1 |
| 外した Element を再利用するプールがない | #2 |
| `GlobalKey` がない | #3 |
| `InheritedWidget` を探すキーが、実行時型ではなく `ScopeType` | #4 |
| relayout boundary だけが変わったとき、`Layout` が早く戻らない | #5 |
| ポインタ入力が、フレームの境目でまとめて配られる | #7 |

## テストでの駆動

三つのツリーは、SteamVR も GPU も使わずにテストできます。手順は [Flutter のウィジェットを移植する](../how-to/PortWidget.md) の「ヘッドレスでテストする」にあります。

- **ビルドとレイアウト** — `RenderView` と `RenderPipeline` を作り、`RenderObjectToWidgetAdapter.AttachToRenderTree` で Widget を接続して、`BuildScope` と `FlushLayout` を呼ぶ(`tests/FloatSoda.Test/Widgets/PaddingTest.cs`)
- **描画** — `WidgetBitmapRenderer` か `RenderObjectBitmapRenderer` でビットマップへ描き、画素を確かめる(`tests/FloatSoda.Test/Widgets/ColoredBoxTest.cs`)
- **ヒットテスト** — RenderObject をレイアウトしてから `HitTest` を直接呼び、`HitTestResult.Path` を確かめる(`tests/FloatSoda.Test/RenderObjects/HitTestTest.cs`)
- **ポインタ入力とジェスチャ** — `WidgetBinding` に `HandlePointerEvent` でイベントを流す(`tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs`、アリーナ単体は `tests/FloatSoda.Test/Gesture/GestureArenaTest.cs`)

## 関連

- [三つのツリーで守ること](../reference/Trees.md) — 変更後も守る条件
- [Flutter のウィジェットを移植する](../how-to/PortWidget.md) — ウィジェットの移植、カスタムクリッパー、ヘッドレスのテスト
- [コアの仕組みと理由](Core.md) — フレームの中で各段が呼ばれる順序
- [WidgetSystem](../WidgetSystem.md)、[BuildPipeline](../BuildPipeline.md)、[RenderObjects](../RenderObjects.md) — 元になった現行ページ
