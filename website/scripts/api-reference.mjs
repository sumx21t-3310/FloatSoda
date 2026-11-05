#!/usr/bin/env node
/**
 * src/ の XML ドキュメントコメントから API リファレンスのページを生成する(#318)。
 * `npm run build` の前に自動実行される(package.json の prebuild)。`npm run dev` では実行しないので、
 * 手元で API のタブを見るときは先に `npm run api` を実行する。
 *
 * 1. docfx(リポジトリ直下の dotnet-tools.json で版を固定)で、PROJECTS の public API を Markdown にする
 *    OpenVR の生バインディング(src/FloatSoda.OVR/openvr_api.cs)の型は、フィルターで外す
 * 2. 出力を Starlight 向けに直し、generated/api/ に書く(Git 管理外。website/.gitignore)
 *    - 名前空間ごとに 1 ディレクトリ: generated/api/<名前空間>/Home.md(一覧)と <型>.md
 *    - 先頭の H1 を「<型名> <種類>」にし、見出しを日本語にする
 *    - <xref> と型どうしのリンクを、生成したページへの相対リンクにする(載せない型は文字だけにする)
 *    - generated/api/Home.md(名前空間の一覧)を作る
 *
 * generated/api/ は docs-source.mjs が docs/ と並ぶ 2 つ目のソースとして読み、api のタブになる
 */
import { execFileSync } from "node:child_process";
import fs from "node:fs";
import path from "node:path";
import { HOME, apiDir, repoRoot, websiteDir } from "./docs-source.mjs";

/** API リファレンスに載せるプロジェクト(src/ からの相対パス)。NuGet で配布しているもの */
const PROJECTS = [
  "FloatSoda/FloatSoda.csproj",
  "FloatSoda.Geometry/FloatSoda.Geometry.csproj",
  "FloatSoda.Engine.Abstractions/FloatSoda.Engine.Abstractions.csproj",
  "FloatSoda.Rendering/FloatSoda.Rendering.csproj",
  "FloatSoda.Engine/FloatSoda.Engine.csproj",
  "FloatSoda.Engine.Desktop/FloatSoda.Engine.Desktop.csproj",
  "FloatSoda.Engine.OVR/FloatSoda.Engine.OVR.csproj",
  "FloatSoda.OVR/FloatSoda.OVR.csproj",
  "FloatSoda.Testing/FloatSoda.Testing.csproj",
];

/** 載せない型を宣言しているファイル(OpenVR の生バインディング)。名前空間は FloatSoda.OVR */
const RAW_BINDINGS = path.join(repoRoot, "src", "FloatSoda.OVR", "openvr_api.cs");

const workDir = path.join(websiteDir, "generated", ".docfx");
const rawDir = path.join(workDir, "raw");

const KIND_LABELS = {
  Class: "クラス",
  Struct: "構造体",
  Interface: "インターフェイス",
  Enum: "列挙型",
  Delegate: "デリゲート",
};

/** 型のページの見出し(## と ####)の訳。ここに無い見出しはそのまま残す */
const SECTION_LABELS = {
  Constructors: "コンストラクター",
  Properties: "プロパティ",
  Methods: "メソッド",
  Fields: "フィールド",
  Events: "イベント",
  Operators: "演算子",
  Remarks: "解説",
  Examples: "例",
  "See Also": "関連項目",
  Parameters: "パラメーター",
  Returns: "戻り値",
  "Property Value": "プロパティの値",
  "Field Value": "フィールドの値",
  "Event Type": "イベントの型",
  Exceptions: "例外",
  Implements: "実装するインターフェイス",
  "Inherited Members": "継承したメンバー",
  Inheritance: "継承",
  Derived: "派生型",
  "Type Parameters": "型パラメーター",
  "Extension Methods": "拡張メソッド",
};

