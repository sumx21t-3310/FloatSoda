# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Working Style

The repository owner develops hands-on and tracks the implementation themselves. By default, **investigate and explain — do not modify code.** When asked about a bug or behavior, report the root cause, the relevant files/lines, and (optionally) how a fix would look, but leave the actual implementation to the owner unless they explicitly ask you to make the change.

## Project Overview

FloatSoda is a SteamVR Overlay UI framework for .NET 10 / C# 14 that brings Flutter-like declarative UI to VR overlays. It renders via SkiaSharp → OpenGL (GLFW/OpenTK) → OpenVR overlay texture.

## Target Users

FloatSoda targets three personas. Use them as the yardstick for API design, docs tone, and error messages:

1. **VRChatters who vibe-code personal tools** — they barely write code themselves; an LLM does. The real "reader" of `docs/` and the API surface is the LLM, so optimize for "an LLM cannot misuse this API." Concrete wants: a FaceEmo expression switcher (OSC), a VRChat photo album, a friend-online toast notifier.
2. **Booth creators selling overlays** — Unity-native programmers capable of building a game world, but they only know uGUI and have never seen declarative UI. Explain concepts by mapping from Unity/Udon vocabulary. They can handle exe distribution given a guide.
3. **Engineers who avoid uGUI** — their core pain is that uGUI is not text-based (scenes/prefabs don't diff, review, or LLM-generate). The headline value: **the entire UI lives in C# code — no scenes, no prefabs**. Never introduce state that can't be expressed in code (e.g. external asset/config files) without weighing this trade-off.

## Build & Run Commands

```bash
# Run the sample overlay app
dotnet run --project samples/FloatSoda.Samples.OverlayApp

# Build the whole solution
dotnet build

# Run all tests
dotnet test

# Run a specific test project
dotnet test tests/FloatSoda.Test
dotnet test tests/FloatSoda.Rendering.Test

# Run a specific test by name
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.TopLeft"
```

Tests use xunit. `FloatSoda.Test` tests geometry types, RenderObjects, and Widgets. `FloatSoda.Rendering.Test` tests the Layer tree.

## Project Structure

| Project | Role |
|---|---|
| `src/FloatSoda.Abstractions` | Shared engine contracts, geometry types, input events, and frame pacing |
| `src/FloatSoda.Rendering` | Skia layer tree, shared Layer renderer, and bitmap rendering |
| `src/FloatSoda.Engine` | Platform layer: GLFW/OpenGL (`GLView`), `Renderer`, `OverlayWindow`, `RenderThreadRunner`, `FramePacer` |
| `src/FloatSoda.OVR` | OpenVR wrappers, overlay types (`DashboardOverlay`, `WorldSpaceOverlay`, `DeviceTrackedOverlay`), `VREventDispatcher`, and exception types |
| `src/FloatSoda` | Framework core: RenderObject tree, Widget/Element system, `RenderPipeline`, `FloatSodaApp` |
| `src/FloatSoda.Testing` | Headless Widget and RenderObject bitmap renderers for tests and tooling |
| `src/FloatSoda.UI` | Headless UI layer: behavior-only widgets (`ButtonBase`, `InteractionState`), no visuals — see `docs/UILayering.md` |
| `src/FloatSoda.UI.Cream` | Design system #1: retro creamy colors, flat design (`Button`, `ButtonStyle`, `CreamTheme`) |
| `src/FloatSoda.UI.FizzyPop` | Design system #2: translucency / glassmorphism (`Button`, `ButtonStyle`, `FizzyPopTheme`) |
| `samples/FloatSoda.Samples.OverlayApp` | Runnable sample (requires SteamVR running) |
| `tests/FloatSoda.Test` | xunit tests for geometry types, RenderObjects, and Widgets |
| `tests/FloatSoda.Rendering.Test` | xunit tests for the Layer tree |
| `docs/` | Developer documentation, wiki-style with `Home.md` as the entry point (Home, TargetUsers, GettingStarted, Architecture, WidgetSystem, UILayering, Animation, BuildPipeline, RenderObjects, OVRIntegration, APIDesign, Localization). Synced to the GitHub Wiki by `.github/workflows/sync-wiki.yml` |

### UI Layering Rules (see `docs/UILayering.md`)

The UI is split into three layers: `FloatSoda` (core + primitive widgets; anything touching Skia/render-tree types), `FloatSoda.UI` (headless behavior: interaction state machines, typed builder slots), and two parallel design systems `FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop` (state→visual mapping only). Two rules: (1) behavior/state machines always go in `FloatSoda.UI`, never in a design system; (2) `FloatSoda.UI` widgets must work without any design-system `InheritedWidget` (themes' `Of()` returns null and components fall back to defaults). Litmus test: each design system must be buildable without copying code from the other. Design systems never reference each other.

## Architecture: Three Trees

FloatSoda mirrors Flutter's three-tree model. The RenderObject and Layer trees are fully implemented; the Widget/Element tree has incremental (dirty-list) rebuilds working for `StatelessWidget`, `StatefulWidget`, and `InheritedWidget`, including keyed multi-child list diffing:

### 1. RenderObject Tree (`src/FloatSoda/RenderObjects/`)

The low-level layout-and-paint tree with incremental updates. Property changes call `MarkNeedsLayout()` / `MarkNeedsPaint()`, which propagate to the nearest relayout/repaint boundary and register that node on `RenderPipeline.NodesNeedingLayout` / `NodesNeedingPaint`. Each frame:
1. `RenderPipeline.FlushLayout()` → processes dirty nodes in `Depth` order via `LayoutWithoutResize()`
2. `RenderPipeline.FlushPaint()` → repaints dirty nodes via `PaintingContext.RepaintCompositedChild()`, recording Skia draw calls into `PictureLayer`s inside a `ContainerLayer` tree

`RenderView.PrepareInitialFrame()` seeds both lists for the first frame. Frames with no dirty nodes skip layout/paint entirely.

Key classes:
- `RenderObject` — abstract base; subclasses implement `PerformLayout()` + `Paint(PaintingContext, Offset)`; `Layout(BoxConstraints)` is the non-virtual entry point that handles relayout boundaries
- `RenderBox` — adds `SKSize Size`; most layout objects extend this
- `RenderProxyBox` — single-child passthrough base (delegates layout/paint to child)
- `SingleChildContainer<T>` / `MultiChildrenCollection<T>` — composition helpers for child holding (adopt/drop on assignment, attach/detach/visit forwarding)
- `RenderImage` — renders an `SKImage` loaded via `FileImageProvider`

### 2. Layer Tree (`src/FloatSoda.Rendering/Layers/`)

Produced by the RenderObject tree's paint phase. Represents composited draw operations sent to the render thread:
- `ContainerLayer` — node with child layers
- `PictureLayer` — leaf holding an `SKPicture` (recorded Skia commands)
- `ClipRectLayer`, `ClipRoundRectLayer`, `ClipPathLayer`, `OpacityLayer`, `TransformLayer` — compositing effects

The layer tree is **cloned** (`ILayer.Clone()`) before being handed to the render thread to avoid data races.

### 3. Widget/Element Tree (`src/FloatSoda/Widgets/`, `src/FloatSoda/Elements/`) — Core implemented

Flutter-style declarative layer built on top of the RenderObject tree. `StatelessWidget` / `StatefulWidget` / `InheritedWidget` and their elements are all wired, including incremental rebuilds via `BuildOwner` and keyed two-ended list diffing in `MultiChildRenderObjectElement`. Remaining gaps: many convenience widgets (`Padding`, `Container`, `ListView`, `GridView`, `Opacity`, `GestureDetector`, `Listener`) are still `NotImplementedException` stubs, and gesture/hit-testing is not implemented (`Button`/`Icon` now live in the design-system layer; see UI Layering Rules above):
- `Widget` — immutable `abstract record`; declares `CreateElement()` and holds an optional `Key`. `Widget.CanUpdate` is Flutter-style (same runtime type + equal `Key`); a fast-path record-equality check in `Element.UpdateChild` short-circuits identical widgets before that
- `RenderObjectWidget<T>` — widget that owns a `RenderObject`; declares `CreateRenderObject()` / `UpdateRenderObject(T)`
- `StatelessWidget` — `Build()` is called by `StatelessElement`; usable today
- `StatefulWidget<T>` / `State<T>` — state lifecycle wired via `StatefulElement`; `State.SetState()` mutates state and calls `Element.MarkNeedsBuild()` to schedule a rebuild
- `InheritedWidget` / `InheritedElement` — context propagation with dependent tracking; descendants registered via `DependOnInheritedWidget` are notified on change
- `Element` — mutable tree node; `Mount()` / `UpdateChild()` / `InflateWidget()` / `MarkNeedsBuild()` / `Rebuild()`
- `BuildOwner` — per-window rebuild scheduler; holds the dirty-element list, `BuildScope()` rebuilds dirty elements in `Depth` order (parents first)
- `RenderObjectElement` — element that attaches its `RenderObject` into the render tree; `UpdateChildren()` implements the keyed two-ended list-diff for multi-child widgets
- `RenderObjectToWidgetAdapter` — bridges the widget tree root to a `RenderView`; `AttachToRenderTree(owner, element)` mounts on first call and schedules a rebuild (via `NewWidget` + `MarkNeedsBuild`) on re-attach
- `WidgetBinding` — per-window coordinator; owns the `BuildOwner`; `DrawFrame()` runs `BuildOwner.BuildScope()` then flushes layout/paint only when `NeedsVisualUpdate` is set
- `src/FloatSoda.Hooks` — R3-based `HookWidget` / `HookElement` (`UseState` via `ReactiveProperty`), partially implemented, not yet integrated with the build loop; the `HookExtension` helpers (`UseState`/`UseEffect`/`UseMemo`/etc.) still throw `NotImplementedException`

## Frame Pipeline

```
FloatSodaApp.Run() [main thread, STA]
  └─ PollEvents()          — VREvent_Quit etc.
  └─ DrawFrame()           — per WidgetBinding (window)
       └─ BuildOwner.BuildScope()        — rebuild dirty Elements → UpdateRenderObject
       └─ (skip rest unless NeedsVisualUpdate)
       └─ RenderPipeline.FlushLayout()   — layout dirty RenderObjects (Depth order)
       └─ RenderPipeline.FlushPaint()    — repaint dirty RenderObjects → Layer tree
       └─ RenderThreadRunner.PostRender(layer.Clone())
            └─ [render thread] OverlayWindow.Update()
                 └─ Renderer.Render(layer)
                      └─ GLView.Clear() → layer.Layout(ctx) → layer.Paint(ctx) → GLView.Flush()
                 └─ OpenVR SetRenderTexture(GL texture handle)
  └─ FramePacer.WaitForNextFrame()
```

The render thread runs in `RenderThreadRunner` (a `ThreadRunner` subclass) and owns the GLFW/GL context. Work is posted via `ConcurrentQueue<Action>` (`_pendingTasks`). Window creation is also deferred through this queue so all GL calls stay on the same thread.

## API Design Conventions (from `docs/APIDesign.md`)

- **Object-initializer first**: no constructor arguments on components — use `init` properties only.
- **Single child** → `Child` property; **multiple children** → `Children` (`IList<Widget>`).
- **Geometry types** → `readonly record struct` (e.g. `Offset`, `Size`, `Rect`, `EdgeInsets`, `BoxConstraints`).
- **Context/theme objects** → `record` (reference type, supports `with`-expression propagation).
- **Static factory methods** follow the naming table: `All(v)`, `Symmetric(h,v)`, `Only(...)`, `Zero`/`Empty`, `FromXxx(...)`.
- All `public` properties get XML doc comments (`/// <summary>…</summary>`).
- Properties use `init` accessors to enforce immutability; use `required` for mandatory fields.
- Event handlers are typed `Action?` / `Action<T>?` / `Func<Task>?` with an `On` prefix (e.g. `OnClick`, `OnChanged`).
- Style attributes live in a separate `*Style` record (e.g. `ButtonStyle`), not on the component itself.
