# FloatSoda ドキュメントサイト

`https://floatsoda.sumx21t.com` で公開するドキュメントサイトです。[Astro Starlight](https://starlight.astro.build/) で組み、LLM 向けに `llms.txt` / `llms-full.txt` と各ページの素の Markdown も配信します(Issue #219)。

## ソースは `docs/` だけ

サイトのページは、リポジトリ直下の `docs/*.md` から**ビルドのたびに生成**します。このディレクトリに本文はありません。

| 場所 | 役割 | Git 管理 |
|---|---|---|
| `../docs/*.md` | 唯一のソース。ページを直すときはここを編集する | あり |
| `scripts/sync-docs.mjs` | `docs/` → Starlight 形式への変換(`npm run build` / `npm run dev` の前に自動実行) | あり |
| `src/content/docs/` | 変換結果。Starlight が読む | **なし(生成物)** |
| `public/<slug>.md` | 変換結果。LLM 向けの素の Markdown(`https://floatsoda.sumx21t.com/<slug>.md`) | **なし(生成物)** |
| `dist/` | ビルド出力。GitHub Pages へそのままデプロイされる | なし |

変換で行うこと(詳細はスクリプト冒頭のコメント):

- 先頭の H1 を frontmatter の `title` に移す
- `docs/Home.md` の「ページ一覧」表を、サイドバーの並び順と各ページの `description` の正典として使う
- `← [Home](Home.md)` のナビ行を除く
- `Foo.md#anchor` 形式のリンクをサイト内パスへ、`../CONTRIBUTING.md` のように `docs/` の外を指すリンクを GitHub の URL へ書き換える
- `editUrl`(GitHub の編集ページ)と `lastUpdated`(`docs/` 側の最終コミット日時)を付ける

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
| サイドバーの並び | `docs/Home.md` の「ページ一覧」表の行順 |
| ページの一行説明 | 同じ表の「内容」列 |
| サイト名、`llms.txt` のプロジェクト説明、mermaid | `astro.config.mjs` |