/** 名前空間のページの見出し(###)の訳 */
const NAMESPACE_SECTION_LABELS = {
  Namespaces: "名前空間",
  Classes: "クラス",
  Structs: "構造体",
  Interfaces: "インターフェイス",
  Enums: "列挙型",
  Delegates: "デリゲート",
};

const FENCE = /^\s*(```|~~~)/;

/** openvr_api.cs が宣言する型の名前(入れ子の型も含む) */
function rawBindingTypes() {
  const names = new Set();
  // 行頭に [StructLayout(...)] のような属性が付いた宣言も拾う
  const typeDecl =
    /^\s*(?:\[[^\]]*\]\s*)*public\s+(?:(?:static|sealed|abstract|readonly|unsafe|partial)\s+)*(?:class|struct|interface|enum)\s+(\w+)/;
  const delegateDecl = /^\s*(?:\[[^\]]*\]\s*)*public\s+(?:unsafe\s+)?delegate\s+.+?\s+(\w+)\s*\(/;
  for (const line of fs.readFileSync(RAW_BINDINGS, "utf8").split(/\r?\n/)) {
    const match = line.match(typeDecl) ?? line.match(delegateDecl);
    if (match) names.add(match[1]);
  }
  if (names.size === 0) throw new Error(`${RAW_BINDINGS} から型の宣言が見つかりません(除外のフィルターを作れません)`);
  return [...names].sort();
}

/** docfx の設定とフィルターを書き、metadata を実行して rawDir に Markdown を出す */
function runDocfx() {
  fs.rmSync(workDir, { recursive: true, force: true });
  fs.mkdirSync(workDir, { recursive: true });

  const filter = [
    "apiRules:",
    "- exclude:",
    `    uidRegex: ^FloatSoda\\.OVR\\.(?:${rawBindingTypes().join("|")})$`,
    "    type: Type",
    "",
  ].join("\n");
  fs.writeFileSync(path.join(workDir, "filterConfig.yml"), filter);

  const srcDir = path.relative(workDir, path.join(repoRoot, "src")).replaceAll("\\", "/");
  const config = {
    metadata: [
      {
        src: [{ src: srcDir, files: PROJECTS }],
        dest: "raw",
        outputFormat: "markdown",
        filter: "filterConfig.yml",
      },
    ],
  };
  const configFile = path.join(workDir, "docfx.json");
  fs.writeFileSync(configFile, `${JSON.stringify(config, null, 2)}\n`);

  execFileSync("dotnet", ["tool", "run", "docfx", "metadata", configFile], { cwd: repoRoot, stdio: "inherit" });
}

const slug = (text) => text.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");
const unescapeMd = (text) => text.replace(/\\([<>[\]_*`\\()#-])/g, "$1");

/**
 * docfx の出力 1 ファイルを読み、ページの情報を返す。
 * uid はファイル名(ジェネリックの `1 は -1 になる)、kind は Namespace / Class など
 */
function readRawPage(file) {
  const uid = path.basename(file, ".md");
  const text = fs.readFileSync(file, "utf8").replace(/\r\n/g, "\n");
  const h1 = text.match(/^# (?:<a id="[^"]*"><\/a> ?)?(\w+) (.+)$/m);
  if (!h1) throw new Error(`${file}: 先頭の H1 が docfx の形(# <種類> <名前>)ではありません`);
  const kind = h1[1];
  const name = unescapeMd(h1[2].trim());
  const ns = kind === "Namespace" ? name : text.match(/^Namespace: \[([^\]]+)\]/m)?.[1] ?? null;
  if (!ns) throw new Error(`${file}: 名前空間の行が見つかりません`);
  const anchors = new Set([...text.matchAll(/<a id="([^"]+)"><\/a>/g)].map((m) => m[1]));
  const rel =
    kind === "Namespace"
      ? `api/${slug(ns)}/${HOME}`
      : `api/${slug(ns)}/${slug(uid.slice(ns.length + 1))}`;
  return { uid, kind, name, ns, text, anchors, rel };
}

/** docfx のアンカー ID の付け方(英数字以外を _ にする)で、uid からアンカーを作る */
const anchorOf = (uid) => uid.replace(/[^A-Za-z0-9]/g, "_");

/** 型と名前空間のページ(uid → page)と、アンカー(id → page)の索引を作る */
function indexPages(pages) {
  const byUid = new Map();
  const byAnchor = new Map();
  for (const page of pages) {
    byUid.set(page.uid, page);
    for (const id of page.anchors) byAnchor.set(id, page);
  }
  const rels = new Map();
  for (const page of pages) {
    const other = rels.get(page.rel);
    if (other) throw new Error(`API のページの出力先が衝突します: ${page.uid} と ${other.uid}(${page.rel})`);
    rels.set(page.rel, page);
  }
  return { byUid, byAnchor };
}

/** from のページから to のページ(と、あればアンカー)への相対リンク */
function linkFrom(from, to, anchor = "") {
  const dir = path.posix.dirname(from.rel);
  return `${path.posix.relative(dir, to.rel)}.md${anchor}`;
}

/** <xref href="uid"> の参照先を、リンクか `名前` の文字にする */
function resolveXref(from, rawUid, index) {
  const uid = rawUid.replace(/^[A-Z!]:/, "").replace(/&lt;/g, "<").replace(/&gt;/g, ">");
  const short = uid.replace(/\(.*$/, "").split(".").pop().replace(/`\d+$/, "").replace(/^#ctor$/, "コンストラクター");
  const pageUid = uid.replace(/`/g, "-");
  const page = index.byUid.get(pageUid);
  if (page) return `[${short}](${linkFrom(from, page)})`;
  const id = anchorOf(uid);
  const owner = index.byAnchor.get(id);
  if (owner) return `[${short}](${owner === from ? `#${id}` : linkFrom(from, owner, `#${id}`)})`;
  return `\`${short}\``;
}

/** 本文の 1 行(コードフェンスの外)を直す */
function rewriteLine(line, page, index) {
  let out = line;
  out = out.replace(/<xref href="([^"]+)"[^>]*><\/xref>/g, (_, uid) => resolveXref(page, uid, index));
  out = out.replace(/<code class="(?:paramref|typeparamref)">([^<]*)<\/code>/g, "`$1`");
  out = out.replace(/<a href="([^"]+)">([^<]*)<\/a>/g, "[$2]($1)");
  out = out.replace(/<b>([^<]*)<\/b>/g, "**$1**");
  out = out.replace(/^\s*<\/?p>\s*$/, "");
  // 型どうしのリンク(Foo.Bar.md、Foo.Bar.md#anchor)を生成先の相対パスへ。載せない型は文字だけにする。
  // docfx はリンク先の - と # と _ を \ でエスケープする(Foo.Bar\-1.md\#Foo\_Bar)ので、外してから引く
  const typeLink = /\[((?:[^\]\\]|\\.)*)\]\(([A-Za-z_](?:[\w.]|\\[-_])*)\.md(\\?#(?:[\w-]|\\[-_])*)?\)/g;
  out = out.replace(typeLink, (match, text, rawUid, rawAnchor = "") => {
    const uid = rawUid.replace(/\\/g, "");
    const anchor = rawAnchor.replace(/\\/g, "");
    const target = index.byUid.get(uid);
    if (!target) return text;
    const keep = anchor && target.anchors.has(anchor.slice(1)) ? anchor : "";
    return `[${text}](${linkFrom(page, target, keep)})`;
  });
  const heading = out.match(/^(#{2,4}) (?:(<a id="[^"]*"><\/a>) ?)?(.+)$/);
  if (heading) {
    const [, hashes, anchor, label] = heading;
    const labels = page.kind === "Namespace" && hashes === "###" ? NAMESPACE_SECTION_LABELS : hashes === "###" ? {} : SECTION_LABELS;
    // アンカーと見出しの文字の間の空白は詰める。空白があると、Starlight が付ける見出しの ID が "-spacing" のように - で始まる
    out = `${hashes} ${anchor ?? ""}${labels[label] ?? label}`;
  }
  return out;
}

/** docfx の 1 ページを、generated/api/ に書く Markdown にする */
function transformPage(page, index) {
  const lines = page.text.split("\n");
  const body = [];
  const header = [];
  let inFence = false;
  let headerPlaced = page.kind === "Namespace";

  for (const line of lines) {
    if (FENCE.test(line)) {
      // 名前空間とアセンブリの行は、型の要約(最初の段落)のあと、宣言のコードの前に置く。
      // ページの description(sync-docs.mjs の describe)が要約になるようにするため
      if (!inFence && !headerPlaced) {
        body.push(...header, "");
        headerPlaced = true;
      }
      inFence = !inFence;
      body.push(line);
      continue;
    }
    if (inFence) {
      body.push(line);
      continue;
    }
    if (/^# /.test(line)) continue;
    const nsLine = line.match(/^Namespace: (.+?)\s*$/);
    if (nsLine) {
      header.push(`名前空間: ${rewriteLine(nsLine[1], page, index)}  `);
      continue;
    }
    const asmLine = line.match(/^Assembly: (.+?)\s*$/);
    if (asmLine) {
      header.push(`アセンブリ: ${asmLine[1]}`);
      continue;
    }
    body.push(rewriteLine(line, page, index));
  }
  if (!headerPlaced) body.unshift(...header, "");

  const title = page.kind === "Namespace" ? page.name : `${page.name} ${KIND_LABELS[page.kind] ?? page.kind}`;
  const intro = page.kind === "Namespace" ? [`\`${page.ns}\` 名前空間の公開型です。`, ""] : [];
  const text = [`# ${title}`, "", ...intro, ...body].join("\n").replace(/\n{3,}/g, "\n\n").trimEnd();
  return `${text}\n`;
}

/** generated/api/Home.md: API リファレンスの入口(名前空間の一覧) */
function homePage(namespaces) {
  const assemblies = PROJECTS.map((p) => `\`${path.posix.basename(p, ".csproj")}\``).join("、");
  return [
    "# API リファレンス",
    "",
    "FloatSoda の公開 API の一覧です。ソースコードの XML ドキュメントコメントから生成しています。型やメンバーの名前と、引数・戻り値・例外の意味を引くときに使います。",
    "",
    `対象のパッケージは ${assemblies} です。OpenVR の API をそのまま写した生バインディング(\`src/FloatSoda.OVR/openvr_api.cs\`)は載せていません。`,
    "",
    "## 名前空間",
    "",
    ...namespaces.map((page) => `- [${page.ns}](${path.posix.relative("api", page.rel)}.md)`),
    "",
  ].join("\n");
}

function main() {
  runDocfx();

  const pages = fs
    .readdirSync(rawDir)
    .filter((name) => name.endsWith(".md"))
    .map((name) => readRawPage(path.join(rawDir, name)));
  if (pages.length === 0) throw new Error("docfx が API のページを出力しませんでした");
  const index = indexPages(pages);

  fs.rmSync(apiDir, { recursive: true, force: true });
  for (const page of pages) {
    const file = path.join(apiDir, `${page.rel.slice("api/".length)}.md`);
    fs.mkdirSync(path.dirname(file), { recursive: true });
    fs.writeFileSync(file, transformPage(page, index));
  }
  const namespaces = pages.filter((page) => page.kind === "Namespace").sort((a, b) => a.ns.localeCompare(b.ns));
  fs.writeFileSync(path.join(apiDir, `${HOME}.md`), homePage(namespaces));
  console.log(`api-reference: ${pages.length - namespaces.length} types in ${namespaces.length} namespaces generated`);
}

main();
