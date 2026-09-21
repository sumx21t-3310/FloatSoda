---
name: floatsoda-device-test-gen
description: >-
  FloatSoda のデバイステストを生成する — SteamVR が実際に稼働しているときにしか壊れえないシナリオと、
  鏡写しにしている Flutter 移植からの挙動差異を Codex に網羅的に列挙させ、1件ずつヘッドレス
  xunit か HMD 実機ハーネスへ振り分け、そのテストとハーネスのシナリオを書く。このスキルはテストを
  作る側で、HMD は被らない — 実機での実行とトリアージは floatsoda-device-test-run が担う。
  VR 専用や移植差異のシナリオを洗い出したい・テストにしたいとき、「デバイステスト」
  「シナリオを洗い出したい」「Flutterとの挙動差」「移植差異」「device test」「ハーネスに
  シナリオを足して」に言及されたとき、いまの単体テストでは決して捕まえられない壊れ方を
  尋ねられたときに使う。列挙は Codex に委任する。
---

<!-- 派生互換スタブ — 直接編集禁止。一次情報: .agents/skills/floatsoda-device-test-gen/ -->

**このスキルの一次情報は [`.agents/skills/floatsoda-device-test-gen/SKILL.md`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md)。そのファイルを読み、そこにある手順に従うこと。** `references/` ファイルは一次情報の隣、`.agents/skills/floatsoda-device-test-gen/references/` にある。

このスタブは、Claude Code が `.agents/skills/` を読まず `.claude/skills/` を読むためだけに存在する。一次情報から生成された派生物で、固有の手順は持たない。このスキルの動きを変えるときは一次情報を編集すること — このファイルは決して編集しない。
