← [Home](Home.md)

# ドキュメント執筆ガイド

FloatSoda のドキュメントは、読者ごとに **User docs / Contributor docs / API Reference** の3系統に分かれています(情報設計の正典は [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。本ページは、新しい情報を書くときに「どの系統のどのページに書くか」「何が正典か」「変更時に何を更新するか」を決めるための入口です。書き方の規約はそれぞれの正典に置いているため、ここには再掲しません。

| 系統 | 読者 | 答える問い |
|---|---|---|
| User docs | FloatSoda でアプリを作る人 | **What / How** — 何であり、どう使うか |
| Contributor docs | FloatSoda 本体を変える・レビューする人 | **How it works / Why / Must** — どう動き、なぜそう設計され、何を守るか |
| API Reference | 利用者・コントリビュータ・Coding Agent の全員 | **Exactly what exists** — 公開 API の正確な契約 |

> **実装状況** — 系統ごとのディレクトリ(`docs/user/`、`docs/contributor/`、`docs/api/`)への再編は、#188 の手順4で進めます。現行の `docs/` は Contributor 向けの詳細なリファレンスが中心です。そのため、再編では **現行ページを `contributor/` へ移し、User docs は新しく書きます**。それまでは [再編の地図](#再編の地図) で該当ページを選んでください。このガイド自体も、再編後は `docs/contributor/Documentation.md` へ移ります。Contributor 側の「書くもの / 書かないもの」とテンプレートは、移管した実物のページから逆算し、[Issue #245](https://github.com/sumx21t-3310/FloatSoda/issues/245) で追加します。

## 1. どこに書くか

書こうとしている情報について、上から順に問いに答えます。

```text
利用者が正しく使うために必要な情報?
 ├─ はい
 │   ├─ 概念を理解するための説明          → User / Concept
 │   ├─ 特定の目的を達成する手順          → User / Guide
 │   └─ 完成物を作りながら学ぶ道筋        → User / Tutorial
 └─ いいえ
     └─ FloatSoda を変更する人に必要な情報?
         ├─ いまどう動いているか            → Contributor / Architecture
         ├─ なぜそう設計したか              → Contributor / Design
         ├─ 変更後も何を維持するか          → Contributor / Requirements
         └─ どう変更・レビュー・公開するか  → Contributor / Development

型・メンバー・引数・戻り値・例外・nullability の正確な公開契約?
 → XML ドキュメントコメント(API Reference の原稿)
```

境界で迷いやすい情報は、次のように分類します。

- **利用者から観測できる制約**は User docs に書き、**その制約を成立させる内部要件**は Contributor docs に書きます。例: 「`WorldSpaceWindow` では現在ポインタ入力を使えない」は User docs、「`WorldSpaceWindow` の入力経路を Controller Ray → HitTest として接続する」は Contributor docs、`WorldSpaceWindow` のプロパティ・型・例外契約は API Reference
- **Flutter との差異**は、判断原則を [APIDesign](APIDesign.md) に、確認済み差異の台帳を [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) に置きます。利用者から見える差異は、該当する User docs のページと、対応するサンプルの「Flutterとの違い」節にも記載します(記録ルールは [APIDesign](APIDesign.md))
- **同じテーマは1つの系統で詳述**し、ほかの系統からはリンクします。概念を Guide で説明し直したり、シグネチャを Concept に並べたりしません

### User docs の範囲

User docs は、公開 API および API Reference と合わせて、**#188 の層3(統合ミニアプリ型)のサンプルをジュニアコーダーテストで書ける範囲**を扱います。これが「どこまで書くか」の合格ラインです。テストで不足が判明した概念や制約は、User docs または API Reference の不足として回収します(手順は #188)。内部実装を知らないとコードを書けない箇所が見つかった場合、それは User docs の不足を意味します。

### 再編の地図

再編(#188 手順4)の出発点です。現行ページは内部の仕組みの説明が中心です。そのため、User 向けの部分だけを抜き出して新しいページに書き、本体は `contributor/` へ移します。種別は再編時に実物で確定します。

**現行ページの行き先**

| 現行ページ | 行き先 | 備考 |
|---|---|---|
| [Architecture](Architecture.md) | Contributor / Architecture | 利用者向けの全体像は User の「Widget」へ書き下ろす |
| [BuildPipeline](BuildPipeline.md) | Contributor / Architecture | |
| [RenderObjects](RenderObjects.md) | Contributor / Architecture | 制約モデルの利用者向け説明は User の「Layout」へ |
| [WidgetSystem](WidgetSystem.md) | Contributor / Architecture | 組み込みウィジェット一覧と「押せるボタンを作る」は User 側へ |
| [Animation](Animation.md) | Contributor / Architecture | 使い方は User の「Animation」へ |
| [OVRIntegration](OVRIntegration.md) | Contributor / Architecture | オーバーレイ種別と制約は User の「Window / Overlay」へ |
| [UILayering](UILayering.md) | Contributor / Design | 設計方針(未提供) |
| [APIDesign](APIDesign.md) | Contributor / Design + Requirements | |
| [DocumentationComments](DocumentationComments.md) | Contributor / Development | |
| [Localization](Localization.md) | Contributor / Design + Development | |
| このページ | Contributor / Development | `contributor/Documentation.md` |
| [TargetUsers](TargetUsers.md) | User(入口) | 利用者向けに書かれているので、ほぼそのまま `user/` へ |
| [GettingStarted](GettingStarted.md) | User / Tutorial | 利用者向けに書き直す |
| [Input](Input.md) | User / Concept + Guide | 利用者向けに書き直す。実装の説明は Contributor 側へ |

**新しい User ページと元ネタ**(#188 の Concept の列挙に沿う)

| User ページ | 種別 | 元ネタ(利用者が観測できる制約と使い方を抜き出す) |
|---|---|---|
| Widget(宣言的 UI と Widget ツリー) | Concept | WidgetSystem の組み込み一覧と「押せるボタンを作る」、Architecture の全体像、層1サンプル |
| State(`SetState`、`InheritedWidget`) | Concept | WidgetSystem、BuildPipeline のうち利用者から観測できる再ビルドの挙動 |
| Layout(制約・サイズ・配置) | Concept | RenderObjects の制約モデル、層1サンプル(レイアウト基本・制約変換系) |
| Input(ポインタ・ジェスチャ・アクション入力) | Concept + Guide | Input、WidgetSystem の「ジェスチャとヒットテスト」、層1サンプル(入力系)。ダッシュボード限定の制約を明記する |
| Animation | Concept | Animation |
| Window / Overlay(ダッシュボード・ワールド座標・デバイス追従) | Concept + Guide | OVRIntegration、Home の実装状況(表示専用の制約)、[Issue #151](https://github.com/sumx21t-3310/FloatSoda/issues/151)(物理サイズの発見性) |
| 〜するには(各種) | Guide | 層2(Cookbook 型)サンプル |
| GettingStarted | Tutorial | 現行 GettingStarted、`samples/FloatSoda.Samples.GettingStarted` |
| ミニアプリを作る | Tutorial | 層3(統合ミニアプリ型)サンプル(Phase 2 完了後) |

## 2. 正典の対応表

同じ情報は1箇所のみに書き、ほかの場所からはリンクします。

| 情報 | 正典 |
|---|---|
| API のシグネチャ・引数・戻り値・例外・nullability・副作用・スレッド制約 | XML ドキュメントコメント(規約は [DocumentationComments](DocumentationComments.md)) |
| API の具体的な使い方 | User Guide |
| 概念モデル(Widget、State、Layout、Input、Animation、Window / Overlay) | User Concept |
| 完成物を作る手順 | User Tutorial とサンプル(`samples/`) |
| ウィジェットの最小の使用例 | サンプルの README(構成は [CONTRIBUTING.md](../CONTRIBUTING.md) の「サンプルを追加する場合の規約」) |
| 内部の動作(ツリー、Build / Layout / Paint、入力、スレッド) | Contributor Architecture |
| 設計理由と採用しなかった選択肢 | Contributor Design |
| invariant、observable behavior、ライフサイクル・所有権・スレッドの必須条件 | Contributor Requirements |
| API 設計の原則と Flutter parity / divergence の判断基準 | [APIDesign](APIDesign.md) |
| 確認済みの Flutter との差異 | [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) |
| 開発・レビュー・リリースの手順 | [CONTRIBUTING.md](../CONTRIBUTING.md) / [REVIEW.md](../REVIEW.md) / [RELEASING.md](../RELEASING.md) |
| ドキュメントの分類とサンプルの3層構成 | [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) |

XML ドキュメントコメントは `internal` や `private` にも書きます([DocumentationComments](DocumentationComments.md) の適用範囲)。公開サイトの API Reference に載せるのは `public` API のみであり、`internal` 側のコメントはコントリビュータ向けの内部資料として扱います。

## 3. Documentation Impact Matrix

変更の種別ごとに、更新を検討すべき文書の一覧です。**変更によって正典の説明が変わる場合のみ更新します。** 全項目を機械的に更新する必要はありません。

| 変更 | 更新対象 |
|---|---|
| public API を追加・変更する | XML ドキュメントコメント。利用者の使い方が増えるなら User Guide と Catalog 型サンプル |
| observable behavior を変更する | User docs とテスト。API 契約に影響するなら XML ドキュメントコメント |
| 公開制約・既知の制限を変更する | User docs。必要なら XML ドキュメントコメント |
| 内部アルゴリズムだけを変更する | Contributor Architecture の説明が変わる場合のみ |
| アーキテクチャの境界を変更する | Contributor Architecture と Requirements |
| 設計判断を変更する | Contributor Design。必要なら Requirements と [APIDesign](APIDesign.md) |
| invariant を追加・変更する | Contributor Requirements とテスト |
| Flutter との差異を導入・変更する | [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) と divergence テスト。利用者から観測できるなら User docs とサンプルの「Flutterとの違い」節 |
| 新しい代表ユースケースを追加する | User Guide / Tutorial と、#188 の層に沿ったサンプル |
| サンプルを追加・変更する | サンプルの README と `checklist.md`([CONTRIBUTING.md](../CONTRIBUTING.md) の規約) |
| ドキュメントのページを移動・改名する | リンク元のページと [Home](Home.md) の「ページ一覧」表(公開サイトの検査がリンク切れを検出します) |

PR では、テンプレートの `## Documentation` 節に、変更種別と更新した正典(更新が不要と判断した場合はその理由)を記載します。レビューでは、その判断がこの表と一致しているかを確認します。

## 4. User docs のテンプレート

User docs は新しく書くページが多いため、先に骨格を定めます。ページの目的に不要な節は省きます。Contributor 側のテンプレートは、再編で移管した実物のページから逆算し、#245 で追加します。

読者は [TargetUsers](TargetUsers.md) で定義する3タイプです。**コードを書くのも読むのも LLM である**という前提を忘れず、「LLM がこの API を誤用しない」書き方を優先します。Unity の知識のみを持つ読者(Booth クリエイター)には、Unity や uGUI の語彙による読み替えを添えます。

### Concept

```text
# 名前
## 概要                        — 何であり、何を解決するか
## いつ使うか
## 基本モデル                  — 利用者から見た仕組み。内部実装は書かない
## 最小例                      — 対応する層1(カタログ型)サンプルへのリンクと、必要なら最小の断片
## Unity / uGUI からの読み替え — 対応する概念と、違う点
## 制約と未実装                — 利用者から観測できる制限。使えるようになる Phase を添える
## 関連ページ
```

### Guide

```text
# 〜する
## 前提
## 手順
## 完成形          — 対応する層2(Cookbook 型)サンプルへのリンク
## 注意点
## 関連 API / サンプル
```

### Tutorial

```text
# 〜を作る
## 作るもの
## 前提
## Step 1 …
## Step 2 …
## 完成            — 対応する層3(統合ミニアプリ型)サンプルへのリンク
## 次に読むもの
```

サンプルコードは `samples/` を正典とし、本文には必要最小限の断片のみを載せます。ドキュメント用に完全な実装を別途記述することはしません。

## 関連

- [DocumentationComments](DocumentationComments.md) — XML ドキュメントコメントの規約
- [APIDesign](APIDesign.md) — API 設計の原則と Flutter parity
- [TargetUsers](TargetUsers.md) — 想定する3タイプの作り手
- [CONTRIBUTING.md](../CONTRIBUTING.md) — 開発・PR・テスト・サンプルの規約
- [REVIEW.md](../REVIEW.md) — レビューの判断基準
- [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) — ドキュメント・サンプルの情報設計
- [Issue #245](https://github.com/sumx21t-3310/FloatSoda/issues/245) — このガイドの残りの節(Contributor 側の「書くもの / 書かないもの」とテンプレート)
- [Issue #219](https://github.com/sumx21t-3310/FloatSoda/issues/219) — 公開サイト(`docs/` の構造がそのままサイトの構造になります)
