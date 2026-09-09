#!/usr/bin/env node
/**
 * docs/ 配下の Markdown(サブディレクトリ込み)を Starlight のコンテンツへ変換する。
 * `npm run build` / `npm run dev` の前に自動実行される(package.json の prebuild / predev)。
 *
 * docs/ が唯一のソースで、生成先はどちらも Git 管理外(website/.gitignore):
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
 * - 相対 .md リンク(同じ階層も別の階層も)をサイト内パスへ、docs/ の外を指すリンクを GitHub の URL へ書き換える
 * - editUrl(GitHub の編集ページ)と lastUpdated(docs/ 側の最終コミット日時)を付ける
 * - コードフェンスの中は一切変更しない
 */
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import {
  HOME,
  contentPathOf,
  docsDir,
  homeSummaries,
  hrefOf,
  hrefOfName,
  listDocs,
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
const summaries = homeSummaries();

const FENCE = /^\s*(```|~~~)/;
const HOME_NAV = /^←\s*\[[^\]]*\]\((?:\.\.\/)*Home\.md\)\s*$/;
const LINK = /(!?)\[([^\]]*)\]\(([^)\s]+)((?:\s+"[^"]*")?)\)/g;

/** Markdown 中のリンクをサイト向けに書き換える(1 行ぶん)。selfDir はそのページの docs/ 内ディレクトリ */
function rewriteLinks(line, selfDir) {
  return line.replace(LINK, (match, bang, text, target, title) => {
    if (bang === "!") return match; // 画像はそのまま
    if (/^[a-z][a-z0-9+.-]*:/i.test(target)) return match; // http: などの絶対 URL
    if (target.startsWith("#")) return match; // ページ内アンカー

    const hashIndex = target.indexOf("#");
    const file = hashIndex < 0 ? target : target.slice(0, hashIndex);
    const anchor = hashIndex < 0 ? "" : target.slice(hashIndex);
    const resolved = path.posix.normalize(path.posix.join("docs", selfDir, file));

    if (resolved.startsWith("docs/")) {
      const page = resolved.slice("docs/".length).match(/^(.+)\.md$/i);
      if (page && rels.has(page[1])) {
        return `[${text}](${hrefOf(page[1])}${anchor}${title})`;
      }
    }
    // docs/ の外(../CONTRIBUTING.md など)や、docs/ 内の .md 以外はリポジトリの GitHub ページへ
    return `[${text}](${repositoryUrl}/blob/main/${resolved}${anchor}${title})`;
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

/** docs/ 側ファイルの最終コミット日時(ISO 8601)。Git が使えなければ undefined */
function lastCommitDate(rel) {
  try {
    const out = execFileSync("git", ["log", "-1", "--format=%cI", "--", `docs/${rel}.md`], {
      cwd: repoRoot,
      encoding: "utf8",
      stdio: ["ignore", "pipe", "ignore"],
    }).trim();
    return out || undefined;
  } catch {
    return undefined;
  }
}

/** 1 ページぶんを変換する */
function transform(doc, source) {
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
    body.push(rewriteLinks(line, doc.dir));
  }

  if (title === undefined) {
    throw new Error(`docs/${doc.rel}.md: 先頭の H1(ページタイトル)が見つかりません`);
  }
  const text = `${body.join("\n").replace(/^\n+/, "").replace(/\n{3,}/g, "\n\n").trimEnd()}\n`;
  return { title, description: summaries.get(doc.rel) ?? describe(text), body: text };
}

function frontmatter(doc, page) {
  const fields = [`title: ${JSON.stringify(page.title)}`];
  if (page.description) fields.push(`description: ${JSON.stringify(page.description)}`);
  fields.push(`editUrl: ${JSON.stringify(`${repositoryUrl}/edit/main/docs/${doc.rel}.md`)}`);
  const date = lastCommitDate(doc.rel);
  if (date) fields.push(`lastUpdated: ${date}`);
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
    const slug = slugOf(doc.rel);
    if (slug === "index") {
      throw new Error(
        `docs/${doc.rel}.md はサイトのトップ(/)と衝突します。トップはランディング(website/content/index.mdx)が担います`,
      );
    }
    const source = fs.readFileSync(path.join(docsDir, `${doc.rel}.md`), "utf8");
    const page = transform(doc, source);

    writeFile(path.join(contentDir, `${contentPathOf(doc.rel)}.md`), frontmatter(doc, page) + page.body);

    // LLM 向けの素の Markdown。サイト内リンクは絶対 URL にして、単体で読んでも辿れるようにする
    const raw = `# ${page.title}\n\n${page.body}`.replace(/\]\(\//g, `](${siteUrl}/`);
    writeFile(path.join(publicDir, `${slug}.md`), raw);
  }
  writeFile(landingFile, `${JSON.stringify(landingData(), null, 2)}\n`);
  console.log(`sync-docs: ${docs.length} pages generated from docs/`);
}

main();
