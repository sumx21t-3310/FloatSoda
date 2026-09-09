# FloatSoda ドキュメントサイト

`https://floatsoda.sumx21t.com` で公開するドキュメントサイトです。[Astro Starlight](https://starlight.astro.build/) で組み、LLM 向けに `llms.txt` / `llms-full.txt` と各ページの素の Markdown も配信します(Issue #219)。

## ソースは `docs/` と、ランディングだけ

ドキュメントのページは、リポジトリ直下の `docs/` 配下の Markdown(サブディレクトリ込み)から**ビルドのたびに生成**します。このディレクトリが持つ本文は、サイト専用の入口(ランディング)だけです。

| 場所 | 役割 | Git 管理 |
|---|---|---|
| `../docs/**/*.md` | ドキュメントの唯一のソース。ページを直すときはここを編集する | あり |
| `content/index.mdx` | ランディング(`/`)の本文と hero。案内と導線だけを書き、事実情報は `docs/` へリンクする | あり |
| `src/pages/index.astro` | ランディングを Starlight の `StarlightPage` で描画する。docs コレクションの外なので、サイドバーと `llms-*.txt` には入らない | あり |
| `scripts/sync-docs.mjs` | `docs/` → Starlight 形式への変換と、ランディングの導線データの生成(`npm run build` / `npm run dev` の前に自動実行) | あり |
| `src/content/docs/` | 変換結果。Starlight が読む | **なし(生成物)** |
| `public/<slug>.md` | 変換結果。LLM 向けの素の Markdown(`https://floatsoda.sumx21t.com/<slug>.md`) | **なし(生成物)** |
| `src/generated/landing.json` | ランディングの導線(3 系統の入口と主要ページへのリンク先)。`docs/` の実在から計算する | **なし(生成物)** |
| `dist/` | ビルド出力。GitHub Pages へそのままデプロイされる | なし |

### `docs/` の構造がそのままサイトの構造になる

#188 の情報設計(User / Contributor / API Reference の 3 系統)に合わせ、`docs/` のディレクトリを正典にします。

| `docs/` | サイト | サイドバー |
|---|---|---|
| `Home.md` | `/home/` | 先頭の単独リンク |
| `user/Home.md` | `/user/` | 「User Guide」グループの先頭(ラベルはこの `Home.md` の H1) |
| `user/Concepts.md` | `/user/concepts/` | 同グループ内。並びは `user/Home.md` でリンクされる順、残りはアルファベット順 |

系統どうしの並びは `docs/Home.md` でリンクされる順です。ランディングの 3 入口は、`docs/user/Home.md` などが現れると自動で `/user/` へ向き、それまでは `/home/` へ送ります(API Reference は「準備中」表示)。

`docs/` 直下に置かれたページは、再編が済むまでの暫定として `docs/Home.md` の「ページ一覧」表の「対象読者」列でグループ化します(`利用者向け` / `コントリビュータ向け`)。全ページが系統ディレクトリへ移ったら、この分岐は `scripts/docs-source.mjs` から消します。

変換で行うこと(詳細はスクリプト冒頭のコメント):

- 先頭の H1 を frontmatter の `title` に移す
- `docs/Home.md` の「ページ一覧」表の「内容」列を、直下ページの `description` にする(表に無いページは本文の最初の段落)
- `← [Home](Home.md)` / `← [Home](../Home.md)` のナビ行を除く
- `Foo.md#anchor` や `../Foo.md` 形式のリンクをサイト内パスへ、`../CONTRIBUTING.md` のように `docs/` の外を指すリンクを GitHub の URL へ書き換える
- `editUrl`(GitHub の編集ページ)と `lastUpdated`(`docs/` 側の最終コミット日時)を付ける
- ランディングが参照するページ(`GettingStarted` など)をファイル名で探し、見つからなければ失敗する(改名・削除の検出)

ランディングの「最小構成のコード」は、`samples/FloatSoda.Samples.GettingStarted/Program.cs` をビルド時に読み込んで表示します。サンプルを直せばランディングも追従します。

同じ「`docs/` を読み取り専用ソースにする」型は、GitHub Wiki 同期(`scripts/sync-docs-to-wiki.js`)と共通です。Wiki 同期は移行期間中そのまま残します。

## コマンド

```bash
cd website
npm ci            # 依存の導入(Node 22 以上)
npm run dev       # http://localhost:4321 で確認(docs/ を編集したら dev を再起動して再変換)
npm run build     # dist/ を生成
npm run verify    # dist/ の検査。ページ・llms.txt の網羅、サイト内リンクとアンカーの実在を確認する
```

`npm run verify` は CI(`.github/workflows/docs-site.yml`)でも必須にしています。`docs/` のリンク先やアンカーを壊すと、ここで落ちます。

## デプロイ

`main` への push(`docs/**` または `website/**` の変更)で `.github/workflows/docs-site.yml` がビルド・検査し、GitHub Pages へデプロイします。Pull Request では同じビルドと検査だけを行います。

初回だけ、リポジトリのオーナーによる設定が必要です。

1. GitHub の Settings → Pages → Source を **GitHub Actions** にする
2. Cloudflare DNS に `floatsoda` の CNAME レコード(値 `sumx21t-3310.github.io`)を追加する
3. Settings → Pages → Custom domain に `floatsoda.sumx21t.com` を入れ、Enforce HTTPS を有効にする

カスタムドメイン名は `public/CNAME` にもあり、ビルド出力に含めてデプロイのたびに GitHub Pages へ渡します。

## 設定の置き場所

| 変えたいこと | 場所 |
|---|---|
| サイト URL、リポジトリ URL | `scripts/docs-source.mjs`(1 箇所で定義し、Astro 設定と変換スクリプトの両方が参照する) |
| サイドバーのグループと並び | `docs/` のディレクトリ構造と、各 `Home.md` のリンク順(直下のページは `docs/Home.md` の表の「対象読者」列と行順) |
| ページの一行説明 | `docs/Home.md` の表の「内容」列 |
| ランディングの文言 | `content/index.mdx` |
| ランディングの導線(どのページへ送るか) | `scripts/sync-docs.mjs` の `landingData()` |
| サイト名、`llms.txt` のプロジェクト説明、mermaid、配色 | `astro.config.mjs` |
