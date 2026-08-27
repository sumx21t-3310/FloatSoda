---
name: phase2-adversarial-review
description: >
  FloatSoda の Phase 2 完了時に、フェーズ全体のサーフェスを Codex に敵対的監査させるための
  /goal プロンプト群を生成する。finding は「再現する失敗テスト(red test)を添えたもの」だけを
  受理する契約で、PR をまたぐ問題(ライフサイクル・差分更新・Flutter parity・Layer clone)を狙う。
  「敵対的レビュー」「adversarial review」「フェーズ末の横断レビュー」「Phase2の総点検」
  「PRをまたぐ問題を洗いたい」「red testで証明させたい」などの文脈で発火する。
  個別 PR のレビューや、次タスクの委任(phase2-codex-goal)とは別物。
---

<!-- Derived compatibility stub — DO NOT EDIT. Canon: .agents/skills/phase2-adversarial-review/ -->

**The canon for this skill is [`.agents/skills/phase2-adversarial-review/SKILL.md`](../../../.agents/skills/phase2-adversarial-review/SKILL.md). Read that file and follow the procedure there.** Its `references/` files live beside it, in `.agents/skills/phase2-adversarial-review/references/`.

This stub exists only because Claude Code reads `.claude/skills/` and not `.agents/skills/`. It is generated from the canon and holds no procedure of its own. To change how this skill works, edit the canon — never this file.
