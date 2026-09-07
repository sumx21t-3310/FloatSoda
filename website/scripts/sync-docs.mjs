#!/usr/bin/env node
/**
 * docs/*.md を Starlight のコンテンツへ変換する。
 * `npm run build` / `npm run dev` の前に自動実行される(package.json の prebuild / predev)。
 *
 * docs/ が唯一のソースで、生成先はどちらも Git 管理外(website/.gitignore):
 * - src/content/docs/<slug>.md … Starlight が読むページ
 * - public/<slug>.md           … LLM 向けの素の Markdown(https://<site>/<slug>.md で配信)
 *
 * ランディング(website/content/index.mdx)はこのスクリプトの対象外で、src/pages/index.astro が描画する。
 *
 * 変換内容:
 * - 先頭の H1 を frontmatter の title にし、本文からは除く(Starlight がタイトルを描画する)
 * - 「← [Home](Home.md)」のナビ行と、その直前の水平線を除く(サイドバーが担う)
 * - 相対 .md リンクをサイト内パスへ、docs/ の外を指すリンクを GitHub の URL へ書き換える
 * - editUrl(GitHub の編集ページ)と lastUpdated(docs/ 側の最終コミット日時)を付ける
 * - コードフェンスの中は一切変更しない
 */
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import {
  docsDir,
  homeSummaries,
  hrefOf,
  listDocNames,
  repoRoot,
  repositoryUrl,
  siteUrl,
  slugOf,
  websiteDir,
} from "./docs-source.mjs";

const contentDir = path.join(websiteDir, "src", "content", "docs");
const publicDir = path.join(websiteDir, "public");
const docNames = listDocNames();
const summaries = homeSummaries();

const FENCE = /^\s*(```|~~~)/;
const HOME_NAV = /^←\s*\[Home\]\(Home\.md\)\s*$/;
const LINK = /(!?)\[([^\]]*)\]\(([^)\s]+)((?:\s+"[^"]*")?)\)/g;

/** Markdown 中のリンクをサイト向けに書き換える(1 行ぶん) */
function rewriteLinks(line) {
  return line.replace(LINK, (match, bang, text, target, title) => {
    if (bang === "!") return match; // 画像はそのまま
    if (/^[a-z][a-z0-9+.-]*:/i.test(target)) return match; // http: などの絶対 URL
    if (target.startsWith("#")) return match; // ページ内アンカー

    const hashIndex = target.indexOf("#");
    const file = hashIndex < 0 ? target : target.slice(0, hashIndex);
    const anchor = hashIndex < 0 ? "" : target.slice(hashIndex);
    const resolved = path.posix.normalize(path.posix.join("docs", file));

    if (resolved.startsWith("docs/")) {
      const rel = resolved.slice("docs/".length);
      const page = rel.match(/^([A-Za-z0-9_-]+)\.md$/);
      if (page && docNames.includes(page[1])) {
        return `[${text}](${hrefOf(page[1])}${anchor}${title})`;
      }
    }
    // docs/ の外(../CONTRIBUTING.md など)や、docs/ 内の .md 以外はリポジトリの GitHub ページへ
    return `[${text}](${repositoryUrl}/blob/main/${resolved}${anchor}${title})`;
  });
}

/**
 * description の予備手段: 最初の見出しより前にある最初の地の文を使う。
 * 通常は Home.md のページ一覧表の「内容」列(homeSummaries)を使い、表に無いページだけここへ来る。
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
function lastCommitDate(name) {
  try {
    const out = execFileSync("git", ["log", "-1", "--format=%cI", "--", `docs/${name}.md`], {
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
function transform(name, source) {
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
    body.push(rewriteLinks(line));
  }

  if (title === undefined) {
    throw new Error(`docs/${name}.md: 先頭の H1(ページタイトル)が見つかりません`);
  }
  const text = `${body.join("\n").replace(/^\n+/, "").replace(/\n{3,}/g, "\n\n").trimEnd()}\n`;
  return { title, description: summaries.get(name) ?? describe(text), body: text };
}

function frontmatter(name, page) {
  const fields = [`title: ${JSON.stringify(page.title)}`];
  if (page.description) fields.push(`description: ${JSON.stringify(page.description)}`);
  fields.push(`editUrl: ${JSON.stringify(`${repositoryUrl}/edit/main/docs/${name}.md`)}`);
  const date = lastCommitDate(name);
  if (date) fields.push(`lastUpdated: ${date}`);
  return `---\n${fields.join("\n")}\n---\n\n`;
}

/** 生成先の .md だけを消す。それ以外のファイルには触れない */
function clearGenerated(dir) {
  if (!fs.existsSync(dir)) return;
  for (const entry of fs.readdirSync(dir)) {
    if (/\.md$/i.test(entry)) fs.rmSync(path.join(dir, entry));
  }
}

function main() {
  fs.mkdirSync(contentDir, { recursive: true });
  clearGenerated(contentDir);
  clearGenerated(publicDir);

  for (const name of docNames) {
    const source = fs.readFileSync(path.join(docsDir, `${name}.md`), "utf8");
    const page = transform(name, source);
    const slug = slugOf(name);

    fs.writeFileSync(path.join(contentDir, `${slug}.md`), frontmatter(name, page) + page.body);

    // LLM 向けの素の Markdown。サイト内リンクは絶対 URL にして、単体で読んでも辿れるようにする
    const raw = `# ${page.title}\n\n${page.body}`.replace(/\]\(\//g, `](${siteUrl}/`);
    fs.writeFileSync(path.join(publicDir, `${slug}.md`), raw);
  }
  console.log(`sync-docs: ${docNames.length} pages generated from docs/`);
}

main();
