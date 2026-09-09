← [Home](Home.md)

# ドキュメント執筆ガイド

FloatSoda のドキュメントは、読者ごとに **User docs / Contributor docs / API Reference** の3系統に分かれています(情報設計の正典は [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。このページは、新しい情報を書くときに「どの系統のどのページに書くか」「何が正典か」「変更したとき何を更新するか」を決めるための入口です。書き方の規約そのものは、それぞれの正典に置いてあり、ここには再掲しません。

| 系統 | 読者 | 答える問い |
|---|---|---|
| User docs | FloatSoda でアプリを作る人 | **What / How** — 何であり、どう使うか |
| Contributor docs | FloatSoda 本体を変える・レビューする人 | **How it works / Why / Must** — どう動き、なぜそう設計され、何を守るか |
| API Reference | 利用者・コントリビュータ・Coding Agent の全員 | **Exactly what exists** — 公開 API の正確な契約 |

> **実装状況** — 系統ごとのディレクトリ(`docs/user/`、`docs/contributor/`、`docs/api/`)への再編は #188 の手順4で進めます。それまでは、[現行ページとの対応](#現行ページとの対応)で該当ページを選んでください。このガイド自体も、再編後は `docs/contributor/Documentation.md` へ移ります。各ページ種別の「書くもの / 書かないもの」と推奨テンプレートは、再編で実際に分割したページから逆算して [Issue #245](https://github.com/sumx21t-3310/FloatSoda/issues/245) で追加します。

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

境界で迷いやすいものは、次のように分けます。

- **利用者から観測できる制約**は User docs に書き、**その制約を成立させている内部要件**は Contributor docs に書きます。例: 「`WorldSpaceWindow` では現在ポインタ入力を使えない」は User docs、「`WorldSpaceWindow` の入力経路を Controller Ray → HitTest として接続する」は Contributor docs、`WorldSpaceWindow` のプロパティ・型・例外契約は API Reference
- **Flutter との差異**は、判断原則を [APIDesign](APIDesign.md) に、確認済み差異の台帳を [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) に置きます。利用者から見える差異は、該当する User docs のページと、対応するサンプルの「Flutterとの違い」節にも書きます(記録ルールは [APIDesign](APIDesign.md))
- **同じテーマは1つの系統で詳述**し、ほかの系統からはリンクします。概念を Guide で説明し直したり、シグネチャを Concept に並べたりしません

### 現行ページとの対応

再編(#188 手順4)までの間、系統はこの表で読み替えます。「分割」は、利用者向けとコントリビュータ向けの内容が同居しているページで、再編時に2つに分ける候補です(出発点の案であり、再編時に確定します)。

| 現行ページ | 系統 / 種別 | 再編時の扱い |
|---|---|---|
| [TargetUsers](TargetUsers.md) | User / Concept(読者の自己判定と読み進め方) | `user/` へ |
| [GettingStarted](GettingStarted.md) | User / Tutorial | `user/` へ |
| [WidgetSystem](WidgetSystem.md) | User / Concept + Guide(組み込みウィジェット一覧、押せるボタン) | `user/` へ |
| [Animation](Animation.md) | User / Concept | 分割(内部の動作原理は `contributor/` へ) |
| [OVRIntegration](OVRIntegration.md) | User / Concept + Guide | 分割(ラッパーの内部構造は `contributor/` へ) |
| [Input](Input.md) | User / Concept + Guide | `user/` へ |
| [Architecture](Architecture.md) | Contributor / Architecture | 分割(利用者向けの全体像は `user/` の Concept へ) |
| [BuildPipeline](BuildPipeline.md) | Contributor / Architecture | `contributor/` へ |
| [RenderObjects](RenderObjects.md) | Contributor / Architecture | `contributor/` へ |
| [UILayering](UILayering.md) | Contributor / Design(設計方針、未提供) | `contributor/` へ |
| [APIDesign](APIDesign.md) | Contributor / Design + Requirements(API 規約、Flutter parity の判断) | `contributor/` へ |
| [DocumentationComments](DocumentationComments.md) | Contributor / Development(XML ドキュメントコメントの規約) | `contributor/` へ |
| [Localization](Localization.md) | Contributor / Design + Development | `contributor/` へ |
| このページ | Contributor / Development | `contributor/Documentation.md` へ |

## 2. 正典の対応表

同じ情報は1箇所にだけ書き、ほかからはリンクします。

| 情報 | 正典 |
|---|---|
| API のシグネチャ・引数・戻り値・例外・nullability・副作用・スレッド制約 | XML ドキュメントコメント(規約は [DocumentationComments](DocumentationComments.md)) |
| API の具体的な使い方 | User Guide |
| 概念モデル(Widget、State、Layout、Input、Overlay など) | User Concept |
| 完成物を作る手順 | User Tutorial とサンプル(`samples/`) |
| ウィジェットの最小の使用例 | サンプルの README(構成は [CONTRIBUTING.md](../CONTRIBUTING.md) の「サンプルを追加する場合の規約」) |
| 内部の動作(ツリー、Build / Layout / Paint、入力、スレッド) | Contributor Architecture |
| 設計理由と採用しなかった選択肢 | Contributor Design |
| invariant、observable behavior、ライフサイクル・所有権・スレッドの必須条件 | Contributor Requirements |
| API 設計の原則と Flutter parity / divergence の判断基準 | [APIDesign](APIDesign.md) |
| 確認済みの Flutter との差異 | [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) |
| 開発・レビュー・リリースの手順 | [CONTRIBUTING.md](../CONTRIBUTING.md) / [REVIEW.md](../REVIEW.md) / [RELEASING.md](../RELEASING.md) |
| ドキュメントの分類とサンプルの3層構成 | [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) |

XML ドキュメントコメントは `internal` / `private` にも書きます([DocumentationComments](DocumentationComments.md) の適用範囲)。公開サイトの API Reference に載せるのは `public` API だけで、`internal` 側のコメントはコントリビュータ向けの内部資料として扱います。

## 3. Documentation Impact Matrix

変更の種別ごとに、更新を検討する文書です。**その変更によって正典の説明が変わる場合だけ更新します。** 全項目を機械的に触る必要はありません。

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

PR では、テンプレートの `## Documentation` 節に、変更種別と更新した正典(更新が不要と判断した場合はその理由)を書きます。レビューでは、その判断がこの表と一致しているかを見ます。

## 関連

- [DocumentationComments](DocumentationComments.md) — XML ドキュメントコメントの規約
- [APIDesign](APIDesign.md) — API 設計の原則と Flutter parity
- [CONTRIBUTING.md](../CONTRIBUTING.md) — 開発・PR・テスト・サンプルの規約
- [REVIEW.md](../REVIEW.md) — レビューの判断基準
- [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) — ドキュメント・サンプルの情報設計
- [Issue #245](https://github.com/sumx21t-3310/FloatSoda/issues/245) — このガイドの残りの節(書くもの / 書かないもの、テンプレート)
- [Issue #219](https://github.com/sumx21t-3310/FloatSoda/issues/219) — 公開サイト(`docs/` の構造がそのままサイトの構造になります)
