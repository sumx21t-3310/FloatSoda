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

<!-- 派生互換スタブ — 直接編集禁止。正典: .agents/skills/phase2-adversarial-review/ -->

**このスキルの正典は [`.agents/skills/phase2-adversarial-review/SKILL.md`](../../../.agents/skills/phase2-adversarial-review/SKILL.md)。そのファイルを読み、そこにある手順に従うこと。** `references/` ファイルは正典の隣、`.agents/skills/phase2-adversarial-review/references/` にある。

このスタブは、Claude Code が `.agents/skills/` を読まず `.claude/skills/` を読むためだけに存在する。正典から生成された派生物で、固有の手順は持たない。このスキルの動きを変えるときは正典を編集すること — このファイルは決して編集しない。
