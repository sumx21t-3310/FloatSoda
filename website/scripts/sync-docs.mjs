#!/usr/bin/env node
/**
 * docs/ 配下の Markdown(サブディレクトリ込み)と、Widget カタログ(層1サンプルの README)を Starlight のコンテンツへ変換する。
 * `npm run build` / `npm run dev` の前に自動実行される(package.json の prebuild / predev)。
 *
 * docs/ と samples/*\/README.md が唯一のソースで、生成先はどちらも Git 管理外(website/.gitignore):
 * - src/content/docs/<path>.md … Starlight が読むページ(docs/user/Home.md は user/index.md になる)
 * - public/<slug>.md           … LLM 向けの素の Markdown(https://<site>/<slug>.md で配信)
 *
 * ランディング(website/content/index.mdx)の本文はこのスクリプトの対象外で、src/pages/index.astro が描画する。
 * ただし、ランディングの導線(3 系統の入口と主要ページへのリンク先)は docs/ の実在から計算して
 * src/generated/landing.json に書き出す(Git 管理外)。ページの移動・改名はここで例外になる。
 *
 * 変換内容:
 * - 先頭の H1 を frontmatter の title にし、本文からは除く(Starlight がタイトルを描画する)
 * - 「← [Home](Home.md)」「← [Home](../Home.md)」のナビ行と、その直前の水平線を除く(サイドバーが担う)
 * - 相対 .md リンク(同じ階層も別の階層も)をサイト内パスへ、サンプルのディレクトリへのリンク(../FloatSoda.Samples.X)を
 *   カタログのページへ、それ以外のリポジトリ内を指すリンクを GitHub の URL へ書き換える
 * - サンプルの README だけは、リンク先がリポジトリに無ければ例外にする(docs/ のページの移動・改名を検出する)
 * - editUrl(GitHub の編集ページ)と lastUpdated(ソース側の最終コミット日時)を付ける
 * - カタログのページの末尾に、サンプルのソース一式(GitHub)へのリンクを足す
 * - コードフェンスの中は一切変更しない
 */
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import {
  CATALOG_HOME,
  HOME,
  catalogHomeMarkdown,
  contentPathOf,
  descriptionOf,
  homeSummaries,
  hrefOf,
  hrefOfName,
  isCatalogRel,
  listCatalog,
  listDocs,
  readDoc,
  repoRoot,
  repositoryUrl,
  siteUrl,
  slugOf,
  trackEntry,
  websiteDir,
} from "./docs-source.mjs";

const contentDir = path.join(websiteDir, "src", "content", "docs");
const publicDir = path.join(websiteDir, "public");
const landingFile = path.join(websiteDir, "src", "generated", "landing.json");
const docs = listDocs();
const rels = new Set(docs.map((doc) => doc.rel));
const catalog = listCatalog();
const summaries = homeSummaries();

