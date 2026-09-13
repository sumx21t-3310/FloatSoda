---
name: floatsoda-device-test-run
description: >-
  The driver for every on-device run of FloatSoda: launch one target at a time on the real HMD —
  a catalog sample (samples/FloatSoda.Samples.*) or a harness scenario built by
  floatsoda-device-test-gen — tell the owner what to look at, take a yes/no verdict per item, record
  it, stop, next, then triage what failed. Designed for a voice session (the owner is wearing
  the headset and answers by speaking), but works over text too. Use whenever the user wants to
  run anything on real hardware, mentions "実機テスト", "実機で確認", "HMDで動かして確認",
  "サンプルを実機で見る", "目視テスト", "音声でテスト", "サンプル一巡", "sample walkthrough",
  "#188 のテスト", or asks whether the samples or harness scenarios pass in VR. Verdicts are the
  owner's alone; the agent drives, records, and triages afterwards.
---

# FloatSoda Device Test — Run

## What this is, and what it is not

This skill is the **driver for every on-device run**: one target at a time, a few yes/no items,
a verdict, next. It runs two kinds of target with the same loop:

| Target | Where it comes from | Items |
|---|---|---|
| Catalog sample `samples/FloatSoda.Samples.<Name>` | Issue #188, layer 1. Each is also an integration scenario: "if you write what the docs say, does it actually look like that?" (`CONTRIBUTING.md`, サンプルを追加する場合の規約) | `references/checklist.md`, 1–3 per sample |
| Harness scenario `tests/FloatSoda.DeviceTest --scenario <Id>` | Produced by `floatsoda-device-test-gen` (enumerate → route → build). Each scenario carries its own expected result | 1 per scenario: the scenario's expected result, spoken as a yes/no question |

The split with `floatsoda-device-test-gen` is producer / runner: that skill decides *what* could
break and writes the tests (xunit for anything headless, harness scenarios for the rest); this
skill is the only one that puts a headset on. Triage of what falls out lives here, because the
results do.

## Roles

| Who | Does |
|---|---|
| Owner (in the HMD) | Opens the SteamVR dashboard, looks, answers each item with OK / NG (+ one phrase on NG) |
| Agent (this skill) | Builds, launches one sample at a time, reads out the instruction, records verdicts, stops the process, triages afterwards |

The owner never leaves the headset to type. If the session has a voice channel, speak the
instruction. If not, print it as one short line — the owner reads it through the desktop mirror.

**Voice sessions (e.g. Codex Voice mode):** start the chat in voice mode *before* loading this
skill — a chat started in text mode only gets dictation, not a conversational thread. Keep the walk
(step 1) in the voice thread itself: it needs turn-taking and interruption. Triage (step 2) may be
delegated to a separate task; hand it the path of `results.md`, which is the shared state.

## Ground rules

- **One sample process at a time.** Every sample registers its own `AppKey`; two processes of the
  same sample fight over the overlay, and a leftover process from the previous sample stays on
  screen (there is no window teardown API, issue #218). Stop before launching the next.
- **Verdicts are the owner's.** Do not infer a verdict from a screenshot or from the absence of a
  crash. Record exactly what the owner said.
- **Do not fix code during the walk.** An NG is a note, not a work order. Triage comes after the
  last sample, when the headset is off.
- **Keep the instruction short.** One sentence, one thing to look at. The read-aloud lines in the
  checklist are sized for this (roughly 20–40 characters).
- **Launch only the samples.** Never start VRChat as part of this loop — on the owner's machine
  VRChat start is the known GPU-hang trigger (see `Memory/environment.md` in the shared vault).

## Procedure

### 0. Prepare (headset off)

1. `dotnet build FloatSoda.slnx` — 0 errors. Samples are built as
   `samples/FloatSoda.Samples.<Name>/bin/Debug/net10.0/FloatSoda.Samples.<Name>.exe`.
2. Confirm SteamVR is running (`vrserver` / `vrmonitor` processes). Do not start it yourself; the
   owner does.
3. Create the results file from `references/results-template.md` at
   `$HOME/tmp/floatsoda-walkthrough/<yyyy-MM-dd>/results.md` (outside the repository — it carries
   local paths and is never committed).
4. Optionally start the read-only stack monitor beside you (`vr-stack-watch -Watch`, a user-level
   skill on the owner's machine) so a GPU hang during the walk gets a timestamp.
5. Read `references/checklist.md`. It was source-verified on 2026-09-13 against the samples on
   branch `test/188-all-samples`; **spot-check the `Demo.cs` line references for the samples you
   are about to run** — samples change, the checklist does not follow automatically.

### 1. Walk (headset on)

Order: catalog samples first (layout → constraints → painting → input last, as listed at the end
of the checklist), then harness scenarios in the order the enumeration gave them. For each target:

1. `references/run-sample.ps1 <Name>` for a sample, or `references/run-sample.ps1 -Scenario <Id>`
   for a harness scenario — stops the previous process, launches this one, waits a few seconds,
   and reports whether it is still alive. If it exited early, record the last log lines as the
   verdict (`CRASH`) and move on.
2. Say the target name, then the first read-aloud line (for a scenario: its expected result,
   rephrased as one yes/no question).
3. Wait for OK / NG. On NG, ask for one phrase describing what is wrong, nothing more.
4. Append one row per item to `results.md` immediately — do not batch.
5. Repeat for the remaining items, then `run-sample.ps1 stop`.

For the three input samples (Listener, GestureDetector, PointerRegion), say the operating step
before the item ("controller ray on the left box, pull the trigger"). Pointer input only reaches
dashboard overlays (issue #182); these samples use `DashboardWindow`, so the dashboard must be
open.

### 2. Triage (headset off)

For every NG / CRASH row:

1. Try to reproduce headlessly first (`src/FloatSoda.Testing` bitmap renderers or an xunit test).
   A visual NG that reproduces headlessly is a regression test waiting to be written.
2. Propose a classification — library bug / sample bug / docs gap / intended (the sample's
   `## Flutterとの違い` already says so) — and **let the owner confirm** before filing. For a
   harness scenario from axis B (Flutter divergence) the label is deliberate / not ported / port
   mistake, and it stays provisional until the owner confirms it.
3. File confirmed library / sample / docs issues. Strip local paths, machine and user names from
   anything pasted (public repository).
4. Append every owner-confirmed divergence to
   `.agents/skills/floatsoda-device-test-gen/references/known-divergences.md`, so the next
   enumeration starts further along. A divergence settled as "deliberate" is a `docs/` gap until
   documented.
5. If an NG turns out to be a checklist error (the sample changed, the item was wrong), fix
   `references/checklist.md` in a PR that changes nothing else.

## Report format

Lead with the tally: samples walked / items answered / NG / CRASH. Then the NG rows verbatim with
the owner's phrase, each with the sample and item number. Then the proposed classification per NG,
clearly marked as proposed. Say which checklist line references you spot-checked before the walk.
