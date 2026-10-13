# 全体: 守ること

層をまたいで、実装を変えても維持する条件の一覧です。変更が何を壊しうるかを、変更の前とレビューのときに確かめるために使います。条件の一次情報が別の場所にあるものは、ここに写さずにリンクします。

## 適用範囲

このページの条件は、次の範囲に効きます。

| 条件 | 効く範囲 | 一次情報 |
|---|---|---|
| [ツリーの不変条件](#ツリーの不変条件) | RenderObject、Element、Widget、Layer に触れる変更 | [REVIEW.md の「4. FloatSoda 固有の不変条件」](../../../REVIEW.md#4-floatsoda-固有の不変条件) |
| [コンポーネント API の必須条件](#コンポーネント-api-の必須条件) | Widget と、その `*Style` を足す・変える変更 | [APIDesign の「2. コンポーネントAPI設計ガイドライン」](../APIDesign.md#2-コンポーネントapi設計ガイドライン) |
| [namespace とディレクトリの一致](#namespace-とディレクトリの一致) | `src/`、`tests/`、`samples/` のすべての C# ファイル | [CONTRIBUTING.md の「namespace とディレクトリ」](../../../CONTRIBUTING.md#namespace-とディレクトリ) |
| [Flutter との関係](#flutter-との関係) | Flutter 由来の Widget と RenderObject | [APIDesign の「判断原則: Flutter 由来の observable behavior に差異を作らない」](../APIDesign.md#判断原則-flutter-由来の-observable-behavior-に差異を作らない) |

## ツリーの不変条件

ツリーの所有権とライフサイクル、差分更新、Layer の条件は、[REVIEW.md の「4. FloatSoda 固有の不変条件」](../../../REVIEW.md#4-floatsoda-固有の不変条件)が一次情報です。置き場所は [AGENTS.md](../../../AGENTS.md) の規約表で決まっているため、ここには写しません。4 章は次の3つに分かれています。

- ツリーの所有権とライフサイクル — 親子の adopt と drop、attach と detach、mount から dispose まで、keyed update
- 差分更新 — dirty な Element の再ビルド順、`MarkNeedsLayout` / `MarkNeedsPaint` の伝播と登録、relayout / repaint boundary、不要な再ビルドと再描画
- Layer — clone したあとに可変オブジェクトを共有しない

それぞれの層での詳しい条件は、その層の「守ること」のページにあります。

## コンポーネント API の必須条件

Widget の公開 API の形は、次の条件を満たします。どの条件も、Flutter を知る利用者とコード生成 AI が自然に書く形を、そのまま正しいコードにするためのものです。理由と例は APIDesign の各節にあります。

| 条件 | 例外 | 一次情報 |
|---|---|---|
| コンストラクタ引数を使わず、オブジェクト初期化子だけで構成できる | 子を取らず、データそのものを表示する末端ウィジェットは、主たる値1つを取るコンストラクタを持てる(例: `Text(string)`)。そのウィジェットは、ほかに `required` メンバーを持たない | [2.1](../APIDesign.md#21-オブジェクト初期化子ファーストの原則) |
| 子が1つなら `Child`、複数なら `Children` で受ける | なし | [2.2](../APIDesign.md#22-子要素の表現) |
| 視覚的な属性は Widget 本体に持たせず、`*Style` に分ける。`*Style` とスタイル付きのコンポーネントはデザインシステム層に置く | なし | [2.4](../APIDesign.md#24-スタイルの分離) |
| 兄弟ウィジェット(共通基底の薄い特殊化、対で使うもの、実装詳細を共有するもの)は同じファイルに置く | 同じカテゴリに属するだけのウィジェットは別ファイルにする | [2.5](../APIDesign.md#25-ファイル構成--兄弟ウィジェットは同じファイルに置く) |

`Children` の型は、APIDesign の 2.2 では `IList<Widget>` ですが、実コード(`src/FloatSoda/Widgets/MultiChildRenderObjectWidget.cs`、`src/FloatSoda/Widgets/Layout/Flex.cs`)では `List<Widget>` です。どちらに揃えるかは ※要確認 です。

破ると、利用者と LLM が Flutter の形で書いたコードがそのままでは通らなくなります。たとえばコンストラクタ引数を足すと、利用者は引数の順序を覚える必要があり、後からプロパティを足すときにも引数の並びを変えることになります。

これらの条件を固定する自動の検査とテストは未設定です。レビューで、[REVIEW.md の「1. 重要度の順序」](../../../REVIEW.md#1-重要度の順序)の public API consistency として確かめます。新しい public API は、ジュニアコーダーテストで docs だけから使えるかも確かめます(手順は [全体: 作業の手順](HowTo.md#public-api-を-docs-だけで使えるか確かめる))。

## namespace とディレクトリの一致

C# の namespace と、プロジェクトのルートから下の物理ディレクトリは、常に一致します。namespace を移すときは、同じ変更でファイルも移します。`samples/` では、さらにディレクトリ名・プロジェクト名・ルート名前空間を一致させます。

破ると、namespace から探したファイルが別のディレクトリにあり、型の置き場所を名前から辿れなくなります。

ファイルスコープ namespace(`namespace X;`)の形は、`.editorconfig` の `IDE0161` がビルド時に検出します。namespace とディレクトリの一致そのものを検出するテストは未設定で、レビュー([REVIEW.md の「7. namespace / ディレクトリ」](../../../REVIEW.md#7-namespace--ディレクトリ))で確かめます。

## Flutter との関係

Flutter 由来の Widget と RenderObject は、FloatSoda 固有の理由が明示されていない限り、Flutter と同じ observable behavior を保ちます。対象は、プロパティの意味、既定値、レイアウト、描画とクリップ、ヒットテスト、子の扱い、Widget の更新、不正な入力と退化した入力の扱い、レイアウトと描画が dirty になる条件、Element と State のライフサイクルです。

差異が必要なときは、[`known-divergences.md`](../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) にエントリを足し、次の5点を記録します。

1. Flutter の挙動
2. FloatSoda の挙動
3. 差異が必要な理由
4. 差異を固定するテスト(ファイルとテストメソッド名)
5. 利用者に影響する場合のドキュメント(該当する `docs/` のページと、サンプルの `## Flutterとの違い` 節)

テストのない差異は、次の移植で気づかれずに Flutter の挙動へ戻されます。台帳の `Test` 欄が `— (not set)` のエントリは、完了した記録ではなく、テストを書く作業が残っているものとして扱います。確認済みの差異の一覧は、ここに写さずに台帳で確かめます。

`event`、`init` / `required`、`record` / `record struct`、.NET 標準の機構への置き換えは、利用者から見た挙動を変えないため、差異として扱いません。Semantics のように、移植しないと決めた機能は、parity を理由に持ち込みません([APIDesign の「判断原則: 対象ユーザー・ランタイムに不要な Flutter API は移植しない」](../APIDesign.md#判断原則-対象ユーザーランタイムに不要な-flutter-api-は移植しない))。

## 関連

- [全体: 仕組みと理由](Explanation.md) — これらの条件の背景にある設計の判断原則
- [全体: 作業の手順](HowTo.md) — テストを足す手順と、規約が守られる場所
- [REVIEW.md](../../../REVIEW.md) — レビューの重要度と、ツリーの不変条件
- [APIDesign](../APIDesign.md) — API 設計の規約と Flutter parity の判断原則
