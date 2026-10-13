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
 *
 * docs/ の外から取り込むページは、Widget カタログだけ(#188 の層1サンプル。#219 の完了条件7):
 *   samples/FloatSoda.Samples.Padding/README.md → /user/catalog/padding/
 *   (一覧ページ。README から生成する)          → /user/catalog/
 * 「FloatSoda を使う」グループの中の入れ子のグループになる。Wiki 同期(scripts/sync-docs-to-wiki.js)は docs/ だけを読むので、Wiki には載らない。
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

export const websiteDir = path.dirname(path.dirname(fileURLToPath(import.meta.url)));
export const repoRoot = path.dirname(websiteDir);
export const docsDir = path.join(repoRoot, "docs");
export const samplesDir = path.join(repoRoot, "samples");

export const siteUrl = "https://floatsoda.sumx21t.com";
export const repositoryUrl = "https://github.com/sumx21t-3310/FloatSoda";

/** 各階層の入口ページ。docs/Home.md はドキュメント全体、docs/<dir>/Home.md はその系統の入口 */
export const HOME = "Home";

/**
 * docs/ 直下のページ(Home.md を除く)をまとめるグループ。#188 の C4 Lv1(導入)に当たる。
 * key は系統ディレクトリ(user / contributor / api)と衝突しない語にする。衝突すると、そのディレクトリのグループに合流してしまう
 */
const INTRO_GROUP = { key: "intro", label: "導入" };

/**
 * Widget カタログ。層1サンプル(samples/FloatSoda.Samples.<Name>/ に README.md と <Name>Demo.cs があるもの)を、
 * dir の系統グループの中に、label の入れ子グループとして並べる。ページの rel は "<dir>/<name>/<Name>"、一覧は "<dir>/<name>/Home"
 */
export const CATALOG = { dir: "user", name: "catalog", label: "Widget カタログ" };
const SAMPLE_PREFIX = "FloatSoda.Samples.";
const CATALOG_PREFIX = `${CATALOG.dir}/${CATALOG.name}/`;
export const CATALOG_HOME = `${CATALOG_PREFIX}${HOME}`;

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

/**
 * Widget カタログに載せるサンプルを、名前の順に返す。要素は次の形:
 *   rel    … カタログ内のページ。例: "user/catalog/Padding"
 *   name   … サンプル名。例: "Padding"
 *   sample … samples/ 直下のディレクトリ名。例: "FloatSoda.Samples.Padding"
 *   source … リポジトリからの相対パス。例: "samples/FloatSoda.Samples.Padding/README.md"
 */
export function listCatalog() {
  if (!fs.existsSync(samplesDir)) return [];
  return fs
    .readdirSync(samplesDir, { withFileTypes: true })
    .filter((entry) => entry.isDirectory() && entry.name.startsWith(SAMPLE_PREFIX))
    .map((entry) => ({ sample: entry.name, name: entry.name.slice(SAMPLE_PREFIX.length) }))
    .filter(
      ({ sample, name }) =>
        fs.existsSync(path.join(samplesDir, sample, "README.md")) &&
        fs.existsSync(path.join(samplesDir, sample, `${name}Demo.cs`)),
    )
    .map(({ sample, name }) => ({
      rel: `${CATALOG_PREFIX}${name}`,
      name,
      sample,
      source: `samples/${sample}/README.md`,
    }))
    .sort((a, b) => a.name.localeCompare(b.name));
}

/** rel が Widget カタログのページ(一覧を含む)か */
export function isCatalogRel(rel) {
  return rel.startsWith(CATALOG_PREFIX);
}

/** rel の本文のファイル(リポジトリからの相対パス)。カタログの一覧は生成するので null */
export function sourceOf(rel) {
  if (rel === CATALOG_HOME) return null;
  if (isCatalogRel(rel)) {
    const entry = listCatalog().find((item) => item.rel === rel);
    if (!entry) throw new Error(`Widget カタログに ${rel} がありません`);
    return entry.source;
  }
  return `docs/${rel}.md`;
}

/** rel の本文(Markdown)。カタログの一覧は catalogHomeMarkdown() で作る */
export function readDoc(rel) {
  const source = sourceOf(rel);
  return source ? fs.readFileSync(path.join(repoRoot, source), "utf8") : catalogHomeMarkdown();
}

/** ページのタイトル(先頭の H1)。無ければファイル名 */
export function titleOf(rel) {
  if (rel === CATALOG_HOME) return CATALOG.label;
  return firstHeading(readDoc(rel)) ?? rel.split("/").pop();
}

/** カタログのページの一行説明: README の H1 の直後の段落の 1 文目。H1 の直後が見出しなら undefined */
export function catalogSummary(rel) {
  if (rel === CATALOG_HOME) return "ウィジェットごとの紹介と最小の使用例。サンプルの README から作る";
  const afterTitle = readDoc(rel).replace(/\r\n/g, "\n").split(/^# .*$/m)[1];
  const paragraph = afterTitle
    ?.split(/\n\s*\n/)
    .map((block) => block.trim())
    .find((block) => block);
  if (!paragraph || paragraph.startsWith("#")) return undefined;
  const plain = paragraph
    .replace(/\[([^\]]*)\]\([^)]*\)/g, "$1")
    .replace(/[*_]/g, "")
    .replace(/\s+/g, " ");
  const end = plain.indexOf("。");
  return end < 0 ? plain : plain.slice(0, end + 1);
}

