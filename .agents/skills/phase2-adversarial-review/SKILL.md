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

# Phase 2 敵対的監査 — Codex /goal プロンプト生成

Phase 2 の各 PR は入口レビュー(CodeRabbit + PR 単位のレビュー)を通過済み。このスキルはフェーズ末に 1 回、
**PR をまたぐ問題**を Codex に監査させるための /goal プロンプトを攻撃軸ごとに生成する。

敵対性はプロンプトの口調ではなく**受理条件**で定義する:

> **finding は、契約違反を再現する失敗テスト(red test)を添えたものだけ受理する。**

これは REVIEW.md 2章の concrete failure mode 基準の機械化であり、「敵対的に」とだけ指示された LLM が
hypothetical な指摘を量産する問題への対策でもある。書いてみたら通ってしまったテスト(反証)にも
価値があるので、白判定として報告に残させる。

## レビュー層の中での位置づけ

層の分担の正典は REVIEW.md 9章。この監査が**報告してはいけないもの**:

- 重要度 7〜8(style / maintainability / 具体的影響のない performance)— 入口レビュー(CodeRabbit)の領分
- SteamVR 実行時にしか観測できない挙動 — `floatsoda-device-test` の領分
- docs の分かりにくさ・API の発見性 — `floatsoda-junior-coder-test` の領分
- `.agents/skills/floatsoda-device-test/references/known-divergences.md` で `Label: deliberate` とされた
  意図確定済みの差異(それ以外のラベル — unlabelled / port mistake — のエントリは意図が未確定なので、
  除外どころか parity 軸の検証候補になる)

## 手順

### 1. 前提確認

FloatSoda リポジトリで以下を確認する(独立なコマンドは並列で実行してよい):

```bash
cd "$USERPROFILE/projects/libs/FloatSoda"
git fetch --all --quiet
git rev-parse --short origin/main
gh issue view 178 --json body -q .body   # Phase 2 チェックリスト → 監査対象インベントリ
gh pr list --state open                  # マージ漏れの Draft PR が残っていないか
```

- 監査対象インベントリは #178 のチェックリストのマージ済み項目から作る。未マージの Draft PR が残っている場合は、
  その旨をオーナーに報告し、監査を今始めるか PR のマージを待つかを確認する。
- **(任意)入口レビューの残渣回収**: #178 の各 Issue に紐づくマージ済み PR を辿り、
  `gh api "repos/{owner}/{repo}/pulls/<番号>/comments"` から `coderabbitai[bot]` の未対応指摘を抽出する。
  「疑われたが検証されていない仮説」の在庫として、該当する軸のプロンプトの「検証候補」に添付する。
  やるかどうかはオーナーに確認する。

### 2. 攻撃軸の分割

軸ごとに独立した Codex セッションを走らせる(コンテキストを集中させ、後半の監査が雑になるのを防ぐ)。
**確認項目をこのスキルに複製しない** — REVIEW.md が正典で、Codex に直接読ませる。

| 軸 | slug | 確認項目の正典 |
|---|---|---|
| Flutter parity | `parity` | REVIEW.md 6章 + docs/APIDesign.md「判断原則」 |
| ツリーライフサイクル | `lifecycle` | REVIEW.md 4章「ツリーの所有権とライフサイクル」 |
| 差分更新 | `incremental` | REVIEW.md 4章「差分更新」 |
| Layer / スレッド | `layer` | REVIEW.md 4章「Layer」 |

ブランチは軸ごとに `test/phase2-adversarial-<slug>`。新規テストファイルのみ追加させるため、
軸を並行で走らせてもファイル競合しない。並行本数はオーナー判断。

### 3. /goal プロンプトの生成

軸ごとに以下のテンプレートで生成する。Codex はこのプロンプトだけを見て自走するので、自己完結させること。
`<...>` のプレースホルダー(リポジトリと flutter_reference の絶対パス、SHA、インベントリ、軸情報)は
生成時に実際に埋める(このスキル本体は公開リポジトリに入るので、テンプレート側には絶対パスを書かない)。

