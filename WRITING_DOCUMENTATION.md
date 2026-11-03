# ドキュメント執筆ガイド

FloatSoda のドキュメントは、想定読者ごとに **User docs / Contributor docs / API Reference** の3系統に分かれています(情報設計の一次情報は [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。本ページは、新しい情報を記述する際に「どの系統のどのページに書くか」「どこを一次情報とするか」「変更時に何を更新すべきか」を判断するための入口です。なお、具体的な書き方の規約はそれぞれの配置場所に定めているため、本書には再掲しません。

| 系統 | 読者 | 答える問い | 置き場所 |
|---|---|---|---|
| User docs | FloatSoda でアプリを作る人 | **What / How** — 何であり、どう使うか | `docs/tutorials/`、`docs/guide/`、`docs/reference/` |
| Contributor docs | FloatSoda 本体の変更やレビューを行う人 | **How it works / Why / Must** — どう動き、なぜそう設計され、何を守るか | `docs/contributor/`。規約の一部はリポジトリ直下の文書 |
| API Reference | 利用者・コントリビュータ・Coding Agent の全員 | **Exactly what exists** — 公開 API の正確な契約 | `src/` の XML ドキュメントコメントから生成するサイトのタブ。`docs/` には置かない |

種別ごとの「書くもの / 書かないもの」やテンプレートは、User 側が [4 章](#4-user-docs-の書くもの--書かないものとテンプレート)、Contributor 側が [5 章](#5-contributor-docs-の書くもの--書かないものとテンプレート)に記載しています。

## 1. どこに書くか

新しく書こうとしている情報について、次の3つの問いに順に答えて配置場所を判断します。1問目では、どの抽象度(ズームレベル)で捉えるべき情報かを C4 Model のレベルに当てはめます。2問目は Lv3 の情報にのみ適用し、読者が何をしたいのか(目的)に応じて分類します。3問目では、系統内の具体的な種別を Diátaxis に基づいて決定します。なお、C4 と Diátaxis はあくまで書き手側の判定基準です。公開サイトの見出しには直接出さず、読者向けにはタブとグループで提示します。各 Lv における FloatSoda での具体的な対象範囲は、[Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) の「情報の置き場所を決める順序」の表を一次情報とします。

```text
1. どのズームまで見れば理解できる情報か
   Lv1  アプリと SteamVR・VRChat・HMD の関係、3ペルソナ          → 導入(`docs/Home.md`、`docs/guide/1-discover/`)
   Lv2  参照するパッケージ、スレッド、ウィンドウ種別               → User
   Lv3  三ツリー、BuildOwner、RenderPipeline、Layer、入力の流れ    → 2 へ
   Lv4  型・メンバー・引数・戻り値・例外・nullability の契約       → XML ドキュメントコメント(API Reference の原稿)

2. Lv3 の情報は、読者が何をしたいかで割る
   使う   — 正しく使うために必要                                   → User
   変える — 変更とレビューに必要                                   → Contributor
   調べる — 型どうしの対応関係                                     → XML ドキュメントコメントの <seealso>

3. 系統の中の種別を Diátaxis で決める
   完成物を作りながら学ぶ道筋                                       → User / Tutorial
   特定の目的を達成する手順                                         → User / Guide(How-to)
   概念を理解するための説明                                         → User / Concept(Explanation)
   いまどう動いているか                                             → Contributor / Architecture(Explanation)
   なぜそう設計したか                                               → Contributor / Design(Explanation)
   変更後も何を維持するか                                           → Contributor / Requirements(Reference)
   どう変更・レビュー・公開するか                                   → Contributor / Development(How-to)
```

境界で判断に迷いやすい情報は、次のように分類します。

- **利用者から観測できる制約**は User docs に書き、**その制約を成立させる内部要件**は Contributor docs に書きます(例: 「`WorldSpaceWindow` では現在ポインタ入力を使えない」は User docs、「`WorldSpaceWindow` の入力を Controller Ray → HitTest として接続する」は Contributor docs、`WorldSpaceWindow` のプロパティ・型・例外契約は API Reference)
- **Flutter との差異**については、判断原則を [API_DESIGN.md](API_DESIGN.md) に、確認済み差異の台帳を [`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) に置きます(利用者から見える差異は、該当する User docs のページと対応するサンプルの「Flutterとの違い」節にも記載する。記録ルールは [API_DESIGN.md](API_DESIGN.md))
- **同じテーマは1つの系統でのみ詳述**し、ほかの系統からはリンクで参照します(Guide で概念を重複して説明し直したり、Concept にシグネチャを並べたりしません)

### User docs の範囲

User docs は、公開 API および API Reference と組み合わせることで、**#188 の層3(統合ミニアプリ型)のサンプルをジュニアコーダーテストで作成できる範囲**を扱います。これがドキュメントにおける「どこまで書くか」の合格ラインです。テストを通じて不足が判明した概念や制約は、User docs または API Reference の不足として回収します(手順は #188)。もし内部実装を知らなければコードを書けない箇所が見つかった場合、それは User docs の説明が不足していることを意味します。

### 種別ごとの置き場所

3問目で決めた種別は、次の場所に置きます。ファイルの並びとサイドバーの関係は [website/README.md](website/README.md) にあります。

| 種別 | 置き場所 | 例 |
|---|---|---|
| User / Tutorial | `docs/tutorials/` | [最初のオーバーレイを作る](docs/tutorials/first-overlay.md) |
| User / Guide | `docs/guide/` の各グループ | [アニメーションを付ける](docs/guide/4-build/screen/add-animation.md) |
| User / Concept | `docs/guide/` の各グループ。同じグループの How-to の後に並べる | [値が変わると、画面が変わる仕組み](docs/guide/4-build/mechanics/state-and-rebuild.md) |
| User の一覧(部品、制約、作例、用語) | `docs/reference/` | [できないこと・まだ使えないもの](docs/reference/limitations.md) |
| Contributor / Architecture | `docs/contributor/2-understand/` | [フレームワーク層](docs/contributor/2-understand/framework-layer.md) |
| Contributor / Design | `docs/contributor/2-understand/`、[決定の記録(ADR)の一覧](docs/contributor/2-understand/adr-index.md)、リポジトリ直下の規約 | [UI 層](docs/contributor/2-understand/ui-layer.md)、[LOCALIZATION.md](LOCALIZATION.md) |
| Contributor / Requirements | 独立したページはない。[5 章の Requirements](#requirements) を参照 | [REVIEW.md](REVIEW.md) の「4. FloatSoda 固有の不変条件」 |
| Contributor / Development | `docs/contributor/3-prepare/` から `6-propose/`、リポジトリ直下の規約 | [テストの分け方](docs/contributor/3-prepare/test-strategy.md)、[CONTRIBUTING.md](CONTRIBUTING.md) |
| API Reference | `src/` の XML ドキュメントコメント。[DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md) の規約で書く | サイトの API リファレンスのタブ |

## 2. 情報ごとの置き場所

同一の情報は1箇所にのみ記述し、ほかの場所からはリンクで参照します。

| 情報 | 置き場所 |
|---|---|
| API のシグネチャ・引数・戻り値・例外・nullability・副作用・スレッド制約 | XML ドキュメントコメント(規約は [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md)) |
| API の具体的な使い方 | User Guide |
| 概念モデル(Widget、State、Layout、Input、Animation、Window / Overlay) | User Concept |
| 完成物を作る手順 | User Tutorial とサンプル(`samples/`) |
| ウィジェットの最小の使用例 | サンプルの README(構成は [CONTRIBUTING.md](CONTRIBUTING.md) の「サンプルを追加する場合の規約」) |
| 内部の動作(ツリー、Build / Layout / Paint、入力、スレッド) | Contributor Architecture |
| 設計理由と採用しなかった選択肢 | Contributor Design |
| invariant、observable behavior、ライフサイクル・所有権・スレッドの必須条件 | Contributor Requirements。ただし、ツリーの不変条件(所有権・ライフサイクル・差分更新・Layer)の置き場所は [REVIEW.md](REVIEW.md) の「4. FloatSoda 固有の不変条件」([AGENTS.md](AGENTS.md) の規約表) |
| API 設計の原則と Flutter parity / divergence の判断基準 | [API_DESIGN.md](API_DESIGN.md) |
| 確認済みの Flutter との差異 | [`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) |
| 開発・レビュー・リリースの手順 | [CONTRIBUTING.md](CONTRIBUTING.md) / [REVIEW.md](REVIEW.md) / [RELEASING.md](RELEASING.md) |
| ドキュメントの分類とサンプルの3層構成 | [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) |

XML ドキュメントコメントは `internal` や `private` のメンバーにも記述します([DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md) の適用範囲)。公開サイトの API Reference に掲載するのは `public` API のみであり、`internal` メンバーのコメントはコントリビュータ向けの内部資料として扱います。

### 変わる情報の書き方

コードや Issue の状態が変わると誤りになる内容は、次のとおりに書きます。文を書くときは、「コードや Issue が変わると誤りになるか」を確かめてください。

- 件数、一覧、型の一覧のように導出元(コード、CI、`samples/` のディレクトリ)があるものは、書かずに導出元へのリンクで示すこと
- 利用者から観測できる制約の内容は、`llms.txt` に本文が入らない Issue だけに置かず、docs に書くこと
- 「次のリリースで導入する予定」のような時期の約束は書かず、解除の条件を「追跡: #NN」で示すこと(例: [できないこと・まだ使えないもの](docs/reference/limitations.md)の「追跡」列)
- 更新が止まると放置されるため、README に ADR や文書の一覧を置かないこと
- ADR の影響には現状を書かず、「解消の状況は Issue #NN を参照」と書くこと

この方針は 2026-10-12 にオーナーが決めました。

## 3. Documentation Impact Matrix

変更の種別ごとに、更新を検討すべきドキュメントの一覧です。**変更によって該当ドキュメントの説明内容が変わる場合にのみ更新します。** すべての項目を機械的に更新する必要はありません。

| 変更 | 更新対象 |
|---|---|
| public API を追加・変更する | XML ドキュメントコメント。利用者の使い方が増えるなら User Guide と Catalog 型サンプル |
| observable behavior を変更する | User docs とテスト。API 契約に影響するなら XML ドキュメントコメント |
| 公開制約・既知の制限を変更する | User docs。必要なら XML ドキュメントコメント |
| 内部アルゴリズムだけを変更する | Contributor Architecture の説明が変わる場合のみ |
| アーキテクチャの境界を変更する | Contributor Architecture と Requirements |
| 設計判断を変更する | Contributor Design。必要なら Requirements と [API_DESIGN.md](API_DESIGN.md) |
| invariant を追加・変更する | Contributor Requirements とテスト |
| Flutter との差異を導入・変更する | [`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) と divergence テスト。利用者から観測できるなら User docs とサンプルの「Flutterとの違い」節 |
| 新しい代表ユースケースを追加する | User Guide / Tutorial と、#188 の層に沿ったサンプル |
| サンプルを追加・変更する | サンプルの README と `checks/items.jsonl`([CONTRIBUTING.md](CONTRIBUTING.md) の規約) |
| ドキュメントのページを移動・改名する | リンク元のページと、そのタブの `Home.md`(サイドバーの並びに使う。公開サイトの検査がリンク切れを検出する) |

Pull Request では、テンプレートの `## Documentation` 節に変更種別と更新したドキュメント(更新が不要と判断した場合はその理由)を記載します。レビューでは、その判断が本表の基準と合致しているかを確認します。

## 4. User docs の書くもの / 書かないものとテンプレート

User docs のページは、次の骨格を土台にします。なお、ページの目的に照らして不要な節は省略します。

想定読者は [AGENTS.md の Target Users](AGENTS.md#target-users) で定義されている3タイプです。**コードを書くのも読むのも LLM である**という前提を常に意識し、「LLM がこの API を誤用しない」明確な書き方を優先します。また、Unity の知識のみを持つ読者(Booth クリエイター)に向けては、Unity や uGUI の語彙に対応させた解説を補足します。

種別ごとに、書くものと書かないものを以下のように整理します。

| 種別 | 書くもの | 書かないもの |
|---|---|---|
| Concept | 概念の目的、利用者から見たモデル、ほかの概念との関係、利用に必要な最低限の仕組み、最小例、観測できる制約 | `private` / `internal` の実装、内部アルゴリズム、設計判断の経緯、API メンバーの一覧 |
| Guide | 「〜するには」への答え。目的に必要な手順と API の組み合わせ | Concept の説明のやり直し、シグネチャや引数の契約(API Reference へ) |
| Tutorial | 具体的な完成物を作る一本道の学習体験 | API の網羅 |

サンプルは #188 の3層に対応します。

| 種別 | 対応するサンプル |
|---|---|
| Concept | 層1: Widget カタログ型(1サンプル = 1ウィジェット群の最小デモ) |
| Guide | 層2: Cookbook 型(複数の Widget / API を組み合わせる) |
| Tutorial | 層3: 統合ミニアプリ型(公開 API だけで本番に近い形を作る) |

### Concept

```text
# 概念の名前
## 言いたいことを言い切る見出し(複数) — 利用者から見たモデル。内部実装は書かない
## 別の作り方を知っている人へ         — Unity / uGUI や Flutter との対応と違い。要るときだけ
## 期待どおりに動かないとき           — 観測できる症状と直し方
## 制約                               — 観測できる制限の内容と「追跡: #NN」。無ければ省く
## 次に読むページ
```

### Guide

```text
# 〜する
## この手順でできること
## 前提条件
## 手順1: …
## 手順2: …
## 結果を確かめる       — 成功したときに見えるもの。対応するサンプルがあればリンクする
## うまくいかないとき
```

### Tutorial

```text
# 〜を作る
## このチュートリアルで作るもの
## 始める前に用意するもの
## 1. …
## 2. …
## 次に読むページ
```

サンプルコードは `samples/` ディレクトリ配下をマスターとし、ドキュメント本文には必要最小限のコード断片のみを掲載します。ドキュメント専用に完全な実装を別途記述することはしません。

## 5. Contributor docs の書くもの / 書かないものとテンプレート

Contributor docs は `docs/contributor/` に置きます。本章で提示する「書くもの / 書かないもの」およびテンプレートは、既存のページや [REVIEW.md](REVIEW.md) の「4. FloatSoda 固有の不変条件」、[`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) のエントリから逆算して定めたものです。種別ごとに最も近い既存ページを例として挙げていますので、新しいページを書く際は、その例に合わせて構成してください。

既存のページには複数の種別が混在している箇所があります。たとえば [フレームワーク層](docs/contributor/2-understand/framework-layer.md) の「レイアウトと描画は、境界までを描き直す」節では、更新が伝わる仕組み(Architecture)と、「値が変わったときだけ dirty にする」という契約(Requirements)が同一の節に含まれています。ページを書き直すときは、混在している節を種別ごとに分割するか、適切な種別の別ページへとリンクしてください。

### Architecture

対象のコンポーネントが現状**どのように動いているか**を書きます。想定読者は、そのコンポーネントに変更を加える前に責務の境界や処理の流れを把握したいコントリビュータです。

書くもの:

- **責務と境界** — 何を担い、何を担わないか(例: [フレームワーク層](docs/contributor/2-understand/framework-layer.md) の「フレームワーク層が持つ4つのツリー」表)
- **処理の流れ** — フレーム、イベント、データが通る順序。図を用いてもよい(例: [1フレームが SteamVR に届くまで](docs/contributor/2-understand/frame-to-steamvr.md) の「メインスレッド: ビルドから Layer の複製まで」)
- **所有権とライフサイクル** — 誰が何を所有し、いつ生成され、いつ破棄されるか。スレッドごとの所有物とスレッド間通信の方法(例: 同じページの「1フレームを担う2つのスレッド」表)
- **現在の実装の要点** — ソースコードのファイルパス、型名、メソッド名を明記する(例: フレームワーク層の「変更が dirty として登録され、ビルドされる」)
- **未実装の領域** — 内容を書き、解除の条件は「追跡: #NN」で示す。「動かない理由が未実装なのかバグなのか」を読者が切り分けるために必要となる。進み具合は、導出元の調べ方を案内する(例: [UI 層](docs/contributor/2-understand/ui-layer.md) の「実装の進み具合の追い方」)
- **テストでの駆動** — 実時間経過や実機環境なしで、その仕組みを動かしてテストする方法(例: [テストの分け方](docs/contributor/3-prepare/test-strategy.md) の「HMD なしで確かめられる3つの範囲」)
- **Flutter との対応** — どこまで Flutter と同じ戦略を採用しているか。差異がある場合は台帳のエントリ番号を明記する

書かないもの:

- **なぜその設計にしたのか** → Design に書く。「Flutter と同じ戦略」と一言添える程度は構わないが、比較検討した選択肢やその理由は Design に置く(例: フレームワーク層の Semantics の注記は、比較した選択肢を書かずに REVIEW.md の「Flutter parity」へリンクしている)
- **変更後も守るべき条件の列挙** → Requirements に書く。Architecture には現在の仕組みを記述し、満たすべき条件は Requirements に書く
- **利用者向けの使い方や注意点** → User docs に書く。「intrinsic 測定はコストに注意」のような利用者向けの注意は、[負荷を抑える](docs/guide/5-troubleshoot/reduce-load.md)のような User docs のページに置く
- **API のシグネチャや引数の契約** → XML ドキュメントコメントに書く。組み込みウィジェットの一覧は、リファレンスの[部品を目的から探す](docs/reference/find-widgets.md)に置く

コントリビュータ向けのサンプル(#188 の分類における `PaintingSample` / `PrimitiveOverlay`)は、Architecture の実例として本文からリンクできます。

```text
# 層や仕組みの名前
## 何を担うか                         — 責務と境界。型とソースのパス
## 言いたいことを言い切る見出し(複数) — 処理の流れ、所有権、ライフサイクル。図を使ってよい
## Flutter と違うところ               — 同じ戦略か、差異は台帳のどれか
## 実験で確かめる                     — 実時間や実機なしで動かす方法
## 次に読むページ
```

### Design

設計において**なぜそうしたのか(判断理由)**を書きます。想定読者は、その判断の背景を知らないまま「修正したくなる」コントリビュータや、将来その判断を覆すかどうかを決定する人です。

書くもの:

- **判断対象と背景** — 何を決定したのか、どのような課題が存在したのか(例: [UI 層](docs/contributor/2-understand/ui-layer.md) の「UI を3層に分ける理由」)
- **採用した方針と客観的な判定手順** — 「〜のほうが自然」といった主観に流れないよう、判定可能な手順や基準を添える(例: [API_DESIGN.md](API_DESIGN.md) の「.NET が標準で提供する機構を再実装しない」の3ステップ、UI 層の「2つ目のデザインシステムが、1つ目のコードをコピーせずに同じコンポーネントを作れるか」)
- **採用理由と、理由にならないもの** — FloatSoda 固有の事情(Flutter / .NET / VR オーバーレイ / [AGENTS.md の Target Users](AGENTS.md#target-users))に根拠を置く(例: API_DESIGN.md の「理由にならないもの」)
- **採用しなかった選択肢とその理由**(例: [LOCALIZATION.md](LOCALIZATION.md) の「ニュートラル = 英語が定石だが、主な対象ユーザーと Booth での流通を優先した」)
- **影響範囲と変更手続き** — その判断が適用される範囲と、覆す際の手続き(例: LOCALIZATION.md の「変更する場合は必ず issue で議論」)
- **「コントリビュータへ」の注意書き** — 背景を知らないと修正したくなるような判断には、それが意図的な設計であることを明記する(例: LOCALIZATION.md の 1 章末尾)

書かないもの:

- **現在の処理の流れの詳細** → Architecture に書く
- **Flutter との個別の差異** → [`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) に書く。Design は判断原則のみを扱い、個別差異の記録とは役割を分担する([API_DESIGN.md](API_DESIGN.md) と台帳の分担と同様)
- **作業手順** → Development に書く
- **議論の途中経過** — 決定事項と確定した理由のみを書き、議論の変遷は Issue に残す

```text
# 判断対象
## 背景                 — どんな問題があったか
## 採用した方針         — 判定できる手順や基準を添える
## 理由                 — FloatSoda 固有の事情。理由にならないものも書く
## 採用しなかった選択肢 — 何を、なぜ採らなかったか
## 影響範囲と変更手続き — 効く範囲と、覆すときの手続き
## 関連 Requirements / Architecture
```

### Requirements

実装を変更しても**維持すべき条件**を書きます。想定読者は、自身の変更が何を壊しうるかを確認したいコントリビュータやレビュアーです。

Requirements の独立したページは現時点ではまだありません。もっとも近い既存の実物は、[REVIEW.md](REVIEW.md) の「4. FloatSoda 固有の不変条件」、[API_DESIGN.md](API_DESIGN.md) の「差異が必要な場合の記録義務」、および台帳のエントリ(`Test` 欄で検証方法と対になる)です。テンプレートはこれらの構成から逆算して定めています。なお、ツリーの不変条件(所有権・ライフサイクル・差分更新・Layer)の配置場所は REVIEW.md 4 章のままとします([AGENTS.md](AGENTS.md) の規約表)。Requirements のページを作成する際も、4 章の内容を複製せずリンクで参照してください。4 章に含まれていない条件(observable behavior、failure mode、検証方法、Flutter との関係)を Requirements 側に記述します。

書くもの:

- **適用範囲** — どのツリー、型、操作に適用されるか
- **必須条件と不変条件** — 「常に成り立つ」条件の文として記述する。REVIEW.md の 4 章にある確認の問い(「drop 後に `Parent` が `null` に戻るか」など)を裏返した形にする。なお、4 章にある条件は複製せずリンクで参照する
- **Observable behavior** — 利用者やテストから観測できる契約(例: [1フレームが SteamVR に届くまで](docs/contributor/2-understand/frame-to-steamvr.md) の「すべてのフレームで、レイアウトと描画が走るわけではありません」で始まる段落は、Requirements へ移せる observable behavior)
- **違反時の failure mode** — どのような入力や状態において、何が破損・破綻するのか。REVIEW.md の指摘基準(concrete failure mode)と同じ粒度で記述する(例: REVIEW.md の「Layer clone 後に意図しない mutable state の共有がないこと」の項)
- **検証方法** — その条件を固定しているテストのファイル名とメソッド名。存在しない場合は「未設定」と明記する。台帳の `Test: — (not set)` と同様に、「未設定」は記録完了ではなく未処理のタスクとして扱う
- **Flutter との関係** — parity(同等)であるか、あるいは台帳のどのエントリに該当するか

書かないもの:

- **なぜその条件が必要なのか** → Design に書く。一文程度の理由を添えるのは差し支えないが、選択肢の比較や経緯は Design に置く
- **現在の実装がどのように条件を満たしているか** → Architecture に書く。Requirements には、実装が変わっても維持される条件のみを記述する
- **実装詳細に依存する条件** — `private` フィールドの状態や呼び出し回数などは書かない。テストと同様に、外部から観測可能な observable behavior として記述する([CONTRIBUTING.md](CONTRIBUTING.md) の「テスト観点」)
- **作業手順** → Development に書く

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

特定の作業における**手順と規約**を書きます。想定読者は、その作業にこれから取りかかるコントリビュータや Coding Agent です。

開発・レビュー・リリースに関する全体規約は [CONTRIBUTING.md](CONTRIBUTING.md) / [REVIEW.md](REVIEW.md) / [RELEASING.md](RELEASING.md) に定められており、`docs/` 側の Development ページがこれらを置き換えるものではありません。特定の作業領域に絞った個別の規約は、リポジトリ直下の文書に置きます(例: [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md)、本ページ)。特定の作業の手順は、`docs/contributor/` に置きます(例: [テストの分け方](docs/contributor/3-prepare/test-strategy.md))。

書くもの:

- **冒頭に、このページの責務と隣接する規約の配置場所を示した表** — 読者が「探している規約がこのページにあるか」を最初に確認できるようにする(例: CONTRIBUTING.md と REVIEW.md の冒頭)
- **規約の強度** — 必須(MUST)と推奨(SHOULD)を明確に区別する
- **手順** — 記載順にそのまま実行できる形式とする(例: [LOCALIZATION.md](LOCALIZATION.md) の「メッセージを追加する手順」)
- **規約が守られる場所** — CI、PR テンプレート、lint、`npm run verify` など、違反すると検出される仕組み。無い場合は、レビューで確認する項目として記載する(例: [3 章](#3-documentation-impact-matrix)の Impact Matrix と PR テンプレートの `## Documentation` 節)
- **変わる情報** — 件数や配置のように変動する情報は書かず、導出元へのリンクで示す([変わる情報の書き方](#変わる情報の書き方)に従う)

書かないもの:

- **別の場所にすでに定義されている規約の再掲** — リンクにとどめる。複数箇所に記述すると、将来片方のみが更新されて食い違う原因となるためである
- **仕組みの説明** → Architecture に書く、**理由** → Design に書く。理由は一文で添える程度にとどめ、詳細は該当ページへリンクする

手順のページは、[Guide](#guide) と同じ形で書きます。規約のページは、次の形で書きます。

```text
# 名前
冒頭: このページの責務と、隣接する規約の置き場所の表
## 規約               — 必須 / 推奨を明示
## 手順               — 順番どおりに実行できる形
## 規約が守られる場所 — CI、テンプレート、lint、verify
## 関連
```

## 6. 文体

FloatSoda のドキュメントは日本語で記述し、散文と箇条書きを適切に使い分けます。本節では、`docs/` 配下、サンプルの README、ならびに Issue や PR の本文に共通して適用する文体を定めます。XML ドキュメントコメントおよび例外メッセージの文体については、それぞれの個別規約を参照してください。

| 対象 | 文体の規約の置き場所 |
|---|---|
| `docs/` 配下、サンプルの README、Issue と PR の本文 | この節 |
| XML ドキュメントコメント | [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md)(敬体、日本語のみ) |
| 例外メッセージ | [LOCALIZATION.md](LOCALIZATION.md) の「メッセージの文体」 |
| `AGENTS.md` | 英語。[CONTRIBUTING.md](CONTRIBUTING.md) の「エージェント向けファイルの言語」 |

### 規約

必須事項は「〜こと」、推奨事項は「〜が望ましい」として書き分けます。

- 散文は敬体で書くこと
- 箇条書きと表のセルは常体で書くこと。箇条書きは Markdown のリスト項目を指し、1項目を1文または体言止めにすること。2文以上になる場合は散文へ移すこと
- 条件と制約は箇条書きで示し、必須は「〜こと」、推奨は「〜が望ましい」で終えること
- 規約は、読者に求める行動として書くこと。禁止そのものが要点である場合のみ禁止形を用い、続けて代替となる行動を示すこと
- 語彙は読者の専門領域の言葉で書くこと。読者の領域は [AGENTS.md の Target Users](AGENTS.md#target-users) にある3ペルソナが理解できる Unity、Flutter、.NET、VRChat の用語とする。他分野の用語を比喩として持ち込まず、対象の概念をそのまま説明すること。定義どおりに用いる専門用語(不変条件、冪等、直交)はそのまま使用すること
- 英語の慣用句は直訳せず、自然な日本語の表現に置き換えること。技術用語(deploy、revert、worktree)は英語表記またはカタカナ表記のまま書くこと
- 手順は「〜する」の形式とし、実行する作業者の視点で書くこと。作業の経緯や結果は、手順とは別の節に配置すること
- チェックリストの項目は、「はい」か「いいえ」で答えられる単一の事実にすること

### 言い換える語

次の語彙は、既存の文書から言い換えを行いました（2026-09-15）。一覧にある語を新しく記述する場合も、右側の言い換えを使用してください。この一覧は [Issue #262](https://github.com/sumx21t-3310/FloatSoda/issues/262) で決定されました。機械による検査は [Issue #260](https://github.com/sumx21t-3310/FloatSoda/issues/260) で扱います。

| 語 | 言い換え | 備考 |
|---|---|---|
| 正典 | 一次情報、置き場所 | 宗教の語を比喩に借りた表現 |
| 担保する | 保証する | 法務の語を比喩に借りた表現 |
| Litmus test | 判定基準 | 化学の語を比喩に借りた表現 |
| 経路 | 読ませ先、手順、流れ | 「入力経路」のように信号の道筋を指す使い方は残す |
| 直交する | 無関係 | `Wrap` の交差軸のように幾何の定義どおりの使い方は残す |

### 規約が守られる場所

- 敬体と常体の使い分け、文の長さ、禁止語は textlint で検査する。設定は `website/` に配置し、`npm run verify` から呼び出す(未導入。導入は [Issue #260](https://github.com/sumx21t-3310/FloatSoda/issues/260))
- 必須と推奨の書き分け、記述の視点、チェックリストの形式は、レビュー時に確認する
- Issue と PR の本文は、テンプレートの見出しに従って手順と経緯を分けて記述する

## 関連

- [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md) — XML ドキュメントコメントの規約
- [LOCALIZATION.md](LOCALIZATION.md) — 記述言語と例外メッセージの文体
- [API_DESIGN.md](API_DESIGN.md) — API 設計の原則と Flutter parity
- [AGENTS.md の Target Users](AGENTS.md#target-users) — 想定する3タイプの作り手
- [CONTRIBUTING.md](CONTRIBUTING.md) — 開発・PR・テスト・サンプルの規約
- [REVIEW.md](REVIEW.md) — レビューの判断基準と、ツリーの不変条件の置き場所(「4. FloatSoda 固有の不変条件」)
- [テストの分け方](docs/contributor/3-prepare/test-strategy.md) — テストの置き場所を決める地図
- [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) — ドキュメント・サンプルの情報設計
- [Issue #219](https://github.com/sumx21t-3310/FloatSoda/issues/219) — 公開サイト(`docs/` の構造がそのままサイトの構造になります)
