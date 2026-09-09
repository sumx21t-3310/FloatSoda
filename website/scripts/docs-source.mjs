/**
 * docs/ をサイトのソースとして読むための共有モジュール。
 * astro.config.mjs(サイト URL・サイドバー)、scripts/sync-docs.mjs(ページ生成)、
 * scripts/verify-dist.mjs(検査)、src/pages/index.astro(ランディングの導線)が使う。
 * サイト URL とリポジトリ URL の定義はここ 1 箇所に置く。
 *
 * docs/ の構造がそのままサイトの構造になる(#188 の情報設計):
 *   docs/Home.md            → /home/        ドキュメント全体の入口
 *   docs/user/Home.md       → /user/        系統(User Guide)の入口
 *   docs/user/Concepts.md   → /user/concepts/
 * 系統ディレクトリが 1 つのサイドバーグループになる。
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const websiteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
export const repoRoot = path.dirname(websiteDir);
export const docsDir = path.join(repoRoot, "docs");

export const siteUrl = "https://floatsoda.sumx21t.com";
export const repositoryUrl = "https://github.com/sumx21t-3310/FloatSoda";

/** 各階層の入口ページ。docs/Home.md はドキュメント全体、docs/<dir>/Home.md はその系統の入口 */
export const HOME = "Home";

/**
 * docs/ 配下の .md を再帰的に列挙する。要素は次の形:
 *   rel  … docs/ からの相対パス(拡張子なし、区切りは "/")。例: "user/GettingStarted"
 *   dir  … 直上のディレクトリ("" は docs/ 直下)。例: "user"
 *   name … ファイル名(拡張子なし)。例: "GettingStarted"
 */
export function listDocs() {
  const results = [];
  const walk = (dir) => {
    for (const entry of fs.readdirSync(path.join(docsDir, dir), { withFileTypes: true })) {
      const rel = dir ? `${dir}/${entry.name}` : entry.name;
      if (entry.isDirectory()) walk(rel);
      else if (entry.isFile() && /\.md$/i.test(entry.name)) {
        results.push({ rel: rel.replace(/\.md$/i, ""), dir, name: entry.name.replace(/\.md$/i, "") });
      }
    }
  };
  walk("");
  return results.sort((a, b) => a.rel.localeCompare(b.rel));
}

const HOME_SUFFIX = `/${HOME.toLowerCase()}`;

/**
 * Starlight のスラッグ(URL のパス部分)。
 *   Home          → home         (/home/。サイトのトップ / はランディングが担う)
 *   user/Home     → user         (/user/。系統の入口)
 *   user/Concepts → user/concepts
 */
export function slugOf(rel) {
  const lower = rel.toLowerCase();
  return lower.endsWith(HOME_SUFFIX) ? lower.slice(0, -HOME_SUFFIX.length) : lower;
}

/** ページ → サイト内パス */
export function hrefOf(rel) {
  return `/${slugOf(rel)}/`;
}

/** Starlight のコンテンツ(src/content/docs/)内の相対パス(拡張子なし)。系統の入口は <dir>/index にする */
export function contentPathOf(rel) {
  const lower = rel.toLowerCase();
  return lower.endsWith(HOME_SUFFIX) ? `${lower.slice(0, -HOME_SUFFIX.length)}/index` : lower;
}

/** ファイル名でページを探し、サイト内パスを返す。見つからなければ例外(リンク先の消失をビルドで検出する) */
export function hrefOfName(name) {
  const hits = listDocs().filter((doc) => doc.name === name);
  if (hits.length === 0) throw new Error(`docs/ に ${name}.md が見つかりません`);
  hits.sort((a, b) => a.rel.length - b.rel.length);
  return hrefOf(hits[0].rel);
}

/**
 * 系統ディレクトリの入口。docs/<dir>/Home.md があれば { exists: true, href: "/<dir>/" }、
 * まだ無ければ { exists: false, href: fallbackHref }。#188 の再編でディレクトリが現れると自動で切り替わる
 */
export function trackEntry(dir, fallbackHref) {
  const exists = fs.existsSync(path.join(docsDir, dir, `${HOME}.md`));
  return { exists, href: exists ? `/${dir.toLowerCase()}/` : fallbackHref };
}

