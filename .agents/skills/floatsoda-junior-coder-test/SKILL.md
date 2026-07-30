---
name: floatsoda-junior-coder-test
description: >-
  Run FloatSoda's junior-coder test — hand a lower-effort LLM (default Sonnet 5, effort
  medium) only the public docs and a realistic VRChatter feature request, have it vibe-code
  the tool in an isolated scratch project as a black box, then triage every failure against
  the real FloatSoda source to surface docs bugs, library bugs, and API-discoverability gaps.
  Use whenever the user wants to dogfood or stress-test FloatSoda's docs or public API, mentions
  a "junior coder test", "vibe coding test", "ジュニアコーダーテスト", "バイブコーディングテスト",
  wants to check whether an LLM can build something from FloatSoda's docs alone, or wants to find
  where the docs/API mislead an LLM. Reach for this even if the user just describes the workflow
  ("let a junior model try to build X with FloatSoda and see what breaks") without naming it.
---

# FloatSoda Junior-Coder Test

## Why this exists

FloatSoda's #1 persona is a VRChatter who barely writes code — an LLM writes it for them. So the
real reader of the docs and the API surface is an LLM, and the design goal is "an LLM cannot misuse
this API." This skill operationalizes that goal: it uses a **lower-effort model as a stand-in for
that junior coder**, gives it nothing but the public docs and a realistic task, and then rigorously
triages where it stumbled. Every stumble is a signal — either the docs led it astray, the library
has a bug, or the API is easy to misuse.

This is a proven test: a first run (Sonnet 5, medium) surfaced two real library bugs — a docs-recommended
frame limiter that crashed every overlay, and an infinite recursion on dynamic child removal. See the
running log in memory: `vibe-coding-test-sonnet5-result`.

## The one rule that makes the test valid: black box

The junior model must experience FloatSoda **exactly as a NuGet consumer would** — through the docs
only. It must never read `src/`. If it can peek at the implementation, the test no longer measures
docs/API quality, which is the whole point. Enforce this in the subagent prompt (below) and by keeping
its working directory outside the FloatSoda repo tree.

## Two ways this runs as a gate

This test is not just ad-hoc — it backs two process gates. The workflow below is the same; only how you
pick the task (step 1) differs.

- **Release gate** (see `RELEASING.md`): run before tagging a release, on the release commit. Use a **main**
  or **hard** task and **rotate the theme** each release (toast / photo album / FaceEmo) so the surface isn't
  overfit to one scenario. Blocks the release if a docs bug (ⓑ) or library bug (ⓒ) surfaces.
- **New-API PR gate** (see `CONTRIBUTING.md`): run on a PR that adds or changes **public, documented API**.
  Write a **targeted task that cannot be completed without the new API**, and hand the junior the PR branch's
  **updated docs only**. This is the higher-leverage use: it catches a misuse-prone API while it can still be
  changed, before it's public and locked in. Read the failure as a design signal:
  - *can't find it* → discoverability / docs gap (not in WidgetSystem, non-intuitive name)
  - *uses it wrong* → the API isn't misuse-proof, or the docs are ambiguous
  - *hallucinates a different shape* → the model wrote a more natural API than the one built; strongly consider
    reshaping the real API to match that intuition. This is the project's "an LLM cannot misuse this API" goal
    made concrete, and PR-review time is when it's cheapest to act on.

## Workflow

### 1. Pick the task

Choose a task from `references/prompts.md` (easy / main / hard, in the VRChatter voice) or write a new
one in the same spirit. Match difficulty to what you want to measure:
- **easy** — does the getting-started path work at all (one overlay on screen)
- **main** — a stateful, dynamic UI using only implemented widgets (the sweet spot)
- **hard** — deliberately pushes into known gaps (hit-testing, unimplemented stub widgets) to see whether
  the model hallucinates an API, correctly gives up, or finds a workaround

Confirm the chosen task with the user before spending a subagent on it.

### 2. Set up an isolated scratch project

Create (or reuse) a .NET console project **outside the repo**, e.g. `../FloatSodaJuniorTest/`.
It should reference the **local** FloatSoda projects (so current source, including uncommitted fixes on
the working branch, is what gets tested) via `ProjectReference` to `src/FloatSoda/FloatSoda.csproj`
(and any other needed projects). Referencing local source — not the published NuGet — is deliberate:
it lets the test catch bugs in the code you're actively changing. The black-box rule keeps the model
honest even though the source is reachable on disk.

