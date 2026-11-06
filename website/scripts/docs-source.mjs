/**
 * docs/ をサイトのソースとして読むための共有モジュール。
 * astro.config.mjs(サイト URL・サイドバー・LLM 向け出力)、scripts/sync-docs.mjs(ページ生成)、
 * scripts/verify-dist.mjs(検査)、src/pages/index.astro(ランディングの導線)が使う。
 * サイト URL とリポジトリ URL の定義はここ 1 箇所に置く。
 *
 * docs/ の構造がそのままサイトの構造になる(#188 の情報設計):
 *   docs/Home.md            → /home/        ドキュメント全体の入口
 *   docs/user/Home.md       → /user/        系統(User Guide)の入口
 *   docs/user/Concepts.md   → /user/concepts/
 * 系統ディレクトリが 1 つのサイドバーグループになり、LLM 向けの分割ファイル(_llms-txt/<key>.txt)の単位にもなる。
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const websiteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
export const repoRoot = path.dirname(websiteDir);
export const docsDir = path.join(repoRoot, "docs");

/**
 * API リファレンスのタブ。ページは docs/ の外の generated/api/ に scripts/api-reference.mjs が生成する(#318)。
 * ページの rel は "api/<名前空間>/<型>" で、docs/ のページと同じ形で扱う(ソースの場所だけが違う)
 */
export const API_TAB = "api";
export const apiDir = path.join(websiteDir, "generated", API_TAB);

/** API リファレンスの生成ページか */
export function isGenerated(rel) {
  return rel === API_TAB || rel.startsWith(`${API_TAB}/`);
}

/** ページ(rel)のソースファイルの絶対パス。docs/ のページは docs/ の、API のページは generated/api/ のファイル */
export function sourceFileOf(rel) {
  return isGenerated(rel)
    ? path.join(apiDir, `${rel.slice(API_TAB.length + 1)}.md`)
    : path.join(docsDir, `${rel}.md`);
}

export const siteUrl = "https://docs.floatsoda.dev";

/**
 * #219 の作り直しで消した旧ページのスラッグ → 中身が移った新しいページのサイト内パス。
 * astro.config.mjs のリダイレクトと、sync-docs.mjs が書く旧 /<slug>.md の移動の案内の両方が使う。
 * リポジトリ直下に戻した規約(API_DESIGN.md など)はサイトに載らないので、それらを案内するページへ送る
 */
export const LEGACY_PAGES = {
  apidesign: "/contributor/3-prepare/conventions/",
  animation: "/guide/4-build/screen/add-animation/",
  architecture: "/contributor/2-understand/frame-to-steamvr/",
  buildpipeline: "/contributor/2-understand/framework-layer/",
  documentationcomments: "/contributor/3-prepare/conventions/",
  gettingstarted: "/tutorials/first-overlay/",
  input: "/contributor/2-understand/framework-input/",
  localization: "/contributor/3-prepare/conventions/",
  ovrintegration: "/contributor/2-understand/ovr-layer/",
  renderobjects: "/contributor/2-understand/framework-layer/",
  targetusers: "/guide/1-discover/what-is-floatsoda/",
  teststrategy: "/contributor/3-prepare/test-strategy/",
  uilayering: "/contributor/2-understand/ui-layer/",
  widgetsystem: "/reference/find-widgets/",
  writingdocumentation: "/contributor/3-prepare/conventions/",
};
export const repositoryUrl = "https://github.com/sumx21t-3310/FloatSoda";

/** 各階層の入口ページ。docs/Home.md はドキュメント全体、docs/<dir>/Home.md はその系統の入口 */
export const HOME = "Home";

/**
 * docs/ 直下のページを「対象読者」列でグループ化するときの ASCII キー(#188 の再編までの暫定)。
 * 系統ディレクトリ(user / contributor / api)と同じ語にして、_llms-txt/<key>.txt のファイル名を再編の前後で揃える
 */
const AUDIENCE_KEYS = { 利用者: "user", コントリビュータ: "contributor" };

/**
 * docs/ 配下の .md を再帰的に列挙する。要素は次の形:
 *   rel  … docs/ からの相対パス(拡張子なし、区切りは "/")。例: "user/GettingStarted"
 *   dir  … 直上のディレクトリ("" は docs/ 直下)。例: "user"
 *   name … ファイル名(拡張子なし)。例: "GettingStarted"
 *   generated … API リファレンスの生成ページなら true
 * generated/api/ があれば、その中のページも "api/..." の rel で含める
 */