/**
 * カタログの一覧ページの本文(H1 を含む Markdown)。手で書く一覧は置かず、サンプルの README から毎回作る。
 * リンクはサイト内パス(/user/catalog/padding/)
 */
export function catalogHomeMarkdown() {
  const rows = listCatalog().map(
    (item) => `| [${item.name}](${hrefOf(item.rel)}) | ${(catalogSummary(item.rel) ?? "").replace(/\|/g, "\\|")} |`,
  );
  return [
    `# ${CATALOG.label}`,
    "",
    "1 ページで 1 つのウィジェット群を紹介し、最小の使用例を載せています。各ページは、リポジトリの `samples/` にあるサンプルの README から作っています。",
    "",
    "| ウィジェット | 概要 |",
    "|---|---|",
    ...rows,
    "",
  ].join("\n");
}

/** ページの一行説明(description)。直下のページは docs/Home.md の表、カタログは README から。無ければ undefined */
export function descriptionOf(rel) {
  return isCatalogRel(rel) ? catalogSummary(rel)?.replace(/`/g, "") : homeSummaries().get(rel);
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
 * サイドバーのグループを返す(`[{ key, label, rels }]`)。key は ASCII で、_llms-txt/<key>.txt のファイル名にも使う。
 *
 * 1. docs/ 直下のページ: 「導入」グループ 1 つにまとめる。並びは Home.md の表の行順、表に無いページは後ろへ
 * 2. 系統ディレクトリ(docs/user/ など): 1 ディレクトリ = 1 グループ。ラベルは <dir>/Home.md の H1、
 *    並びは入口 → 同 Home.md のリンク順 → 残りをアルファベット順。系統どうしの並びは docs/Home.md でリンクされる順
 */
export function sidebarGroups() {
  const docs = listDocs();
  const groups = [];
  const push = (key, label, rel) => {
    let group = groups.find((g) => g.key === key);
    if (!group) {
      group = { key, label, rels: [] };
      groups.push(group);
    }
    if (!group.rels.includes(rel)) group.rels.push(rel);
  };

  const topLevel = docs.filter((doc) => doc.dir === "" && doc.name !== HOME);
  if (topLevel.length > 0) {
    for (const row of homeTableRows()) push(INTRO_GROUP.key, INTRO_GROUP.label, row.name);
    for (const doc of topLevel) push(INTRO_GROUP.key, INTRO_GROUP.label, doc.rel);
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
    const key = dir.toLowerCase();
    if (home) push(key, label, homeRel);
    for (const rel of linkedRels(homeRel, dir)) {
      if (pages.some((doc) => doc.rel === rel)) push(key, label, rel);
    }
    for (const doc of pages) push(key, label, doc.rel);
  }

  // Widget カタログ: 系統グループの末尾に、一覧 → サンプル名の順で足す。sidebarItems() が入れ子のグループにする
  const catalog = listCatalog();
  if (catalog.length > 0) {
    const key = CATALOG.dir.toLowerCase();
    const label = groups.find((g) => g.key === key)?.label ?? CATALOG.dir;
    push(key, label, CATALOG_HOME);
    for (const item of catalog) push(key, label, item.rel);
  }

  return groups;
}

/**
 * #188 の再編で種別のディレクトリへ移す前のページを、系統グループの末尾にまとめる入れ子のグループ。
 * dir 直下の Home.md 以外のページが対象。移し終えて直下が Home.md だけになると、グループは出なくなる
 */
const LEGACY_GROUP = { dir: "contributor", label: "移行前のページ" };

/**
 * Home.md を持たないディレクトリの、サイドバーでのグループ名。Diátaxis の種別名はサイトの見出しに出さない
 * (docs/contributor/WritingDocumentation.md の 1 章)ため、読者向けの名前に置き換える。表に無いディレクトリはディレクトリ名のまま
 */
const DIR_LABELS = {
  tutorials: "貢献までの流れ",
  "how-to": "作業の手順",
  reference: "守ること",
  explanation: "仕組みと理由",
};

/**
 * Starlight のサイドバーの items。系統グループの中を、docs/ のディレクトリの形で入れ子にする(初期状態は閉じる)。
 *
 * - 系統ディレクトリ直下のページは、そのまま並べる。ただし LEGACY_GROUP の系統では、Home.md 以外を末尾の入れ子にまとめる
 * - 深いディレクトリは、ディレクトリ 1 つ = 入れ子のグループ 1 つ。並びは group.rels の順(各 Home.md のリンク順)を保つ
 * - グループ名は、Home.md があればその H1、無ければ DIR_LABELS
 * - Widget カタログのページは、CATALOG.label の入れ子のグループにまとめる
 */
export function sidebarItems(group) {
  const rels = group.rels.filter((rel) => !isCatalogRel(rel));
  const root = { items: [], dirs: new Map() };
  const legacy = [];

  for (const rel of rels) {
    const parts = rel.split("/");
    if (parts.length <= 2) {
      const isLegacy = parts[0] === LEGACY_GROUP.dir && parts.length === 2 && parts[1] !== HOME;
      (isLegacy ? legacy : root.items).push({ rel });
      continue;
    }
    let node = root;
    for (const dir of parts.slice(1, -1)) {
      if (!node.dirs.has(dir)) {
        const child = { dir, items: [], dirs: new Map() };
        node.dirs.set(dir, child);
        node.items.push(child);
      }
      node = node.dirs.get(dir);
    }
    node.items.push({ rel });
  }

  const items = root.items.map(toSidebarEntry);
  if (legacy.length > 0) {
    items.push({ label: LEGACY_GROUP.label, collapsed: true, items: legacy.map(({ rel }) => ({ slug: slugOf(rel) })) });
  }
  const catalog = group.rels.filter(isCatalogRel);
  if (catalog.length > 0) {
    items.push({ label: CATALOG.label, collapsed: true, items: catalog.map((rel) => ({ slug: slugOf(rel) })) });
  }
  return items;
}

/** ページ 1 つ、または入れ子のグループ 1 つ(子の items を含む)を作る */
function toSidebarEntry(node) {
  if (node.rel) return { slug: slugOf(node.rel) };
  const home = node.items.find((item) => item.rel?.endsWith(`/${HOME}`));
  const label = home ? titleOf(home.rel) : (DIR_LABELS[node.dir] ?? node.dir);
  return { label, collapsed: true, items: node.items.map(toSidebarEntry) };
}

/** 分割ファイル(_llms-txt/<label>.txt)1 つの文字数(コードポイント数)の上限。scripts/verify-dist.mjs が出力で確かめる */
export const LLMS_SET_LIMIT = 70_000;

/**
 * 分けるときの目安。ページの元ファイルの文字数で詰める。出力にはページごとのタイトルと description、
 * ファイル先頭の <SYSTEM> 行が足されるので、上限より小さくして余白を残す
 */
const LLMS_SET_BUDGET = 60_000;

/** ページの元ファイルの文字数(コードポイント数) */
function sizeOf(rel) {
  return [...readDoc(rel)].length;
}

/**
 * ページの「束」: 系統ディレクトリからの相対ディレクトリ(例: contributor の "framework/core"、直下は "")。
 * カタログはまとめて 1 束
 */
function bundleOf(rel) {
  if (isCatalogRel(rel)) return CATALOG_PREFIX;
  return rel.split("/").slice(1, -1).join("/");
}

/**
 * 1 つのグループのページを、1 ファイル LLMS_SET_BUDGET 文字以下に分ける。戻り値はページ(rel)の配列の配列。
 * 1. グループ全体が収まれば、分けない
 * 2. 収まらなければ、束(ディレクトリ)ごとに、サイドバーの順で前から詰める。収まる束は分けずに 1 ファイルへ入れる
 * 3. 1 つの束だけで収まらなければ、その束はページの順で、いまのファイルの続きから詰めて分ける
 * 1 ページだけで上限を超えるときは、そのページだけのファイルになり、verify-dist が失敗する
 */
function packGroup(rels) {
  const total = rels.reduce((sum, rel) => sum + sizeOf(rel), 0);
  if (total <= LLMS_SET_BUDGET) return [rels];

  const bundles = [];
  for (const rel of rels) {
    const key = bundleOf(rel);
    let bundle = bundles.find((b) => b.key === key);
    if (!bundle) {
      bundle = { key, rels: [] };
      bundles.push(bundle);
    }
    bundle.rels.push(rel);
  }

  const parts = [];
  let current = [];
  let currentSize = 0;
  const flush = () => {
    if (current.length > 0) parts.push(current);
    current = [];
    currentSize = 0;
  };
  const add = (rel, size) => {
    if (currentSize + size > LLMS_SET_BUDGET) flush();
    current.push(rel);
    currentSize += size;
  };
  for (const bundle of bundles) {
    const size = bundle.rels.reduce((sum, rel) => sum + sizeOf(rel), 0);
    // 収まる束は、いまのファイルに入りきらなければ次のファイルから始める。収まらない束は、いまのファイルの続きからページ単位で詰める
    if (size <= LLMS_SET_BUDGET && currentSize + size > LLMS_SET_BUDGET) flush();
    for (const rel of bundle.rels) add(rel, sizeOf(rel));
  }
  flush();
  return parts;
}

/**
 * starlight-llms-txt の customSets。サイドバーのグループ(系統)ごとに _llms-txt/<key>.txt を出す。
 * llms-full.txt を読み切れない取得経路でも、1 回で読める大きさにする(各 LLMS_SET_LIMIT 文字以下)。
 * グループが大きければ packGroup() で分け、_llms-txt/<key>-1.txt、<key>-2.txt … とする。
 * label がそのままファイル名になるので ASCII の key を使い、人間向けの名前と含むページは description に書く
 */
export function llmsCustomSets() {
  return sidebarGroups().flatMap((group) => {
    const parts = packGroup(group.rels);
    return parts.map((rels, index) => {
      const names = rels.map((rel) => (rel.includes("/") ? rel.split("/").slice(1).join("/") : rel));
      const part = parts.length > 1 ? ` ${index + 1}/${parts.length}` : "";
      return {
        label: parts.length > 1 ? `${group.key}-${index + 1}` : group.key,
        description: `${group.label}のページ${part}(${names.join(" / ")})`,
        paths: rels.map(slugOf),
      };
    });
  });
}

/**
 * llms.txt に載せる全ページの索引。各ページの素の Markdown(/<slug>.md)へのリンクと一行説明。
 * 取得上限が小さい経路でも、必要なページだけを取りに行けるようにする。並びはサイドバーと同じ
 */
export function llmsPageLinks() {
  const rels = [HOME, ...sidebarGroups().flatMap((group) => group.rels)];
  return rels.map((rel) => ({
    label: titleOf(rel),
    url: `${siteUrl}/${slugOf(rel)}.md`,
    description: descriptionOf(rel),
  }));
}
