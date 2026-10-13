# 全体: 作業の手順

層をまたいで繰り返す作業を、規約どおりに終えるための手順です。各節は「いつその作業をするか」と「何をどの順で確かめるか」だけを書き、規約の本文は一次情報のページへリンクします。規約を二重に書くと、片方だけが更新されて食い違うためです。

| 作業 | このページの節 | 規約の一次情報 |
|---|---|---|
| Issue を書き、ブランチを切る | [Issue を書いてブランチを切る](#issue-を書いてブランチを切る) | [CONTRIBUTING.md](../../../CONTRIBUTING.md) |
| XML ドキュメントコメントを書く | [ドキュメントコメントを書く](#ドキュメントコメントを書く) | [DocumentationComments](../DocumentationComments.md) |
| `docs/` のページを書く | [docs を書く](#docs-を書く) | [WritingDocumentation](../WritingDocumentation.md) |
| テストを足す | [テストを足す](#テストを足す) | [CONTRIBUTING.md](../../../CONTRIBUTING.md)、[TestStrategy](../TestStrategy.md) |
| public API を足す | [public API を docs だけで使えるか確かめる](#public-api-を-docs-だけで使えるか確かめる) | [CONTRIBUTING.md](../../../CONTRIBUTING.md) |
| 設計の判断を記録する | [ADR を書く](#adr-を書く) | 未定 ※要確認 |
| 実機で確かめる | [実機で確かめる](#実機で確かめる) | [CONTRIBUTING.md](../../../CONTRIBUTING.md)、[`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) |
| リリースする | [リリースする](#リリースする) | [RELEASING.md](../../../RELEASING.md) |

規約の強さは、一次情報のページの書き方に従います。CONTRIBUTING.md と REVIEW.md では、**必須**が MUST、**推奨**が SHOULD です。

## Issue を書いてブランチを切る

コードや docs に手を入れる前に、Issue が要るかを決め、要るなら Issue を立ててからブランチを切ります。

1. [CONTRIBUTING.md の「Issue が必須な変更」](../../../CONTRIBUTING.md#issue-が必須な変更)で、Issue が要るかを判断する
2. Issue が要るなら、`.github/ISSUE_TEMPLATE/` のテンプレートで立てる。タイトルに `feat(Widget):` のような接頭辞を付けず、分類はラベルで表す([「タイトルとラベル」](../../../CONTRIBUTING.md#タイトルとラベル))
3. 種別ラベルを1つ、領域ラベル(`area:*`)を該当するだけ付ける。ラベルの一覧は [Labels ページ](https://github.com/sumx21t-3310/FloatSoda/labels) で確かめる
4. [「ブランチ名」](../../../CONTRIBUTING.md#ブランチ名)の形でブランチを切る。エージェントやツールの名前は入れない

## ドキュメントコメントを書く

型やメンバーを足したり、契約を変えたりしたときに、XML ドキュメントコメントを書きます。`public` だけでなく `internal` と `private` も対象です。`internal` 側のコメントは、コントリビュータ向けの内部資料として読まれます。

1. [DocumentationComments の「1. 適用範囲」](../DocumentationComments.md#1-適用範囲)で、省略できる要素かを確かめる
2. [「2. 基本方針」](../DocumentationComments.md#2-基本方針)に沿って、`<summary>`、`<param>`、`<returns>`、`<exception>` と、単位・範囲・`null` の可否・既定値・副作用を書く
3. Dirty フラグを変えるメンバーは、[「5. Dirty フラグの変更」](../DocumentationComments.md#5-dirty-フラグの変更)の書き方に合わせる
4. 日本語だけで書く。理由は [Localization](../Localization.md) にある

長い使用例、設計の理由、内部構造の解説は、コメントではなく `docs/` に書きます。

## docs を書く

新しい情報を `docs/` に書くときは、置き場所を決めてから、種別のテンプレートで書きます。

1. [WritingDocumentation の「1. どこに書くか」](../WritingDocumentation.md#1-どこに書くか)の問いに順に答え、区分とページを決める
2. 変更の種別から更新する文書を [「3. Documentation Impact Matrix」](../WritingDocumentation.md#3-documentation-impact-matrix)で確かめる
3. 種別のテンプレートで書く。コンセプトとユーザー向けは [4 章](../WritingDocumentation.md#4-コンセプトとユーザー向けの書くもの--書かないものとテンプレート)、コントリビューター向けは [5 章](../WritingDocumentation.md#5-コントリビューター向けの書くもの--書かないものとテンプレート)
4. [「6. 文体」](../WritingDocumentation.md#6-文体)の規約で見直す
5. `website/` で `npm run build` と `npm run verify` を実行し、`verify-dist: OK` を確かめる

件数や一覧のように、コード・`samples/`・CI から導けるものは本文に書かず、導出元へリンクします。書き写すと、導出元が変わったときに本文だけが古くなります。

## テストを足す

実装を変えたら、変更に当たる観点のテストを、観測できる範囲のいちばん小さいテストプロジェクトに足します。

1. [CONTRIBUTING.md の「テスト観点」](../../../CONTRIBUTING.md#テスト観点)から、変更に当たる観点をすべて選ぶ。バグ修正では、修正前に落ちて修正後に通る regression test を書く(必須)
2. Flutter 由来の Widget や RenderObject なら、Flutter 本体の分岐と公式テストを確かめ、対応する重要なケースを足す(必須)
3. [TestStrategy の「1. 3つの軸」](../TestStrategy.md#1-3つの軸)で、範囲(単体・結合・描画・実機)とオラクルを決める。同じ不具合を小さい範囲で観測できるなら、大きい範囲は使わない
4. テスト名を [「テストの命名」](../../../CONTRIBUTING.md#テストの命名)の `対象メンバー名_条件_期待結果` の形にする

テストプロジェクトごとの対象は、[TestStrategy の「3. 現在の配置」](../TestStrategy.md#3-現在の配置2026-09-09-時点)にあります。

## public API を docs だけで使えるか確かめる

新しい public API(ウィジェット、オーバーレイ種別、`*Style`、使い方が変わるプロパティ)を足す PR は、マージの前にジュニアコーダーテストを通します。xunit が守るのは内部の動作で、このテストが守るのは「docs だけを読んだ LLM がその API を正しく使えるか」です。

1. PR のブランチで、その API を説明する `docs/` のページを先に更新する
2. スキル [`floatsoda-junior-coder-test`](../../../.agents/skills/floatsoda-junior-coder-test/SKILL.md) で、その API を使わないと解けないお題を、docs だけを渡したモデルに解かせる
3. 結果の扱いは [CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」](../../../CONTRIBUTING.md#新しい-public-api-を追加する場合の受け入れ条件)に従う

## ADR を書く

設計の判断を、採らなかった選択肢と一緒に残す記録(ADR)の手順です。ADR の置き場所と書式は、まだ決まっていません ※要確認。決まるまでは、判断と理由を Issue に残し、恒久的な理由はその層の「仕組みと理由」のページに書きます(書き方は [WritingDocumentation の「Design」](../WritingDocumentation.md#design))。

## 実機で確かめる

SteamVR が動いている状態でしか観測できない変更は、HMD で確かめます。対象は、オーバーレイの表示、コントローラーやレーザーポインターの入力、レンダースレッドの描画です。

1. 実機で確かめる必要がある Issue に `needs-device-test` ラベルを付ける
2. 確かめるサンプルかシナリオを用意する。ウィジェットを足したときは、サンプルの `checks/items.jsonl` をスキル [`floatsoda-device-test-gen`](../../../.agents/skills/floatsoda-device-test-gen/SKILL.md) で生成する(規約は [CONTRIBUTING.md の「サンプルを追加する場合の規約」](../../../CONTRIBUTING.md#サンプルを追加する場合の規約))
3. スキル [`floatsoda-device-test-run`](../../../.agents/skills/floatsoda-device-test-run/SKILL.md) で、項目ごとに見て判定し、記録する

サンプルは `--desktop` を付けるとデスクトップのウィンドウに表示できます。ただし、HMD の接続と SteamVR の起動は必要です。デスクトップで動いても、ダッシュボードのオーバーレイで動く証明にはならないため、ポインター操作を伴うサンプルはダッシュボードでも確かめます。

## リリースする

リリースは、`v*` タグの push をきっかけに `.github/workflows/release.yml` が NuGet へ公開します。タグを切る前後の作業は [RELEASING.md](../../../RELEASING.md) にあり、スキル [`floatsoda-release`](../../../.agents/skills/floatsoda-release/SKILL.md) がその手順を実行します。

タグの push と GitHub Release の作成は、リポジトリのオーナーの明示的な承認が要ります。リリースの範囲を確定してから、リリース対象のコミットでジュニアコーダーゲートを通します(合格基準は [RELEASING.md の「ジュニアコーダーゲート」](../../../RELEASING.md#ジュニアコーダーゲート))。

## 規約が守られる場所

規約のうち、機械で検出されるものと、レビューで確かめるものを分けます。

| 規約 | 検出する仕組み |
|---|---|
| ビルドとテストが通る | CI(`.github/workflows/ci.yml`)。PR ごとに Release 構成でビルドし、テストプロジェクトを実行する |
| `docs/` のリンクとアンカーが実在する | 公開サイトの検査(`.github/workflows/docs-site.yml` の `npm run verify`)。`docs/` か `website/` を変える PR で実行する |
| ファイルスコープ namespace | `.editorconfig` の `IDE0161`。ビルド時に検出する |
| PR に Flutter の参照、breaking change の判断、テスト、更新した文書が書かれている | [PR テンプレート](../../../.github/PULL_REQUEST_TEMPLATE.md)の見出しと、レビュー |
| namespace とディレクトリの一致、PR のスコープ、テスト観点 | レビュー([REVIEW.md](../../../REVIEW.md)) |
| `docs/` の文体 | レビュー。textlint による検査は未導入(追跡: [#260](https://github.com/sumx21t-3310/FloatSoda/issues/260)) |

## 関連

- [全体: docs の小さな修正で PR を出す](Tutorial.md) — 最初の PR を一本道で出す
- [全体: 守ること](Reference.md) — 変更の前後で維持する条件
- [CONTRIBUTING.md](../../../CONTRIBUTING.md) — 開発・コントリビューション規約の一次情報
- [REVIEW.md](../../../REVIEW.md) — レビューの基準
