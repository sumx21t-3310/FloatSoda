# Confirmed Flutter port divergences

**This ledger is the canonical record of confirmed FloatSoda ↔ Flutter divergences.** The design
principle that governs when a divergence is allowed at all lives in
[`docs/APIDesign.md`](../../../../docs/APIDesign.md) ("判断原則: Flutter 由来の observable behavior に
差異を作らない"); this file holds the individual entries. Recording a divergence anywhere else instead
of here splits the record — append it here.

It doubles as the **starting point** for axis B enumeration, not a complete list — hand it to Codex so
it extends rather than rediscovers, and append newly confirmed entries after each run.

Flutter clone for cross-reference: `~/code_reading/flutter_reference`. The `flutter-widget-source`
skill resolves a widget to its Widget / Element / RenderObject implementations.

Entries are source-verified unless a **Status** line says otherwise; an unconfirmed entry is a lead
for the enumeration, not a finding.

Each entry carries a label once decided: **deliberate** (a design call — then it is a `docs/` gap if
undocumented) / **not ported** (missing, file an issue) / **port mistake** (implemented but wrong —
the highest-value category).

## Entry template

The first five fields are the ones `docs/APIDesign.md` requires for every deliberate divergence
(Flutter behaviour / FloatSoda behaviour / why / the test that pins it / user-facing docs).
**A divergence with no `Test` will be silently reverted by the next port** — treat an unset `Test` on
a `deliberate` entry as an open task, not a finished record.

```markdown
## N. <one-line statement of the divergence>

- **FloatSoda**: what it does, with `src/…:line` evidence.
- **Flutter**: what Flutter does, with the mirrored file.
- **Why**: why the divergence is necessary. Required once the label is `deliberate`.
- **Test**: file + test method name that fails if the divergence is undone. `— (not set)` if none yet.
- **Docs**: `docs/` page and/or the sample's `## Flutterとの違い` section, when users can observe it.
  `— (not set)` if it is invisible to users.
- **Observation**: `HEADLESS` or `VR` — where the difference can actually be seen.
- **Label**: deliberate / not ported / port mistake / unlabelled.
```

`Why` is omitted below wherever the label is still `unlabelled`: an entry that has not been judged yet
has no agreed reason to record.

---

## 1. Widget equality is structural, not identity

- **FloatSoda**: `child.Widget == newWidget` at `src/FloatSoda/Elements/Element.cs:173`. `Widget` is an
  `abstract record`, so `==` is **value equality**. A structurally identical new widget short-circuits
  the whole update — `Update()` never runs, so no `didUpdateWidget` equivalent fires and the subtree
  is not rebuilt.
- **Flutter**: the same-shaped check compares by identity (`Widget` does not override `==`), so a fresh
  instance always reaches `canUpdate` → `child.update(newWidget)` → `didUpdateWidget`.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled — `AGENTS.md` describes it as an intentional fast path, but the
  `didUpdateWidget` consequence is not documented anywhere.

## 2. No inactive-element pool

- **FloatSoda**: `src/FloatSoda/Elements/ComponentElement.cs:164` states it outright — with no
  reactivation pool, `Deactivate` is terminal (equivalent to unmount). Moving a keyed subtree to a
  different parent destroys its `State`.
- **Flutter**: `BuildOwner._inactiveElements` holds deactivated elements so they can be reactivated
  within the same frame; `finalizeTree()` unmounts whatever wasn't.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled

## 3. `GlobalKey` is not implemented

- **FloatSoda**: no occurrences anywhere under `src/`.
- **Flutter**: cross-tree `State` access and reparenting depend on it.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: not ported (pairs with #2 — reactivation is its prerequisite)

## 4. InheritedWidget registration key is `ScopeType`, not the runtime type

- **FloatSoda**: `src/FloatSoda/Widgets/InheritedWidget.cs:20` defines `ScopeType` (defaults to
  `GetType()`, overridable). `src/FloatSoda/Widgets/WindowWidget.cs:32` overrides it to
  `typeof(WindowWidget)` so all three concrete window kinds register under the base type and
  descendants keep their dependency when the concrete type changes.
- **Flutter**: keyed by `runtimeType`; `dependOnInheritedWidgetOfExactType<T>()` is an exact match.
- **Why**: descendants must keep their `WindowWidget` dependency when the concrete window type is
  swapped, so all three window kinds register under the base type (issue #90 relies on this).
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: deliberate (issue #90 relies on it) — so it needs documenting.

## 5. Layout early-return does not cover a boundary-only change

- **FloatSoda**: `src/FloatSoda/RenderObjects/RenderObject.cs:91-107`. The early return requires
  `RelayoutBoundary == relayoutBoundary`, so when only the relayout boundary changed, control falls
  through and **`PerformLayout()` runs**.
- **Flutter**: with unchanged constraints it reassigns the boundary, cleans children, and returns —
  `performLayout` does not run.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` (may also surface as extra layout cost on device)
- **Label**: unlabelled — likely port mistake, worth confirming first.

## 6. Frame phases are missing several Flutter stages

- **FloatSoda**: `src/FloatSoda/Core/WidgetBinding.cs:186-245` — transient callbacks → build → layout
  → paint → `PostRender`. No post-frame callbacks (`PostFrame` has no occurrences under `src/`), no
  `finalizeTree`, no compositing-bits flush, no semantics.
- **Flutter**: transient → persistent (build / layout / compositing bits / paint / composite /
  semantics) → `finalizeTree` → post-frame callbacks.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — every missing phase listed above is observable without a headset.
- **Label**: unlabelled. The missing `addPostFrameCallback` equivalent is the one that bites porters
  most often; semantics is plausibly out of scope for VR overlays.
- **When enumerating**: one scenario carries exactly one verdict. If a derived scenario depends on
  render-thread timing rather than on the missing phase itself, enumerate it separately as `VR`.

## 7. Pointer input is quantised to the frame boundary

- **FloatSoda**: `FlushPointerEvents()` runs at the top of `BeginFrame`
  (`src/FloatSoda/Core/WidgetBinding.cs:234`, implementation at `:277`), so input is processed once
  per frame.
- **Flutter**: `GestureBinding.handlePointerEvent` dispatches independently of the frame.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — the quantisation itself is verifiable by feeding several pointer
  events within one frame and asserting they are dispatched together.
- **Label**: unlabelled

## 8. Input delay or loss under a degraded frame rate — **unconfirmed**

- **Status**: not source-verified. Unlike every other entry here, this one is *derived* from #7
  rather than read off the implementation, and it is listed so the enumeration picks it up — not as
  an established divergence. Do not cite it as confirmed.
- The user-visible consequence of #7. **Whether events are actually dropped, or merely delayed, is
  the open question**; it depends on the real event source and the actual frame budget.
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: **`VR`** — needs SteamVR event delivery and a real frame rate; not reproducible
  from a synthetic event queue.
- **Label**: unlabelled — resolve #7 first; if the headless verdict already explains the behaviour,
  this entry collapses into it.

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
