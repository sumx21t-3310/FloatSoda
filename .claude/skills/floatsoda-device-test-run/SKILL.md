---
name: floatsoda-device-test-run
description: >-
  FloatSoda の実機実行すべてを運転する — カタログサンプル(samples/FloatSoda.Samples.*)か
  floatsoda-device-test-gen が作ったハーネスシナリオを、実 HMD 上で1つずつ起動し、オーナーに
  見るべき点を伝え、項目ごとに yes/no の判定を取り、記録し、止め、次へ進み、最後に落ちたものを
  トリアージする。音声セッション(オーナーは HMD を被ったまま声で答える)を前提に設計しているが、
  テキストでも動く。実機で何かを動かしたいとき、「実機テスト」「実機で確認」「HMDで動かして確認」
  「サンプルを実機で見る」「目視テスト」「音声でテスト」「サンプル一巡」「device test を回して」
  「#188 のテスト」に言及されたとき、サンプルやハーネスシナリオが VR で通るか尋ねられたときに使う。
  判定はオーナーだけのもの。エージェントは運転・記録・事後のトリアージを担う。
---

<!-- 派生互換スタブ — 直接編集禁止。一次情報: .agents/skills/floatsoda-device-test-run/ -->

**このスキルの一次情報は [`.agents/skills/floatsoda-device-test-run/SKILL.md`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md)。そのファイルを読み、そこにある手順に従うこと。** `references/` ファイルは一次情報の隣、`.agents/skills/floatsoda-device-test-run/references/` にある。

このスタブは、Claude Code が `.agents/skills/` を読まず `.claude/skills/` を読むためだけに存在する。一次情報から生成された派生物で、固有の手順は持たない。このスキルの動きを変えるときは一次情報を編集すること — このファイルは決して編集しない。
