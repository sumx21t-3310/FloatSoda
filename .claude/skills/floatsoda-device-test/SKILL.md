---
name: floatsoda-device-test
description: >-
  FloatSoda のデバイステストを実行する — SteamVR が実際に稼働しているときにしか壊れえないシナリオと、
  鏡写しにしている Flutter 移植からの挙動差異を Codex に網羅的に列挙させ、1件ずつヘッドレス
  xunit か HMD 実機ハーネスへ振り分け、そのハーネスを構築し、オーナーに VR で実行してもらい、
  落ちたものをトリアージする。FloatSoda を実機でテストしたいとき、「実機テスト」「実機で確認」
  「デバイステスト」「HMDで動かして確認」「シナリオを洗い出したい」「Flutterとの挙動差」
  「移植差異」「device test」に言及されたとき、いまの単体テストでは決して捕まえられない
  壊れ方を尋ねられたときに使う。既存ハーネスへのシナリオ追加にも使う。
  列挙は Codex に委任する。VR での実行はオーナーのみが行う。
---

<!-- 派生互換スタブ — 直接編集禁止。正典: .agents/skills/floatsoda-device-test/ -->

**このスキルの正典は [`.agents/skills/floatsoda-device-test/SKILL.md`](../../../.agents/skills/floatsoda-device-test/SKILL.md)。そのファイルを読み、そこにある手順に従うこと。** `references/` ファイルは正典の隣、`.agents/skills/floatsoda-device-test/references/` にある。

このスタブは、Claude Code が `.agents/skills/` を読まず `.claude/skills/` を読むためだけに存在する。正典から生成された派生物で、固有の手順は持たない。このスキルの動きを変えるときは正典を編集すること — このファイルは決して編集しない。
