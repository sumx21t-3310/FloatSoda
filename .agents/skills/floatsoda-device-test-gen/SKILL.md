---
name: floatsoda-device-test-gen
description: >-
  FloatSoda のデバイステストを生成する — SteamVR が実際に稼働しているときにしか壊れえないシナリオと、
  鏡写しにしている Flutter 移植からの挙動差異を Codex に網羅的に列挙させ、1件ずつヘッドレス
  xunit か HMD 実機ハーネスへ振り分け、そのテストとハーネスのシナリオを書く。このスキルはテストを
  作る側で、HMD は被らない — 実機での実行とトリアージは floatsoda-device-test-run が担う。
  VR 専用や移植差異のシナリオを洗い出したい・テストにしたいとき、「デバイステスト」
  「シナリオを洗い出したい」「Flutterとの挙動差」「移植差異」「device test」「ハーネスに
  シナリオを足して」に言及されたとき、いまの単体テストでは決して捕まえられない壊れ方を
  尋ねられたときに使う。列挙は Codex に委任する。
---

# FloatSoda デバイステスト — 生成(gen)

## このスキルが存在する理由

FloatSoda には 84 のテストファイルがあるが、`WidgetBinding` を一気通貫で駆動しているのは
そのうち **2つ**(`tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs` と
`tests/FloatSoda.Test/Core/WidgetBindingTest.cs`)だけ。`src/FloatSoda.OVR` と
`src/FloatSoda.Engine` は事実上テストされていない。SteamVR が稼働しているときにしか存在しない
もの — オーバーレイハンドル、コントローラーレイの座標、レンダースレッド上の GL コンテキスト、
ダッシュボードの開閉イベント — にはカバレッジが一切なく、`dotnet test` から得る手段もない。

それとは別に、FloatSoda は Flutter の三層ツリーモデルを鏡写しにしており、docs も Flutter の
語彙で概念を教えている。そのため利用者(と、利用者のために書く LLM)は Flutter の挙動を期待して
やって来る。**移植が乖離している箇所では、乖離がバグでも意図的な設計判断でも、利用者が払う
コストは同じ。** だから移植差異は、VR 専用の欠陥クラスと並べて列挙する価値のある、それ自体
独立した欠陥クラスになる。

このスキルは、この両方を場当たりな VR セッションではなく反復可能なプロセスにする。担当するのは
**生成(producer)側**: 列挙 → 検証 → 振り分け → テストとハーネスシナリオの作成。
**実行(runner)側** — ハーネスの各シナリオを HMD で起動し、オーナーの判定を取り、落ちたものを
トリアージする — は `floatsoda-device-test-run` で、カタログサンプルとハーネスシナリオを同じ
ループで回す。

## 以下のすべてを規定する経済性

デバイス実行はこのプロジェクトで最も高価なテスト。HMD を被り、SteamVR を起動し、手で操作し、
目で判定する。だから:

> **網羅性は列挙の段階で達成する。実行は意図的に絞る。**

この2つを同じサイズにしないこと。40 シナリオを列挙して 34 をヘッドレス xunit に、6 を HMD に
振り分けられたら、それは不足ではなく成功。

## オーケストレーションとハードストップ

Claude Code が主導する: Codex への brief の作成、**Codex の報告内容の検証**、振り分け、
ヘッドレステストの作成、ハーネスシナリオの構築。

次の2つはオーナーだけのもの:

1. **最終的な設計判断** — 軸B の暫定ラベルは、オーナーが確定するまで暫定のまま(確定は
   `floatsoda-device-test-run` のトリアージで行う)。
2. **コミット / push / タグ** — 必ず確認を取る。

VR での実行もオーナーだけのものだが、このスキルの範囲ではない。構築したシナリオを
`floatsoda-device-test-run` へ引き渡す。

## ワークフロー

### 1. 列挙する — Codex に委任、2つの軸で

brief には `references/codex-enumeration-prompt.md` を使う。`task-codex-subagent` スキル
(`subagent_type: "codex-runner"`)でバックグラウンド起動し、**読み取り専用の意図**で実行する:
Codex が書くのはちょうど1ファイル(スクラッチパッド内のシナリオリスト)だけで、リポジトリには
何も触れない。コードもテストも書かせない。

brief には必ず次を含める:

