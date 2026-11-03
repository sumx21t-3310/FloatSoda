# AGENTS.md

This file provides guidance to coding agents — Codex, Claude Code (claude.ai/code), and others — when working with code in this repository.

> **This file is the single source of project instructions for every agent.** Codex and other AGENTS.md-aware tools read it directly; Claude Code reads it through a one-line `@AGENTS.md` import in `CLAUDE.md`. Put project instructions here, never in `CLAUDE.md` — anything added there is invisible to every other agent.

## Working Style

The repository owner develops hands-on and tracks the implementation themselves. By default, **investigate and explain — do not modify code.** When asked about a bug or behavior, report the root cause, the relevant files/lines, and (optionally) how a fix would look, but leave the actual implementation to the owner unless they explicitly ask you to make the change.

When you are asked to implement, how much you may take on depends on the kind of work — see the tiers in `CONTRIBUTING.md` (「コーディングエージェントに任せる範囲」): new APIs that Flutter does not have are implemented by a human; bug fixes start with investigation only; porting Flutter widgets (with Flutter's tests) may be delegated fully.

## Output Language

**Write all output to the repository owner in Japanese.** This is a hard requirement, not a preference.

- **Code review** — findings, severity reasoning, and suggested fixes are written in Japanese, even though the code and identifiers under review are English.
- **Investigation reports** — root cause, affected files/lines, and how a fix would look.
- **Commit messages and PR titles/bodies.**

Do not translate identifiers, type names, file paths, or code fences — those stay verbatim as they appear in the source.

Rationale: FloatSoda treats **Japanese as the neutral (default) language** — see `LOCALIZATION.md`. `docs/`, XML doc comments, and exception messages are all Japanese by design. Review output in Japanese keeps the entire surface consistent for the owner and for the LLMs that relay it to end users. If you feel English "should" be the default here, read `LOCALIZATION.md` before changing anything.

## Repository Conventions

Conventions have one canonical home each. **Read the canon before acting on a convention, and do not copy its body into this file** — a duplicated rule is a rule that will drift.

| What you need | Canon |
|---|---|
| Branch naming, namespace/directory layout, agent skills, PR flow, scope discipline, test perspectives | `CONTRIBUTING.md` |
| Code review criteria — priorities, finding bar, FloatSoda tree invariants | `REVIEW.md` |
| API design principles and Flutter parity (including when a divergence is allowed and how to record it) | `API_DESIGN.md` |
| Release procedure | `RELEASING.md` |
| Where a piece of documentation goes, and which docs a change must update (Documentation Impact Matrix) | `WRITING_DOCUMENTATION.md` |
| Writing style of `docs/`, sample READMEs, and Issue / PR bodies | `CONTRIBUTING.md` 「ドキュメントのトンマナ」 |
| XML documentation comments | `DOCUMENTATION_COMMENTS.md` |
| Default language (Japanese) and exception messages | `LOCALIZATION.md` |

Three that catch agents out most often:

- **Branch names must not carry the name of the agent or tool that did the work.** No `codex/`, `claude/`, `agent/` prefixes. Use `<issue-number>-<primary-area>-<slug>`, where `<primary-area>` comes from the issue's `area:*` labels — full rules in `CONTRIBUTING.md`. Issue classification (type / area / auxiliary attributes) lives in GitHub Labels, not in title prefixes.
- **A C# namespace and its physical directory must match**, and must be changed together. Full rules in `CONTRIBUTING.md`.
- **Do not mix unrelated refactoring, renames, cleanup, or dependency changes into a PR.** Full rules in `CONTRIBUTING.md`.

## Repository Skills

**Look in `.agents/skills/` first** — it is the single canon for repository-provided agent skills. Every skill's procedure and its `references/` files live there.

`.claude/skills/` exists only because Claude Code does not read `.agents/skills/`. Those files are **derived compatibility stubs: frontmatter plus a pointer to the canon, and nothing else.** Never edit them, and never treat one as the source of a procedure. Adding or changing a skill always starts in `.agents/skills/` (→ `CONTRIBUTING.md`).

## Rider MCP

Where JetBrains Rider is installed and its MCP server is available in the session, **prefer it for analysing and changing C#/.NET code**. Use it especially for:

- symbol declaration / usages
- inheritance / implementations
- type and member relationships
- semantic rename
- structural refactoring
- IDE / compiler diagnostics
- solution / project structure

**For operations where C# symbol resolution matters, prefer a semantic IDE operation over plain text search.** For namespace moves and renames, prefer Rider's semantic refactoring over string replacement — text replacement silently misses generic constraints, XML doc `cref`s, and `nameof`, and silently hits comments and strings it should not.

If Rider is not installed, **suggest installing it to the user before starting substantial C#/.NET work**. Rider is free for non-commercial use under JetBrains' current licensing.

**None of this is a hard dependency.** If the user does not want Rider, or the MCP server is unavailable, fall back to the `dotnet` CLI, repository search, and source inspection, and carry on. **Never install software on your own initiative** — propose it and let the user decide.

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
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.ComputeOffset"
```

Tests use xunit. `FloatSoda.Test` tests geometry types, RenderObjects, and Widgets. `FloatSoda.Rendering.Test` tests the Layer tree.

### Test naming

Test method names follow **`MemberName_条件_期待結果`** (member name in English, condition and expected result in Japanese). What to test — boundaries, invalid inputs, lifecycle, incremental behavior, a regression test for every bug fix, and Flutter's own branches and tests for Flutter-derived widgets — is a separate convention. Both live in `CONTRIBUTING.md` (「テストの命名」「テスト観点」).

## Project Structure

| Project | Role |
|---|---|
| `src/FloatSoda.Abstractions` | Shared engine contracts, geometry types, input events, and frame pacing |
| `src/FloatSoda.Rendering` | Skia Layer tree, shared Layer renderer, and bitmap rendering |
| `src/FloatSoda.Engine` | Platform layer: GLFW/OpenGL window, renderer, and the render-thread task runner |
| `src/FloatSoda.OVR` | OpenVR wrappers, overlay types, and VR event dispatch |
| `src/FloatSoda` | Framework core: Widget / Element / RenderObject trees and the frame pipeline |
| `src/FloatSoda.Testing` | Headless Widget and RenderObject bitmap renderers for tests and tooling |
| `src/FloatSoda.UI` | Headless UI layer: behavior-only widgets, no visuals |
| `src/FloatSoda.UI.Cream` / `src/FloatSoda.UI.FizzyPop` | The two design systems: state → visual mapping only |
| `src/FloatSoda.Hooks` | R3-based hooks for widgets |
| `samples/` | Runnable samples, one directory per sample (most require SteamVR running) |
| `tests/` | xunit tests: `FloatSoda.Test` (geometry, RenderObjects, Widgets) and `FloatSoda.Rendering.Test` (Layer tree) |
| `docs/` | Source of the docs site, one directory per site tab. Every `.md` under it becomes a site page, so do not put scratch files there |
| `website/` | Docs site (Astro Starlight) generated from `docs/`. Edit `docs/`, never `website/src/content/docs/` — see `website/README.md` |

**How far each project is implemented changes often, so it is not recorded here.** Read the source, and for the UI layers read 「実装の進み具合の追い方」 in `docs/contributor/2-understand/ui-layer.md`.

Where new UI code goes — core, `FloatSoda.UI`, or a design system — is decided by the rules in 「2つの規約と、判定基準」 in `docs/contributor/2-understand/ui-layer.md`. Read it before adding any widget outside the core.

## Architecture

FloatSoda mirrors Flutter's Widget / Element / RenderObject trees and adds a Layer tree that is cloned and handed to a separate render thread. The canon for how this works is the contributor docs, not this file:

| What you need | Canon |
|---|---|
| One frame end to end: build → layout → paint → Layer clone → render thread → SteamVR | `docs/contributor/2-understand/frame-to-steamvr.md` |
| Widget / Element / RenderObject trees, dirty lists, relayout and repaint boundaries | `docs/contributor/2-understand/framework-layer.md` |
| Layer types, `Clone()`, bitmap rendering | `docs/contributor/2-understand/rendering-layer.md` |
| GLFW/OpenGL and the render thread | `docs/contributor/2-understand/engine-layer.md` |
| OpenVR overlays | `docs/contributor/2-understand/ovr-layer.md` |
| Input | `docs/contributor/2-understand/framework-input.md` |
| Tree invariants a change must keep (ownership, lifecycle, incremental updates, Layer) | `REVIEW.md` 「4. FloatSoda 固有の不変条件」 |
