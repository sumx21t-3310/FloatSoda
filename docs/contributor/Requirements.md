# Requirements — 変更後も何を維持するか

> **骨組み** — [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) 手順4の骨組みです。本文は次の現行ページから、契約の部分を分けて作ります。

## 移す元

| 現行ページ | 移す部分 |
|---|---|
| [APIDesign](APIDesign.md) | 必須条件の部分 |
| [RenderObjects](RenderObjects.md) | 「差分更新」節の契約(プロパティを変更したら `MarkNeedsLayout()` / `MarkNeedsPaint()` を呼ぶ) |

ツリーの不変条件(所有権・ライフサイクル・差分更新・Layer)は [REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」に置きます。ここからはリンクします。確認済みの Flutter との差異は [`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) にあります。

## 各ページの構成

[WritingDocumentation 5 章](WritingDocumentation.md#requirements) の Requirements テンプレートに寄せます。

- 適用範囲
- 必須条件
- 不変条件
- Observable behavior
- 破ったときの failure mode
- 検証方法
- Flutter との関係
- 関連 Design / Architecture
