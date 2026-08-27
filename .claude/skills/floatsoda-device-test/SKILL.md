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

<!-- Derived compatibility stub — DO NOT EDIT. Canon: .agents/skills/floatsoda-device-test/ -->

**The canon for this skill is [`.agents/skills/floatsoda-device-test/SKILL.md`](../../../.agents/skills/floatsoda-device-test/SKILL.md). Read that file and follow the procedure there.** Its `references/` files live beside it, in `.agents/skills/floatsoda-device-test/references/`.

This stub exists only because Claude Code reads `.claude/skills/` and not `.agents/skills/`. It is generated from the canon and holds no procedure of its own. To change how this skill works, edit the canon — never this file.
