# GettingStarted — 最初のオーバーレイを作る

> **骨組み** — 本文はまだありません。下は本文を書くための設計シートです(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。書き方は [WritingDocumentation](../contributor/WritingDocumentation.md) にあります。

## 設計シート

- 分類: 説明型 / tutorial
- 読み手: FloatSoda でオーバーレイを作る人(TargetUsers の3タイプ。コードを書くのは LLM のことが多い) / C# のプロジェクトを作れる / 最初のオーバーレイを出し、FloatSoda の書き方に慣れたい
- テーマ: 新しいプロジェクトから、状態を持つ最初のオーバーレイを表示するまで
- Why: 最初の1つが表示できないと、ほかのページを読んでも試せないため
- What: SteamVR のダッシュボードに、ボタンで数が増えるオーバーレイが出ること
- How: 一本道の手順で書き、完成形は `samples/FloatSoda.Samples.GettingStarted` へリンクする
- 要素:
  - 前提: SteamVR、.NET 10、パッケージの参照方法(今の contributor/GettingStarted)
  - プロジェクトを作り、パッケージを参照する(今の contributor/GettingStarted)
  - 最初のウィンドウを出す(今の contributor/GettingStarted)
  - Widget を置く(今の contributor/GettingStarted)
  - 状態を持たせる(新しく書く)
  - 完成形: `samples/FloatSoda.Samples.GettingStarted`
- 見出し(案):
  - 作るもの
  - 前提
  - Step 1 プロジェクトを作り、パッケージを参照する
  - Step 2 最初のウィンドウを出す
  - Step 3 Widget を置く
  - Step 4 状態を持たせる
  - 完成
  - 次に読むもの
- ※要確認:
  - 手順の参照方法(プロジェクト参照か NuGet 参照か)。2026-10 の仕分けで、骨組みの「NuGet 参照」と今の手順が食い違っていた
