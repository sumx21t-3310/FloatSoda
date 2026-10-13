# docs の小さな修正で PR を出す

`docs/` の1ページにある小さな誤りを直し、PR がマージされるまでを一本道で進めます。題材は docs の修正なので、FloatSoda のコードを読まずに、ブランチ・検証・PR・レビューという貢献の流れを一周できます。規約の本文は [CONTRIBUTING.md](../../../CONTRIBUTING.md) にあり、このページは各手順で読む節へ案内します。

## 作るもの

`docs/` のページ1つを直す PR を1本出します。直すのは、誤字、リンク切れ、コードと食い違う記述のような、設計の判断を伴わない誤りです。

書き換えるのは `docs/` 配下のファイルだけです。GitHub Wiki と公開サイトは `docs/` から作られるため、Wiki や `website/src/content/docs/` を直接書き換えても、次の同期やビルドで上書きされます。

## 前提

次の道具を使います。

- Git と GitHub のアカウント
- Node.js 22 以上(公開サイトのビルドに使う)
- .NET 10 SDK(PR の前に CI と同じ検証を流すために使う。[CONTRIBUTING.md の「開発環境のセットアップ」](../../../CONTRIBUTING.md#開発環境のセットアップ))

リポジトリを clone して、`main` を最新にしておきます。外部のコントリビュータが fork から PR を出すときの手順は、CONTRIBUTING.md に書かれていません ※要確認。

## Step 1: 直す箇所を決める

`docs/` のページを読み、直したい箇所を1つに決めます。1つの PR に入れる修正は、目的が同じものだけにします。ほかに気づいた誤りは、別の PR にします(→ [CONTRIBUTING.md の「PR のスコープ」](../../../CONTRIBUTING.md#pr-のスコープ))。

誤字や明白な誤りの修正なら、Issue を立てずに進めてかまいません。記述の方針を変える修正は、先に Issue を立てます。どちらに当たるかは [CONTRIBUTING.md の「Issue が必須な変更」](../../../CONTRIBUTING.md#issue-が必須な変更)で判断します。

## Step 2: ブランチを切る

`main` からブランチを切ります。名前の形は [CONTRIBUTING.md の「ブランチ名」](../../../CONTRIBUTING.md#ブランチ名)に従います。Issue を省略した小さな修正では、Issue 番号のない `<slug>` の形になります。

```bash
git switch -c <ブランチ名> main
```

## Step 3: docs を直す

`docs/` のファイルを直します。文体は [WritingDocumentation の「6. 文体」](../WritingDocumentation.md#6-文体)に従います。地の文は敬体、箇条書きと表のセルは常体です。

直す内容が既存の記述の訂正ではなく、新しい情報の追加になったときは、[WritingDocumentation の「1. どこに書くか」](../WritingDocumentation.md#1-どこに書くか)で置き場所を確かめます。同じ情報が別のページに書かれていたら、そちらへのリンクに変えます。

## Step 4: 公開サイトをビルドして確かめる

`website/` で公開サイトをビルドし、検査を通します。検査は、ページの網羅、サイト内リンクとアンカーの実在、UTF-8 を確かめます。

```bash
cd website
npm ci
npm run build
npm run verify
```

`npm ci` は初回だけ実行します。`npm run verify` が `verify-dist: OK` で始まる行を出力すれば成功です。`broken link` が出たら、直したページのリンク先かアンカーが間違っています。

表示を目で確かめたいときは、`npm run dev` を実行して `http://localhost:4321` を開きます。コマンドの詳細は [website/README.md](../../../website/README.md) にあります。

## Step 5: CI と同じ検証を通す

docs だけの修正でも、PR を出す前に CI と同じビルドとテストをローカルで通します(必須)。実行するコマンドは [CONTRIBUTING.md の「ブランチ・PRフロー」](../../../CONTRIBUTING.md#ブランチprフロー)にあります。

## PR を出す

変更をコミットしてブランチを push し、`main` へ向けた PR を作ります。本文は [PR テンプレート](../../../.github/PULL_REQUEST_TEMPLATE.md)の見出しに沿って書きます。

docs の修正で迷いやすいのは、次の2つの節です。

- `## Documentation` — 変更の種別と、更新した文書を書く。種別は [WritingDocumentation の「3. Documentation Impact Matrix」](../WritingDocumentation.md#3-documentation-impact-matrix)で選ぶ
- `## Flutter reference` — Flutter 由来の Widget の変更ではないので、`N/A` と書く

PR を作ると、CI のビルドとテストと、公開サイトのビルドと検査(`.github/workflows/docs-site.yml`)が走ります。レビューでは [REVIEW.md](../../../REVIEW.md) の基準で指摘が付きます。指摘を直すときは、同じブランチにコミットを足して push します。

## 次に読むもの

- [作業の手順](../how-to/Home.md) — ドキュメントコメント、テスト、実機での確認、リリースの手順
- [全体の仕組みと理由](../explanation/Overall.md) — アセンブリ構成、フレームの流れ、スレッド、入力の流れ
- [守ること](../reference/Home.md) — 変更の前後で維持する条件
- [FloatSoda を変える](../Home.md) — ほかの「貢献までの流れ」。コードを変える最初の PR は、触りたい部分の題材から始める
