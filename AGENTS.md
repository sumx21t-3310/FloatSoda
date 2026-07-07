# AGENTS.md

This file provides guidance to coding agents (Codex etc.) when working with code in this repository.

## Working Style

The repository owner develops hands-on and tracks the implementation themselves. By default, **investigate and explain — do not modify code.** When asked about a bug or behavior, report the root cause, the relevant files/lines, and (optionally) how a fix would look, but leave the actual implementation to the owner unless they explicitly ask you to make the change.

## Project Overview

FloatSoda is a SteamVR Overlay UI framework for .NET 10 / C# 14 that brings Flutter-like declarative UI to VR overlays. It renders via SkiaSharp → OpenGL (GLFW/OpenTK) → OpenVR overlay texture.

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
dotnet test tests/FloatSoda.Common.Test

# Run a specific test by name
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.TopLeft"
```

Tests use xunit. `FloatSoda.Test` tests geometry types, RenderObjects, and Widgets. `FloatSoda.Common.Test` tests the Layer tree.

## Project Structure

| Project | Role |
|---|---|
| `src/FloatSoda.Common` | Shared geometry types (`Offset`, `GeometryExtension`) and the Layer tree (`ILayer`, `ContainerLayer`, `PictureLayer`, clip layers) |
| `src/FloatSoda.Engine` | Platform layer: GLFW/OpenGL (`GLView`), `Renderer`, `OverlayWindow`, `RenderThreadRunner`, `FrameLimiter` |
| `src/FloatSoda.OVR` | OpenVR wrappers, overlay types (`DashboardOverlay`, `WorldSpaceOverlay`, `DeviceTrackedOverlay`), `VREventDispatcher`, and exception types |
| `src/FloatSoda` | Framework core: RenderObject tree, Widget/Element system, `RenderPipeline`, `FloatSodaApp` |
| `samples/FloatSoda.Samples.OverlayApp` | Runnable sample (requires SteamVR running) |
| `tests/FloatSoda.Test` | xunit tests for geometry types, RenderObjects, and Widgets |
| `tests/FloatSoda.Common.Test` | xunit tests for the Layer tree |
| `docs/` | Developer documentation, wiki-style with `Home.md` as the entry point (Home, GettingStarted, Architecture, WidgetSystem, BuildPipeline, RenderObjects, OVRIntegration, APIDesign). Synced to the GitHub Wiki by `.github/workflows/sync-wiki.yml` |

## Architecture: Three Trees

FloatSoda mirrors Flutter's three-tree model. The RenderObject and Layer trees are fully implemented; the Widget/Element tree has incremental (dirty-list) rebuilds working for `StatelessWidget`:

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

### 2. Layer Tree (`src/FloatSoda.Common/Layer/`)

Produced by the RenderObject tree's paint phase. Represents composited draw operations sent to the render thread:
- `ContainerLayer` — node with child layers
- `PictureLayer` — leaf holding an `SKPicture` (recorded Skia commands)
- `ClipRectLayer`, `ClipRoundRectLayer`, `ClipPathLayer`, `OpacityLayer`, `TransformLayer` — compositing effects

The layer tree is **cloned** (`ILayer.Clone()`) before being handed to the render thread to avoid data races.

### 3. Widget/Element Tree (`src/FloatSoda/Widgets/`, `src/FloatSoda/Elements/`) — Partially implemented

Flutter-style declarative layer built on top of the RenderObject tree. `StatelessWidget` / `StatelessElement` are fully wired, including incremental rebuilds via `BuildOwner`. `StatefulWidget` / `StatefulElement` and `InheritedWidget` / `InheritedElement` are skeletons (`NotImplementedException`); `MultiChildRenderObjectElement.PerformRebuild()` (children diffing) is also not implemented yet:
- `Widget` — immutable `abstract record`; declares `CreateElement()`. `Widget.CanUpdate` is currently full record equality (not Flutter's type+key check); `Key` types exist but aren't wired in
- `RenderObjectWidget<T>` — widget that owns a `RenderObject`; declares `CreateRenderObject()` / `UpdateRenderObject(T)`
- `StatelessWidget` — `Build()` is called by `StatelessElement`; usable today
- `StatefulWidget<T>` — state lifecycle scaffolded but `StatefulElement.Build()` not yet implemented
- `Element` — mutable tree node; `Mount()` / `UpdateChild()` / `InflateWidget()` / `MarkNeedsBuild()` / `Rebuild()`
- `BuildOwner` — per-window rebuild scheduler; holds the dirty-element list, `BuildScope()` rebuilds dirty elements in `Depth` order (parents first)
- `RenderObjectElement` — element that attaches its `RenderObject` into the render tree
- `RenderObjectToWidgetAdapter` — bridges the widget tree root to a `RenderView`; `AttachToRenderTree(owner, element)` mounts on first call and schedules a rebuild (via `NewWidget` + `MarkNeedsBuild`) on re-attach
- `WidgetBinding` — per-window coordinator; owns the `BuildOwner`; `DrawFrame()` runs `BuildOwner.BuildScope()` then flushes layout/paint only when `NeedsVisualUpdate` is set
- `src/FloatSoda.Hooks` — R3-based `HookWidget` / `HookElement` (`UseState` via `ReactiveProperty`), partially implemented, not yet integrated with the build loop

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
  └─ FrameLimiter.Wait()
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
