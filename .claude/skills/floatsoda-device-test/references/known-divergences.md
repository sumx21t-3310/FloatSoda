# Confirmed Flutter port divergences

Source-verified differences between FloatSoda and the Flutter implementation it mirrors. This file is
the **starting point** for axis B enumeration, not a complete list — hand it to Codex so it extends
rather than rediscovers, and append newly confirmed entries after each run.

Flutter clone for cross-reference: `~/code_reading/flutter_reference`. The `flutter-widget-source`
skill resolves a widget to its Widget / Element / RenderObject implementations.

Each entry carries a label once decided: **deliberate** (a design call — then it is a `docs/` gap if
undocumented) / **not ported** (missing, file an issue) / **port mistake** (implemented but wrong —
the highest-value category).

---

## 1. Widget equality is structural, not identity

- **FloatSoda**: `child.Widget == newWidget` at `src/FloatSoda/Elements/Element.cs:173`. `Widget` is an
  `abstract record`, so `==` is **value equality**. A structurally identical new widget short-circuits
  the whole update — `Update()` never runs, so no `didUpdateWidget` equivalent fires and the subtree
  is not rebuilt.
- **Flutter**: the same-shaped check compares by identity (`Widget` does not override `==`), so a fresh
  instance always reaches `canUpdate` → `child.update(newWidget)` → `didUpdateWidget`.
- **Observation**: `HEADLESS`
- **Label**: unlabelled — `AGENTS.md` describes it as an intentional fast path, but the
  `didUpdateWidget` consequence is not documented anywhere.

## 2. No inactive-element pool

- **FloatSoda**: `src/FloatSoda/Elements/ComponentElement.cs:164` states it outright — with no
  reactivation pool, `Deactivate` is terminal (equivalent to unmount). Moving a keyed subtree to a
  different parent destroys its `State`.
- **Flutter**: `BuildOwner._inactiveElements` holds deactivated elements so they can be reactivated
  within the same frame; `finalizeTree()` unmounts whatever wasn't.
- **Observation**: `HEADLESS`
- **Label**: unlabelled

## 3. `GlobalKey` is not implemented

- **FloatSoda**: no occurrences anywhere under `src/`.
- **Flutter**: cross-tree `State` access and reparenting depend on it.
- **Observation**: `HEADLESS`
- **Label**: not ported (pairs with #2 — reactivation is its prerequisite)

## 4. InheritedWidget registration key is `ScopeType`, not the runtime type

- **FloatSoda**: `src/FloatSoda/Widgets/InheritedWidget.cs:20` defines `ScopeType` (defaults to
  `GetType()`, overridable). `src/FloatSoda/Widgets/WindowWidget.cs:32` overrides it to
  `typeof(WindowWidget)` so all three concrete window kinds register under the base type and
  descendants keep their dependency when the concrete type changes.
- **Flutter**: keyed by `runtimeType`; `dependOnInheritedWidgetOfExactType<T>()` is an exact match.
- **Observation**: `HEADLESS`
- **Label**: deliberate (issue #90 relies on it) — so it needs documenting.

## 5. Layout early-return does not cover a boundary-only change

- **FloatSoda**: `src/FloatSoda/RenderObjects/RenderObject.cs:91-107`. The early return requires
  `RelayoutBoundary == relayoutBoundary`, so when only the relayout boundary changed, control falls
  through and **`PerformLayout()` runs**.
- **Flutter**: with unchanged constraints it reassigns the boundary, cleans children, and returns —
  `performLayout` does not run.
- **Observation**: `HEADLESS` (may also surface as extra layout cost on device)
- **Label**: unlabelled — likely port mistake, worth confirming first.

## 6. Frame phases are missing several Flutter stages

- **FloatSoda**: `src/FloatSoda/Core/WidgetBinding.cs:186-245` — transient callbacks → build → layout
  → paint → `PostRender`. No post-frame callbacks (`PostFrame` has no occurrences under `src/`), no
  `finalizeTree`, no compositing-bits flush, no semantics.
- **Flutter**: transient → persistent (build / layout / compositing bits / paint / composite /
  semantics) → `finalizeTree` → post-frame callbacks.
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
- **Observation**: `HEADLESS` — the quantisation itself is verifiable by feeding several pointer
  events within one frame and asserting they are dispatched together.
- **Label**: unlabelled

## 8. Input loss under a degraded frame rate

- The user-visible consequence of #7. Whether events are dropped or merely delayed once frame pacing
  slips depends on the real event source and the actual frame budget.
- **Observation**: **`VR`** — needs SteamVR event delivery and a real frame rate; not reproducible
  from a synthetic event queue.
- **Label**: unlabelled — resolve #7 first; if the headless verdict already explains the behaviour,
  this entry collapses into it.
