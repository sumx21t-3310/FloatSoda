@AGENTS.md

<!--
このファイルは意図的にスタブです。プロジェクト指示の正典は AGENTS.md 一本に統一しています。

- Codex など AGENTS.md 対応ツールは AGENTS.md を直接読みます
- Claude Code は AGENTS.md を読まないため、上記 `@AGENTS.md` インポートで取り込みます
  (https://code.claude.com/docs/en/memory の「AGENTS.md」節に記載の推奨パターン)
- symlink でも同じことができますが、Windows では管理者権限か開発者モードが必要なため
  インポート方式を採用しています

プロジェクト指示を追記するときは AGENTS.md を編集してください。ここに書いた内容は
Claude Code 以外のエージェントからは見えません。Claude Code 固有の指示が必要になった
場合のみ、上記インポート行の下に見出しを立てて書き足します。

この HTML コメントは Claude Code がコンテキストへ注入する前に除去されるため、
トークンを消費しません。
-->
