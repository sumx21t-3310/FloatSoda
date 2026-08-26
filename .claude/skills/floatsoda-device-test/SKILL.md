---
name: floatsoda-device-test
description: >-
  Run FloatSoda's device test — enumerate, with Codex, every scenario that can only break with
  SteamVR actually running plus every behavioural divergence from the Flutter port it mirrors,
  route each one to headless xunit or to the on-HMD harness, build that harness, have the owner
  run it in VR, and triage what falls out. Use whenever the user wants to test FloatSoda on real
  hardware, mentions "実機テスト", "実機で確認", "デバイステスト", "HMDで動かして確認",
  "シナリオを洗い出したい", "Flutterとの挙動差", "移植差異", "device test", or asks what could
  break that the current unit tests would never catch. Also use when adding scenarios to an
  existing harness. Enumeration is delegated to Codex; the VR run itself is owner-only.
---

# FloatSoda Device Test

## Why this exists

FloatSoda has 84 test files, but only **two** of them (`tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs`
and `tests/FloatSoda.Test/Core/WidgetBindingTest.cs`) drive `WidgetBinding` end to end. `src/FloatSoda.OVR`
and `src/FloatSoda.Engine` are effectively untested. Everything that only exists when SteamVR is
running — overlay handles, controller ray coordinates, the GL context on the render thread, dashboard
open/close events — has no coverage at all and no way to get any from `dotnet test`.

Separately, FloatSoda mirrors Flutter's three-tree model and the docs teach it in Flutter vocabulary.
So users (and the LLMs writing for them) arrive expecting Flutter's behaviour. **Where the port
diverges, the user pays the same cost whether the divergence is a bug or a deliberate design call.**
That makes divergence its own defect class, worth enumerating alongside the VR-only one.

This skill turns both into a repeatable process instead of an ad-hoc VR session.

## The economics that shape everything below

A device run is the most expensive test in this project: put on the HMD, start SteamVR, operate by
hand, judge by eye. So:

> **Exhaustiveness is achieved during enumeration. Execution is deliberately narrow.**

Never let the two be the same size. A 40-scenario enumeration that routes 34 to headless xunit and
6 to the HMD is a success, not a shortfall.

## Orchestration and hard stops

Claude Code drives: writing the Codex brief, **verifying what Codex reports**, routing, building the
harness, triaging failures, filing issues.

Three things are the owner's alone:

1. **The VR run** — physically not delegable.
2. **Final design calls** — whether a divergence is accepted or fixed.
3. **Commit / push / tag** — always ask.

Issue filing needs no confirmation (existing repo practice).

## Workflow

### 1. Enumerate — delegate to Codex, two axes

Use `references/codex-enumeration-prompt.md` as the brief. Launch via the `task-codex-subagent` skill
(`subagent_type: "codex-runner"`), background, **read-only intent**: Codex writes exactly one file
(the scenario list in the scratchpad) and touches nothing in the repo. It must not write code or tests.

The brief must always carry:

- **Axis A — VR-only**: overlay lifecycle, controller pointer coordinates, device tracking, SteamVR
  events, action manifests, GL context / render thread / frame pacing.
- **Axis B — Flutter divergence**: `Elements/`, `RenderObjects/RenderObject.cs`, `Core/WidgetBinding.cs`,
  `Gesture/`, cross-referenced against the Flutter clone at `~/code_reading/flutter_reference`.
  Hand it `references/known-divergences.md` so it extends the list instead of rediscovering it.
- **The known-issue exclusion list** (see below) so enumeration effort doesn't go into re-finding
  filed bugs.
- **Per-scenario required fields**: identifier (usable as `--scenario` argument) / hypothesis for why
  it breaks / `file:line` / `HEADLESS` or `VR` + why / repro steps / expected result. Axis B entries
  also carry the corresponding Flutter source path.
- **An anti-hallucination clause**: every `file:line` must come from actually reading the file;
  anything unconfirmed must be marked as such.

### 2. Verify, then route

**Do not trust the enumeration as delivered.** Spot-check the cited `file:line` — confirm the symbol
exists and behaves as claimed. A scenario built on a misread line wastes an HMD session.

Then route every scenario:

| Verdict | Goes to | Test |
|---|---|---|
| `HEADLESS` | `tests/FloatSoda.Test` (xunit) | Reproducible within Widget / Element / RenderObject / Layer, observable via xunit or `src/FloatSoda.Testing`'s bitmap renderers |
| `VR` | `tests/FloatSoda.DeviceTest` | Needs the OpenVR runtime, a GL context, real device tracking, or SteamVR event delivery |

