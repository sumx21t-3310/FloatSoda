import { defineConfig } from "astro/config";
import starlight from "@astrojs/starlight";
import catppuccin from "@catppuccin/starlight";
import mermaid from "astro-mermaid";
import starlightLlmsTxt from "starlight-llms-txt";
import {
  llmsCustomSets,
  llmsPageLinks,
  repoRoot,
  repositoryUrl,
  sidebarGroups,
  siteUrl,
  slugOf,
} from "./scripts/docs-source.mjs";

export default defineConfig({
  site: siteUrl,
  integrations: [
    // Starlight より前に置く(astro-mermaid の要件)。```mermaid をブラウザ側で描画する
    mermaid({ autoTheme: true }),
    starlight({
      title: "FloatSoda",
      description:
        "SteamVR Overlay を Flutter のような宣言的な書き心地で作れる .NET 10 / C# 14 向け UI フレームワーク",
      locales: {
        root: { label: "日本語", lang: "ja" },
      },
      social: [{ icon: "github", label: "GitHub", href: repositoryUrl }],
      // 各ページの editUrl は scripts/sync-docs.mjs が docs/ 側のパスで上書きする
      editLink: { baseUrl: `${repositoryUrl}/edit/main/` },
      lastUpdated: true,
      plugins: [
        // 配色テーマ(Catppuccin)。ライトは latte / sapphire、ダークは frappe / blue
        catppuccin({
          light: { flavor: "latte", accent: "sapphire" },
          dark: { flavor: "frappe", accent: "blue" },
        }),
        starlightLlmsTxt({
          projectName: "FloatSoda",
          description:
            "SteamVR Overlay を Flutter のような宣言的 UI で作る .NET 10 / C# 14 向けフレームワーク。UI はすべて C# コードで書き、シーンや外部アセットを持たない",
          details:
            "ドキュメントは日本語で書かれている。全文は llms-full.txt、系統ごとの分割は Documentation Sets に列挙した _llms-txt/<系統>.txt、各ページの素の Markdown は Optional の一覧(https://floatsoda.sumx21t.com/<page>.md)から取得できる。取得できる文字数に上限がある場合は、系統ごとのファイルかページ単位を使う。ソースは GitHub リポジトリの docs/ で、XML ドキュメントコメントと合わせて事実情報の正典になる",
          // 系統ごとの分割ファイル(/_llms-txt/user.txt など。パスはプラグインの仕様)。取得上限のある LLM でも 1 回で読める大きさにする
          customSets: llmsCustomSets(),
          // 全ページの索引。ページ単位で /<slug>.md を取りに行けるようにする
          optionalLinks: [
            ...llmsPageLinks(),
            {
              label: "GitHub リポジトリ",
              url: repositoryUrl,
              description: "ソースコード・サンプル・Issue",
            },
          ],
          // docs/Home.md を llms-full.txt の先頭に置く(ランディングは docs コレクションの外なので含まれない)
          promote: ["home"],
          // HTML を経由せず、docs/ から変換した Markdown 本文をそのまま llms-*.txt に入れる
          rawContent: true,
        }),
      ],
      // サイドバーは docs/ の構造を正典にする(#188)。系統ディレクトリ(docs/user/ など)が 1 グループ。
      // docs/ 直下のページは再編が済むまで、docs/Home.md の表の「対象読者」列でグループ化する。詳細は scripts/docs-source.mjs
      sidebar: [
        { slug: "home" },
        ...sidebarGroups().map((group) => ({
          label: group.label,
          items: group.rels.map((rel) => ({ slug: slugOf(rel) })),
        })),
      ],
    }),
  ],
  vite: {
    // ランディングがリポジトリ側のサンプルコード(samples/)を ?raw で読み込むため、dev サーバーの参照範囲を広げる
    server: { fs: { allow: [repoRoot] } },
  },
});