/** Markdown 本文の先頭の H1(コードフェンス内は無視)。無ければ null */
export function firstHeading(markdown) {
  let inFence = false;
  for (const line of markdown.replace(/\r\n/g, "\n").split("\n")) {
    if (/^\s*(```|~~~)/.test(line)) {
      inFence = !inFence;
      continue;
    }
    if (inFence) continue;
    const match = line.match(/^# (.+)$/);
    if (match) return match[1].trim();
  }
  return null;
}

function readDoc(rel) {
  return fs.readFileSync(path.join(docsDir, `${rel}.md`), "utf8");
}

/** Home.md 本文からリンク先(docs/ からの相対パス、拡張子なし)を出現順に返す。重複なし */
function linkedRels(homeRel, homeDir) {
  if (!fs.existsSync(path.join(docsDir, `${homeRel}.md`))) return [];
  const rels = [];
  const link = /\[[^\]]*\]\(((?:[A-Za-z0-9_-]+\/)*[A-Za-z0-9_-]+)\.md(?:#[^)]*)?\)/g;
  for (const match of readDoc(homeRel).matchAll(link)) {
    const rel = path.posix.normalize(path.posix.join(homeDir, match[1]));
    if (!rels.includes(rel)) rels.push(rel);
  }
  return rels;
}

/**
 * docs/Home.md の「ページ一覧」表の行を、表に現れる順で返す。
 * 行の形は `| [Name](Name.md) | 内容 | 対象読者 |`。docs/ 直下のページだけが対象
 */
function homeTableRows() {
  const names = new Set(listDocs().filter((doc) => doc.dir === "").map((doc) => doc.name));
  const rows = [];
  const row = /^\|\s*\[[^\]]+\]\(([A-Za-z0-9_-]+)\.md\)\s*\|([^|\n]*)\|([^|\n]*)\|/gm;
  for (const match of readDoc(HOME).matchAll(row)) {
    const name = match[1];
    if (names.has(name) && !rows.some((r) => r.name === name)) {
      rows.push({ name, summary: match[2].trim(), audience: match[3].trim() });
    }
  }
  return rows;
}

/**
 * docs/Home.md の「ページ一覧」表の「内容」列を、ページ(rel)→ 一行要約の Map で返す。
 * 人手で書かれた要約なので、ページの description(SEO と llms.txt の索引)にそのまま使う。
 */
export function homeSummaries() {
  return new Map(homeTableRows().map((row) => [row.name, row.summary.replace(/[*_`]/g, "")]));
}

/**
 * サイドバーのグループを返す(`[{ label, rels }]`)。
 *
 * 1. docs/ 直下のページ: Home.md の表の「対象読者」列でグループ化し、表の行順を読む順にする。
 *    #188 の再編で全ページが系統ディレクトリへ移るまでの暫定で、移り終わったらこの分岐は消す
 * 2. 系統ディレクトリ(docs/user/ など): 1 ディレクトリ = 1 グループ。ラベルは <dir>/Home.md の H1、
 *    並びは入口 → 同 Home.md のリンク順 → 残りをアルファベット順。系統どうしの並びは docs/Home.md でリンクされる順
 */
export function sidebarGroups() {
  const docs = listDocs();
  const groups = [];
  const push = (label, rel) => {
    let group = groups.find((g) => g.label === label);
    if (!group) {
      group = { label, rels: [] };
      groups.push(group);
    }
    if (!group.rels.includes(rel)) group.rels.push(rel);
  };

  const topLevel = docs.filter((doc) => doc.dir === "" && doc.name !== HOME);
  if (topLevel.length > 0) {
    const rows = homeTableRows();
    for (const row of rows) push(`${row.audience.split("/")[0].trim()}向け`, row.name);
    for (const doc of topLevel) {
      if (!rows.some((row) => row.name === doc.name)) push("その他", doc.rel);
    }
  }

  const dirs = [...new Set(docs.filter((doc) => doc.dir !== "").map((doc) => doc.dir.split("/")[0]))];
  const homeOrder = linkedRels(HOME, "").map((rel) => rel.split("/")[0]);
  dirs.sort((a, b) => {
    const ia = homeOrder.indexOf(a);
    const ib = homeOrder.indexOf(b);
    if (ia === -1 && ib === -1) return a.localeCompare(b);
    if (ia === -1) return 1;
    if (ib === -1) return -1;
    return ia - ib;
  });

  for (const dir of dirs) {
    const pages = docs.filter((doc) => doc.rel.startsWith(`${dir}/`));
    const homeRel = `${dir}/${HOME}`;
    const home = pages.find((doc) => doc.rel === homeRel);
    const label = (home && firstHeading(readDoc(homeRel))) || dir;
    if (home) push(label, homeRel);
    for (const rel of linkedRels(homeRel, dir)) {
      if (pages.some((doc) => doc.rel === rel)) push(label, rel);
    }
    for (const doc of pages) push(label, doc.rel);
  }

  return groups;
}
