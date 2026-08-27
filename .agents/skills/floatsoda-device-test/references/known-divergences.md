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
