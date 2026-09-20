# 確認済みの Flutter 移植差異

**この台帳が、確認済みの FloatSoda ↔ Flutter 差異の正典。** そもそも差異がいつ許されるかを
定める設計原則は [`docs/APIDesign.md`](../../../../docs/APIDesign.md)(「判断原則: Flutter 由来の
observable behavior に差異を作らない」)にあり、このファイルは個別のエントリを保持する。
差異をここ以外の場所に記録すると記録が分裂する — 必ずここに追記すること。

このファイルは軸B 列挙の**出発点**も兼ねる。完全なリストではない — Codex に渡して再発見ではなく
拡張をさせ、実行のたびに新たに確定したエントリを追記する。

突き合わせ用の Flutter クローン: `~/code_reading/flutter_reference`。`flutter-widget-source`
スキルで、ウィジェットから Widget / Element / RenderObject の実装を特定できる。

エントリは、**Status** 行に断りがない限りソース検証済み。未確認のエントリは列挙のための
手がかりであって、確定した知見ではない。

各エントリには、判断が下り次第ラベルが付く: **deliberate**(設計判断 — 未文書なら `docs/` の
ギャップになる)/ **not ported**(未移植。Issue を立てる)/ **port mistake**(実装されているが
誤り — 最も価値の高いカテゴリ)。

## エントリのテンプレート

最初の5フィールドは、`docs/APIDesign.md` がすべての意図的差異に要求するもの(Flutter の挙動 /
FloatSoda の挙動 / 理由 / 差異を固定するテスト / 利用者向け docs)。
**`Test` の無い差異は、次の移植で静かに巻き戻される** — `deliberate` エントリの未設定の `Test` は、
完了した記録ではなく未処理のタスクとして扱うこと。

```markdown
## N. <差異の一行要約>

- **FloatSoda**: 何をするか。`src/…:line` の根拠つき。
- **Flutter**: Flutter が何をするか。対応するファイルつき。
- **Why**: なぜ差異が必要か。ラベルが `deliberate` になったら必須。
- **Test**: 差異を戻すと失敗するテストのファイル + テストメソッド名。未設定なら `— (not set)`。
- **Docs**: 利用者から観測できる場合、`docs/` のページやサンプルの `## Flutterとの違い` 節。
  利用者から見えないなら `— (not set)`。
- **Observation**: `HEADLESS` か `VR` — 差異が実際に観測できる場所。
- **Label**: deliberate / not ported / port mistake / unlabelled。
```

以下でラベルが `unlabelled` のままのエントリには `Why` を書いていない: まだ判断されていない
エントリには、記録すべき合意された理由が存在しないため。

---

## 1. Widget の等価性が同一性ではなく構造的

- **FloatSoda**: `src/FloatSoda/Elements/Element.cs:173` の `child.Widget == newWidget`。`Widget` は
  `abstract record` なので `==` は**値の等価性**。構造的に同一の新しいウィジェットは更新全体を
  短絡させる — `Update()` が走らないため、`didUpdateWidget` 相当は発火せず、サブツリーは
  再構築されない。
- **Flutter**: 同じ位置のチェックは同一性比較(`Widget` は `==` をオーバーライドしない)。
  そのため新しいインスタンスは必ず `canUpdate` → `child.update(newWidget)` → `didUpdateWidget`
  に到達する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled — `AGENTS.md` は意図的な fast path として説明しているが、
  `didUpdateWidget` への帰結はどこにも文書化されていない。

## 2. inactive-element プールが無い

- **FloatSoda**: `src/FloatSoda/Elements/ComponentElement.cs:164` に明記されている — 再活性化
  プールが無いため `Deactivate` は終端(unmount と等価)。キー付きサブツリーを別の親へ移動すると
  `State` が破棄される。
- **Flutter**: `BuildOwner._inactiveElements` が deactivate された要素を保持し、同一フレーム内で
  再活性化できる。`finalizeTree()` が残りを unmount する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled

## 3. `GlobalKey` が未実装

- **FloatSoda**: `src/` 配下のどこにも出現しない。
- **Flutter**: ツリーをまたぐ `State` アクセスと reparenting がこれに依存する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: not ported(#2 と対 — 再活性化がその前提)

## 4. InheritedWidget の登録キーが実行時型ではなく `ScopeType`

- **FloatSoda**: `src/FloatSoda/Widgets/InheritedWidget.cs:20` が `ScopeType` を定義(既定は
  `GetType()`、オーバーライド可)。`src/FloatSoda/Widgets/WindowWidget.cs:32` がこれを
  `typeof(WindowWidget)` にオーバーライドし、3つの具象ウィンドウ種別すべてが基底型で登録され、
  具象型が変わっても子孫が依存を保てるようにしている。
- **Flutter**: `runtimeType` がキー。`dependOnInheritedWidgetOfExactType<T>()` は完全一致。
- **Why**: 具象ウィンドウ型が差し替えられても子孫は `WindowWidget` への依存を保つ必要があるため、
  3つのウィンドウ種別すべてを基底型で登録する(issue #90 がこれに依存)。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: deliberate(issue #90 が依存)— よって文書化が必要。

## 5. レイアウトの early-return が boundary のみの変更をカバーしない

- **FloatSoda**: `src/FloatSoda/RenderObjects/RenderObject.cs:91-107`。early return は
  `RelayoutBoundary == relayoutBoundary` を要求するため、relayout boundary だけが変わった場合は
  素通りして **`PerformLayout()` が走る**。
- **Flutter**: 制約が不変なら boundary を付け替え、子を掃除して return — `performLayout` は
  走らない。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`(実機では余分なレイアウトコストとしても現れうる)