Wipe any prior `*.cs` the junior wrote so each run starts clean.

### 3. Spawn the junior subagent

Spawn one subagent (default `model: sonnet`, medium effort — the realistic persona-class model, not the
flagship). Its prompt must contain, verbatim in spirit:

- The task, in the VRChatter voice.
- **Docs pointer only**: the GitHub Wiki (`https://github.com/sumx21t-3310/FloatSoda/wiki`, entry `Home`)
  or the repo's `docs/` folder. Tell it to read `Home / GettingStarted / WidgetSystem / OVRIntegration`
  first.
- **Black-box instruction**: "Treat FloatSoda as a black box. Use ONLY the docs above. Do NOT open, read,
  grep, or infer from anything under `src/`. If the docs don't cover something, say so rather than guessing."
- **Constraints**: C# only; no Unity scenes/prefabs; use the exact NuGet/API names from the docs, never
  invented ones; write the code into the scratch project.
- The scratch project path to write into.

Have it report which docs pages it actually consulted and any point where it was unsure — that self-report
is gold for the triage.

### 4. Build (headless triage)

Run `dotnet build` on the scratch project. Compile errors are the cheapest, clearest signal:
- Unresolved types/members → **hallucinated API** (category ⓐ) or **discoverability gap** (the real API
  exists but the model couldn't find it in the docs).
- Everything compiles → move to runtime.

### 5. VR run (owner-driven, manual)

Runtime bugs (like the two found so far) usually need SteamVR running — the skill can't automate this.
Ask the owner to run the built exe with SteamVR up and paste the console output / stack trace. Treat
whatever they paste as the runtime evidence for triage. (If a headless/desktop host exists later — see
memory `desktop-storybook-direction` — this step can be automated.)

### 6. Triage every failure into four categories

For each thing that went wrong, classify it — and **verify against the real source** before concluding.
The verification step is what makes findings trustworthy: grep `src/` for every API the junior used and
confirm it exists and behaves as the docs implied.

| Cat | Name | What it means | How to confirm |
|---|---|---|---|
| ⓐ | **Hallucination** | Model invented an API/name that doesn't exist | Grep `src/` — no such type/member. Then ask: should this API plausibly exist? If yes → discoverability/API-shape gap, not just model error |
| ⓑ | **Docs bug** | Docs told the model to do something wrong/broken | Reproduce from the doc snippet; find the contradicting source or sample. (e.g. GettingStarted recommended `WithOpenVRFrameLimiter()`, which always crashes overlays) |
| ⓒ | **Library bug** | The API exists and was used per docs, but the library is broken | Read the implementation; confirm the defect and the trigger path (e.g. `CleanChildRelayoutBoundary` recursing on `this`) |
| ⓓ | **Spec deviation** | Model ignored/reinterpreted the task | Compare output to the prompt. Note whether the deviation was *worse* or actually *better judgment* (e.g. choosing HMD-follow over world-space for a toast) |

Also note the **positives**: which non-obvious APIs the model got right first try, and whether it correctly
refused to hallucinate on hard tasks. Those validate the docs.

### 7. Remediate and record

- **ⓑ docs bug / ⓒ library bug** → fix on a branch. Keep commits separated by nature (a core library fix and
  a docs/API removal are different release-note lines). Run `dotnet test` before considering it done.
- **ⓐ hallucination that *should* be a real API, or a discoverability gap** → this is docs/API feedback, not
  a code fix. Per the repo's out-of-scope workflow, ask the user whether to file a GitHub issue, leave a TODO,
  or nothing — don't silently act. (Issue filing itself needs no confirmation; see memory `feedback-github-issue-no-confirmation`.)
- **ⓓ deviation** → record as a judgment-call data point; may indicate the docs communicate intent well or poorly.
- **Always** append the run's conditions (model, effort, task, docs-delivery mode) and findings to the memory
  log `vibe-coding-test-sonnet5-result` (or a new dated memory if the conditions differ a lot), so results
  accumulate into a trend rather than evaporating.

## Report format

Give the user a compact verdict, then the findings ranked by severity, each tagged ⓐ/ⓑ/ⓒ/ⓓ with a
`file:line` link into the real source. Lead with whether the docs/API held up overall (the headline the
test is designed to answer), then the bugs it shook loose.
