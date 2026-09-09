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

# FloatSoda リリース

## このスキルが存在する理由

リリースパイプラインは半分しか自動化されていない。[`.github/workflows/release.yml`](../../../.github/workflows/release.yml)
が引き継ぐのは `git push origin vX.Y.Z` からで、build → test → pack → NuGet を処理する。その
タグより前のすべてと、タグの後の GitHub Release は人間の儀式 — そして儀式は風化する。この
リポジトリでの実証: `v0.2.0` / `v0.3.0` / `v0.3.1` はタグが打たれ NuGet に公開されたが、
**GitHub Release は一度も作成されず**、CHANGELOG の compare リンクは `v0.3.0...main` を指した
まま、`[0.3.1]` のリンク行は丸ごと欠けていた。それらの手順が記憶に依存しないようにするのが
このスキル。

## 唯一の正典

[`RELEASING.md`](../../../RELEASING.md) が**ポリシーの正典**で、このスキルはその執行者。

- **毎回の実行の最初に `RELEASING.md` を読み**、記憶ではなくそこに書いてあることに従う。
- このスキルと `RELEASING.md` が食い違ったら `RELEASING.md` が勝つ — そして食い違いを口に出す。
  スキルが直されるように。
- **ポリシーの例外をここに書き込まない。** ゲートの動かし方を変えるとオーナーが決めたら(たとえば
  ジュニアコーダーゲートを Phase 完了まで先送りする、など)、その変更は `RELEASING.md` に属する。
  このスキルに「ただし当面は代わりに X する」という分岐を持たせてはならない。

## ハードストップ — オーナーの明示的な承認が要るもの

このワークフローの他のすべては可逆。これらはそうではない。

1. **`git push origin vX.Y.Z`** — Trusted Publishing 経由で NuGet に公開される。公開された
   バージョンは差し替えられず、deprecate / unlist しかできない。
2. **`gh release create`** — 公開の、外向きの操作。

準備を整え、diff と計画を提示して、止まる。前の手順が「明らかに」示唆していたからといって、
タグを push しない。

## ワークフロー

タスクツールで追跡すること — リリースは手順が多く、1つ抜けるのが既定の失敗モードになる。

### 0. 事前確認

- 作業ツリーがクリーン(`git status --porcelain` が空)で `main` にいて、`origin/main` と同期
  している。リリースをフィーチャーブランチから切ってはならない。
- リリースコミットで CI がグリーン(`gh run list --branch main --limit 5`)。
- `gh auth status` が通る(手順1、7、8 で必要)。

### 1. リリーススコープを確定する

```bash
git describe --tags --abbrev=0
```

そのタグから `HEAD` までを diff する(`git log --oneline <prev>..HEAD`、PR 単位の視点には
`--merges`)。3方向でクロスチェックし、不一致は黙って直さず報告する:

- **コミット vs `[Unreleased]`** — CHANGELOG エントリの無い利用者可視の変更、またはコミットの
  無いエントリ。
- **マイルストーン** — `gh api repos/sumx21t-3310/FloatSoda/milestones` で Phase 一覧を取り、
  `gh issue list --milestone "<title>" --state closed` でこのリリースが実際に閉じる Issue を見る。
  上のコミットで修正されたのにまだ開いている Issue が、報告すべき発見。
- **public API サーフェス** — この範囲に public API の追加・変更はあるか? あるなら `docs/` が
  それに合わせて更新済みかを確認する(CONTRIBUTING は新しい public API に docs 先行を要求する)。
  手順5のジュニアゲートが読むのはまさにそのページだから。

### 2. バージョンを決めて設定する

SemVer のステップを理由つきで提案し、編集の前にオーナーの確認を得る。0.x のルール:
breaking change は **minor** を上げ、patch は後方互換の修正のみ。そのうえで
[`Directory.Build.props`](../../../Directory.Build.props) の `<Version>` を更新する。

### 3. CHANGELOG を仕上げる

`RELEASING.md` §3 に従う。機械的な部分こそ実際に壊れた前歴があるので、意識して行う:

- `## [Unreleased]` → `## [X.Y.Z] - YYYY-MM-DD`(今日の実際の日付を使う)。その上に空の
  `## [Unreleased]` を新しく置く。
- **ファイル末尾のリンク定義**: `[Unreleased]` を `.../compare/vX.Y.Z...main` に付け替え、
  **かつ** `[X.Y.Z]: .../releases/tag/vX.Y.Z` の行を追加する。最後の数行を読み返して検証する —
  v0.3.0 と v0.3.1 の間で静かに腐ったのがこの手順。