const FENCE = /^\s*(```|~~~)/;
const HOME_NAV = /^←\s*\[[^\]]*\]\((?:\.\.\/)*Home\.md\)\s*$/;
const LINK = /(!?)\[([^\]]*)\]\(([^)\s]+)((?:\s+"[^"]*")?)\)/g;
/** samples/ 直下のサンプルのディレクトリ(末尾の "/" や "/README.md" を含む) */
const SAMPLE_LINK = /^samples\/([^/]+?)(?:\/(?:README\.md)?)?$/i;

/** リポジトリ内のパスの GitHub URL。ディレクトリは tree、ファイルは blob */
function githubUrl(repoPath) {
  const full = path.join(repoRoot, repoPath);
  const kind = fs.existsSync(full) && fs.statSync(full).isDirectory() ? "tree" : "blob";
  return `${repositoryUrl}/${kind}/main/${repoPath}`;
}

/**
 * リポジトリ内のパス(正規化済み)を、サイト内パスか GitHub の URL に書き換える。
 * strict のときは、リンク先がリポジトリに無ければ例外にする
 */
function resolveTarget(resolved, strict, origin) {
  const page = resolved.match(/^docs\/(.+)\.md$/i);
  if (page && rels.has(page[1])) return hrefOf(page[1]);

  const trimmed = resolved.replace(/\/$/, "");
  if (trimmed === "samples") return hrefOf(CATALOG_HOME);
  const sample = trimmed.match(SAMPLE_LINK);
  const entry = sample && catalog.find((item) => item.sample === sample[1]);
  if (entry) return hrefOf(entry.rel);

  if (strict && !fs.existsSync(path.join(repoRoot, resolved))) {
    throw new Error(`${origin}: リンク先 ${resolved} がリポジトリにありません(ページの移動・改名なら、リンクを直してください)`);
  }
  // docs/ の外(../CONTRIBUTING.md など)や、docs/ 内の .md 以外はリポジトリの GitHub ページへ
  return githubUrl(resolved);
}

/**
 * Markdown 中のリンクをサイト向けに書き換える(1 行ぶん)。
 * baseDir はそのページのリポジトリ内ディレクトリ(例: "docs/user"、"samples/FloatSoda.Samples.Padding")
 */
function rewriteLinks(line, baseDir, strict, origin) {
  return line.replace(LINK, (match, bang, text, target, title) => {
    if (bang === "!") return match; // 画像はそのまま
    if (/^[a-z][a-z0-9+.-]*:/i.test(target)) return match; // http: などの絶対 URL
    if (target.startsWith("#")) return match; // ページ内アンカー
    if (target.startsWith("/")) return match; // サイト内の絶対パス(カタログの一覧が使う)

    const hashIndex = target.indexOf("#");
    const file = hashIndex < 0 ? target : target.slice(0, hashIndex);
    const anchor = hashIndex < 0 ? "" : target.slice(hashIndex);
    const resolved = path.posix.normalize(path.posix.join(baseDir, file));
    return `[${text}](${resolveTarget(resolved, strict, origin)}${anchor}${title})`;
  });
}

/**
 * description の予備手段: 最初の見出しより前にある最初の地の文を使う。
 * 通常は docs/Home.md のページ一覧表の「内容」列(homeSummaries)を使い、表に無いページだけここへ来る。
 */
function describe(body) {
  const intro = body.split(/\n(?=##\s)/)[0];
  for (const block of intro.split(/\n\s*\n/)) {
    const text = block.trim();
    if (!text || /^(#|>|\||[-*+]\s|\d+\.\s|```|~~~|<)/.test(text)) continue;
    const plain = text
      .replace(/\[([^\]]*)\]\([^)]*\)/g, "$1")
      .replace(/[*_`]/g, "")
      .replace(/\s+/g, " ");
    return plain.length > 160 ? `${plain.slice(0, 159)}…` : plain;
  }
  return undefined;
}

/** リポジトリ内のファイルの最終コミット日時(ISO 8601)。Git が使えなければ undefined */
function lastCommitDate(repoPath) {
  try {
    const out = execFileSync("git", ["log", "-1", "--format=%cI", "--", repoPath], {
      cwd: repoRoot,
      encoding: "utf8",
      stdio: ["ignore", "pipe", "ignore"],
    }).trim();
    return out || undefined;
  } catch {
    return undefined;
  }
}

/**
 * 1 ページぶんを変換する。
 *   source  … Markdown の本文
 *   origin  … ソースのリポジトリ内パス(エラーメッセージと editUrl 用。生成するページは null)
 *   baseDir … 相対リンクの起点(リポジトリ内ディレクトリ)
 *   strict  … リンク先の実在を確かめるか
 */
function transform({ source, origin, baseDir, strict }) {
  const lines = source.replace(/\r\n/g, "\n").split("\n");
  const body = [];
  let title;
  let inFence = false;

  for (const line of lines) {
    if (FENCE.test(line)) {
      inFence = !inFence;
      body.push(line);
      continue;
    }
    if (inFence) {
      body.push(line);
      continue;
    }
    if (title === undefined && /^#\s+/.test(line)) {
      title = line.replace(/^#\s+/, "").trim();
      continue;
    }
    if (HOME_NAV.test(line)) {
      // ナビ行の直前に置かれた区切りの水平線も一緒に除く
      while (body.length > 0 && /^(\s*|-{3,}\s*)$/.test(body[body.length - 1])) body.pop();
      continue;
    }
    body.push(rewriteLinks(line, baseDir, strict, origin));
  }

  if (title === undefined) {
    throw new Error(`${origin}: 先頭の H1(ページタイトル)が見つかりません`);
  }
  const text = `${body.join("\n").replace(/^\n+/, "").replace(/\n{3,}/g, "\n\n").trimEnd()}\n`;
  return { title, body: text };
}

/** origin が null(生成するページ)なら、編集リンクと更新日時を付けない */
function frontmatter(page, origin) {
  const fields = [`title: ${JSON.stringify(page.title)}`];
  if (page.description) fields.push(`description: ${JSON.stringify(page.description)}`);
  if (origin) {
    fields.push(`editUrl: ${JSON.stringify(`${repositoryUrl}/edit/main/${origin}`)}`);
    const date = lastCommitDate(origin);
    if (date) fields.push(`lastUpdated: ${date}`);
  } else {
    fields.push("editUrl: false");
  }
  return `---\n${fields.join("\n")}\n---\n\n`;
}

/** 生成先の .md を再帰的に消し、空になったディレクトリも消す。それ以外のファイルには触れない */
function clearGenerated(dir) {
  if (!fs.existsSync(dir)) return;
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) {
      clearGenerated(full);
      if (fs.readdirSync(full).length === 0) fs.rmdirSync(full);
    } else if (/\.md$/i.test(entry.name)) {
      fs.rmSync(full);
    }
  }
}

function writeFile(file, content) {
  fs.mkdirSync(path.dirname(file), { recursive: true });
  fs.writeFileSync(file, content);
}

/** Starlight のページと、LLM 向けの素の Markdown を書き出す */
function writePage(rel, page, origin) {
  writeFile(path.join(contentDir, `${contentPathOf(rel)}.md`), frontmatter(page, origin) + page.body);

  // LLM 向けの素の Markdown。サイト内リンクは絶対 URL にして、単体で読んでも辿れるようにする
  const raw = `# ${page.title}\n\n${page.body}`.replace(/\]\(\//g, `](${siteUrl}/`);
  writeFile(path.join(publicDir, `${slugOf(rel)}.md`), raw);
}

/**
 * ランディング(src/pages/index.astro)が使う導線。
 * 3 系統の入口は docs/<dir>/Home.md が現れると自動で切り替わり、それまでは docs/Home.md へ送る(#188 の再編待ち)。
 * 主要ページへのリンクはファイル名で探すので、ページが移動しても追従し、消えたらここで例外になる
 */
function landingData() {
  const home = hrefOfName(HOME);
  const tracks = {
    user: trackEntry("user", home),
    contributor: trackEntry("contributor", home),
    api: trackEntry("api", null),
  };
  const links = {
    home,
    gettingStarted: hrefOfName("GettingStarted"),
    targetUsers: hrefOfName("TargetUsers"),
    widgetSystem: hrefOfName("WidgetSystem"),
    architecture: hrefOfName("Architecture"),
    apiDesign: hrefOfName("APIDesign"),
    roadmap: `${home}#ロードマップphase`,
  };
  // 素の Markdown 版の例(末尾の "/" を ".md" に置き換えた URL)
  links.widgetSystemMd = links.widgetSystem.replace(/\/$/, ".md");
  return { tracks, links };
}

function main() {
  fs.mkdirSync(contentDir, { recursive: true });
  clearGenerated(contentDir);
  clearGenerated(publicDir);

  for (const doc of docs) {
    const origin = `docs/${doc.rel}.md`;
    if (slugOf(doc.rel) === "index") {
      throw new Error(`${origin} はサイトのトップ(/)と衝突します。トップはランディング(website/content/index.mdx)が担います`);
    }
    if (isCatalogRel(doc.rel) || slugOf(doc.rel) === slugOf(CATALOG_HOME)) {
      throw new Error(`${origin} は Widget カタログ(${hrefOf(CATALOG_HOME)})と衝突します。カタログは samples/ の README から生成します`);
    }
    const page = transform({ source: readDoc(doc.rel), origin, baseDir: `docs/${doc.dir}`.replace(/\/$/, ""), strict: false });
    page.description = summaries.get(doc.rel) ?? describe(page.body);
    writePage(doc.rel, page, origin);
  }

  // Widget カタログ: 一覧は README から生成し、各ページは README をそのまま変換する
  if (catalog.length > 0) {
    const home = transform({ source: catalogHomeMarkdown(), origin: null, baseDir: "samples", strict: true });
    home.description = descriptionOf(CATALOG_HOME);
    writePage(CATALOG_HOME, home, null);
  }
  for (const item of catalog) {
    const page = transform({ source: readDoc(item.rel), origin: item.source, baseDir: `samples/${item.sample}`, strict: true });
    page.description = descriptionOf(item.rel);
    page.body += `\nこのサンプルのコード一式は [samples/${item.sample}](${githubUrl(`samples/${item.sample}`)}) にあります。\n`;
    writePage(item.rel, page, item.source);
  }

  writeFile(landingFile, `${JSON.stringify(landingData(), null, 2)}\n`);
  console.log(`sync-docs: ${docs.length} pages generated from docs/, ${catalog.length} catalog pages from samples/`);
}

main();