- **軸A — VR 専用**: オーバーレイのライフサイクル、コントローラーポインタの座標、デバイス
  トラッキング、SteamVR イベント、アクションマニフェスト、GL コンテキスト / レンダースレッド /
  フレームペーシング。
- **軸B — Flutter 移植差異**: `Elements/`、`RenderObjects/RenderObject.cs`、`Core/WidgetBinding.cs`、
  `Gesture/` を、`~/code_reading/flutter_reference` の Flutter クローンと突き合わせる。
  `references/known-divergences.md` を渡し、再発見ではなくリストの拡張をさせる。
- **既知 Issue の除外リスト**(後述)。起票済みバグの再発見に列挙の労力を使わせないため。
- **シナリオごとの必須フィールド**: 識別子(`--scenario` 引数に使える形)/ 壊れる理由の仮説 /
  `file:line` / `HEADLESS` か `VR` かとその根拠 / 再現手順 / 期待結果。軸B のエントリには
  対応する Flutter 本家実装のファイルパスも併記する。
- **ハルシネーション対策の条項**: すべての `file:line` は実際にファイルを読んだ結果であること。
  未確認のものはその旨を明記させる。

### 2. 検証し、それから振り分ける

**列挙結果を渡されたまま信用しない。** 引用された `file:line` を抜き取り検査し、シンボルが実在して
主張どおりに振る舞うことを確認する。行の読み違いの上に組まれたシナリオは HMD セッションを浪費する。

そのうえで全シナリオを振り分ける:

| 判定 | 行き先 | テスト |
|---|---|---|
| `HEADLESS` | `tests/FloatSoda.Test`(xunit) | Widget / Element / RenderObject / Layer の範囲で再現でき、xunit または `src/FloatSoda.Testing` のビットマップレンダラで観測できる |
| `VR` | `tests/FloatSoda.DeviceTest` | OpenVR ランタイム、GL コンテキスト、実デバイスのトラッキング、SteamVR のイベント配送のいずれかが必要 |

迷ったら `HEADLESS` へ。VR 専用と思われていたバグがヘッドレスで再現すると分かるのは勝ち — なので
VR で実際に落ちたシナリオも、根本原因を確定と呼ぶ前にヘッドレス再現を試みること(その試みは
`floatsoda-device-test-run` のトリアージで行う)。

軸B のシナリオにはさらに暫定ラベルを付ける: **deliberate(意図的な設計判断)/ not yet ported
(未移植)/ port mistake(移植ミス)**。ラベルが付くまでは何もアクションできない。「deliberate」で
確定した差異も、それで閉じたことにはならない — 利用者は依然その驚きの代金を払うので、
`docs/` のギャップになる。

### 3. ハーネスを構築する

場所: **`tests/FloatSoda.DeviceTest/`**、1プロジェクト。

このパスである具体的な理由:

- `tests/Directory.Build.props` が `IsPackable=false` と `GenerateDocumentationFile=false` を
  供給する。これが効く: ルートの `Directory.Build.props` は `IsPackable=false` を**設定しておらず**、
  `release.yml` は `artifacts/*.nupkg` を**すべて** NuGet に push する。独自の
  `Directory.Build.props` を持たない新しいトップレベルディレクトリにハーネスを置くと、次のタグで
  NuGet.org に公開されてしまう。
- CI はテストプロジェクトを明示パスで指名している(`dotnet test tests/FloatSoda.Rendering.Test`、
  `dotnet test tests/FloatSoda.Test`)ので、ここにあるものは CI に巻き込まれない。ハーネスを
  xunit を参照しないコンソール exe にしておけば、ルートでの `dotnet test` もこれをスキップする。

**1シナリオ = 1プロセス。ただし1プロジェクトではない。**

プロセス分離は現在の設計上強制される。理由は2つで、修正されている可能性があるため実行のたびに
再確認する価値がある:

- ウィンドウを破棄する API がない — `FloatSodaApp._bindings` は追加専用で、`WidgetBinding` は
  `IDisposable` ではなく、作られたオーバーレイはアプリ終了まで生き続ける(issue #218)。
  プロセス内でシナリオを切り替えると、前のシナリオのオーバーレイが画面に残る。
- `FloatSodaApp.MainLoop` は各 `catch` ブロックでループを `break` するため、1つのシナリオの例外が
  アプリ全体を終了させる。

だが `--scenario <name>` 引数を持つ単一プロジェクトなら、exe の再起動でその分離を得つつ、共有
ハーネス(シナリオ一覧、期待結果の表示、ロギング)の置き場所を1つに保てる。シナリオごとの
プロジェクト分割は共有ライブラリかコピペを強制し、何の得もなく `FloatSoda.slnx` を太らせる。

別プロジェクトに分割するのは、**プロジェクト構成そのものが異なる場合だけ**: 別の `AppKey` や
アクションマニフェスト、起動シーケンスの検証(DI なし、SteamVR 不在)など。

各シナリオは1クラスとし、`Name` / 目的 / 操作手順 / 期待結果 / `Build()` を持たせる。

### 4. ヘッドセットの中から判定できるシナリオにする

デバイスでの失敗は、操作者が「合格」の姿を忘れた瞬間に価値を失う。runner
(`floatsoda-device-test-run`)は各シナリオの期待結果を yes/no の質問として読み上げ、オーナーの
判定を記録するので、すべてのシナリオは runner に「言うこと」を渡せる形にする:

- **期待結果をオーバーレイ上に日本語で表示する。** HMD を被っているとコンソールは見えない。
- **`Name` / 操作手順 / 期待結果を、exe を起動せずに runner が読める形で公開する**
  (`--list` 引数か、プロジェクトの隣に生成した Markdown の一覧)。
- **ログをファイルに追記する** — シナリオ、タイムスタンプ、スタックトレース。クラッシュが
  データになる: どのシナリオが、どこまで進んだか。

### 5. 引き渡す

ヘッドレステスト(`HEADLESS` シナリオ → `tests/FloatSoda.Test`、`CONTRIBUTING.md` のテスト命名に
従う)とハーネスシナリオ(`VR` → `tests/FloatSoda.DeviceTest`)を書き、ビルドして、止まる。
そのうえで、どのシナリオがヘッドセットの準備できたかを実行順でオーナーに伝える。
実行、判定、失敗ごとのヘッドレス再現の試み、分類の提案、`references/known-divergences.md` への
追記は、すべて `floatsoda-device-test-run` で行う。

## 既知 Issue の除外リスト

起票済みとして Codex に渡し、再列挙させない。**実行のたびに再確認すること** — 直っていくものだから。
文脈として参照するのは構わないし、既知 Issue を再現するハーネスシナリオには依然価値がある。

- **#218** ウィンドウを破棄する API がない(`FloatSodaApp._bindings` が追加専用)
- **#216** `PostTaskRunner.Stop` が正常停止でもエラーログを出す
- **#191** `FocusEnter` の hover hit-test が古い座標を使う
- **#182** `ControllerPointerSystem` が非ダッシュボードオーバーレイに接続されていない —
  `WorldSpaceWindow` / `DeviceTrackedWindow` では `GestureDetector` が発火しない
- **#151** オーバーレイの物理サイズ(メートル)の発見性が低い
- **#150** バックグラウンドスレッドからの `SetState` が未対応
- **#147** `OVRApplication.Identify()` の失敗時ハンドリング
- **#140** SteamVR 不在起動・遅延接続・再接続
- **#90** オーバーレイ種別の実行時切り替えが未実装

このリストを丸ごと信用せず、`gh issue list --state open` で最新化すること。

## 関連

- **#141**(デスクトップ Storybook / OverlayViewer)が実現すると、いま `VR` のものの一部が
  `HEADLESS` へ移る。ハーネスのシナリオは、その移行を生き延びる粒度に保つこと。
- `floatsoda-junior-coder-test` は docs/API の品質をブラックボックスで測る。このスキルは
  ホワイトボックスで実装の正しさを測る。混ぜないこと: ジュニアモデルにバグシナリオのリストを
  渡すと、あちらのテストが測っているものが壊れる。
- `floatsoda-device-test-run` は、このスキルが作ったものすべての runner であり、確定した差異が
  `references/known-divergences.md` へ追記される場所でもある。

## 報告形式

振り分けの内訳(列挙総数のうち `HEADLESS` と `VR` が何件か)を最初に置く — どれだけの作業が
ヘッドセットを回避できたかを示す、これが見出しになる数字。続いて書いたヘッドレステスト、
次に `VR` シナリオを実行順に(`floatsoda-device-test-run` に渡せる状態で)、その後に軸B の
差異を暫定ラベル別にまとめ、それぞれ実ソースへの `file:line` リンクを添える。
どの `file:line` を自分で検証し、どれを検証していないかを率直に述べること。
