// OpenVR 公式の openvr_api.cs に、FloatSoda 用の書き換えを当てる。
// 使い方: node patch-openvr-api.mjs <上流の openvr_api.cs> <出力先>
// 書き換えの内容と理由は、src/FloatSoda.OVR/openvr_api.cs の冒頭コメントと .editorconfig を参照。
import { readFileSync, writeFileSync } from "node:fs";

const [input, output] = process.argv.slice(2);
if (!input || !output) {
  console.error("usage: node patch-openvr-api.mjs <upstream openvr_api.cs> <output>");
  process.exit(2);
}

let text = readFileSync(input, "utf8").replace(/\r\n/g, "\n");

// 書き換えごとに、当たった回数を確かめる。上流の形が変わって当たらなくなったら、失敗させる。
function replaceOnce(pattern, replacement, name) {
  const matches = text.match(new RegExp(pattern.source, pattern.flags + "g"));
  if (!matches || matches.length !== 1) {
    console.error(`patch "${name}" matched ${matches ? matches.length : 0} times (expected 1)`);
    process.exit(1);
  }
  text = text.replace(pattern, replacement);
}

// 1. nullable の警告だけを止める(注釈は有効のまま)
replaceOnce(
  /^#if !OPENVR_XR_API\n/m,
  [
    "",
    "// FloatSoda: 再生成したときは、名前空間の書き換えと合わせてこの行も残す。",
    "// nullable の警告だけを止め、注釈は有効のままにする (PublicAPI.*.txt の ! を変えないため)。",
    "#nullable disable warnings",
    "",
    "#if !OPENVR_XR_API",
    "",
  ].join("\n"),
  "nullable",
);

// 2. 暗黙の using と、自分の名前空間への using を消す
replaceOnce(/^using System;\n/m, "", "using System");
replaceOnce(/^using Valve\.VR;\n\n/m, "", "using Valve.VR");

// 3. 名前空間を FloatSoda.OVR にする
replaceOnce(/^namespace Valve\.VR$/m, "namespace FloatSoda.OVR", "namespace");

// 4. DllImportAttribute を DllImport にする(個数は上流の版で変わるので、回数は固定しない)
const before = text;
text = text.replaceAll("[DllImportAttribute(", "[DllImport(");
if (text === before) {
  console.error('patch "DllImport" matched 0 times');
  process.exit(1);
}

writeFileSync(output, text, "utf8");
