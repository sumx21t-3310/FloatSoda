| `/_llms-txt/<系統>.txt` |# FloatSoda ドキュメントサイト
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |`https://floatsoda.sumx21t.com` で公開するドキュメントサイトです。[Astro Starlight](https://starlight.astro.build/) で組み、LLM 向けに `llms.txt`(索引)/ `llms-full.txt`(全文)/ `_llms-txt/<系統>.txt`(系統ごとの分割)と各ページの素の Markdown も配信します(Issue #219)。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |## ソースは `docs/` と、ランディングだけ
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |ドキュメントのページは、リポジトリ直下の `docs/` 配下の Markdown(サブディレクトリ込み)から**ビルドのたびに生成**します。このディレクトリが持つ本文は、サイト専用の入口(ランディング)だけです。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` || 場所 | 役割 | Git 管理 |
| `/_llms-txt/<系統>.txt` ||---|---|---|
| `/_llms-txt/<系統>.txt` || `../docs/**/*.md` | ドキュメントの唯一のソース。ページを直すときはここを編集する | あり |
| `/_llms-txt/<系統>.txt` || `content/index.mdx` | ランディング(`/`)の本文と hero。案内と導線だけを書き、事実情報は `docs/` へリンクする | あり |
| `/_llms-txt/<系統>.txt` || `src/pages/index.astro` | ランディングを Starlight の `StarlightPage` で描画する。docs コレクションの外なので、サイドバーと `llms-*.txt` には入らない | あり |
| `/_llms-txt/<系統>.txt` || `scripts/sync-docs.mjs` | `docs/` → Starlight 形式への変換と、ランディングの導線データの生成(`npm run build` / `npm run dev` の前に自動実行) | あり |
| `/_llms-txt/<系統>.txt` || `src/content/docs/` | 変換結果。Starlight が読む | **なし(生成物)** |
| `/_llms-txt/<系統>.txt` || `public/<slug>.md` | 変換結果。LLM 向けの素の Markdown(`https://floatsoda.sumx21t.com/<slug>.md`) | **なし(生成物)** |
| `/_llms-txt/<系統>.txt` || `src/generated/landing.json` | ランディングの導線(3 系統の入口と主要ページへのリンク先)。`docs/` の実在から計算する | **なし(生成物)** |
| `/_llms-txt/<系統>.txt` || `dist/` | ビルド出力。GitHub Pages へそのままデプロイされる | なし |
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |### `docs/` の構造がそのままサイトの構造になる
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |#188 の情報設計(User / Contributor / API Reference の 3 系統)に合わせ、`docs/` のディレクトリを正典にします。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` || `docs/` | サイト | サイドバー |
| `/_llms-txt/<系統>.txt` ||---|---|---|
| `/_llms-txt/<系統>.txt` || `Home.md` | `/home/` | 先頭の単独リンク |
| `/_llms-txt/<系統>.txt` || `user/Home.md` | `/user/` | 「User Guide」グループの先頭(ラベルはこの `Home.md` の H1) |
| `/_llms-txt/<系統>.txt` || `user/Concepts.md` | `/user/concepts/` | 同グループ内。並びは `user/Home.md` でリンクされる順、残りはアルファベット順 |
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |系統どうしの並びは `docs/Home.md` でリンクされる順です。ランディングの 3 入口は、`docs/user/Home.md` などが現れると自動で `/user/` へ向き、それまでは `/home/` へ送ります(API Reference は「準備中」表示)。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |`docs/` 直下に置かれたページは、再編が済むまでの暫定として `docs/Home.md` の「ページ一覧」表の「対象読者」列でグループ化します(`利用者向け` / `コントリビュータ向け`)。全ページが系統ディレクトリへ移ったら、この分岐は `scripts/docs-source.mjs` から消します。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |変換で行うこと(詳細はスクリプト冒頭のコメント):
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |- 先頭の H1 を frontmatter の `title` に移す
| `/_llms-txt/<系統>.txt` |- `docs/Home.md` の「ページ一覧」表の「内容」列を、直下ページの `description` にする(表に無いページは本文の最初の段落)
| `/_llms-txt/<系統>.txt` |- `← [Home](Home.md)` / `← [Home](../Home.md)` のナビ行を除く
| `/_llms-txt/<系統>.txt` |- `Foo.md#anchor` や `../Foo.md` 形式のリンクをサイト内パスへ、`../CONTRIBUTING.md` のように `docs/` の外を指すリンクを GitHub の URL へ書き換える
| `/_llms-txt/<系統>.txt` |- `editUrl`(GitHub の編集ページ)と `lastUpdated`(`docs/` 側の最終コミット日時)を付ける
| `/_llms-txt/<系統>.txt` |- ランディングが参照するページ(`GettingStarted` など)をファイル名で探し、見つからなければ失敗する(改名・削除の検出)
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |ランディングの「最小構成のコード」は、`samples/FloatSoda.Samples.GettingStarted/Program.cs` をビルド時に読み込んで表示します。サンプルを直せばランディングも追従します。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |同じ「`docs/` を読み取り専用ソースにする」型は、GitHub Wiki 同期(`scripts/sync-docs-to-wiki.js`)と共通です。Wiki 同期は移行期間中そのまま残します。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |## コマンド
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |```bash
| `/_llms-txt/<系統>.txt` |cd website
| `/_llms-txt/<系統>.txt` |npm ci            # 依存の導入(Node 22 以上)
| `/_llms-txt/<系統>.txt` |npm run dev       # http://localhost:4321 で確認(docs/ を編集したら dev を再起動して再変換)
| `/_llms-txt/<系統>.txt` |npm run build     # dist/ を生成
| `/_llms-txt/<系統>.txt` |npm run verify    # dist/ の検査。ページ・llms-*.txt の網羅と索引、サイト内リンクとアンカーの実在、UTF-8 を確認する
| `/_llms-txt/<系統>.txt` |```
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |`npm run verify` は CI(`.github/workflows/docs-site.yml`)でも必須にしています。`docs/` のリンク先やアンカーを壊すと、ここで落ちます。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |## LLM 向け出力
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |読む側の取得上限に合わせて、3 つの粒度で配信します。どれも `docs/` から生成され、内容は同じです。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` || ファイル | 中身 | 使いどころ |
| `/_llms-txt/<系統>.txt` ||---|---|---|
| `/_llms-txt/<系統>.txt` || `/llms.txt` | 索引。全ページの素の Markdown へのリンクと一行説明、分割ファイルの一覧 | エージェントに最初に渡す |
| `/_llms-txt/<系統>.txt` || `/llms-full.txt` | 全ページの全文(約 13 万文字) | 一度に読み切れる経路(ChatGPT など) |
| `/_llms-txt/<系統>.txt` || `/_llms-txt/<系統>.txt` | サイドバーのグループ(再編前は `user` / `contributor`、再編後は `docs/` の系統ディレクトリ名)ごとの全文。各 7 万文字以下 | 取得上限が 10 万文字前後の経路(Claude.ai など) |
| `/_llms-txt/<系統>.txt` || `/<slug>.md` | ページ単位の素の Markdown | 上限が小さい経路、または必要なページだけ読むとき |
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |分割ファイルの単位と索引の並びは `scripts/docs-source.mjs` の `sidebarGroups()` から作るので、`docs/` の構造が変わればサイドバーと一緒に追従します。HTML しか取得できない経路(2026-09-09 時点の Gemini の Web チャット)には、各ページの HTML をそのまま読ませます。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |`npm run preview`(`astro preview`)は `llms.txt` や `/<slug>.md` を charset なしの `text/plain` で返すため、ブラウザで開くと日本語が化けて見えます。ファイル自体は UTF-8 で、本番の GitHub Pages は `text/plain; charset=utf-8` を付けて配信します。ローカルで中身を確かめるときは `npm run verify`(UTF-8 の検査を含む)か、`curl http://localhost:4321/llms.txt` を使ってください。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |## デプロイ
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |公開面は **NuGet の最新リリースと同じコミット**の `docs/` を映します。リリースタグ(`v*`)の push で `.github/workflows/docs-site.yml` がそのコミットをビルド・検査し、GitHub Pages へデプロイします。`main` への push と Pull Request では、同じビルドと検査だけを行い、公開はしません。`main` にしか無い API を利用者(と `llms-full.txt` を読む LLM)に見せないためです。docs の修正は次のリリースで公開されます。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |初回だけ、リポジトリのオーナーによる設定と手動デプロイが必要です。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |1. GitHub の Settings → Pages → Source を **GitHub Actions** にする
| `/_llms-txt/<系統>.txt` |2. Cloudflare DNS に `floatsoda` の CNAME レコード(値 `sumx21t-3310.github.io`)を追加する
| `/_llms-txt/<系統>.txt` |3. Settings → Pages → Custom domain に `floatsoda.sumx21t.com` を入れ、Enforce HTTPS を有効にする
| `/_llms-txt/<系統>.txt` |4. Actions → Docs Site → Run workflow を `main` で実行する(過去のタグにはこの workflow が無いので、最初の1回だけ `main` から公開する。次のリリースからはタグで自動)
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |カスタムドメイン名は `public/CNAME` にもあり、ビルド出力に含めてデプロイのたびに GitHub Pages へ渡します。
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` |## 設定の置き場所
| `/_llms-txt/<系統>.txt` |
| `/_llms-txt/<系統>.txt` || 変えたいこと | 場所 |
| `/_llms-txt/<系統>.txt` ||---|---|
| `/_llms-txt/<系統>.txt` || サイト URL、リポジトリ URL | `scripts/docs-source.mjs`(1 箇所で定義し、Astro 設定と変換スクリプトの両方が参照する) |
| `/_llms-txt/<系統>.txt` || サイドバーのグループと並び | `docs/` のディレクトリ構造と、各 `Home.md` のリンク順(直下のページは `docs/Home.md` の表の「対象読者」列と行順) |
| `/_llms-txt/<系統>.txt` || ページの一行説明 | `docs/Home.md` の表の「内容」列 |
| `/_llms-txt/<系統>.txt` || ランディングの文言 | `content/index.mdx` |
| `/_llms-txt/<系統>.txt` || ランディングの導線(どのページへ送るか) | `scripts/sync-docs.mjs` の `landingData()` |
| `/_llms-txt/<系統>.txt` || サイト名、`llms.txt` のプロジェクト説明、mermaid、配色 | `astro.config.mjs` |