- **Label**: unlabelled — port mistake の可能性が高いが、先に確認する価値がある。

## 6. フレームフェーズに Flutter の複数ステージが欠けている

- **FloatSoda**: `src/FloatSoda/Core/WidgetBinding.cs:186-245` — transient コールバック → build →
  layout → paint → `PostRender`。post-frame コールバックが無く(`PostFrame` は `src/` 配下に
  出現しない)、`finalizeTree` も、compositing-bits の flush も、semantics も無い。
- **Flutter**: transient → persistent(build / layout / compositing bits / paint / composite /
  semantics)→ `finalizeTree` → post-frame コールバック。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — 上に挙げた欠落フェーズはすべてヘッドセット無しで観測できる。
- **Label**: unlabelled。移植者が最も頻繁に噛まれるのは `addPostFrameCallback` 相当の欠落。
  semantics は VR オーバーレイではスコープ外というのがありそうな線。
- **列挙時の注意**: 1シナリオが持つ判定は1つ。派生シナリオが、欠落フェーズそのものではなく
  レンダースレッドのタイミングに依存するなら、`VR` として別に列挙する。

## 7. ポインタ入力がフレーム境界に量子化されている

- **FloatSoda**: `FlushPointerEvents()` が `BeginFrame` の先頭で走る
  (`src/FloatSoda/Core/WidgetBinding.cs:234`、実装は `:277`)ため、入力はフレームごとに1回
  処理される。
- **Flutter**: `GestureBinding.handlePointerEvent` はフレームとは独立にディスパッチする。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — 量子化そのものは、1フレーム内に複数のポインタイベントを流し込み、
  まとめてディスパッチされることをアサートすれば検証できる。
- **Label**: unlabelled

## 8. フレームレート低下時の入力遅延または喪失 — **未確認**

- **Status**: ソース未検証。ここにある他のすべてのエントリと違い、これは実装を読んで得たものでは
  なく #7 からの*派生*で、列挙に拾わせるために載せている — 確立した差異としてではない。
  確定済みとして引用しないこと。
- #7 の利用者から見える帰結。**イベントが実際に落ちるのか、単に遅れるだけなのかが未解決の問い**で、
  実際のイベントソースと実際のフレームバジェットに依存する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: **`VR`** — SteamVR のイベント配送と実フレームレートが必要。合成イベントキュー
  からは再現できない。
- **Label**: unlabelled — まず #7 を解決する。ヘッドレスの判定で挙動の説明が付くなら、この
  エントリはそちらへ吸収される。

## 9. 既定フォントサイズが 14 ではなく 30

- **FloatSoda**: `FontSize` 未指定時は描画時に 30 へ解決される
  (`src/FloatSoda/Painting/TextStyle.cs` — `DefaultFontSize`。`ToRichTextKitStyle` で適用)。
  4299340「Textのスタイル指定を完成させる」で意図的に導入され、以来 `TextStyle` の既定値と
  `TextPainter` のフォールバックの両方が一貫して 30(現在は `TextStyle` へ一本化済み)。
- **Flutter**: 祖先がサイズを与えない場合のエンジン既定は 14.0
  (`DefaultTextStyle.fallback` の `TextStyle` は `fontSize: null`)。
- **Why**: 14 は HMD のレンズ越しでは小さすぎて読めないため、VR オーバーレイ向けに 30 へ引き上げた。
  **30 は暫定値** — 目視で決めたもので計測に基づかない。確定扱いにする前に、実機の可読性検証
  (device test)で再調整すること。