export function listDocs() {
  if (fs.existsSync(path.join(docsDir, API_TAB))) {
    throw new Error(`docs/${API_TAB}/ は API リファレンスの生成先(generated/${API_TAB}/)と衝突します`);
  }
  const results = [];
  const walk = (root, dir, prefix) => {
    for (const entry of fs.readdirSync(path.join(root, dir), { withFileTypes: true })) {
      const sub = dir ? `${dir}/${entry.name}` : entry.name;
      if (entry.isDirectory()) walk(root, sub, prefix);
      else if (entry.isFile() && /\.md$/i.test(entry.name)) {
        const rel = `${prefix}${sub}`.replace(/\.md$/i, "");
        const relDir = rel.includes("/") ? rel.slice(0, rel.lastIndexOf("/")) : "";
        results.push({ rel, dir: relDir, name: entry.name.replace(/\.md$/i, ""), generated: prefix !== "" });
      }
    }
  };
  walk(docsDir, "", "");
  if (fs.existsSync(apiDir)) walk(apiDir, "", `${API_TAB}/`);
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
  const exists = fs.existsSync(sourceFileOf(`${dir}/${HOME}`));
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
  return fs.readFileSync(sourceFileOf(rel), "utf8");
}

/** ページのタイトル(先頭の H1)。無ければファイル名 */
export function titleOf(rel) {
  return firstHeading(readDoc(rel)) ?? rel.split("/").pop();
}

/** Home.md 本文からリンク先(docs/ からの相対パス、拡張子なし)を出現順に返す。重複なし */
function linkedRels(homeRel, homeDir) {
  if (!fs.existsSync(sourceFileOf(homeRel))) return [];
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
 * 行の形は `| [Name](Name.md) | 内容 | 対象読者 |` か `| [Name](tab/Home.md) | 内容 | 対象読者 |`。
 * リンク先は docs/ からの相対パス(rel)に直し、docs/ に実在するページだけを返す
 */
function homeTableRows() {
  const rels = new Set(listDocs().map((doc) => doc.rel));
  const rows = [];
  const row = /^\|\s*\[[^\]]+\]\(((?:[A-Za-z0-9_-]+\/)*[A-Za-z0-9_-]+)\.md\)\s*\|([^|\n]*)\|([^|\n]*)\|/gm;
  for (const match of readDoc(HOME).matchAll(row)) {
    const rel = path.posix.normalize(match[1]);
    if (rels.has(rel) && !rows.some((r) => r.rel === rel)) {
      rows.push({ rel, summary: match[2].trim(), audience: match[3].trim() });
    }
  }
  return rows;
}

/**
 * docs/Home.md の「ページ一覧」表の「内容」列を、ページ(rel)→ 一行要約の Map で返す。
 * 人手で書かれた要約なので、ページの description(SEO と llms.txt の索引)にそのまま使う。
 */
