# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

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
| `docs/` | Developer documentation (Architecture, GettingStarted, RenderObjects, WidgetSystem, OVRIntegration, APIDesign) |

## Architecture: Two Parallel Trees

FloatSoda mirrors Flutter's three-tree model, currently with two trees implemented:

### 1. RenderObject Tree (`src/FloatSoda/RenderObjects/`)

The low-level layout-and-paint tree. Every frame:
1. `RenderPipeline.FlushLayout()` → calls `RenderView.PerformLayout()` top-down, passing `BoxConstraints`
2. `RenderPipeline.FlushPaint()` → walks the tree calling `Paint(PaintingContext, Offset)`, which records Skia draw calls into `PictureLayer`s inside a `ContainerLayer` tree

Key classes:
- `RenderObject` — abstract base with `Layout(BoxConstraints)` + `Paint(PaintingContext, Offset)`
- `RenderBox` — adds `SKSize Size`; most layout objects extend this
- `RenderProxyBox` — single-child passthrough base (delegates layout/paint to child)
- `RenderObjectWithChild<T>` mixin — typed `Child` property helper
- `RenderImage` — renders an `SKImage` loaded via `FileImageProvider`

### 2. Layer Tree (`src/FloatSoda.Common/Layer/`)

Produced by the RenderObject tree's paint phase. Represents composited draw operations sent to the render thread:
- `ContainerLayer` — node with child layers
- `PictureLayer` — leaf holding an `SKPicture` (recorded Skia commands)
- `ClipRectLayer`, `ClipRoundRectLayer`, `ClipPathLayer`, `OpacityLayer`, `TransformLayer` — compositing effects

The layer tree is **cloned** (`ILayer.Clone()`) before being handed to the render thread to avoid data races.

### 3. Widget/Element Tree (`src/FloatSoda/Widgets/`, `src/FloatSoda/Elements/`) — Partially implemented

Flutter-style declarative layer built on top of the RenderObject tree. `StatelessWidget` / `StatelessElement` are fully wired. `StatefulWidget` / `StatefulElement` are WIP (build loop not yet driven):
- `Widget` — immutable `abstract record`; declares `CreateElement()`
- `RenderObjectWidget` — widget that owns a `RenderObject`; declares `CreateRenderObject()`
- `StatelessWidget` — `Build()` is called by `StatelessElement`; usable today
- `StatefulWidget` — state lifecycle scaffolded but `StatefulElement.Build()` not yet implemented
- `Element` — mutable tree node; `Mount()` / `UpdateChild()` / `InflateWidget()`
- `RenderObjectElement` — element that attaches its `RenderObject` into the render tree
- `RenderObjectToWidgetAdapter` — bridges the widget tree root to a `RenderView`
- `WidgetBinding` — per-window coordinator; calls `AttachRootWidget()` then `DrawFrame()` each frame

## Frame Pipeline

```
FloatSodaApp.Run() [main thread, STA]
  └─ PollEvents()          — VREvent_Quit etc.
  └─ DrawFrame()
       └─ RenderPipeline.FlushLayout()   — RenderObject layout pass
       └─ RenderPipeline.FlushPaint()    — RenderObject paint pass → Layer tree
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
