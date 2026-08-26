---
name: floatsoda-release
description: >-
  Drive a FloatSoda release end to end — pin the release scope against the previous tag and the
  Phase milestones, bump the version, finalize CHANGELOG (including the compare links that keep
  drifting), run the same verification CI runs, put the release through the junior-coder gate,
  then tag, watch the automated NuGet publish, and create the GitHub Release. Use whenever the
  user wants to cut, prepare, or ship a FloatSoda release, mentions "リリースしたい", "リリース準備",
  "タグを切る", "vX.Y.Z を出す", "release FloatSoda", or asks what still has to happen before a
  release. Also use for a post-mortem check of a release that was already tagged (the last steps
  are the ones most often skipped). Tag push and GitHub Release creation always require the
  owner's explicit go-ahead.
---

# FloatSoda Release

## Why this exists

The release pipeline is only half automated. [`.github/workflows/release.yml`](../../../.github/workflows/release.yml)
takes over at `git push origin vX.Y.Z` and handles build → test → pack → NuGet. Everything before
that tag, and the GitHub Release after it, is a human ritual — and rituals drift. Evidence from this
repo: `v0.2.0` / `v0.3.0` / `v0.3.1` were tagged and published to NuGet but **no GitHub Release was
ever created** for them, and the CHANGELOG compare links still point at `v0.3.0...main` with the
`[0.3.1]` link line missing entirely. This skill exists so those steps stop depending on memory.

## Single source of truth

[`RELEASING.md`](../../../RELEASING.md) is the **policy canon**; this skill is its executor.

- **Read `RELEASING.md` at the start of every run** and follow what it says, not what you remember.
- If this skill and `RELEASING.md` disagree, `RELEASING.md` wins — and say so, so the skill gets fixed.
- **Never encode a policy exception here.** If the owner decides a gate should work differently (for
  example that the junior-coder gate is deferred until a Phase completes), that change belongs in
  `RELEASING.md`. This skill must not carry a "but for now we do X instead" branch.

## Hard stops — these need the owner's explicit go-ahead

Everything else in this workflow is reversible; these are not.

1. **`git push origin vX.Y.Z`** — publishes to NuGet through Trusted Publishing. A published version
   cannot be replaced, only deprecated/unlisted.
2. **`gh release create`** — public, outward-facing.

Do the preparation, present the diff and the plan, and stop. Never push a tag because an earlier
step "obviously" implied it.

## Workflow

Track it with the task tools — a release has enough steps that a dropped one is the default failure.

### 0. Preflight

- Working tree clean (`git status --porcelain` empty) and on `main`, up to date with `origin/main`.
  A release must not be cut from a feature branch.
- CI green on the release commit (`gh run list --branch main --limit 5`).
- `gh auth status` works (steps 1, 7 and 8 need it).

### 1. Pin the release scope

```bash
git describe --tags --abbrev=0
```

Then diff that tag to `HEAD` (`git log --oneline <prev>..HEAD`, and `--merges` for the PR-level view).
Cross-check three ways and report the mismatches rather than silently fixing them:

- **commits vs `[Unreleased]`** — user-visible changes with no CHANGELOG entry, or entries with no commit.
- **milestones** — `gh api repos/sumx21t-3310/FloatSoda/milestones` for the Phase list, then
  `gh issue list --milestone "<title>" --state closed` to see which issues this release actually closes.
  Issues fixed by the commits above but still open are the finding to surface.
- **public API surface** — did anything in this range add or change public API? If so, confirm `docs/`
  was updated with it (CONTRIBUTING requires docs-first for new public API), because the junior gate in
  step 5 will be reading exactly those pages.

### 2. Decide and set the version

Propose the SemVer step with the reasoning, and let the owner confirm before editing. 0.x rules:
a breaking change bumps **minor**, patch is backward-compatible fixes only. Then update `<Version>`
in [`Directory.Build.props`](../../../Directory.Build.props).

### 3. Finalize CHANGELOG

Per `RELEASING.md` §3. The mechanical part that has actually broken before, so do it deliberately:

- `## [Unreleased]` → `## [X.Y.Z] - YYYY-MM-DD` (use today's real date), with a fresh empty
  `## [Unreleased]` above it.
- **Link definitions at the end of the file**: re-point `[Unreleased]` to `.../compare/vX.Y.Z...main`
  **and** add the `[X.Y.Z]: .../releases/tag/vX.Y.Z` line. Verify by reading the last lines back —
  this is the step that silently rotted between v0.3.0 and v0.3.1.
- Entries are written from the **user's** point of view, in Japanese, under Keep a Changelog headings.

### 4. Run the same verification CI runs

The three commands in `RELEASING.md` §4 (`build --configuration Release`, then both test projects
with `--no-build`). Then the two pre-flight checks that keep the tag from failing in CI:

- **Tag/version parity** — same rule as release.yml's `Verify tag matches` step: tag name minus the
  leading `v` must equal `<Version>`. Also confirm the tag does not already exist locally or on the
  remote (`git tag -l vX.Y.Z`, `git ls-remote --tags origin vX.Y.Z`).
- **Pack contents** — `dotnet pack --configuration Release --no-build --output <dir outside the repo>`,
  then list the `.nupkg` files and check the set is exactly the intended public packages. **Output must
  go outside the repo** (use the session scratchpad): `artifacts/` is not in `.gitignore`, so packing
  in-tree leaves untracked files behind. Report the package list to the owner — an unexpected package
  appearing or disappearing is a release-blocking surprise.

### 5. Junior-coder gate

Invoke the [`floatsoda-junior-coder-test`](../floatsoda-junior-coder-test/SKILL.md) skill on the
release commit, in its **release gate** mode. `RELEASING.md` owns the pass criteria — read them there
and check them off literally. Two things this skill is responsible for:

- **Rotate the theme.** Check the memory log (`vibe-coding-test-sonnet5-result`) for what the previous
  release used and pick a different one, so the surface isn't overfit to one scenario.
- **Respect the block.** A ⓑ docs bug or ⓒ library bug **blocks the release**. Fix it, then re-run the
  gate — do not proceed to step 6 with a known ⓑ/ⓒ and a promise to fix it next time.

### 6. Tag and push — stop for approval

Present: version, CHANGELOG section, test results, pack list, gate verdict. Ask for the go-ahead.
Only then `git tag vX.Y.Z` and `git push origin vX.Y.Z`.

### 7. Watch the automated release

`gh run watch` (or `gh run list --workflow=Release`) until the Release workflow is green, then confirm
the version is live on NuGet. If the workflow fails **after** a successful NuGet push, say so plainly —
the version is public and cannot be reused; the fix is a new patch version, never a re-tag.

### 8. Create the GitHub Release — stop for approval

Draft the notes from the CHANGELOG section for this version, propose the title
(`vX.Y.Z — <short headline>`, matching the existing releases), show the draft, and create it only after
the owner approves.

## Report format

Japanese, per `AGENTS.md`. Lead with the release's state in one line (what version, which gate passed,
what is left), then a step-by-step table with ✅/⚠️/⛔, then the findings that need a decision —
scope mismatches, milestone issues left open, junior-gate ⓐ/ⓓ items worth filing. Link files as
`file:line`. End with the exact next command the owner has to approve, never with a claim that
something outward-facing was done unless it was.
