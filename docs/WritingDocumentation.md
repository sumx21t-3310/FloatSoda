← [Home](Home.md)

# ドキュメント執筆ガイド

FloatSoda のドキュメントは、読者ごとに **User docs / Contributor docs / API Reference** の3系統に分かれています(情報設計の正典は [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。このページは、新しい情報を書くときに「どの系統のどのページに書くか」「何が正典か」「変更したとき何を更新するか」を決めるための入口です。書き方の規約そのものは、それぞれの正典に置いてあり、ここには再掲しません。

| 系統 | 読者 | 答える問い |
|---|---|---|
| User docs | FloatSoda でアプリを作る人 | **What / How** — 何であり、どう使うか |
| Contributor docs | FloatSoda 本体を変える・レビューする人 | **How it works / Why / Must** — どう動き、なぜそう設計され、何を守るか |
| API Reference | 利用者・コントリビュータ・Coding Agent の全員 | **Exactly what exists** — 公開 API の正確な契約 |

> **実装状況** — 系統ごとのディレクトリ(`docs/user/`、`docs/contributor/`、`docs/api/`)への再編は #188 の手順4で進めます。現行の `docs/` はほぼ Contributor 向けの密なリファレンスなので、再編では **現行ページを `contributor/` へ移し、User docs は新しく書きます**。それまでは [再編の地図](#再編の地図) で該当ページを選んでください。このガイド自体も、再編後は `docs/contributor/Documentation.md` へ移ります。種別ごとの「書くもの / 書かないもの」とテンプレートは、User 側が [4 章](#4-user-docs-の書くもの--書かないものとテンプレート)、Contributor 側が [5 章](#5-contributor-docs-の書くもの--書かないものとテンプレート)にあります。

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

### User docs の範囲

User docs は、公開 API と API Reference と合わせて、**#188 の層3(統合ミニアプリ型)のサンプルをジュニアコーダーテストで書ける範囲**を扱います。これが「どこまで書くか」の合格ラインです。テストで足りないと分かった概念や制約は、User docs か API Reference の不足として回収します(手順は #188)。内部実装を知らないと書けない箇所が見つかったら、それは User docs の穴です。

### 再編の地図

再編(#188 手順4)の出発点です。現行ページは内部の仕組みの説明が中心なので、User 向けの部分だけを抜き出して新しいページに書き、本体は `contributor/` へ移します。種別は再編時に実物で確定します。

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
| [TestStrategy](TestStrategy.md) | Contributor / Development | テストの置き場所を決める地図。配置と件数は日付付きで更新する |
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

同じ情報は1箇所にだけ書き、ほかからはリンクします。

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

## 4. User docs の書くもの / 書かないものとテンプレート

User docs は新しく書くページが多いので、骨格を先に決めておきます。節はページの目的に不要なら省きます。

読者は [TargetUsers](TargetUsers.md) の3タイプです。**コードを書くのは LLM で、読むのも LLM** という前提を忘れずに、「LLM がこの API を誤用しない」書き方を優先します。Unity しか知らない読者(Booth クリエイター)には、Unity / uGUI の語彙からの読み替えを添えます。

種別ごとに、書くものと書かないものを分けます。

| 種別 | 書くもの | 書かないもの |
|---|---|---|
| Concept | 概念の目的、利用者から見たモデル、ほかの概念との関係、利用に必要な最低限の仕組み、最小例、観測できる制約 | `private` / `internal` の実装、内部アルゴリズム、設計判断の経緯、API メンバーの一覧 |
| Guide | 「〜するには」への答え。目的に必要な手順と API の組み合わせ | Concept の説明のやり直し、シグネチャや引数の契約(API Reference へ) |
| Tutorial | 具体的な完成物を作る一本道の学習体験 | API の網羅 |

サンプルは #188 の3層と対応します。

| 種別 | 対応するサンプル |
|---|---|
| Concept | 層1: Widget カタログ型(1サンプル = 1ウィジェット群の最小デモ) |
| Guide | 層2: Cookbook 型(複数の Widget / API を組み合わせる) |
| Tutorial | 層3: 統合ミニアプリ型(公開 API だけで本番に近い形を作る) |

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

サンプルコードは `samples/` を正典にし、本文には必要な最小の断片だけを載せます。完全な実装をドキュメント用に別に書きません。

## 5. Contributor docs の書くもの / 書かないものとテンプレート

Contributor docs は、現行の `docs/` のページを `contributor/` へ移して作ります。ここに書く「書くもの / 書かないもの」とテンプレートは、[再編の地図](#再編の地図)で行き先が決まっている現行ページと、[REVIEW.md](../REVIEW.md) の「4. FloatSoda 固有の不変条件」、[`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) のエントリから逆算したものです。種別ごとに、いちばん近い実物のページを例として挙げます。新しいページを書くときも、既存のページを移すときも、その例に寄せてください。

現行ページには複数の種別が混ざっています。例えば [RenderObjects](RenderObjects.md) の「差分更新」節は、伝播の仕組み(Architecture)と「プロパティを変更したら `MarkNeedsLayout()` / `MarkNeedsPaint()` を呼ぶ」という契約(Requirements)が同じ節にあります。再編では、混ざっている節を種別ごとに分けるか、別種別のページへリンクします。

### Architecture

いま**どう動いているか**を書きます。読者は、そのコンポーネントを変更する前に境界と流れを把握したいコントリビュータです。

書くもの:

- **責務と境界** — 何を担い、何を担わないか(例: [Architecture](Architecture.md) の「アセンブリ構成」表)
- **処理の流れ** — フレーム、イベント、データが通る順序。図を使ってよい(例: Architecture の「レンダリングライフサイクル」、[BuildPipeline](BuildPipeline.md) の「全体の流れ」)
- **所有権とライフサイクル** — 誰が何を持ち、いつ作られ、いつ捨てられるか。スレッドごとの所有物と通信方法(例: Architecture の「スレッドモデル」表)
- **現在の実装の要点** — ソースのパスと型・メソッド名を添える(例: BuildPipeline の「BuildOwner と dirty list」)
- **実装状況と未実装の領域** — Alpha の間は冒頭に置きます。読者が「動かないのは未実装だからか、バグか」を切り分けるために要ります(例: BuildPipeline の冒頭ブロックと「未実装の領域」表)
- **テストでの駆動** — その仕組みを実時間や実機なしで動かす方法(例: [Animation](Animation.md) の「テストでの駆動」)
- **Flutter との対応** — どこまで Flutter と同じ戦略か。差異は台帳のエントリ番号を指す

書かないもの:

- **なぜその設計にしたか** → Design。「Flutter と同じ戦略」と一言添えるのはよいですが、比較した選択肢と理由は Design に置きます(例: RenderObjects の Semantics の注記は、理由を書かずに APIDesign へリンクしている)
- **変更後も守る条件の列挙** → Requirements。Architecture は「いまこうなっている」を書き、「こうでなければならない」は Requirements に書きます
- **利用者向けの使い方と注意** → User docs。「intrinsic 測定はコストに注意」のような利用者向けの注意は、再編で User 側へ抜き出します
- **API のシグネチャと引数の契約** → XML ドキュメントコメント。組み込みの一覧表は API Reference と重なるので、再編で整理します

コントリビュータ向けサンプル(#188 の分類で `PaintingSample` / `PrimitiveOverlay`)は、Architecture の実例として本文からリンクできます。

```text
# 名前
## 実装状況               — Alpha の間は冒頭に。使える範囲と未実装
## 責務と境界             — 何を担い、何を担わないか
## 関係するコンポーネント — 型とソースのパス
## 処理の流れ             — 順序。図を使ってよい
## 所有権・ライフサイクル — 誰が持ち、いつ作られ、いつ捨てられるか。スレッド
## Flutter との対応       — 同じ戦略か、差異は台帳のどれか
## テストでの駆動         — 実時間や実機なしで動かす方法
## 未実装の領域
## 関連 Requirements / Design
```

### Design

**なぜそうしたか**を書きます。読者は、その判断を知らずに「直したくなる」コントリビュータと、将来その判断を覆すかどうかを決める人です。

書くもの:

- **判断対象と背景** — 何を決めたか、どんな問題があったか(例: [UILayering](UILayering.md) の「Material ロックイン」)
- **採用した方針と、判定可能な手順** — 「〜が自然」のような主観に流れないよう、手順や Litmus test を添えます(例: [APIDesign](APIDesign.md) の「.NET が標準で提供する機構を再実装しない」の3ステップ、UILayering の「2つ目のデザインシステムが、1つ目のコードをコピーせずに同じコンポーネントを作れるか」)
- **理由と、理由にならないもの** — FloatSoda 固有の事情(Flutter / .NET / VR オーバーレイ / [TargetUsers](TargetUsers.md))に根拠を置きます(例: APIDesign の「理由にならないもの」)
- **採用しなかった選択肢と、その理由**(例: [Localization](Localization.md) の「ニュートラル = 英語が定石だが、主客層と Booth 流通を優先した」)
- **影響範囲と変更手続き** — この判断が効く範囲と、覆すときの手続き(例: Localization の「変更する場合は必ず issue で議論」)
- **「コントリビュータへ」の注意書き** — 知らないと直したくなる判断には、意図的であることを明記します(例: Localization の 1 章末尾)

書かないもの:

- **現在の処理の流れの詳細** → Architecture
- **Flutter との個別の差異** → [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md)。Design は判断原則だけを持ちます([APIDesign](APIDesign.md) と台帳の分担と同じ)
- **手順** → Development
- **議論の経過** — 決めたことと理由だけを書きます。経過は Issue に残します

```text
# 判断対象
## 背景                 — どんな問題があったか
## 採用した方針         — 判定可能な手順や Litmus test を添える
## 理由                 — FloatSoda 固有の事情。理由にならないものも書く
## 採用しなかった選択肢 — 何を、なぜ採らなかったか
## 影響範囲と変更手続き — 効く範囲と、覆すときの手続き
## 関連 Requirements / Architecture
```

### Requirements

実装を変えても**維持する条件**を書きます。読者は、変更が何を壊しうるかを確認するコントリビュータとレビュアーです。

Requirements の独立したページはまだありません。いちばん近い実物は、[REVIEW.md](../REVIEW.md) の「4. FloatSoda 固有の不変条件」、[APIDesign](APIDesign.md) の「差異が必要な場合の記録義務」、台帳のエントリ(`Test` 欄で検証方法と対になる)です。テンプレートはこれらの形から逆算しています。再編で Requirements のページを作るときは、REVIEW.md の 4 章を出発点にし、REVIEW.md 側はそのページへのリンクにします。

書くもの:

- **適用範囲** — どのツリー、型、操作に効くか
- **必須条件と不変条件** — 「常に成り立つ」形で書きます(例: REVIEW.md の「adopt / drop は代入のたびに対称に走るか」「drop 後に `Parent` が `null` に戻るか」を、確認の問いではなく条件の文にしたもの)
- **Observable behavior** — 利用者とテストから観測できる契約(例: BuildPipeline の「Widget にも RenderObject にも変更がないフレームでは、レイアウト・ペイント・合成のすべてがスキップされます」は、Requirements へ移せる observable behavior)
- **破ったときの failure mode** — どの入力・状態で、何が壊れるか。REVIEW.md の finding の基準(concrete failure mode)と同じ粒度で書きます(例: REVIEW.md の「Layer の clone 後に可変オブジェクトを共有するとデータレースになり、テストではまず落ちない」)
- **検証方法** — 条件を固定しているテストのファイルとメソッド名。無ければ「未設定」と明示します。台帳の `Test: — (not set)` と同じで、未設定は完了した記録ではなく未処理のタスクです
- **Flutter との関係** — parity か、台帳のどのエントリか

書かないもの:

- **なぜその条件が要るか** → Design。一文の理由は添えてよいですが、比較と経緯は Design に置きます
- **現在の実装がどう満たしているか** → Architecture。Requirements は実装が変わっても残る条件だけを書きます
- **実装詳細に依存する条件** — `private` フィールドの状態や呼び出し回数。テストと同じく observable behavior で書きます([CONTRIBUTING.md](../CONTRIBUTING.md) の「テスト観点」)
- **手順** → Development

```text
# 名前
## 適用範囲                  — どのツリー、型、操作に効くか
## 必須条件                  — 実装を変えても維持する条件
## 不変条件                  — 常に成り立つ性質
## Observable behavior       — 利用者とテストから観測できる契約
## 破ったときの failure mode — どの入力・状態で、何が壊れるか
## 検証方法                  — 固定しているテスト。無ければ「未設定」
## Flutter との関係          — parity か、台帳のエントリ番号
## 関連 Design / Architecture
```

### Development

特定の作業の**手順と規約**を書きます。読者は、その作業をこれから行うコントリビュータと Coding Agent です。

開発・レビュー・リリースの規約の正典は [CONTRIBUTING.md](../CONTRIBUTING.md) / [REVIEW.md](../REVIEW.md) / [RELEASING.md](../RELEASING.md) で、`docs/` 側の Development ページはそれを置き換えません。`docs/` に置くのは、特定の作業に絞った規約と手順です(例: [DocumentationComments](DocumentationComments.md)、[TestStrategy](TestStrategy.md)、このページ)。

書くもの:

- **冒頭に、このページの責務と隣接する正典の表** — 読者が「この規約はここが正典か」を最初に確かめられるようにします(例: CONTRIBUTING.md と REVIEW.md の冒頭、TestStrategy の冒頭段落)
- **規約の強度** — 必須(MUST)と推奨(SHOULD)を区別します
- **手順** — 順番どおりに実行できる形(例: [Localization](Localization.md) の「メッセージを追加する手順」)
- **規約が守られる場所** — CI、PR テンプレート、lint、`npm run verify` など、破ると検出される仕組み。無ければ、レビューで見る項目として書きます(例: [3 章](#3-documentation-impact-matrix)の Impact Matrix と PR テンプレートの `## Documentation` 節)
- **日付付きの現状** — 件数や配置のような変わる情報は、日付を添えて更新の責任を明示します(例: TestStrategy の「現在の配置(2026-09-09 時点)」)

書かないもの:

- **正典にある規約の再掲** — リンクします。二重に書くと片方だけ更新されて食い違います
- **仕組みの説明** → Architecture、**理由** → Design。理由は一文で添え、詳細はリンクします

```text
# 名前
冒頭: このページの責務と、隣接する正典の表
## 規約               — 必須 / 推奨を明示
## 手順               — 順番どおりに実行できる形
## 規約が守られる場所 — CI、テンプレート、lint、verify
## 関連
```

## 関連

- [DocumentationComments](DocumentationComments.md) — XML ドキュメントコメントの規約
- [APIDesign](APIDesign.md) — API 設計の原則と Flutter parity
- [TargetUsers](TargetUsers.md) — 想定する3タイプの作り手
- [CONTRIBUTING.md](../CONTRIBUTING.md) — 開発・PR・テスト・サンプルの規約
- [REVIEW.md](../REVIEW.md) — レビューの判断基準。「4. FloatSoda 固有の不変条件」は Requirements の実物に最も近い
- [TestStrategy](TestStrategy.md) — テストの置き場所を決める地図
- [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) — ドキュメント・サンプルの情報設計
- [Issue #219](https://github.com/sumx21t-3310/FloatSoda/issues/219) — 公開サイト(`docs/` の構造がそのままサイトの構造になります)