- **Test**: `tests/FloatSoda.Test/Widgets/TextTest.cs` —
  `ToRichTextKitStyle_全プロパティ未指定_既定値で描画書式を生成する`
- **Docs**: `docs/WidgetSystem.md`(`Text` の既定書式の記載)
- **Observation**: 値の差異そのものは `HEADLESS`。30 が適切な値かどうかの判定は `VR`
  (レンズ越しの可読性)。
- **Label**: deliberate

## 10. `Stack` は範囲を超えた子をクリップしない(`clipBehavior` が無い)

- **FloatSoda**: `src/FloatSoda/RenderObjects/Layout/RenderStack.cs` の `Paint` が子を無条件に
  `PaintChild` する。`src/FloatSoda/Widgets/Layout/Stack.cs` に `Clip` 系のプロパティは無い。
  Flutter で `clipBehavior: Clip.none` を書いた状態に固定されている。
- **Flutter**: `Stack.clipBehavior`(既定 `Clip.hardEdge`、`widgets/basic.dart:4753`)。
  `RenderStack.paint`(`rendering/stack.dart:717-719`)は
  `clipBehavior != Clip.none && _hasVisualOverflow` のときだけ `pushClipRect` する。
- **Test**: — (not set)
- **Docs**: `samples/FloatSoda.Samples.Stack/README.md` の `## Flutterとの違い`(回避策として
  `ClipRect` で包む案内)。差異が解消したら行を削除する。
- **Observation**: `HEADLESS`(はみ出し子の描画有無はビットマップで判定できる)
- **Label**: not ported — 2026-09-14 にオーナーが確定。修正は #257(`ClipBehavior` を追加し、
  既定を `Clip.HardEdge` に揃える。breaking)。

## 11. `Container` がボーダーぶんの余白を `Padding` に加算しない

- **FloatSoda**: `src/FloatSoda/Widgets/Layout/Container.cs:159` が指定された `Padding` だけを
  挿入する。`src/FloatSoda/Painting/BoxDecoration.cs` に `Padding` に相当するプロパティが無い。
  `Border` 付きで `Padding` を省略すると、子がボーダーの下まで広がる。
- **Flutter**: `Container._paddingIncludingDecoration`(`widgets/container.dart:378-384`、
  `8a9f61cfd67`)が `padding` と `decoration.padding`(ボーダー寸法)を加算し、`build`
  (`:400-403`)で `Padding` として挿入する。
- **Test**: — (not set)
- **Docs**: `samples/FloatSoda.Samples.Container/README.md` の `## Flutterとの違い`(回避策として
  ボーダー幅を `Padding` に明示する案内)。差異が解消したら行を削除する。
- **Observation**: `HEADLESS`(子の配置位置はビットマップで判定できる)
- **Label**: port mistake — 余白合成は実装済みで加算だけが抜けている。修正は #267(breaking)。PR #264 の Codex レビューで発見。

## 12. `Child` の無い `Container` が緩い制約の下で 0 に縮む

- **FloatSoda**: `src/FloatSoda/Widgets/Layout/Container.cs:148` の `Child ?? new SizedBox()`。
  `SizedBox` は `BoxConstraints.TightFor(null, null)` なので、緩い制約では最小サイズになる。
- **Flutter**: `Container.build`(`widgets/container.dart:390-395`、`8a9f61cfd67`)は
  `child == null && (constraints == null || !constraints.isTight)` のとき
  `LimitedBox(maxWidth: 0, maxHeight: 0, child: ConstrainedBox(BoxConstraints.expand()))` を使う。
  有限の制約では最大まで広がり、無限の制約でだけ 0 になる。`alignment` を子がある場合だけ
  適用する点は FloatSoda と同じ。
- **Test**: — (not set)
- **Docs**: `samples/FloatSoda.Samples.Container/README.md` の `## Flutterとの違い`(回避策として
  `Width` / `Height` を指定する案内)。差異が解消したら行を削除する。
- **Observation**: `HEADLESS`(サイズはレイアウト結果で判定できる)
- **Label**: port mistake。修正は #268(breaking)。PR #264 の Codex レビューで発見。

## 13. RTL(右から左)レイアウトと start/end ベースの API が無い

- **FloatSoda**: `src/` 配下に `Directionality` / `TextDirection` / `*Directional` 系の型が1つも無い。
  レイアウトは水平 LTR 固定で、`EdgeInsets` / `Alignment` / `Positioned` は left/right だけを持つ。
