← [Home](../Home.md)

# ドキュメント執筆ガイド

FloatSoda のドキュメントは、入口の **導入** と、読者の目的ごとの **コンセプト / ユーザー向け / コントリビューター向け / API Reference** に分かれています(情報設計の一次情報は [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。本ページは、新しい情報を書くときに「どの区分のどのページに書くか」「どこが一次情報か」「変更時に何を更新するか」を決めるための入口です。書き方の規約はそれぞれの置き場所に置いているため、ここには再掲しません。

| 区分 | 読者 | 答える問い |
|---|---|---|
| 導入 | 初めて来た人 | FloatSoda とは何で、だれ向けか |
| コンセプト | 利用者とコントリビュータの両方 | **What** — 公開 API から見て何であり、どう振る舞い、どんな制約があるか |
| ユーザー向け | FloatSoda でアプリを作る人 | **How** — 目的をどう果たすか |
| コントリビューター向け | FloatSoda 本体を変える・レビューする人 | **How it works / Why / Must** — どう動き、なぜそう設計され、何を守るか |
| API Reference | 利用者・コントリビュータ・Coding Agent の全員 | **Exactly what exists** — 公開 API の正確な契約 |

> **実装状況** — この区分への再編は、#188 の手順4で進めています。現行ページは、**ファイル名を変えずに `contributor/` へ移しました。コンセプトとユーザー向けは骨組みだけがあり、これから新しく書きます**。目指すページ構成と、今のページの行き先は、[再編の地図](#再編の地図) にあります。種類ごとの「書くもの / 書かないもの」とテンプレートは、コンセプトとユーザー向けが [4 章](#4-コンセプトとユーザー向けの書くもの--書かないものとテンプレート)、コントリビューター向けが [5 章](#5-コントリビューター向けの書くもの--書かないものとテンプレート)にあります。

## 1. どこに書くか

書こうとしている情報について、次の問いに順に答えます。中心になるのは3問目の「公開 API の外から観測できることだけで書けるか」です。外から観測できる説明はコンセプトに、目的を果たす道筋はユーザー向けに、内部の仕組み・理由・守ることはコントリビューター向けに置きます。Diátaxis の種別名は書き手の判定基準です。サイトの見出しには出さず、読者には「知る / 使う / 変える / 調べる」の区分で示します。

```text
1. 型・メンバー・引数・戻り値・例外・nullability の契約か
   はい → XML ドキュメントコメント(API Reference の原稿)

2. FloatSoda とは何で、だれ向けかの説明か(アプリと SteamVR・VRChat・HMD の関係、3ペルソナ)
   はい → 導入(Home、TargetUsers)

3. 公開 API の外から観測できることだけで書けるか
   書ける   — 何であり、どう振る舞い、どんな制約があるか   → コンセプト
   書ける   — 完成物を作りながら学ぶ道筋                   → ユーザー向け / 作りながら学ぶ(Tutorial)
   書ける   — 1つの Widget の紹介と最小の使用例            → ユーザー向け / Widget カタログ(サンプルの README)
   書ける   — 特定の目的を達成する手順                     → ユーザー向け / 〜するには(How-to)
   書けない — 内部の仕組み・理由・守ること・変更の手順     → コントリビューター向け(4 へ)

4. コントリビューター向けは、種別でディレクトリを決め、その中のトピックを選ぶ
   種別 — 題材を1つ決め、PR を出すまでの一本道           → tutorials/(貢献までの流れ)
          特定の作業の手順                               → how-to/(作業の手順)
          変更後も維持する条件                           → reference/(守ること)
          いまどう動くか、なぜそう設計したか             → explanation/(仕組みと理由)
   動作原理を動かして確かめる問い                         → experiments/(実験)
   トピック — 既存のページに入るなら、そのページに足す。入らないときだけ新しいページを作る
```

例えば「`SetState` を呼ぶと、その Widget から下が作り直される」は、外から観測できるのでコンセプトに置きます。「`BuildOwner` が dirty な Element を `Depth` 順に作り直す」は内部の仕組みなので、コントリビューター向けの [三つのツリーの仕組みと理由](explanation/Trees.md) に置きます。

境界で迷いやすい情報は、次のように分類します。

- **利用者から観測できる制約**はコンセプトに、**その制約を成立させる内部要件**はコントリビューター向けに、**制約を避けて目的を果たす手順**はユーザー向けの「〜するには」に書きます。例: 「`WorldSpaceWindow` では現在ポインタ入力を使えない」はコンセプト、「`WorldSpaceWindow` の入力を Controller Ray → HitTest として接続する」はコントリビューター向け、`WorldSpaceWindow` のプロパティ・型・例外契約は API Reference
- **Flutter との差異**は、判断原則を [APIDesign](APIDesign.md) に、確認済み差異の台帳を [`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) に置きます。利用者から見える差異は、該当するコンセプトのページと、対応するサンプルの「Flutterとの違い」節にも記載します(記録ルールは [APIDesign](APIDesign.md))
- **同じテーマは1か所で詳述**し、ほかの区分からはリンクします。コンセプトの説明を「〜するには」で繰り返したり、シグネチャをコンセプトに並べたりしません

コンセプトとユーザー向けの境界は、見出しの形で見分けられます。コンセプトの見出しは名詞(何であり、どう振る舞うか)、ユーザー向けの見出しは「〜する」(達成したい目的)になります。

| トピック | コンセプト | ユーザー向け(〜するには) |
|---|---|---|
| Widget | Widget は設計図で、作り直しても安い(不変の `record`) | 項目を縦に並べて表示する |
| Widget | 組み込み Widget の種類の地図(レイアウト・描画・入力)。一覧は API Reference へリンクする | 角を丸めて影を付ける |
| State | `SetState` を呼ぶと、その Widget から下が作り直される | チェックボックスの状態を持つ |
| State | `InheritedWidget` は上から下へ値を配り、依存した子だけを作り直す | 画面全体の色をまとめて切り替える |
| Layout | 制約は親から子へ下り、サイズは子から親へ上がる | Row の中でテキストをはみ出させない |
| Layout | `Expanded` は余った幅を flex の比で分ける | 左右に寄せて間を空ける |
| Input | ポインタ入力はダッシュボードオーバーレイだけで使える(制約) | `WorldSpaceWindow` で操作したいときの回り道 ※要確認(回り道があるか) |
| Input | ヒットテストは前面の子から順に当たりを探す | ボタンの押せる範囲を広げる |
| Animation | `Ticker` がフレームごとに値を進め、`Curve` が進み方を変える | フェードインさせる |
| Window | オーバーレイの3種類の違い。物理的な大きさは Dpm で決まる | 34cm の幅で表示する([Issue #151](https://github.com/sumx21t-3310/FloatSoda/issues/151)) |
| Widget(色の節) | FloatSoda の `Color` と SkiaSharp の `SKColor` の関係([Issue #251](https://github.com/sumx21t-3310/FloatSoda/issues/251)) | 16進の色コードで色を指定する |
| Threading | `await` の続きはメインスレッドへ戻らない(制約。追跡: [#281](https://github.com/sumx21t-3310/FloatSoda/issues/281)) | バックグラウンドで取った結果を画面に出す |

### コンセプトとユーザー向けの範囲

コンセプトとユーザー向けは、公開 API および API Reference と合わせて、**#188 の層3(統合ミニアプリ型)のサンプルをジュニアコーダーテストで書ける範囲**を扱います。これが「どこまで書くか」の合格ラインです。テストで不足が判明した概念や制約は、コンセプト、ユーザー向け、API Reference のどれかの不足として回収します(手順は #188)。内部実装を知らないとコードを書けない箇所が見つかった場合、それはコンセプトかユーザー向けの不足を意味します。

### 再編の地図

#188 の再編(手順4)で目指すページ構成と、今のページの行き先です。再編が終わったら、この節は消します(追跡: [#188](https://github.com/sumx21t-3310/FloatSoda/issues/188))。サイトのサイドバーは `docs/` のディレクトリから作られるため、再編が終わったあとは、ディレクトリがページ構成の一次情報になります。

**サイドバーのグループ**

| グループ | 読者の目的 | ディレクトリ | ページ |
|---|---|---|---|
| 導入 | — | `docs/` 直下 | Home、TargetUsers |
| コンセプト | 知る | `docs/concepts/` | Overview(宣言的 UI の考え方、三つのツリーを利用者から見た姿、各トピックへの地図)、Widget、State、Layout、Input、Animation、Window、Threading。色は Widget の1節 |
| ユーザー向け | 使う | `docs/user/` | 作りながら学ぶ(GettingStarted、ミニアプリ)、Widget カタログ、〜するには |
| コントリビューター向け | 変える | `docs/contributor/` | 実験(`experiments/`)と、種別ごとのディレクトリ(`tutorials/`、`how-to/`、`reference/`、`explanation/`) |
| API Reference | 調べる | `docs/api/` | 入口だけ。本体は XML ドキュメントコメントから作る([#219](https://github.com/sumx21t-3310/FloatSoda/issues/219)) |

- ユーザー向けの小見出しに、Diátaxis の種別名は出さない
- Widget カタログは、層1サンプルの `samples/*/README.md` をサイトの生成時に取り込む。Wiki には載せない([#219](https://github.com/sumx21t-3310/FloatSoda/issues/219) の完了条件7)
- コントリビューター向けは、Diátaxis の種別でディレクトリを切り、書くことがあるトピックだけをページにする。中身のないページを先に置かない。Diátaxis 自身が、空の4分類の構造を先に作ることを避けるよう勧めているため([Workflow](https://diataxis.fr/how-to-use-diataxis/) の「Don't worry about structure」)- `docs/assets/logo-sketch.png` は `samples/FloatSoda.Samples.Image` の素材で、ドキュメントのページからは参照していない。サイトと Wiki は `.md` だけを扱うため、どちらにも出ない
- コントリビューター向けの Home は、種別ごとのページの一覧を入口にし、読む順番を「面白がる(実験)→ 貢献する(最初の PR の流れ)→ 調べる(仕組みと理由・守ること)→ 提案する」で示す

**コントリビューター向けのトピックの単位**

トピックは、次の部分ごとに立てます。部分は `src/` のプロジェクトと 1対1 にしません。書くことがない部分には、ページを置きません。

| 部分 | 入るプロジェクト・名前空間 |
|---|---|
| 全体 | なし(部分をまたぐ話。API 設計、ローカライズ、docs、テスト方針、リリース、入力の流れ) |
| プラットフォーム | `FloatSoda.Abstractions`、`FloatSoda.Engine`、`FloatSoda.OVR` |
| 描画 | `FloatSoda.Rendering` |
| コア | `FloatSoda` の `FloatSodaApp` と Generic Host まわり、`Core`、`Geometrics`、`Painting` |
| 三つのツリー | `FloatSoda` の `Widgets`、`Elements`、`RenderObjects`、`Gesture`。`FloatSoda.Hooks`、`FloatSoda.Testing` |
| アニメーション | `FloatSoda` の `Animation` |
| UI | `FloatSoda.UI`、`FloatSoda.UI.Cream`、`FloatSoda.UI.FizzyPop`。実装ができるまでページを置かない |

**種別ごとのページと、今のページの行き先**

「新」は、今のページになく新しく書いたものです。印のないものは、今のページの節から移します。

| ページ | 移す元 |
|---|---|
| [docs の小さな修正で PR を出す](tutorials/FirstPullRequest.md) | 新 |
| [Layer を1つ足して PR を出す](tutorials/ShaderMaskLayer.md) | 新(題材は [#154](https://github.com/sumx21t-3310/FloatSoda/issues/154)) |
| [OpenVR の API を1つ包んで PR を出す](tutorials/OverlayPreCurvePitch.md) | 新(題材は [#103](https://github.com/sumx21t-3310/FloatSoda/issues/103)) |
| [作業の手順](how-to/Home.md)(入口) | Issue を書く・ブランチを切る(CONTRIBUTING.md へリンク)、[DocumentationComments](DocumentationComments.md)、このページ、[TestStrategy](TestStrategy.md)、ADR を書く(置き場所は未定)、実機で確かめる、リリースする(RELEASING.md へリンク) |
| [OpenVR の API を包む](how-to/WrapOpenVRApi.md) | 新(例外を足す手順を含む) |
| [Layer を足す](how-to/AddLayer.md) | 新(描画結果をビットマップで確かめる手順を含む) |
| [ジオメトリの値型を足す](how-to/AddValueType.md) | 新 |
| [Flutter のウィジェットを移植する](how-to/PortWidget.md) | 新、[RenderObjects](RenderObjects.md) の「カスタムクリッパーの実装」、新 ヘッドレスでテストする |
| [アニメーションの部品を移植する](how-to/PortAnimation.md) | 新 |
| [守ること](reference/Home.md)(入口) | [Requirements](Requirements.md)(REVIEW.md の 4 章へリンク)、[APIDesign](APIDesign.md) の 2 章、Flutter との差異の台帳 |
| [プラットフォームで守ること](reference/Platform.md) | [OVRIntegration](OVRIntegration.md) の「例外体系」「オーバーレイプロパティ」のうち `internal` の契約 |
| [描画で守ること](reference/Rendering.md) | REVIEW.md の「Layer」の不変条件 |
| [コアで守ること](reference/Core.md) | `RenderPipeline` の契約、APIDesign の値型の規約 |
| [三つのツリーで守ること](reference/Trees.md) | REVIEW.md の「ツリーの所有権とライフサイクル」「差分更新」、RenderObjects の「基本契約」 |
| [アニメーションで守ること](reference/Animation.md) | 新 |
| [全体の仕組みと理由](explanation/Overall.md) | [Architecture](Architecture.md)(アセンブリ構成、フレームパイプライン、スレッドモデル)、APIDesign の 1 章、[Localization](Localization.md)、新 入力の流れ |
| [プラットフォームの仕組みと理由](explanation/Platform.md) | OVRIntegration(初期化、オーバーレイの種類、VREventDispatcher)、Architecture のレンダースレッドの部分、[Input](Input.md) の実装の部分、新 テクスチャ転送と FramePacer |
| [描画の仕組みと理由](explanation/Rendering.md) | Architecture のレイヤーツリーの部分、RenderObjects の「PaintingContext とレイヤーツリー」、新 Clone して渡す理由 |
| [コアの仕組みと理由](explanation/Core.md) | Architecture のフレームパイプラインのうち `WidgetBinding`・`RenderPipeline` の部分、新 アプリの起動と Generic Host |
| [三つのツリーの仕組みと理由](explanation/Trees.md) | [WidgetSystem](WidgetSystem.md)、[BuildPipeline](BuildPipeline.md)、RenderObjects の「制約フロー」「差分更新」「intrinsic 測定」、新 ジェスチャとヒットテスト、Hooks |
| [アニメーションの仕組みと理由](explanation/Animation.md) | [Animation](Animation.md)、新 `Ticker` とフレームの関係 |
| (UI のページはまだない) | [UILayering](UILayering.md)。UI の実装ができたら、守ることと仕組みと理由に分けて移す |

コントリビューター向け以外へ移るものは、次のとおりです。

| 今のページ・節 | 行き先 |
|---|---|
| [TargetUsers](../TargetUsers.md) | 導入(`docs/` 直下に残す) |
| [GettingStarted](GettingStarted.md) | ユーザー向けの「作りながら学ぶ」(利用者向けに書き直す) |
| WidgetSystem の組み込みウィジェット一覧 | コンセプトの Widget(種類の地図)と Widget カタログ |
| WidgetSystem の「押せるボタンを作る」 | ユーザー向けの「〜するには」 |
| RenderObjects の制約モデル | コンセプトの Layout |
| Animation の使い方 | コンセプトの Animation と、ユーザー向けの「〜するには」 |
| OVRIntegration のオーバーレイ種別と制約 | コンセプトの Window |
| Input | コンセプトの Input と、ユーザー向けの「〜するには」 |

**実験の候補**

書き方は [5 章の「実験」](#実験)にあります。どの候補も、動かせるかは ※要確認 です。プラットフォームの実験は HMD が要る見込みで、`--desktop` の表示で代わりになるかも ※要確認 です。

| 部分 | 問い | やること | 見えるもの / 壊して直す一手 |
|---|---|---|---|
| プラットフォーム | SteamVR へのテクスチャ転送はどうなっているの? | フレームごとに色が変わる絵を出し、`FramePacer` の目標フレームレートを下げる | オーバーレイがカクつく。戻すとなめらかになる |
| プラットフォーム | オーバーレイの大きさは何で決まるの? | 同じ 340px の絵を、Dpm を変えて出す | 34cm の幅になったり、手のひらほどになったりする |
| 描画 | 半透明の子が重なると、どう混ざるの? | 半透明の四角を2枚重ね、`OpacityLayer` を親にかける場合と子ごとにかける場合を比べる | 重なった部分の色が変わる |
| 描画 | なぜ Layer ツリーは Clone してからレンダースレッドへ渡すの? | 渡したあとで元のツリーを書き換える | Clone を外すと絵が崩れる |
| フレームワーク | 親はどうやって子の大きさと位置を決めるの? | 子を円に並べる RenderObject を書く | 時計の文字盤ができる。制約を渡し忘れると、子が消えるか伸びきる |
| フレームワーク | 1フレームで、どこが描き直されているの? | 描き直された範囲に色を付ける | `RepaintBoundary` を足すと、色の付く範囲が縮む |
| フレームワーク | 何も変わらないフレームは何をしているの? | `Paint` が呼ばれた回数を数える | 変化がないと 0 回。`SetState` を1回呼ぶと、そこだけ増える |
| フレームワーク | `Key` をつけると何が変わるの? | 状態を持つ子のリストを並べ替える | `Key` がないと、状態が別の子に付いていく。`Key` をつけると直る |
| フレームワーク | `InheritedWidget` を変えると、誰が作り直されるの? | 各 Widget の `Build` の回数を数える | 依存している子だけ回数が増える |

## 2. 情報ごとの置き場所

同じ情報は1箇所のみに書き、ほかの場所からはリンクします。

| 情報 | 置き場所 |
|---|---|
| API のシグネチャ・引数・戻り値・例外・nullability・副作用・スレッド制約 | XML ドキュメントコメント(規約は [DocumentationComments](DocumentationComments.md)) |
| API の具体的な使い方 | ユーザー向けの「〜するには」 |
| 概念モデル(Widget、State、Layout、Input、Animation、Window、Threading) | コンセプト |
| 完成物を作る手順 | ユーザー向けの「作りながら学ぶ」とサンプル(`samples/`) |
| ウィジェットの最小の使用例 | サンプルの README。サイトの Widget カタログに取り込む(構成は [CONTRIBUTING.md](../../CONTRIBUTING.md) の「サンプルを追加する場合の規約」) |
| 内部の動作(ツリー、Build / Layout / Paint、入力、スレッド) | コントリビューター向けの「仕組みと理由」(`explanation/`)の、該当するページ |
| 設計理由と採用しなかった選択肢 | コントリビューター向けの「仕組みと理由」(`explanation/`)の、該当するページの理由の部分 |
| invariant、observable behavior、ライフサイクル・所有権・スレッドの必須条件 | コントリビューター向けの「守ること」(`reference/`)の、該当するページ。ただし、ツリーの不変条件(所有権・ライフサイクル・差分更新・Layer)の置き場所は [REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」([AGENTS.md](../../AGENTS.md) の規約表) |
| API 設計の原則と Flutter parity / divergence の判断基準 | [APIDesign](APIDesign.md) |
| 確認済みの Flutter との差異 | [`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) |
| 開発・レビュー・リリースの手順 | [CONTRIBUTING.md](../../CONTRIBUTING.md) / [REVIEW.md](../../REVIEW.md) / [RELEASING.md](../../RELEASING.md) |
| ドキュメントの分類とサンプルの3層構成 | [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) |

XML ドキュメントコメントは `internal` や `private` にも書きます([DocumentationComments](DocumentationComments.md) の適用範囲)。公開サイトの API Reference に載せるのは `public` API のみであり、`internal` 側のコメントはコントリビュータ向けの内部資料として扱います。

## 3. Documentation Impact Matrix

変更の種別ごとに、更新を検討すべき文書の一覧です。**変更によって置き場所の文書の説明が変わる場合のみ更新します。** 全項目を機械的に更新する必要はありません。

| 変更 | 更新対象 |
|---|---|
| public API を追加・変更する | XML ドキュメントコメント。利用者の使い方が増えるなら、ユーザー向けの「〜するには」と Widget カタログのサンプル |
| observable behavior を変更する | コンセプトとテスト。API 契約に影響するなら XML ドキュメントコメント |
| 公開制約・既知の制限を変更する | コンセプト。必要なら XML ドキュメントコメント |
| 内部アルゴリズムだけを変更する | コントリビューター向けの Explanation の説明が変わる場合のみ |
| アーキテクチャの境界を変更する | コントリビューター向けの Explanation と Reference |
| 設計判断を変更する | コントリビューター向けの Explanation(理由の部分)。必要なら Reference と [APIDesign](APIDesign.md) |
| invariant を追加・変更する | コントリビューター向けの Reference とテスト |
| Flutter との差異を導入・変更する | [`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) と divergence テスト。利用者から観測できるなら、コンセプトとサンプルの「Flutterとの違い」節 |
| 新しい代表ユースケースを追加する | ユーザー向けの「〜するには」/「作りながら学ぶ」と、#188 の層に沿ったサンプル |
| サンプルを追加・変更する | サンプルの README と `checks/items.jsonl`([CONTRIBUTING.md](../../CONTRIBUTING.md) の規約) |
| ドキュメントのページを移動・改名する | リンク元のページと [Home](../Home.md) の「ページ一覧」表(公開サイトの検査がリンク切れを検出します) |

PR では、テンプレートの `## Documentation` 節に、変更種別と更新した文書(更新が不要と判断した場合はその理由)を記載します。レビューでは、その判断がこの表と一致しているかを確認します。

## 4. コンセプトとユーザー向けの書くもの / 書かないものとテンプレート

コンセプトとユーザー向けは新しく書くページが多いため、先に骨格を定めます。ページの目的に不要な節は省きます。テンプレートの名前(Concept、Guide、Tutorial)は書き手向けの呼び名で、サイトの見出しには「コンセプト」「〜するには」「作りながら学ぶ」と出します。

読者は [TargetUsers](../TargetUsers.md) で定義する3タイプです。**コードを書くのも読むのも LLM である**という前提を忘れず、「LLM がこの API を誤用しない」書き方を優先します。Unity の知識のみを持つ読者(Booth クリエイター)には、Unity や uGUI の語彙による読み替えを添えます。

種別ごとに、書くものと書かないものを分けます。

| 種別 | 書くもの | 書かないもの |
|---|---|---|
| Concept(コンセプト) | 概念の目的、利用者から見たモデル、ほかの概念との関係、利用に必要な最低限の仕組み、最小例、観測できる制約 | `private` / `internal` の実装、内部アルゴリズム、設計判断の経緯、API メンバーの一覧 |
| Widget カタログ(ユーザー向け) | 1つのウィジェット群の紹介と最小の使用例。サンプルの README として書く | 手で書いたウィジェットの一覧(サンプルの README から取り込む)、API メンバーの一覧 |
| Guide(ユーザー向けの「〜するには」) | 「〜するには」への答え。目的に必要な手順と API の組み合わせ | コンセプトの説明のやり直し、シグネチャや引数の契約(API Reference へ) |
| Tutorial(ユーザー向けの「作りながら学ぶ」) | 具体的な完成物を作る一本道の学習体験 | API の網羅 |

Widget カタログのテンプレートは、[CONTRIBUTING.md](../../CONTRIBUTING.md) の「サンプルを追加する場合の規約」にある README の構成です。サンプルは #188 の3層と対応します。

| 種類 | 対応するサンプル |
|---|---|
| Widget カタログ(コンセプトの最小例からもリンクする) | 層1: Widget カタログ型(1サンプル = 1ウィジェット群の最小デモ) |
| Guide | 層2: Cookbook 型(複数の Widget / API を組み合わせる) |
| Tutorial | 層3: 統合ミニアプリ型(公開 API だけで本番に近い形を作る) |

### Concept

```text
# 名前
## 概要                        — 何であり、何を解決するか
## いつ使うか
## 基本モデル                  — 利用者から見た仕組み。内部実装は書かない
## 最小例                      — 対応する Widget カタログのページへのリンクと、必要なら最小の断片
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

サンプルコードは `samples/` をマスターとし、本文には必要最小限の断片のみを載せます。ドキュメント用に完全な実装を別途記述することはしません。

## 5. コントリビューター向けの書くもの / 書かないものとテンプレート

コントリビューター向けは、Tutorial / How-to / Reference / Explanation の4種別のディレクトリに、書くことがあるトピックだけをページとして置き、動作原理を確かめる実験を別に置きます(種別とトピックの決め方は [1 章](#1-どこに書くか))。この章の Architecture / Design / Requirements / Development のテンプレートは、次のとおり4種別に対応します。

| テンプレート | 種別 |
|---|---|
| [Architecture](#architecture) | Explanation(いまどう動くか) |
| [Design](#design) | Explanation(なぜそう設計したか) |
| [Requirements](#requirements) | Reference |
| [Development](#development) | How-to |
| [貢献までの流れ](#貢献までの流れ) | Tutorial |
| [実験](#実験) | 実験(4種別の外に置く) |

コントリビューター向けは、現行の `docs/` のページを `contributor/` へ移して作ります。ここに書く「書くもの / 書かないもの」とテンプレートは、[再編の地図](#再編の地図)で行き先が決まっている現行ページと、[REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」、[`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) のエントリから逆算したものです。種別ごとに、いちばん近い実物のページを例として挙げます。新しいページを書くときも、既存のページを移すときも、その例に寄せてください。

現行ページには複数の種別が混ざっています。例えば [RenderObjects](RenderObjects.md) の「差分更新」節は、伝播の仕組み(Architecture)と「プロパティを変更したら `MarkNeedsLayout()` / `MarkNeedsPaint()` を呼ぶ」という契約(Requirements)が同じ節にあります。再編では、混ざっている節を種別ごとに分けるか、別種別のページへリンクします。

### Architecture

いま**どう動いているか**を書きます。読者は、そのコンポーネントを変更する前に境界と流れを把握したいコントリビュータです。

書くもの:

- **責務と境界** — 何を担い、何を担わないか(例: [Architecture](Architecture.md) の「アセンブリ構成」表)
- **処理の流れ** — フレーム、イベント、データが通る順序。図を使ってよい(例: Architecture の「レンダリングライフサイクル」、[BuildPipeline](BuildPipeline.md) の「全体の流れ」)
- **所有権とライフサイクル** — 誰が何を持ち、いつ作られ、いつ捨てられるか。スレッドごとの所有物と通信方法(例: Architecture の「スレッドモデル」表)
- **現在の実装の要点** — ソースのパスと型・メソッド名を添える(例: BuildPipeline の「BuildOwner と dirty list」)
- **実装状況と未実装の領域** — Alpha の間は冒頭に置く。「動かないのは未実装だからか、バグか」を読者が切り分けるために必要となる(例: BuildPipeline の冒頭ブロックと「未実装の領域」表)
- **テストでの駆動** — その仕組みを実時間や実機なしで動かす方法(例: [Animation](Animation.md) の「テストでの駆動」)
- **Flutter との対応** — どこまで Flutter と同じ戦略か。差異は台帳のエントリ番号を指す

書かないもの:

- **なぜその設計にしたか** → Design。「Flutter と同じ戦略」と一言添えるのはよいが、比較した選択肢と理由は Design に置く(例: RenderObjects の Semantics の注記は、理由を書かずに APIDesign へリンクしている)
- **変更後も守る条件の列挙** → Requirements。Architecture には現在の仕組みを書き、満たすべき条件は Requirements に書く
- **利用者向けの使い方と注意** → コンセプトかユーザー向け。「intrinsic 測定はコストに注意」のような利用者向けの注意は、再編でコンセプトへ抜き出す
- **API のシグネチャと引数の契約** → XML ドキュメントコメント。組み込みの一覧表は API Reference と重なるため、再編で整理する

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
- **採用した方針と、判定可能な手順** — 「〜が自然」のような主観に流れないよう、手順や判定基準を添える(例: [APIDesign](APIDesign.md) の「.NET が標準で提供する機構を再実装しない」の3ステップ、UILayering の「2つ目のデザインシステムが、1つ目のコードをコピーせずに同じコンポーネントを作れるか」)
- **理由と、理由にならないもの** — FloatSoda 固有の事情(Flutter / .NET / VR オーバーレイ / [TargetUsers](../TargetUsers.md))に根拠を置く(例: APIDesign の「理由にならないもの」)
- **採用しなかった選択肢と、その理由**(例: [Localization](Localization.md) の「ニュートラル = 英語が定石だが、主客層と Booth 流通を優先した」)
- **影響範囲と変更手続き** — この判断が効く範囲と、覆すときの手続き(例: Localization の「変更する場合は必ず issue で議論」)
- **「コントリビュータへ」の注意書き** — 知らないと直したくなる判断には、意図的であることを明記する(例: Localization の 1 章末尾)

書かないもの:

- **現在の処理の流れの詳細** → Architecture
- **Flutter との個別の差異** → [`known-divergences.md`](../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md)。Design は判断原則だけを持つ([APIDesign](APIDesign.md) と台帳の分担と同じ)
- **手順** → Development
- **議論の経過** — 決めたことと理由だけを書く。経過は Issue に残す

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

実装を変えても**維持する条件**を書きます。読者は、変更が何を壊しうるかを確認するコントリビュータとレビュアーです。

Requirements は、「守ること」(`reference/`)のトピックごとのページに書きます。今は [Requirements](Requirements.md) の骨組みだけがあります。いちばん近い実物は、[REVIEW.md](../../REVIEW.md) の「4. FloatSoda 固有の不変条件」、[APIDesign](APIDesign.md) の「差異が必要な場合の記録義務」、台帳のエントリ(`Test` 欄で検証方法と対になる)です。テンプレートはこれらの形から逆算したものです。ツリーの不変条件(所有権・ライフサイクル・差分更新・Layer)の置き場所は REVIEW.md の 4 章のままです([AGENTS.md](../../AGENTS.md) の規約表)。再編で Requirements のページを作るときは、4 章を複製せずにリンクします。4 章に無い条件(observable behavior、failure mode、検証方法、Flutter との関係)は Requirements 側に書きます。

書くもの:

- **適用範囲** — どのツリー、型、操作に効くか
- **必須条件と不変条件** — 「常に成り立つ」条件の文で書く。REVIEW.md の 4 章にある確認の問い(「drop 後に `Parent` が `null` に戻るか」)を裏返した形にする。4 章にある条件は複製せずにリンクする
- **Observable behavior** — 利用者とテストから観測できる契約(例: BuildPipeline の「Widget にも RenderObject にも変更がないフレームでは、レイアウト・ペイント・合成のすべてがスキップされます」は、Requirements へ移せる observable behavior)
- **破ったときの failure mode** — どの入力・状態で、何が壊れるか。REVIEW.md の finding の基準(concrete failure mode)と同じ粒度で書く(例: REVIEW.md の「Layer の clone 後に可変オブジェクトを共有するとデータレースになり、テストではまず落ちない」)
- **検証方法** — 条件を固定しているテストのファイルとメソッド名。無ければ「未設定」と明示する。台帳の `Test: — (not set)` と同じく、未設定は完了した記録ではなく未処理のタスクとして扱う
- **Flutter との関係** — parity か、台帳のどのエントリか

書かないもの:

- **なぜその条件が要るか** → Design。一文の理由は添えてもよいが、比較と経緯は Design に置く
- **現在の実装がどう満たしているか** → Architecture。Requirements は実装が変わっても残る条件だけを書く
- **実装詳細に依存する条件** — `private` フィールドの状態や呼び出し回数。テストと同じく observable behavior で書く([CONTRIBUTING.md](../../CONTRIBUTING.md) の「テスト観点」)
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

特定の作業の**手順と規約**を書きます。読者は、その作業にこれから取りかかるコントリビュータと Coding Agent です。

開発・レビュー・リリースの規約は [CONTRIBUTING.md](../../CONTRIBUTING.md) / [REVIEW.md](../../REVIEW.md) / [RELEASING.md](../../RELEASING.md) に定められており、`docs/` 側の Development ページはそれを置き換えません。`docs/` に置くのは、特定の作業に絞った規約と手順です(例: [DocumentationComments](DocumentationComments.md)、[TestStrategy](TestStrategy.md)、このページ)。

書くもの:

- **冒頭に、このページの責務と、隣接する規約の置き場所の表** — 読者が「この規約の置き場所はここか」を最初に確かめられるようにする(例: CONTRIBUTING.md と REVIEW.md の冒頭、TestStrategy の冒頭段落)
- **規約の強度** — 必須(MUST)と推奨(SHOULD)を区別する
- **手順** — 順番どおりに実行できる形(例: [Localization](Localization.md) の「メッセージを追加する手順」)
- **規約が守られる場所** — CI、PR テンプレート、lint、`npm run verify` など、破ると検出される仕組み。無ければ、レビューで確認する項目として書く(例: [3 章](#3-documentation-impact-matrix)の Impact Matrix と PR テンプレートの `## Documentation` 節)
- **日付付きの現状** — 件数や配置のような変わる情報は、日付を添えて更新の責任を明示する(例: TestStrategy の「現在の配置(2026-09-09 時点)」)

書かないもの:

- **置き場所が別にある規約の再掲** — リンクに留める。二重に書くと、片方だけが更新されて食い違うためである
- **仕組みの説明** → Architecture、**理由** → Design。理由は一文で添え、詳細はリンクする

```text
# 名前
冒頭: このページの責務と、隣接する規約の置き場所の表
## 規約               — 必須 / 推奨を明示
## 手順               — 順番どおりに実行できる形
## 規約が守られる場所 — CI、テンプレート、lint、verify
## 関連
```

### 貢献までの流れ

題材を1つ決め、**PR を出すまで**の一本道を書きます。種別は Tutorial です。読者は、その部分を初めて触り、手を動かしながら貢献のしかたを覚えたいコントリビュータです。

書くもの:

- **小さな題材** — 1つの PR で終わる大きさにする。題材は、実在する Issue から選ぶ。部分ごとにそろえるために、取り込む予定のない題材を作らない
- **手順** — 読者が上から順に実行すると PR が出せる形にする
- **規約へのリンク** — ブランチ名、テスト観点、PR の書き方は [CONTRIBUTING.md](../../CONTRIBUTING.md) の該当する節へリンクする

書かないもの:

- **規約の再掲** — CONTRIBUTING.md の本文を写さない。写すと、片方だけが更新されて食い違う
- **仕組みの説明** → 該当する「仕組みと理由」のページ。手順の途中では一文で触れてリンクする

```text
# 〜して PR を出す
## 作るもの
## 前提
## Step 1 …
## Step 2 …
## PR を出す   — CONTRIBUTING.md の該当する節へリンク
## 次に読むもの
```

### 実験

FloatSoda の**動作原理の問い**を、動かして確かめます。読者は、FloatSoda の内部に興味を持ち始めた人です。実験の目的は、コントリビュータになってもらう入口として、面白いと思ってもらうことです。

書くもの:

- **問いのタイトル** — 「〜はどうなっているの?」のように、動作原理の問いにする(例: 「SteamVR へのテクスチャ転送はどうなっているの?」)
- **目に見える結果** — どの実験も、目に見える絵か数で終える。最初の絵が見えるまでのコードは50行以内を目安にする(数値は仮の値で、書きながら見直す)
- **壊して直す一手** — わざと壊すと結果が変わり、直すと戻る操作を1つ入れる
- **なぜそうなるか** — 今見た結果の理由を1〜2段落で書き、仕組みの本体は該当する「仕組みと理由」のページの節へリンクする

書かないもの:

- **仕組みの本体** → 該当する「仕組みと理由」のページ。同じ説明を実験と「仕組みと理由」の2か所に書かない
- **ほかの実験に依存する手順** — 実験は1つずつ独立させ、どの実験からでも始められるようにする

```text
# 〜はどうなっているの?
## 試すこと       — 動かすコード。最初の絵まで50行以内を目安
## 見えるもの
## 壊して直す
## なぜそうなるか — 1〜2段落。続きは「仕組みと理由」のページへリンク
## 関連
```

## 6. 文体

FloatSoda の文書は日本語で書き、散文と箇条書きを使い分けます。この節では、`docs/` 配下、サンプルの README、Issue と PR の本文に共通する文体を定めます。XML ドキュメントコメントと例外メッセージの文体については、それぞれの規約を参照してください。

| 対象 | 文体の規約の置き場所 |
|---|---|
| `docs/` 配下、サンプルの README、Issue と PR の本文 | この節 |
| XML ドキュメントコメント | [DocumentationComments](DocumentationComments.md)(敬体、日本語のみ) |
| 例外メッセージ | [Localization](Localization.md) の「メッセージの文体」 |
| `AGENTS.md` | 英語。[CONTRIBUTING.md](../../CONTRIBUTING.md) の「エージェント向けファイルの言語」 |

### 規約

必須は「〜こと」、推奨は「〜が望ましい」で書き分けます。

- 散文は敬体で書くこと
- 箇条書きと表のセルは常体で書くこと。箇条書きは Markdown のリスト項目を指し、1項目を1文または体言止めにすること。2文以上になる場合は散文へ移すこと
- 条件と制約は箇条書きで示し、必須は「〜こと」、推奨は「〜が望ましい」で終えること
- 規約は、読者に求める行動として書くこと。禁止そのものが要点である場合のみ禁止形を使い、続けて代わりの行動を示すこと
- 語彙は読者の領域の語で書くこと。読者の領域は [TargetUsers](../TargetUsers.md) の3ペルソナが知る Unity、Flutter、.NET、VRChat の語とする。他分野の用語を比喩として借りる代わりに、そのまま説明する。定義どおりに使う用語(不変条件、冪等、直交)はそのまま使う
- 英語の慣用句は直訳せず、日本語の言い回しに置き換えること。技術用語(deploy、revert、worktree)は英語かカタカナのまま書く
- 手順は「〜する」の形とし、実行する人の視点で書くこと。作業の経緯や結果は、手順とは別の節に置くこと
- チェックリストの項目は、「はい」か「いいえ」で答えられる1つの事実にすること

### 言い換える語

次の語彙は、既存の文書から言い換えました（2026-09-15）。一覧にある語を新しく書く場合も、右の言い換えを使います。この一覧は [Issue #262](https://github.com/sumx21t-3310/FloatSoda/issues/262) で決定しました。機械による検査は [Issue #260](https://github.com/sumx21t-3310/FloatSoda/issues/260) で扱います。

| 語 | 言い換え | 備考 |
|---|---|---|
| 正典 | 一次情報、置き場所 | 宗教の語を比喩に借りた表現 |
| 担保する | 保証する | 法務の語を比喩に借りた表現 |
| Litmus test | 判定基準 | 化学の語を比喩に借りた表現 |
| 経路 | 読ませ先、手順、流れ | 「入力経路」のように信号の道筋を指す使い方は残す |
| 直交する | 無関係 | `Wrap` の交差軸のように幾何の定義どおりの使い方は残す |

### 規約が守られる場所

- 敬体と常体の分担、文の長さ、禁止語は textlint で検査する。設定は `website/` に置き、`npm run verify` から呼ぶ(未導入。導入は [Issue #260](https://github.com/sumx21t-3310/FloatSoda/issues/260))
- 必須と推奨の書き分け、視点、チェックリストの形式は、レビューで確認する
- Issue と PR の本文は、テンプレートの見出しに従って手順と経緯を分ける

## 関連

- [DocumentationComments](DocumentationComments.md) — XML ドキュメントコメントの規約
- [Localization](Localization.md) — 記述言語と例外メッセージの文体
- [APIDesign](APIDesign.md) — API 設計の原則と Flutter parity
- [TargetUsers](../TargetUsers.md) — 想定する3タイプの作り手
- [CONTRIBUTING.md](../../CONTRIBUTING.md) — 開発・PR・テスト・サンプルの規約
- [REVIEW.md](../../REVIEW.md) — レビューの判断基準と、ツリーの不変条件の置き場所(「4. FloatSoda 固有の不変条件」)
- [TestStrategy](TestStrategy.md) — テストの置き場所を決める地図
- [Issue #188](https://github.com/sumx21t-3310/FloatSoda/issues/188) — ドキュメント・サンプルの情報設計
- [Issue #219](https://github.com/sumx21t-3310/FloatSoda/issues/219) — 公開サイト(`docs/` の構造がそのままサイトの構造になります)
