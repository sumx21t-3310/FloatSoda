---
name: floatsoda-release
description: >-
  FloatSoda のリリースを最初から最後まで駆動する — 前回タグと Phase マイルストーンに照らして
  リリーススコープを確定し、バージョンを上げ、CHANGELOG を仕上げ(毎回ずれる compare リンクを
  含む)、検証 CI と同じ実行を回し、ジュニアコーダーゲートを通し、タグを打ち、自動 NuGet 公開を
  見届け、GitHub Release を作成する。FloatSoda のリリースを切りたい・準備したい・出荷したいとき、
  「リリースしたい」「リリース準備」「タグを切る」「vX.Y.Z を出す」「release FloatSoda」に
  言及されたとき、リリース前に何が残っているかを尋ねられたときに使う。タグ付け済みリリースの
  事後チェックにも使う(最後の数手順こそ最も飛ばされやすい)。タグの push と GitHub Release の
  作成は、常にオーナーの明示的な承認を必要とする。
---

<!-- 派生互換スタブ — 直接編集禁止。正典: .agents/skills/floatsoda-release/ -->

**このスキルの正典は [`.agents/skills/floatsoda-release/SKILL.md`](../../../.agents/skills/floatsoda-release/SKILL.md)。そのファイルを読み、そこにある手順に従うこと。** `references/` ファイルは正典の隣、`.agents/skills/floatsoda-release/references/` にある。

このスタブは、Claude Code が `.agents/skills/` を読まず `.claude/skills/` を読むためだけに存在する。正典から生成された派生物で、固有の手順は持たない。このスキルの動きを変えるときは正典を編集すること — このファイルは決して編集しない。