- **Flutter**: `Directionality`(`widgets/basic.dart:171`、`8a9f61cfd67`)が `TextDirection` を
  配り、`EdgeInsetsDirectional` / `AlignmentDirectional` / `PositionedDirectional` などが
  start/end を方向に応じて解決する。
- **Why**: 初期の対象言語が日本語・英語・中国語・韓国語で、RTL の要望がまだ無いため。CJK フォント
  フォールバックや絵文字など、優先する項目が先にある。UI の RTL ミラーリングと、ユーザー生成文字列の
  Unicode 双方向テキスト処理は別の関心事として扱う。
- **Test**: — (not set)。型が存在しないことを固定するテストは作りにくい。移植時に `Directional`
  系の型を足す PR は、このエントリと `docs/Localization.md` の方針を先に見直す。
- **Docs**: `docs/Localization.md` の「6. RTL(右から左へのレイアウト)は現時点でスコープ外」(PR #265)
- **Observation**: `HEADLESS`(API の有無と、レイアウト結果が LTR 固定であること)
- **Label**: deliberate — 2026-09-16 にオーナーが確定。PR #265 の Codex レビューで記録漏れを指摘された。

## 14. `Image.LoadingBuilder` は読み込み中だけ呼ばれ、完成した画像を受け取らない

- **FloatSoda**: `LoadingBuilder` は `Func<IBuildContext, ImageLoadingProgress?, Widget>`。読み込みが完了するまでのあいだだけ呼ばれ、
  返したウィジェットが画像の**代わりに**表示される。完了後は呼ばれない(`src/FloatSoda/Widgets/Paint/Image.cs` の `BuildSnapshot`)。
  `ErrorBuilder` は `Func<IBuildContext, Exception, Widget>` で、引数にスタックトレースを持たない(`Exception.StackTrace` にある)。
- **Flutter**: `ImageLoadingBuilder` は `(context, child, loadingProgress)`。読み込み完了後も毎回呼ばれ、完成した画像を `child` として
  受け取って包む。`loadingProgress == null` が「完了または進捗なし」を表す(`widgets/image.dart:231`、`:878`)。
  `errorBuilder` は `(context, error, stackTrace)`(`:893`)。
- **Why**: 「`null` なら完了なので `child` を返す」という読み替えを、API の利用者である LLM が取り違えやすい。戻り値を
  「読み込み中に表示するもの」に限定すると、取り違えても完成した画像が消えない。2026-09-20 にオーナーが #214 で確定。
- **Test**: `tests/FloatSoda.Test/Widgets/ImageTest.cs` の `Build_読み込みが完了_LoadingBuilderの結果を画像へ置き換える` と
  `Build_読み込みが未完了_LoadingBuilderへnullの進捗を渡して結果を表示する`。
- **Docs**: `samples/FloatSoda.Samples.Image/README.md` の `## Flutterとの違い`。
- **Observation**: `HEADLESS`
- **Label**: deliberate

## 15. 画像キャッシュが、借りられているあいだしか画像を保持しない(LRU が無い)

- **FloatSoda**: `ImageCache`(internal)は `ImageHandle` の参照カウントだけで寿命を決める。最後のハンドルが破棄された時点で
  `SKImage` を解放し、エントリを取り除く(`src/FloatSoda/Core/Providers/ImageCache.cs` の `Release`)。同じ画像を表示し直すと
  再度読み込む。
- **Flutter**: `ImageCache` は LRU で、表示が終わった画像も `maximumSize`(既定 1000 件)/ `maximumSizeBytes`(既定 100 MiB)まで
  保持する(`painting/image_cache.dart:86`、`:490`)。
- **Why**: —(差異を意図していない。LRU は #214 の対象外として後続 Issue に分けた)
- **Test**: — (not set)
- **Docs**: `samples/FloatSoda.Samples.Image/README.md` の `## Flutterとの違い`。
- **Observation**: `HEADLESS`
- **Label**: not ported(#275)— `ImageHandle` の貸し借りの形は LRU を足しても変わらないので、public API を壊さずに追加できる。

---

## Triage 2026-09-09 — 範囲の振り分けと着手順(Phase 2 リリース後に実施)

上の9件を「どの範囲のテストで固定できるか」で振り分けた記録です。エントリ本体は変更していません。方法の全体は
[`docs/TestStrategy.md`](../../../../docs/TestStrategy.md) にあります。

**テストを書く前にラベルを決めます。** deliberate は差異を固定するテスト、port mistake は修正前に落ちる
regression test、not ported は Issue で、unlabelled のままテストを書くとバグを仕様として固定します。

| # | 範囲 | ラベル案 | 次の一手 |
|---|---|---|---|
| 1 | HEADLESS 単体 | deliberate 候補(`AGENTS.md` が fast path として説明済み) | ラベル確定後に `UpdateChild_構造的に等しい新Widget_DidUpdateWidgetを呼ばない` を `tests/FloatSoda.Test/Elements/` に追加。参照型プロパティを in-place で変更した場合に `DidUpdateWidget` が呼ばれない点を `docs/` に書く |
| 2 | HEADLESS 単体 | not ported | #3 と統合する。再活性プールが効くのは `GlobalKey` 付きの要素だけで、通常の `Key` では Flutter でも別親への移動で `State` は作り直される。単独では利用者に見えない |
| 3 | HEADLESS 単体 | not ported | Issue。`GlobalKey` を移植するか、コントローラ(`ChangeNotifier` 系)を代替として文書化するかを決める。優先度は junior-coder test で LLM が `GlobalKey` を書くかどうかで決める |
| 4 | HEADLESS 単体 | deliberate(確定済み) | **今すぐ書ける。** `ScopeType_具象Windowを差し替え_依存Elementが再構築される` を `tests/FloatSoda.Test/Widgets/WindowWidgetTest.cs` に追加。既存の `Of_FindsConcreteWindow_ViaBaseTypeLookup` は検索側だけを検証している。`Docs` も未設定なので `docs/WidgetSystem.md` に1行書く |
| 5 | HEADLESS 単体 | port mistake(確定) | **今すぐ書ける。** `Layout_制約が同じで再レイアウト境界だけ変わる_PerformLayoutを再実行しない` を `tests/FloatSoda.Test/RenderObjects/` に追加。`PerformLayout` の回数を数えるプローブは `Widgets/IndexedStackTest.cs` の方式を流用する。現状では落ちる(red)ので、修正は別コミット |
| 6 | HEADLESS 結合 | 4件に分割 | post-frame callback は not ported として Issue(LLM は `addPostFrameCallback` を当然のように書く)。`finalizeTree` は #2 へ吸収。compositing bits は FloatSoda の Layer 構造で必要かを判断してから。semantics は非移植方針が `REVIEW.md` にあるので文書のみ |
| 7 | HEADLESS 結合 | deliberate 候補 | Why 案: OpenVR のオーバーレイ入力は `PollNextOverlayEvent` で取り出す方式で、プッシュの経路が無い。量子化はプラットフォームの性質。`BeginFrame_フレーム間の複数ポインタイベント_次のBeginFrameでまとめて配信する` を `tests/FloatSoda.Test/Core/WidgetBindingTest.cs` に追加。`IEngineWindow` の test double がテストに無いので、`PointerSource` を持つ fake を同じファイルに足す |
| 8 | HEADLESS 結合 → VR | #7 に大半を吸収 | ヘッドレスで答えが出る部分: `PointerController` の待ち行列は無制限の `ConcurrentQueue` で、`SystemEventDispatcher.PollEvents` は毎フレーム全件を取り出す。FloatSoda の内側では「遅延」であって「欠落」は起きない。VR で確かめる問いは「フレームを落としたとき OpenVR 側のイベントバッファが溢れるか」だけに絞る |
| 9 | VR | deliberate(確定済み) | ヘッドレス側は完了。30 が妥当な値かは、レンズ越しの可読性を人が判定する device test のシナリオにする。xunit では扱わない |

### 台帳の更新案(着手時に反映)

- **#5 の Flutter 欄が古い。** ローカル clone `8a9f61cfd67`(2026-07-11)の `packages/flutter/lib/src/rendering/object.dart:2863` では、
  `_isRelayoutBoundary`(bool)を早期 return の**前に**代入するだけで、境界の付け替えも子の掃除も行わない。
  どちらの版でも `performLayout` は走らないので port mistake の判定は変わらないが、直し方は版で異なる。
  書き直すときは参照 commit を添える
- **#2 と #3 を1エントリに統合する**(上の表のとおり)
- **#6 を4エントリに分割する**(上の表のとおり)
- **#8 の Status を「ヘッドレスで一部確認済み」に更新し、VR の問いを絞る**

### 着手順

#4 → #5 → #1 → #7。#4 は判断済みで書くだけ、#5 は red test がそのまま証拠になる。#1 と #7 はラベルと Why を先に1行書く。
