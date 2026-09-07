/**
 * docs/ をサイトのソースとして読むための共有モジュール。
 * astro.config.mjs(サイト URL・サイドバー順)と scripts/sync-docs.mjs(ページ生成)の両方から使う。
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

/** サイトのトップページになる docs/ のファイル名(拡張子なし) */
export const HOME = "Home";

/** docs/ 直下の .md ファイル名(拡張子なし)をアルファベット順で返す */
export function listDocNames() {
  return fs
    .readdirSync(docsDir)
    .filter((file) => /\.md$/i.test(file))
    .map((file) => file.replace(/\.md$/i, ""))
    .sort();
}

/** ページ名 → Starlight のスラッグ(生成ファイル名)。Home はサイトのトップ(index) */
export function slugOf(name) {
  return name === HOME ? "index" : name.toLowerCase();
}

/** ページ名 → サイト内パス */
export function hrefOf(name) {
  return name === HOME ? "/" : `/${name.toLowerCase()}/`;
}

/**
 * Home.md の「ページ一覧」表の行を、表に現れる順で返す。
 * 行の形は `| [Name](Name.md) | 内容 | 対象読者 |`。
 */
function homeTableRows() {
  const home = fs.readFileSync(path.join(docsDir, `${HOME}.md`), "utf8");
  const names = listDocNames();
  const rows = [];
  for (const match of home.matchAll(/^\|\s*\[[^\]]+\]\(([A-Za-z0-9_-]+)\.md\)\s*\|([^|\n]*)\|/gm)) {
    const name = match[1];
    if (names.includes(name) && !rows.some((row) => row.name === name)) {
      rows.push({ name, summary: match[2].trim() });
    }
  }
  return rows;
}

/**
 * Home.md の「ページ一覧」表に現れる順でページ名(Home を除く)を返す。
 * 表に無いページは末尾にアルファベット順で加え、取りこぼしを防ぐ。
 */
export function orderedDocNames() {
  const listed = homeTableRows().map((row) => row.name);
  const rest = listDocNames().filter((name) => name !== HOME && !listed.includes(name));
  return [...listed, ...rest];
}

/**
 * Home.md の「ページ一覧」表の「内容」列を、ページ名 → 一行要約の Map で返す。
 * 人手で書かれた要約なので、ページの description(SEO と llms.txt の索引)にそのまま使う。
 * Markdown の強調や `code` は取り除く。
 */
export function homeSummaries() {
  return new Map(homeTableRows().map((row) => [row.name, row.summary.replace(/[*_`]/g, "")]));
}
