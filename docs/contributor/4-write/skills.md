← [コントリビューター](../Home.md)

# skill を足す・直す

## この手順でできること

エージェント向けの skill を、正しい場所と名前で足せます。手順が終わると、`.agents/skills/<name>/SKILL.md` に正本があり、`.claude/skills/<name>/SKILL.md` に正本へのポインタだけのスタブがある状態になります。

正本を1か所に決めているのは、コピーが2つあると片方だけ更新されてずれるためです。skill の唯一の一次情報は `.agents/skills/` で、追加も変更も必ずこちらから行います(CONTRIBUTING.md の「エージェント向け skill を追加・変更する場合」)。`.claude/skills/` は、`.agents/skills/` を読まない Claude Code のための派生物です。直接編集しません。

手順は5つです。

1. 名前を決める
2. 正本を書く
3. Claude Code 用のスタブを置く
4. 言語を確かめる
5. PR を出す

自分の作業が skill に向くかは、エージェントに繰り返し渡す手順書にしたいかで判断してください。

## 前提条件

リポジトリをクローンしていれば始められます。雛形にする skill を、`.agents/skills/` から1つ選んでください。たとえば `floatsoda-release` は、正本が `.agents/skills/floatsoda-release/`、スタブが `.claude/skills/floatsoda-release/SKILL.md` にあります。

エージェントと一緒に作業する場合は、[エージェントと一緒に作業する](../3-prepare/work-with-agents.md) も読んでください。

## 手順1: 名前を決める

名前は `<project>-<subject>-<verb>` の順にします。例は `floatsoda-device-test-gen` と `floatsoda-device-test-run` です。広い語を先に、動詞を最後に置くと、対になる skill が一覧で隣り合います。

skill のディレクトリはネストできません。Claude Code は `.claude/skills/*/SKILL.md` の1階層だけを探すためです。分類は、ディレクトリではなく名前の接頭辞で表します。

**できた?** 名前が `floatsoda-` で始まり、最後が動詞です。

## 手順2: 正本を書く

`.agents/skills/<name>/SKILL.md` に本体を書きます。参照ファイルは `.agents/skills/<name>/references/` に置きます。

frontmatter の `description` は、エージェントが skill を使うかを判定する材料です。日本語で書き、トリガーのキーフレーズは日本語と英語の両方を含めます(例: 「実機テスト」と "device test")。

<!-- ※要確認: SKILL.md の frontmatter 以外の構成に、リポジトリの規約があるか。CONTRIBUTING.md には見当たらない。確かめ方: オーナーに確認する -->

**できた?** `.agents/skills/<name>/SKILL.md` があり、frontmatter が入っています。

## 手順3: Claude Code 用のスタブを置く

`.claude/skills/<name>/SKILL.md` に、正本と同じ frontmatter と、正本へのポインタだけを置きます。既存のスタブ(たとえば `.claude/skills/floatsoda-release/SKILL.md`)を雛形にしてください。

スタブの本文に手順を足したくなったら、足す先は正本です。スタブは固有の手順を持ちません。

**できた?** スタブの frontmatter が正本と一致し、本文は正本へのポインタだけです。

## 手順4: 言語を確かめる

エージェント向けファイルの言語は、次のとおりです(CONTRIBUTING.md の「エージェント向けファイルの言語」)。

- `AGENTS.md` を英語で書くこと
- skill の本文と `references/` を日本語で書くこと
- 識別子、型名、パス、コマンド、コードを英語のままにすること

**できた?** 本文が日本語で、識別子とコマンドが原文のままです。

## 手順5: PR を出す

今回対象の skill の追加・変更と、それに伴うスタブや参照ファイルの更新だけを PR に入れます。ブランチ名の規約は [コントリビューターの規約](../3-prepare/conventions.md) から案内しています。出したあとの指摘への対応は [レビューの指摘を受けて直す](../5-submit/address-review.md) に進んでください。

## 既存の skill を直す場合

直す場所は `.agents/skills/` の正本です。frontmatter を変えたときは、スタブの frontmatter も同じに直します。

**できた?** 正本とスタブの frontmatter が一致しています。

## うまくいかないとき

<details><summary>skill が発火しない</summary>

正本の `description` のトリガーのキーフレーズを確かめてください。日本語と英語の両方が入っているかを見ます。

<!-- ※要確認: skill が正しく発火するかを確かめる手順の一次情報が見当たらない。確かめ方: オーナーに確認する -->

</details>

<details><summary>正本とスタブがずれた</summary>

正本に合わせて、スタブを直してください。

</details>

<!-- ※要確認: .agents/skills/hello に対応するスタブが .claude/skills/ に無い。意図した例外か抜けか。確かめ方: オーナーに確認する -->