```text
/goal
GOAL: FloatSoda リポジトリ(<リポジトリの絶対パス>)の Phase 2 成果物に対する敵対的監査。
攻撃軸は「<軸名>」。コードを直すのではなく、契約違反を再現する失敗テストで証明する。

コンテキスト:
- レビュー基準の正典はリポジトリの REVIEW.md。まず全文を読むこと。
  この軸の確認項目は <REVIEW.md の該当節> をそのまま使う。
- 監査対象は Phase 2 で実装されたウィジェット群: <#178 から列挙したインベントリ>
- Flutter parity の正典クローン: <flutter_reference の絶対パス>。
  期待値は Flutter 本家の実装・公式テストから引く。FloatSoda 側で期待値を想像しない。
- 既知差異の台帳は .agents/skills/floatsoda-device-test/references/known-divergences.md。
  `Label: deliberate` のエントリは finding にしない。unlabelled / port mistake のエントリは
  意図が未確定なので、この軸に該当すれば検証候補として扱う。not ported とされた機能の欠落自体は、
  既に台帳へ記録済みなので finding にしない(finding は新規の問題に限る)。
- <(任意)検証候補: 入口レビューで未対応だった CodeRabbit 指摘のうちこの軸に該当するもの>
- ブランチは origin/main (<SHA>) から test/phase2-adversarial-<slug> を切り、
  オーナーの checkout ではなく専用 worktree で作業する。

受理条件(これを満たすものだけを finding として数える):
1. 「どの入力・状態列で、何が壊れるか」を再現する xunit テストがあり、dotnet test で実際に失敗する
2. 期待値の根拠が正典で示せる(REVIEW.md 3章の優先順位。「既存実装がそうなっている」は根拠にならない)
3. テスト名は `対象メンバー名_条件_期待結果`(条件・期待結果は日本語)。
   既存テストファイルへ追記せず、新規ファイルとして追加する(例: StackParityFindingsTest.cs)
4. 書いてみたら通ってしまったテスト(反証)は finding ではない。コミットせず、白判定として報告に残す

完了条件:
1. 監査対象の全ウィジェットについて、この軸の確認項目を検討した(検討できなかったものは理由つきで報告)
2. 黒判定の失敗テストだけを test/phase2-adversarial-<slug> にコミットし、push した。
   コミット件名は日本語(例: `test: RenderStack の <契約> 違反を再現する失敗テストを追加`)
3. Draft PR を作成した。タイトル・本文は日本語で、本文は次の3節を含む:
   「## 黒判定」(重要度順。各項目に、対象 / 壊れる入力・状態 / 正典の根拠 / テストへのリンク)
   「## 白判定」(疑ったが反証された仮説と、反証の根拠)
   「## 未検証」(理由つき)
   冒頭に「この PR の CI は黒判定の証明として意図的に失敗する」と明記する
4. リポジトリの変更が tests/ 配下の新規テストファイルの追加**だけ**である
   (src/ はもちろん、docs/・設定・プロジェクトファイルも変更しない)

制約:
- 修正しない。修正案は PR 本文の黒判定の項に1〜2行で添えるだけにする
- 重要度 7〜8(style / maintainability / 具体的影響のない performance)は報告しない(入口レビューの領分)
- SteamVR 実行時にしか観測できない挙動(実機の入力経路等)は追わない(floatsoda-device-test の領分)
- docs の分かりにくさ・API の発見性は追わない(floatsoda-junior-coder-test の領分)
- 到達可能性を示せない hypothetical を finding にしない(REVIEW.md 2章)
```

**`layer` 軸の受理条件の特例**: clone 独立性(clone 後に元のレイヤーを変異させても clone が影響を
受けないこと)は決定的な red test が書けるので原則どおり。**真のスレッド実行順序に依存する race だけは**、
red test の代わりに「clone でコピーされないフィールド / メインスレッド側の変異点 / レンダースレッド側の
読取点」の3点を具体的なコード経路で提示したものを受理する。この軸のプロンプトでは受理条件 1 に
この差し替えを明記する。

### 4. 起動と検証

- **デフォルトはプロンプト生成まで。** Codex の起動はオーナーが行う(オーナーが明示的に依頼した場合は
  codex-runner で起動してよい)。
- Codex 完了後、受理条件を機械的に検証する:
  1. 監査ブランチで `dotnet test` を実行し、コミットされた各テストが**実際に失敗する**こと
  2. `git diff --name-only origin/main` の全パスが tests/ 配下の**新規テストファイル**であること
     (src/ に限らず、docs/・設定・プロジェクトファイルなど許可外のパスが1つでもあれば差し戻す)
  3. PR 本文の黒判定に重要度 7〜8 や住み分け違反(実機挙動・docs)が混ざっていないこと
  4. 白判定・未検証の節が省略されていないこと
- 検証に落ちた項目は codex-runner の resume で差し戻す。

### 5. 報告

軸ごとの黒 / 白 / 未検証を重要度順に集約してオーナーに報告する。**修正はしない**(AGENTS.md の
Working Style)。修正するか・Issue 化するかはオーナー判断。

監査ブランチは **test-only のまま維持する**(完了条件 4 と手順 4 の検証がこの前提)。オーナーが修正する
場合は、finding ごとに別ブランチ・別 PR を切り、該当する失敗テストをそこへ cherry-pick して修正コミットと
同居させる — red → green の遷移が修正 PR 内で示され、失敗テストがそのまま regression test
(CONTRIBUTING.md のテスト観点)になる。全 finding の扱いが決まったら、監査 PR はマージせずクローズする。

## 注意

- 白判定は監査の網羅性の証拠なので、報告から省かせない。「finding ゼロ」と「何も疑わなかった」を
  区別できなくなる。
- 監査 PR の CI が赤いのは仕様(黒判定の証明)。Draft PR の通常の完了条件「CI 成功」はこの PR には
  適用しない。PR 本文冒頭の明記が漏れていないか確認する。
- Phase 2 の PR 単位の差分レビューをやり直させない。この監査の価値は横断面にある。

## 出典と未確定事項

受理条件(red test)、CodeRabbit との層分担、攻撃軸の切り方は 2026-08-27 のオーナーとの検討で決定。
次は未決:

- 軸を何本まで並行起動するか
- 入口レビューの残渣回収(手順 1 の任意項目)を毎回やるか
- Phase 3 以降への一般化(スキル名を汎用化するか、フェーズごとに複製するか)

運用してみて方針が決まったら、この節ごと更新すること。
