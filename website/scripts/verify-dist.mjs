#!/usr/bin/env node
/**
 * ビルド生成物(dist/)を検査する。`npm run build` の後に実行する(CI では必須)。
 *
 * 検査内容:
 * 0. ランディング(サイト専用ページ)が生成されている
 * 1. docs/ の全ページが HTML と素の Markdown の両方で生成されている
 * 2. llms.txt / llms-full.txt が存在し、llms-full.txt に全ページのタイトルが含まれる
 * 3. HTML 内のサイト内リンク(href="/…")の遷移先ページとアンカー(#…)が実在する
 */
import fs from "node:fs";
import path from "node:path";
import { docsDir, listDocNames, slugOf, websiteDir } from "./docs-source.mjs";

const distDir = path.join(websiteDir, "dist");
const failures = [];

function pageFile(slug) {
  return path.join(distDir, slug, "index.html");
}

/** サイト内パス(/foo/ や /foo.md)を dist 上のファイルへ解決する。無ければ undefined */
function resolveHref(href) {
  const clean = href.replace(/^\//, "").replace(/\/$/, "");
  const candidates =
    clean === ""
      ? [path.join(distDir, "index.html")]
      : [path.join(distDir, clean, "index.html"), path.join(distDir, clean)];
  return candidates.find((file) => fs.existsSync(file) && fs.statSync(file).isFile());
}

function decodeAttr(value) {
  const unescaped = value.replace(/&amp;/g, "&").replace(/&#x27;/g, "'").replace(/&quot;/g, '"');
  try {
    return decodeURIComponent(unescaped);
  } catch {
    return unescaped;
  }
}

function* htmlFiles(dir) {
  for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
    const full = path.join(dir, entry.name);
    if (entry.isDirectory()) yield* htmlFiles(full);
    else if (entry.name.endsWith(".html")) yield full;
  }
}

// 0. ランディング(website/content/index.mdx 由来)
if (!fs.existsSync(path.join(distDir, "index.html"))) failures.push("landing page missing: index.html");

// 1. ページの存在
const names = listDocNames();
for (const name of names) {
  const slug = slugOf(name);
  if (!fs.existsSync(pageFile(slug))) failures.push(`page missing: ${slug} (docs/${name}.md)`);
  if (!fs.existsSync(path.join(distDir, `${slug}.md`))) failures.push(`raw markdown missing: ${slug}.md`);
}

// 2. llms.txt
for (const file of ["llms.txt", "llms-full.txt"]) {
  const full = path.join(distDir, file);
  if (!fs.existsSync(full) || fs.statSync(full).size === 0) failures.push(`${file} missing or empty`);
}
const llmsFullPath = path.join(distDir, "llms-full.txt");
if (fs.existsSync(llmsFullPath)) {
  const llmsFull = fs.readFileSync(llmsFullPath, "utf8");
  for (const name of names) {
    const source = fs.readFileSync(path.join(docsDir, `${name}.md`), "utf8");
    const title = source.match(/^#\s+(.+)$/m)?.[1]?.trim();
    if (title && !llmsFull.includes(title)) failures.push(`llms-full.txt lacks page: ${name} ("${title}")`);
  }
}

// 3. サイト内リンクとアンカー
const idCache = new Map();
function idsOf(file) {
  if (!idCache.has(file)) {
    const html = fs.readFileSync(file, "utf8");
    idCache.set(file, new Set([...html.matchAll(/\sid="([^"]+)"/g)].map((m) => decodeAttr(m[1]))));
  }
  return idCache.get(file);
}

let linkCount = 0;
for (const file of htmlFiles(distDir)) {
  const html = fs.readFileSync(file, "utf8");
  for (const match of html.matchAll(/href="(\/[^"]*)"/g)) {
    const href = decodeAttr(match[1]);
    if (href.startsWith("/_astro/") || href.startsWith("/pagefind/")) continue;
    const [target, anchor] = href.split("#");
    const resolved = target === "" ? file : resolveHref(target);
    linkCount++;
    if (!resolved) {
      failures.push(`broken link ${href} in ${path.relative(distDir, file)}`);
      continue;
    }
    if (anchor && resolved.endsWith(".html") && !idsOf(resolved).has(anchor)) {
      failures.push(`missing anchor ${href} in ${path.relative(distDir, file)}`);
    }
  }
}

if (failures.length > 0) {
  console.error(`verify-dist: ${failures.length} problem(s)`);
  for (const failure of failures) console.error(`  - ${failure}`);
  process.exit(1);
}
console.log(`verify-dist: OK (${names.length} pages, ${linkCount} internal links checked)`);