export function homeSummaries() {
  return new Map(homeTableRows().map((row) => [row.rel, row.summary.replace(/[*_`]/g, "")]));
}

/**
 * タブの中のサブディレクトリ(docs/<tab>/<dir>/...)のサイドバーの見出しと並び順。
 * キーは docs/ からの相対パス。ここに無いディレクトリは、ディレクトリ名を見出しにして後ろへアルファベット順に並べる。
 * 見出しはサイトの表示だけに使う(ディレクトリ名は URL になるので英語のスラッグのまま)
 */
const DIR_LABELS = new Map([
  ["guide/1-discover", "1. 探す"],
  ["guide/2-adopt", "2. 採用する"],
  ["guide/3-prepare", "3. 準備する"],
  ["guide/4-build", "4. 作る"],
  ["guide/4-build/screen", "画面"],
  ["guide/4-build/input", "入力"],
  ["guide/4-build/mechanics", "仕組み"],
  ["guide/5-troubleshoot", "5. 困ったとき"],
  ["guide/6-distribute", "6. 配る"],
  ["guide/7-maintain", "7. メンテナンスする"],
  ["contributor/1-get-involved", "1. はじめに"],
  ["contributor/2-understand", "2. 理解する"],
  ["contributor/3-prepare", "3. 準備する"],
  ["contributor/4-write", "4. 書く"],
  ["contributor/5-submit", "5. 出す"],
  ["contributor/6-propose", "6. 提案する"],
]);
const DIR_ORDER = [...DIR_LABELS.keys()];

/**
 * ディレクトリの中のページの並び順(ファイル名、拡張子なし)。キーは docs/ からの相対パス。
 * 入口の Home.md にページの一覧を書かないディレクトリで、サイドバーの並びをここで決める。
 * ここに無いページは、Home.md のリンク順 → アルファベット順で後ろに並ぶ
 */
const PAGE_ORDER = new Map([
  ["reference", ["requirements", "limitations", "sample-catalog", "find-widgets", "flutter-mapping", "glossary"]],
]);

/** 並べ方の比較関数を作る。order に載っているものを先にその順で、残りをアルファベット順に並べる */
function byOrder(order) {
  return (a, b) => {
    const ia = order.indexOf(a);
    const ib = order.indexOf(b);
    if (ia === -1 && ib === -1) return a.localeCompare(b);
    if (ia === -1) return 1;
    if (ib === -1) return -1;
    return ia - ib;
  };
}

/**
 * ディレクトリ(docs/ からの相対パス)の中身を、サイドバーの木にして返す。
 * 要素は { rel }(ページ)か { label, items }(サブディレクトリ)。並びは、直下のページ → サブディレクトリの順。
 * ページは入口(Home)→ PAGE_ORDER の順 → linkOrder に現れる順 → 残りをアルファベット順。サブディレクトリは DIR_ORDER の順
 */
function dirTree(dir, docs, linkOrder) {
  const pages = docs.filter((doc) => doc.dir === dir).map((doc) => doc.rel);
  const homeRel = `${dir}/${HOME}`;
  const pageOrder = (PAGE_ORDER.get(dir) ?? []).map((name) => `${dir}/${name}`);
  pages.sort(byOrder([homeRel, ...pageOrder, ...linkOrder]));
  const items = pages.map((rel) => ({ rel }));

  const prefix = `${dir}/`;
  const subdirs = [
    ...new Set(
      docs
        .filter((doc) => doc.dir.startsWith(prefix))
        .map((doc) => prefix + doc.dir.slice(prefix.length).split("/")[0]),
    ),
  ];
  subdirs.sort(byOrder(DIR_ORDER));
  for (const sub of subdirs) {
    if (isGenerated(sub)) {
      // API リファレンスの名前空間: 見出しは名前空間の名前(<dir>/Home.md の H1)。型が多いので畳んでおく
      const label = firstHeading(readDoc(`${sub}/${HOME}`)) ?? sub.split("/").pop();
      items.push({ label, collapsed: true, items: dirTree(sub, docs, linkOrder) });
      continue;
    }
    items.push({ label: DIR_LABELS.get(sub) ?? sub.split("/").pop(), items: dirTree(sub, docs, linkOrder) });
  }
  return items;
}

/** サイドバーの木を、上から読む順のページの並びにする */
function flattenTree(items) {
  return items.flatMap((item) => (item.rel ? [item.rel] : flattenTree(item.items)));
}

/**
 * サイドバーのグループを返す(`[{ key, label, rels, items }]`)。key は ASCII で、_llms-txt/<key>.txt のファイル名にも使う。
 * rels はグループのページを読む順に並べたもの、items はサイドバーに出す木(dirTree の形)。
 *
 * 1. docs/ 直下のページ: Home.md の表の「対象読者」列でグループ化し、表の行順を読む順にする。
 *    タブのディレクトリへ全ページが移るまでの暫定で、移り終わったらこの分岐は消す
 * 2. タブのディレクトリ(docs/guide/ など): 1 ディレクトリ = 1 グループ。ラベルは <dir>/Home.md の H1。
 *    中のサブディレクトリは、DIR_LABELS の見出しを付けた入れ子のグループにする。
 *    ページの並びは、入口 → タブの Home.md のリンク順 → 残りをアルファベット順。タブどうしの並びは docs/Home.md でリンクされる順
 */
export function sidebarGroups() {
  const docs = listDocs();
  const groups = [];
  const push = (key, label, rel) => {
    let group = groups.find((g) => g.key === key);
    if (!group) {
      group = { key, label, rels: [], items: [] };
      groups.push(group);
    }
    if (!group.rels.includes(rel)) {
      group.rels.push(rel);
      group.items.push({ rel });
    }
  };

  const topLevel = docs.filter((doc) => doc.dir === "" && doc.name !== HOME);
  if (topLevel.length > 0) {
    const rows = homeTableRows().filter((row) => !row.rel.includes("/"));
    for (const row of rows) {
      // 「利用者 / コントリビュータ」は先頭の読者。Markdown 装飾は落とし、空セルは「その他」へ
      const audience = row.audience.split("/")[0].replace(/[*_`]/g, "").trim();
      if (audience) push(AUDIENCE_KEYS[audience] ?? "other", `${audience}向け`, row.rel);
      else push("other", "その他", row.rel);
    }
    for (const doc of topLevel) {
      if (!rows.some((row) => row.rel === doc.rel)) push("other", "その他", doc.rel);
    }
  }

  const dirs = [...new Set(docs.filter((doc) => doc.dir !== "").map((doc) => doc.dir.split("/")[0]))];
  dirs.sort(byOrder(linkedRels(HOME, "").map((rel) => rel.split("/")[0])));

  for (const dir of dirs) {
    const homeRel = `${dir}/${HOME}`;
    const label = (fs.existsSync(sourceFileOf(homeRel)) && firstHeading(readDoc(homeRel))) || dir;
    const items = dirTree(dir, docs, linkedRels(homeRel, dir));
    groups.push({ key: dir.toLowerCase(), label, rels: flattenTree(items), items });
  }

  return groups;
}

/** Starlight の sidebar に渡す形に、グループの木を変換する */
export function sidebarItems(items) {
  return items.map((item) =>
    item.rel
      ? { slug: slugOf(item.rel) }
      : { label: item.label, ...(item.collapsed ? { collapsed: true } : {}), items: sidebarItems(item.items) },
  );
}

/**
 * starlight-llms-txt の customSets。サイドバーのグループ(系統)ごとに _llms-txt/<key>.txt を出す。
 * llms-full.txt(約 13 万文字)を読み切れない取得経路でも、系統単位なら 1 回で読める(各 7 万文字以下)。
 * label がそのままファイル名になるので ASCII の key を使い、人間向けの名前は description に書く
 */
export function llmsCustomSets() {
  const sets = [];
  for (const group of sidebarGroups()) {
    const size = group.rels.reduce((sum, rel) => sum + readDoc(rel).length, 0);
    if (size <= LLMS_SET_LIMIT) {
      sets.push(llmsSet(group.key, group.label, group.rels));
      continue;
    }
    // 大きすぎるグループは、タブの中のサブディレクトリ(docs/<tab>/<dir>/)ごとに分ける。入口(Home)は先頭の組に入れる
    const parts = new Map();
    for (const rel of group.rels) {
      const segments = rel.split("/");
      const part = segments.length > 2 ? segments[1] : "";
      if (!parts.has(part)) parts.set(part, []);
      parts.get(part).push(rel);
    }
    const home = parts.get("") ?? [];
    parts.delete("");
    let first = true;
    for (const [part, rels] of parts) {
      const label = DIR_LABELS.get(`${group.key}/${part}`) ?? part;
      sets.push(llmsSet(`${group.key}-${part}`, `${group.label}(${label})`, first ? [...home, ...rels] : rels));
      first = false;
    }
  }
  return sets;
}

/** customSets の1件を作る */
function llmsSet(key, label, rels) {
  return {
    label: key,
    description: `${label}のページ(${rels.map((rel) => rel.split("/").pop()).join(" / ")})`,
    paths: rels.map(slugOf),
  };
}

/** customSets の1ファイルの目安の上限(文字数)。取得上限が 10 万文字前後の読ませ先でも、1 回で読めるようにする */
const LLMS_SET_LIMIT = 70000;

/**
 * llms.txt に載せる全ページの索引。各ページの素の Markdown(/<slug>.md)へのリンクと一行説明。
 * 取得上限が小さい経路でも、必要なページだけを取りに行けるようにする。並びはサイドバーと同じ
 */
export function llmsPageLinks() {
  const summaries = homeSummaries();
  const rels = [HOME, ...sidebarGroups().flatMap((group) => group.rels)];
  return rels.map((rel) => ({
    label: titleOf(rel),
    url: `${siteUrl}/${slugOf(rel)}.md`,
    description: summaries.get(rel),
  }));
}