When it's ambiguous, route to `HEADLESS`. Discovering that a supposedly VR-only bug reproduces
headlessly is a win — so any scenario that does fail in VR should get a headless reproduction attempt
before the root cause is called final.

Axis B scenarios additionally get a provisional label: **deliberate design call / not yet ported /
port mistake**. Nothing can be acted on until it carries one. A divergence that settles as "deliberate"
is not closed — it becomes a `docs/` gap, because the user still pays for the surprise.

### 3. Build the harness

Location: **`tests/FloatSoda.DeviceTest/`**, one project.

Why that path, concretely:

- `tests/Directory.Build.props` supplies `IsPackable=false` and `GenerateDocumentationFile=false`.
  This matters: the root `Directory.Build.props` sets **no** `IsPackable=false`, and `release.yml`
  pushes **every** `artifacts/*.nupkg` to NuGet. A harness in a new top-level directory without its
  own `Directory.Build.props` gets published to NuGet.org on the next tag.
- CI names test projects by explicit path (`dotnet test tests/FloatSoda.Rendering.Test`,
  `dotnet test tests/FloatSoda.Test`), so nothing here is swept into CI. Keep the harness a console
  exe that does not reference xunit and root-level `dotnet test` skips it too.

**One scenario = one process, but not one project.**

Process isolation is forced by the current design, for two reasons worth re-checking each run in case
they've been fixed:

- There is no window teardown API — `FloatSodaApp._bindings` is add-only, `WidgetBinding` is not
  `IDisposable`, and a created overlay lives until app exit (issue #218). In-process scenario
  switching would leave the previous overlay on screen.
- `FloatSodaApp.MainLoop` `break`s out of the loop in each of its `catch` blocks, so one scenario's
  exception ends the whole app.

But a single project with a `--scenario <name>` argument gets that isolation by relaunching the exe,
while keeping one home for the shared harness (scenario list, expected-result display, PASS/FAIL
capture, logging). Splitting into per-scenario projects forces either a shared library or copy-paste,
and grows `FloatSoda.slnx` without buying anything.

Split into a separate project **only** when project configuration itself differs: a different `AppKey`
or action manifest, or verification of the startup sequence (no-DI, SteamVR absent).

Each scenario is one class carrying: `Name` / purpose / operating steps / expected result / `Build()`.

### 4. Make the run judgeable from inside the headset

A device failure loses its value the moment the operator forgets what "pass" looked like.

- **Show the expected result on the overlay, in Japanese.** The console is invisible with the HMD on.
- **Let PASS/FAIL be recorded from inside VR.** Removing the headset to record a verdict undoes the
  point.
- **Append results and logs to a file** — scenario, timestamp, verdict, stack trace.
- **Record crashes as data too**: which scenario, how far it got.

### 5. Owner runs it; then triage

Ask the owner to run with SteamVR up and hand back the log. For each failure: attempt headless
reproduction → classify as library bug / docs gap / intended behaviour → file the issue. Axis B
findings get their provisional label confirmed or corrected.

Append newly confirmed divergences to `references/known-divergences.md` so the next enumeration
starts further along.

## Known-issue exclusion list

Pass these to Codex as already-filed so they aren't re-enumerated. **Re-check on each run** — they get
fixed. Referencing them as context is fine; a harness scenario that reproduces one is still valuable.

- **#218** no window teardown API (`FloatSodaApp._bindings` is add-only)
- **#216** `PostTaskRunner.Stop` logs an error on a clean stop
- **#191** `FocusEnter` hover hit-test uses stale coordinates
- **#182** `ControllerPointerSystem` not wired to non-dashboard overlays — `GestureDetector` never
  fires on `WorldSpaceWindow` / `DeviceTrackedWindow`
- **#151** overlay physical size (metres) has poor discoverability
- **#150** `SetState` from a background thread is unhandled
- **#147** `OVRApplication.Identify()` failure handling
- **#140** SteamVR-absent startup, late connect, reconnect
- **#90** runtime overlay-kind swap not implemented

Refresh with `gh issue list --state open` rather than trusting this list wholesale.

## Related

- **#141** (desktop Storybook / OverlayViewer) would move part of what's `VR` today into `HEADLESS`.
  Keep harness scenarios at a granularity that survives that migration.
- `floatsoda-junior-coder-test` measures docs/API quality black-box. This skill is white-box and
  measures implementation correctness. Don't merge them: feeding a junior model a bug-scenario list
  destroys what that test measures.

## Report format

Lead with the routing split (how many `HEADLESS` vs `VR`, out of how many enumerated) — that's the
headline, because it says how much of the work avoids the headset. Then the `VR` scenarios in
execution order, then axis B divergences grouped by their label, each with a `file:line` link into
the real source. Say plainly which `file:line` citations you verified yourself and which you did not.
