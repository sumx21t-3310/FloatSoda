/**
 * docs/ をサイトのソースとして読むための共有モジュール。
 * astro.config.mjs(サイト URL・サイドバー)と scripts/sync-docs.mjs(ページ生成)の両方から使う。
 * サイト URL とリポジトリ URL の定義はここ 1 箇所に置く。
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const websiteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
export const repoRoot = path.dirname(websiteDir);
export const docsDir = path.join(repoRoot, "docs");

export const siteUrl = "https://floatsoda.sumx21t.com";
export const repositoryUrl = "https://github.com/sumx21t-3310/FloatSoda";

/** docs/ の入口ページ。「ページ一覧」表を持ち、サイドバーと description の正典になる */
export const HOME = "Home";

/** docs/ 直下の .md ファイル名(拡張子なし)をアルファベット順で返す */
export function listDocNames() {
  return fs
    .readdirSync(docsDir)
    .filter((file) => /\.md$/i.test(file))
    .map((file) => file.replace(/\.md$/i, ""))
    .sort();
}

/** ページ名 → Starlight のスラッグ(生成ファイル名)。Home は /home/ で、サイトのトップはランディングが担う */
export function slugOf(name) {
  return name.toLowerCase();
}

/** ページ名 → サイト内パス */
export function hrefOf(name) {
  return `/${slugOf(name)}/`;
}

/**
 * Home.md の「ページ一覧」表の行を、表に現れる順で返す。
 * 行の形は `| [Name](Name.md) | 内容 | 対象読者 |`。
 */
function homeTableRows() {
  const home = fs.readFileSync(path.join(docsDir, `${HOME}.md`), "utf8");
  const names = listDocNames();
  const rows = [];
  const row = /^\|\s*\[[^\]]+\]\(([A-Za-z0-9_-]+)\.md\)\s*\|([^|\n]*)\|([^|\n]*)\|/gm;
  for (const match of home.matchAll(row)) {
    const name = match[1];
    if (names.includes(name) && !rows.some((r) => r.name === name)) {
      rows.push({ name, summary: match[2].trim(), audience: match[3].trim() });
    }
  }
  return rows;
}

/**
 * Home.md の「ページ一覧」表の「内容」列を、ページ名 → 一行要約の Map で返す。
 * 人手で書かれた要約なので、ページの description(SEO と llms.txt の索引)にそのまま使う。
 * Markdown の強調や `code` は取り除く。
 */
export function homeSummaries() {
  return new Map(homeTableRows().map((row) => [row.name, row.summary.replace(/[*_`]/g, "")]));
}

/**
 * サイドバーのグループを返す(`[{ label, names }]`)。
 * Home.md の表の「対象読者」列で分け、表の行順を各グループ内の並び(読む順)にする。
 * 「利用者 / コントリビュータ」のように複数書かれた行は、先頭の読者のグループに入る。
 * 表に無いページは「その他」にまとめ、取りこぼしを防ぐ。
 */
export function sidebarGroups() {
  const groups = [];
  const add = (label, name) => {
    let group = groups.find((g) => g.label === label);
    if (!group) {
      group = { label, names: [] };
      groups.push(group);
    }
    group.names.push(name);
  };

  const rows = homeTableRows();
  for (const row of rows) {
    const audience = row.audience.split("/")[0].trim();
    add(`${audience}向け`, row.name);
  }
  const listed = rows.map((row) => row.name);
  for (const name of listDocNames()) {
    if (name !== HOME && !listed.includes(name)) add("その他", name);
  }
  return groups;
}