- エントリは**利用者**の視点から、日本語で、Keep a Changelog の見出しの下に書く。

### 4. 検証 CI と同じ実行を回す

`RELEASING.md` §4 の3コマンド(`build --configuration Release`、続いて両テストプロジェクトを
`--no-build` で)。そのあと、タグが CI で落ちるのを防ぐ2つの事前チェック:

- **タグ/バージョンの一致** — release.yml の `Verify tag matches` ステップと同じルール: タグ名
  から先頭の `v` を除いたものが `<Version>` と等しいこと。タグがローカルにもリモートにも既存で
  ないことも確認する(`git tag -l vX.Y.Z`、`git ls-remote --tags origin vX.Y.Z`)。
- **pack の内容** — `dotnet pack --configuration Release --no-build --output <リポジトリ外のディレクトリ>`
  を実行し、`.nupkg` の一覧が意図した公開パッケージと正確に一致するか確認する。**出力は必ず
  リポジトリ外へ**(セッションのスクラッチパッドを使う): `artifacts/` は `.gitignore` に無いため、
  ツリー内で pack すると未追跡ファイルが残る。パッケージ一覧はオーナーに報告する — 想定外の
  パッケージの出現や消失は、リリースを止めるべきサプライズ。

### 5. ジュニアコーダーゲート

リリースコミット上で [`floatsoda-junior-coder-test`](../floatsoda-junior-coder-test/SKILL.md)
スキルを**リリースゲート**モードで起動する。合格基準は `RELEASING.md` が持つ — そこで読み、
文字どおりチェックする。このスキルが責任を持つのは2つ:

- **テーマをローテーションする。** memory ログ(`vibe-coding-test-sonnet5-result`)で前回リリースが
  何を使ったかを確認し、別のものを選ぶ。サーフェスが1つのシナリオに過適合しないようにするため。
- **ブロックを尊重する。** ⓑ docs バグ、ⓒ ライブラリバグは**リリースをブロックする**。修正して
  ゲートを再実行する — 既知の ⓑ/ⓒ を「次で直す」という約束つきでタグ付けへ進まない。

### 6. リリースコミットを main に載せる

手順2〜3の `<Version>` と `CHANGELOG.md` の編集は、この時点ではまだ**未コミット**。いまタグを
打つと古い `HEAD` にタグが付き、`release.yml` は `Verify tag matches Directory.Build.props version`
ステップで落ち、CHANGELOG はリリースから漏れる。

`main` は直接 push を受け付けないので、通常の PR フローで行う(正確なコマンドは `RELEASING.md`
手順6にある)。マージコミットが `origin/main` の `HEAD` であること、作業ツリーがクリーンで
あることを確認してから先へ進む。

### 7. タグを打って push — 承認のため停止

提示するもの: バージョン、CHANGELOG の該当セクション、テスト結果、pack 一覧、ゲートの評決。
承認を求める。それからタグを打つ。`main` を引き直すのではなく、手順6で検証した SHA にタグを
固定する(`git tag vX.Y.Z "$RELEASE_SHA"`)— 正確なコマンドは `RELEASING.md` 手順7にある。
ゲート以降に `origin/main` が動いていたら、実際にタグが付くコミットに対して手順5を再実行する。

### 8. 自動リリースを見届ける

Release ワークフローがグリーンになるまで `gh run watch`(または `gh run list --workflow=Release`)、
その後バージョンが NuGet で公開されたことを確認する。NuGet への push が成功した**後**に
ワークフローが失敗したら、そのまま率直に言う — バージョンは公開済みで再利用できない。直すなら
新しいパッチバージョンであって、タグの打ち直しでは決してない。

### 9. GitHub Release を作成する — 承認のため停止

このバージョンの CHANGELOG セクションからノートを起草し、タイトルを提案し
(`vX.Y.Z — <短い見出し>`、既存リリースに合わせる)、ドラフトを見せ、オーナーの承認後にのみ
作成する。

## 報告形式

日本語で(`AGENTS.md` に従う)。リリースの状態を1行で最初に(どのバージョンで、どのゲートを
通過し、何が残っているか)、次に ✅/⚠️/⛔ つきの手順ごとの表、その次に判断が要る発見 —
スコープの不一致、開いたままのマイルストーン Issue、起票する価値のあるジュニアゲートの ⓐ/ⓓ。
ファイルは `file:line` でリンクする。最後は、オーナーが承認すべき正確な次のコマンドで締める。
外向きの操作は、実際に行ったのでない限り、行ったと決して主張しない。
