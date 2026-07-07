#!/usr/bin/env node
/**
 * docs/ 配下の Markdown を GitHub Wiki (.wiki.git) に同期するスクリプト。
 * GitHub Actions (.github/workflows/sync-wiki.yml) から実行される想定。
 *
 * 仕様:
 * - docs/**\/*.md を Wiki ページに変換する。ページ名はパスの ".md" を除去し "/" を "-" に置換
 *   (docs/Home.md → Home、docs/guide/Setup.md → guide-Setup)
 * - 相対 Markdown リンク [text](Foo.md) / [text](dir/Foo.md#anchor) を Wiki ページリンクに書き換える
 * - _Sidebar.md を同期対象ファイル一覧から自動生成する
 * - "[private]" で始まる Wiki ページは同期・削除の対象外(Wiki 専用メモとして保護)
 * - docs/ に存在しない Wiki ページは削除する(_Sidebar / _Footer / [private]* を除く)
 * - 差分があるときだけ commit / push する
 */

const { execFileSync } = require("node:child_process");
const fs = require("node:fs");
const path = require("node:path");
const os = require("node:os");

const DOCS_DIR = "docs";
const COMMIT_MESSAGE = "Sync docs/ to GitHub Wiki";
const PROTECTED_PAGES = new Set(["_Sidebar", "_Footer"]);

/** docs/ からの相対パスを Wiki ページ名に変換する */
function pageNameFromPath(relPath) {
  return relPath.replace(/\.md$/i, "").split(/[\\/]/).join("-");
}

/**
 * Markdown 中の相対 .md リンクを Wiki ページリンクに書き換える。
 * 外部 URL・画像・アンカーのみのリンクは変更しない。
 */
function rewriteLinks(markdown, selfDir = "") {
  return markdown.replace(
    /(!?)\[([^\]]*)\]\(([^)\s]+?\.md)(#[^)\s]*)?\)/gi,
    (match, bang, text, target, anchor) => {
      if (bang === "!") return match; // 画像はそのまま
      if (/^[a-z][a-z0-9+.-]*:/i.test(target)) return match; // http: などの絶対 URL
      const resolved = path.posix
        .normalize(path.posix.join(selfDir, target))
        .replace(/^(\.\.\/)+/, "");
      const page = pageNameFromPath(resolved);
      return `[${text}](${page}${anchor ?? ""})`;
    },
  );
}

/** docs/ 配下の .md ファイルを相対パスで列挙する */
function listDocFiles(docsDir) {
  const results = [];
  const walk = (dir) => {
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
      const full = path.join(dir, entry.name);
      if (entry.isDirectory()) walk(full);
      else if (entry.isFile() && /\.md$/i.test(entry.name)) {
        results.push(path.relative(docsDir, full).split(path.sep).join("/"));
      }
    }
  };
  walk(docsDir);
  return results.sort();
}

/** _Sidebar.md の内容を生成する */
function buildSidebar(pageNames, privatePages) {
  const lines = ["## Pages", ""];
  const ordered = [
    ...pageNames.filter((p) => p === "Home"),
    ...pageNames.filter((p) => p !== "Home"),
  ];
  for (const page of ordered) {
    lines.push(`- [[${page}]]`);
  }
  if (privatePages.length > 0) {
    lines.push("", "## Wiki 専用ページ", "");
    for (const page of privatePages.sort()) {
      lines.push(`- [[${page}]]`);
    }
  }
  lines.push("");
  return lines.join("\n");
}

function git(cwd, ...args) {
  return execFileSync("git", args, { cwd, encoding: "utf8" });
}

function main() {
  if (!process.env.GITHUB_ACTIONS) {
    console.error(
      "このスクリプトは GitHub Actions 上でのみ実行できます (GITHUB_ACTIONS が未設定)。",
    );
    process.exit(1);
  }

  const token = process.env.GITHUB_TOKEN;
  const repo = process.env.GITHUB_REPO;
  if (!token || !repo) {
    console.error("GITHUB_TOKEN / GITHUB_REPO 環境変数が必要です。");
    process.exit(1);
  }

  const docsDir = path.resolve(DOCS_DIR);
  if (!fs.existsSync(docsDir)) {
    console.error(`${DOCS_DIR}/ が見つかりません。`);
    process.exit(1);
  }

  // 1. Wiki リポジトリを clone
  const wikiUrl = `https://x-access-token:${token}@github.com/${repo}.wiki.git`;
  const wikiDir = fs.mkdtempSync(path.join(os.tmpdir(), "wiki-"));
  console.log(`Cloning ${repo}.wiki.git ...`);
  execFileSync("git", ["clone", "--depth", "1", wikiUrl, wikiDir], {
    stdio: ["ignore", "inherit", "inherit"],
  });
  git(wikiDir, "config", "user.name", "github-actions[bot]");
  git(
    wikiDir,
    "config",
    "user.email",
    "41898282+github-actions[bot]@users.noreply.github.com",
  );

  // 2. docs/ の各ページを Wiki へコピー(リンク書き換え付き)
  const docFiles = listDocFiles(docsDir);
  const syncedPages = new Set();
  for (const relPath of docFiles) {
    const page = pageNameFromPath(relPath);
    const selfDir = path.posix.dirname(relPath) === "." ? "" : path.posix.dirname(relPath);
    const content = fs.readFileSync(path.join(docsDir, relPath), "utf8");
    fs.writeFileSync(path.join(wikiDir, `${page}.md`), rewriteLinks(content, selfDir));
    syncedPages.add(page);
    console.log(`  sync: ${relPath} -> ${page}.md`);
  }

  // 3. docs/ に存在しない Wiki ページを削除([private]* と _Sidebar / _Footer は保護)
  const privatePages = [];
  for (const entry of fs.readdirSync(wikiDir)) {
    if (!/\.md$/i.test(entry)) continue;
    const page = entry.replace(/\.md$/i, "");
    if (page.startsWith("[private]")) {
      privatePages.push(page);
      continue;
    }
    if (PROTECTED_PAGES.has(page)) continue;
    if (!syncedPages.has(page)) {
      fs.rmSync(path.join(wikiDir, entry));
      console.log(`  delete: ${entry}`);
    }
  }

  // 4. _Sidebar.md を自動生成
  fs.writeFileSync(
    path.join(wikiDir, "_Sidebar.md"),
    buildSidebar([...syncedPages].sort(), privatePages),
  );

  // 5. 差分があるときだけ commit / push
  git(wikiDir, "add", "--all");
  try {
    git(wikiDir, "diff", "--cached", "--quiet");
    console.log("変更なし。push をスキップします。");
    return;
  } catch {
    // 差分あり
  }
  git(wikiDir, "commit", "-m", COMMIT_MESSAGE);
  git(wikiDir, "push", "origin", "HEAD");
  console.log("Wiki へ push しました。");
}

if (require.main === module) {
  main();
}

module.exports = { pageNameFromPath, rewriteLinks, buildSidebar };
