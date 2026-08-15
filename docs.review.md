# 添削ログ: docs/ + README.md + AGENTS.md

- 対象: `C:\Users\sumx21t\projects\libs\FloatSoda\docs\`(ルート)、`README.md`、`AGENTS.md`
- 対象ファイル: docs/Home.md / docs/TargetUsers.md / docs/GettingStarted.md / docs/Architecture.md / docs/WidgetSystem.md / docs/UILayering.md / docs/Animation.md / docs/BuildPipeline.md / docs/RenderObjects.md / docs/OVRIntegration.md / docs/Input.md / docs/APIDesign.md / docs/DocumentationComments.md / docs/Localization.md / README.md / AGENTS.md
- 本文の言語: 日本語(`AGENTS.md` のみ英語 — 既存文書の言語に従う)
- 対象領域 / 読者: SteamVR オーバーレイ開発 / 下記「読者の定義」の3ペルソナ + LLM
- 完成稿の想定文字数: 指定なし
- 改稿の範囲: 既存文書の一部を改稿(Step 1・2 は既存文書からの読み取り、Step 3〜10 を差分として実行)
- 現在の工程と状態: Step 10 / 完了
- 次に実行する操作: 完成。コミットはユーザーの判断待ち
- Step 3〜5 の周回数: 2周(上限に到達)
- 最後に処理した指摘: C-027
- 停止点の承認: Step 0 承認済 / Step 1 スキップ(一部改稿・矛盾なし) / Step 5(1周目)承認済 / Step 5(2周目)承認済 / Step 7 未
- 未対応: 0件 / 対応済: 24件 / 見送り: 3件 / 未確認: 0件

Step 6 の内訳: 重大1件(C-009)/ 中5件(C-010・C-011・C-013・C-014・C-015)/ 軽微6件(C-012・C-016〜C-020)。
Step 7 でこのうち11件を修正し、C-017 のみ見送った(理由は該当項目に記載)。

---

## Step 7(推敲)の結果

| 指摘 | 状態 | 実際の修正 |
|---|---|---|
| C-009 | 対応済 | 「一覧にあるものがすべて使用可能」→「一覧で `✓` が付いているものが使えます」。C-015 の組み替えと同時に実施 |
| C-015 | 対応済 | 冒頭の実装状況ブロックを4項目・最長300字超から、記号を行頭に置いた7行の箇条書きへ組み替え。ウィジェット名の数え上げは下の一覧へ委ねた |
| C-010 | 対応済 | 凡例の定義から型の可視性を外した。「`public` で、意図した動作をする」→「公開 API として使える」、「型は存在するが `internal`」→「公開 API からは使えない(型が `internal`、または未接続)」。これで「ポインタ入力源」「ヒットテスト」など型でない行にも当たる |
| C-011 | 対応済 | Phase 表の状況欄を `🚧 進行中` だけに戻し、内訳は残件表へ一本化 |
| C-012 | 対応済 | 残件表の後にあったマイルストーン誘導の1文を削除。導入の1文だけ残し、表の見出しを「残っている主な作業」として非網羅であることを示した |
| C-013 | 対応済 | `Container` の解説とサンプルを Layout 節の末尾(intrinsic の後)から先頭(`ConstrainedBox` 解説の前)へ移動 |
| C-014 | 対応済 | 「組み込みの2つはどちらも自分で宣言するため、通常は1で決着します」「2のルートは、勝利も辞退も宣言しない認識器を自作したときの保険です」を追加。例文から「同じく1のルートで」という冗長な言い回しも外した |
| C-016 | 対応済 | 「スクロール系(`ListView` など)」→ 3件を書き切り、画像・アイコンも型名を明示 |
| C-018 | 対応済 | 型分岐サンプル後の「種別を確かめたいだけなら上の型分岐を使ってください」を削除 |
| C-019 | 対応済 | 「公開 API から除外されている(`internal` の)未実装ウィジェットは」→「`internal` のため公開 API から使えないのは」。三重の修飾を1つに |
| C-020 | 対応済 | README の表から `Container` の括弧書きを外し、直後の「まだ使えないもの」ブロックへ移した。表は「実装済みの Widget」という見出しどおりの内容になった |
| C-017 | 見送り | 該当項目に理由を記載 |

Step 7 後の検証: ID アンカー残存0件、`docs/` の内部リンクとアンカーは全件解決。

---

## Step 8〜10 の結果

別エージェントによるレビューは行っていない(ウィジェットの実地検証は Phase 2 完了後に行う方針をユーザーから確認済み)。
読者ペルソナを自分で演じて査読した。査読前に、読者定義・見送り一覧(C-002 / C-008 / C-017)・
役割(査読者)・見なくてよい範囲(表記と一文の長さは Step 6 / Step 10 の担当)を確認している。

### Step 8(第三者レビュー)

| ID | 重大度 | 内容 | 対応 |
|---|---|---|---|
| C-021 | 重大 | 押せるボタンの作り方が、どのページにもなかった。`TargetUsers.md` で「FaceEmo パネルは現時点で可能」と約束し、`WidgetSystem.md` で「`GestureDetector` で自分で組み立ててください」と言いながら、実際の組み立て方を示していない。読者が取るべき行動に直接届かない | 対応済 |
| C-022 | 中 | `Home.md` の実装状況サマリに、テキスト表示と画像表示の行がなかった。最も基本的な機能が一覧から抜けており、「文字は出せるのか」を確かめられない | 対応済 |
| C-023 | 中 | 「ジェスチャとヒットテスト」節の小節順が読者の関心順と逆だった。`Home.md` の警告から飛んできた読者が、`RawGestureDetector` の内部事情を越えないと目的の答えに届かない | 対応済 |
| C-025 | 中 | サンプルが5プロジェクトあるのに、docs は `OverlayApp` しか案内していなかった。特に `PaintingSample` は **SteamVR も HMD も不要**でツリーを PNG 化できるのに、存在が知られていない | 対応済 |

C-021 の対応では新しいコードを発明せず、実在するサンプル
(`CounterWidget.cs` の `GestureDetector` + `SetState`、`PointerRegionDemo.cs` のホバー・押下・取り消し)を
一次情報として参照させた。あわせて **`GestureDetector` には `OnTapDown` がない**ため
押し下げ時の見た目変更には `RawGestureDetector` + `TapGestureRecognizer` が要る、という分岐も明記した。

C-023 は小節の移動にあたるため構成変更に相当する。順序は
「ポインタ入力が届く範囲 → ヒットテストの振る舞い → 押せるボタンを作る → デザインシステムの `Button` → 認識器を自分で組む」
とし、基本から応用の順に揃えた。

### Step 9(ファクトチェック)

| ID | 重大度 | 内容 | 対応 |
|---|---|---|---|
| C-024 | 致命 | `WidgetSystem.md` の `StatefulWidget` サンプルが `public record WatchState : State<WatchWidget>` と書いていた。**`record` は `record` 以外のクラスを継承できないためコンパイルが通らない。** 直後に「このサンプルの全体は `WatchWidget.cs` にあります」と書いてあるが、実物は `public class WatchState` | 対応済 |

C-024 は今回の改稿で入れた誤りではなく、既存ドキュメントに元からあったもの。
最も基本的なパターン(`StatefulWidget`)の例が動かないため、読者が最初につまずく箇所だった。
`docs/` 全体を `: State<` で検索し、該当は1件だけであることを確認済み。

一次情報にあたって検証した項目:

- Phase の進捗 — GitHub マイルストーン API(Phase 1 は 7 closed / 1 open、Phase 2 は 15 closed / 7 open)
- Phase 2 の残件 — オープン Issue #70 / #170 / #171 / #173 / #196 の実題名から採録
- RenderObject のプロパティ名 — ソースから抽出。`RenderPadding.Padding`(`Spacing` ではない)、
  `RenderOpacity.Opacity`(`Value` ではない)、`RenderSizedOverflowBox.RequestedSize`(`Size` ではない)、
  `RenderTransform.Transform`(`Matrix` ではない)、`RenderOffstage.Offstage`(`IsOffstage` ではない)を訂正
- `PaintingSample` が SteamVR に依存しないこと — `Host` も `OVRApplication` も参照が0件
- `GestureDetector` に `OnTapDown` がないこと — `GestureDetector.cs` の公開プロパティを確認
- ジェスチャの決着順 — `GestureArena.cs:114` の `Sweep` と、両認識器の `Resolve` 呼び出し位置

### Step 10(最終校閲)

機械的検査はすべて通過。

1. ID アンカー残存 — 0件
2. `docs/` の内部リンクとアンカー — 全件解決
3. 旧語彙(「スタブ」「WIP」)の残存 — 0件
4. docs / README / AGENTS が参照するサンプルパスの実在 — 全件存在
5. コード例に出る型名のソース上の実在 — 全件存在

---

## C-027 [S4] 対応済 / — UI3層構成を「予定」として書き直す(ユーザー指示)

**指示**: 「UIシステムは予定として書いてほしい」(2026-08-11)。

**背景となる事実**: `FloatSoda.UI` / `FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop` は
3プロジェクトとも `IsPackable=false` で、**NuGet パッケージとして存在しない**。
加えて `ButtonBase` が `GestureDetector` へ未配線のため、`InteractionState` の
`IsPressed` / `IsHovered` / `IsFocused` は常に `false` のまま。
利用者から見れば「型はあるが使えない」ではなく「まだ無い」が実態に近く、指示の framing と一致する。

**修正**: 全ページで `△ 部分実装` から `予定(Phase 5)` へ表記を変更し、
`docs/Home.md` の凡例に「予定 = 設計は決まっているが、使える形では提供していない」を追加した。

| ファイル | 変更 |
|---|---|
| docs/UILayering.md | 冒頭に「このページは実装ではなく設計方針です」の注意書き。3層表へ「提供状況」列を追加。新規節「実装状況」で NuGet 未配布と Phase 5 の Issue 番号(#102 / #78 / #100 / #38)を明示。本文の断定を「〜する計画です」「〜を採ります」へ。`ButtonBase` のコード例に「目指す姿であり、いまは押下に反応しません」を付記 |
| docs/Home.md | ページ一覧の UILayering 行を「設計方針であり未提供」・対象読者をコントリビュータのみへ。⚠️ を「UI3層構成はまだ提供していません」へ。実装状況サマリと凡例、リポジトリ構成表を更新 |
| docs/WidgetSystem.md | 冒頭ブロックに「**予定**」項目を追加。「デザインシステムの `Button` がまだ反応しない理由」を「用意された `Button` はまだありません」へ改題し、未配布である点を追加 |
| docs/GettingStarted.md | 実装状況ブロックを「`Button` などの UI コンポーネントはまだ提供していません」へ |
| docs/BuildPipeline.md | 「デザインシステムの `Button`」行を「UI3層構成」行へ置き換え |
| docs/Architecture.md | アセンブリ表の UI 3行へ「Phase 5 の予定で未提供」を追記 |
| README.md | 「まだ使えないもの」ブロックとドキュメント表を更新 |
| AGENTS.md | Project Structure の UI 3行と UI Layering Rules へ、コア層だけが出荷済みであることと `IsPackable=false` を明記。レイヤリング規約自体は「実装が入るときの配置を決める規則」として維持 |

**判断**: `Button` を「デザインシステム層へ移動しました」と書いていた既存表現も改めた。
移動先が使えない以上、読者にとっては移動の事実より「いま無い」ことのほうが重要である。

---

## C-026 [S10] 対応済 / 軽微 — 水平線が見出しの直前に置かれている(文書全体・既存の慣習)

**該当**: `docs/` 全ファイルと README.md。

**問題**: Step 10 の「区切り線と装飾を足しすぎない」に照らすと、`---` の直後に見出しが来る形は
見出し自身が作る区切りと役割が重複する。実測すると次のとおりで、**Input.md を除く全ファイルで
`---` の 100% が見出しの直前**にある。

| ファイル | `---` 総数 | うち見出し直前 |
|---|---|---|
| APIDesign.md | 15 | 15 |
| DocumentationComments.md | 12 | 12 |
| WidgetSystem.md | 10 | 10 |
| RenderObjects.md | 8 | 8 |
| Animation.md | 7 | 7 |
| BuildPipeline.md / Localization.md / OVRIntegration.md / README.md | 各 6 | 各 6 |
| GettingStarted.md / Home.md | 各 5 | 各 5 |
| Architecture.md | 4 | 4 |
| TargetUsers.md | 3 | 2 |
| UILayering.md | 1 | 1 |
| Input.md | 0 | 0 |

**当初の判断**: 実行者の判断では直さない(依頼の範囲外・未変更ファイルへ差分が及ぶ・慣習として一貫している)。

**ユーザーの指示(2026-08-11)**: 「---は削除してください」。指示に従い削除した。

**実施内容**: `docs/*.md` と `README.md` から **94本**を削除した。削除条件は次の3つをすべて満たすもの。

1. コードフェンスの外にある
2. **直前の行が空行**(直前が本文だと `---` は setext 見出しの下線になり、削除すると見出しが消える)
3. 空行を挟んだ次の非空行が ATX 見出し(`#` で始まる)

削除時は直後の空行も1つ落とし、空行が二重にならないようにした。

**残した1本**: `docs/TargetUsers.md` 末尾、`← [Home](Home.md)` の直前。
見出しがない場所で本文とナビゲーションを分ける区切りであり、Step 10 が
「水平線を使ってよい場合」として挙げているケースにあたる。

**ファイル別の削除数**: APIDesign 15 / DocumentationComments 12 / WidgetSystem 10 / RenderObjects 8 /
Animation 7 / BuildPipeline・Localization・OVRIntegration・README 各6 / GettingStarted・Home 各5 /
Architecture 4 / TargetUsers・UILayering 各2 / Input 0。
`AGENTS.md` は元から0本のため対象外。`docs.review.md`(このログ)は成果物ではなく、
かつ `---` を指摘の区切りとして使う書式が定められているため対象外とした。

**検証**:

- 改行コードは全ファイル CRLF のまま。BOM の増減なし(先頭バイトを実行前後で比較)
- 残存する `^---$` は上記の1本のみ
- 見出し直前の `---` は0本
- 表の区切り行(`|---|---|`)は無傷。`^---$` にマッチしないため影響を受けない
- 空行が3連続した箇所は0件
- 内部リンクとアンカーは全件解決
- `git diff --check` の空白エラーなし

ログの置き場所をリポジトリ直下にしたのは、`scripts/sync-docs-to-wiki.js` が `docs/` 配下の `.md` を
無条件に Wiki ページへ変換するためである。`docs/docs.review.md` を置くと Wiki に公開されてしまう。

---

## Step 0: 前提(確定済み)

| 確認事項 | 確定内容 |
|---|---|
| 対象ファイル | docs/ 全14ファイル + README.md + AGENTS.md(既存・複数) |
| 完成稿の想定文字数 | 指定なし(Step 4 の 1.5〜3倍規則と Step 7 の着地判定は行わない) |
| 添削ログの置き場所 | リポジトリ直下 `docs.review.md`。バージョン管理に含める |
| HTML コメントを書ける形式か | 可(すべて `.md`) |
| 本文の言語 | 日本語。`AGENTS.md` のみ英語 |

---

## Step 1: 読者の定義(既存文書から読み取り)

`docs/TargetUsers.md` と `AGENTS.md` の "Target Users" 節から読み取った。ユーザーへの確認は取っていない
(一部改稿であり、依頼内容と矛盾しないため)。

**対象領域** = SteamVR オーバーレイ開発。
**初学者** = C# は読めるが、宣言的 UI(Flutter の三ツリーモデル)も VR オーバーレイ開発も未経験。

| 項目 | 内容 |
|---|---|
| 読者が持っている前提知識 | C# の基本文法、object initializer、`record`。.NET のビルドと実行。SteamVR をユーザーとして使った経験 |
| 読者が文章を読む目的 | 「今この API で何が作れるか」を判断し、動くコードを書くこと |
| 読後に理解してほしいこと | どのウィジェットが今すぐ使えて、どれがまだ使えないか。使えないものは代わりに何を使うか |
| 読後に取ってほしい行動 | 使えるウィジェットだけを組み合わせて、コンパイルが通るオーバーレイを書く |
| 説明が必要な用語 | 三ツリー(Widget / Element / RenderObject)、制約(`BoxConstraints`)、ヒットテスト、ダッシュボードオーバーレイ |
| 省略してよい前提 | C# の言語機能そのもの、`dotnet` CLI の使い方、SteamVR の起動方法 |

**3ペルソナ**(`docs/TargetUsers.md`):

1. バイブコーディングする VRChatter — 実際の読み手は LLM。「LLM がこの API を誤用できない」ことが最優先
2. Booth でオーバーレイを売るクリエイター — Unity/uGUI の語彙は持つが宣言的 UI は未経験
3. uGUI を避けたいエンジニア — UI が全部 C# コードであることが価値

**この読者定義が重大度に与える影響**: 3ペルソナのうち1が「実際の読み手は LLM」であるため、
**実装状況の誤記は `致命` になる**。LLM は docs を一次情報として信頼し、そこに「使える」と
書いてあるウィジェットのコードを書く。逆に「未実装」と書いてあるものは提案しない。
どちらの向きの誤記も、読者(LLM 経由のユーザー)がコンパイルできないコードを受け取るか、
使えるはずの機能を使えないまま終わるかのどちらかに直結する。

---

## Step 2: ユビキタス言語(既存文書から読み取り + 今回定義)

### 実装状況を表す語(今回の改稿で最も重要)

既存文書は「未実装」「スタブ」「`internal`」「WIP」「✗」を混在させており、
同じ状態を別の語で呼んでいる箇所と、別の状態を同じ語で呼んでいる箇所が両方ある。
今回、次の4状態に統一する。

| 表記 | 意味 | 判定基準 |
|---|---|---|
| `✓ 実装済み` | `public` で、意図した動作をする | 型が `public` かつ `NotImplementedException` を投げない |
| `△ 部分実装` | `public` だが、機能の一部が未完成 | `public` だが TODO / 既知の欠落がある(例: `Container` に `Padding` がない) |
| `✗ 未実装(internal)` | 型は存在するが `internal` で、公開 API から見えない | 型が `internal` |
| `✗ 未着手` | 型そのものが存在しない | ソースに型がない |

**「スタブ」という語は使わない。** 既存文書ではこの語が `internal` な型(`ListView`)と
`public` で実装済みの型(`GestureDetector`)の両方に使われており、状態を識別していない。

**「WIP」も使わない。** 「部分実装」と重なるうえ、英略語で初学者の前提知識に入らない。
`FloatSoda.Hooks` は `△ 部分実装` に統一する。

### 概念の境界(混同されている箇所)

| 用語 | 意味 | 混同しやすい概念との違い |
|---|---|---|
| ヒットテスト | ポインタ座標から、そこにある RenderObject を特定する仕組み | 「ジェスチャ認識」とは別。ヒットテストは座標→対象、ジェスチャは対象上のイベント列→意味 |
| ジェスチャ | タップ・パンなど、ポインタイベント列を意味へ解釈する仕組み(`GestureArena` / `*GestureRecognizer`) | 上記 |
| ポインタ入力源 | ヒットテストへ渡す座標の供給元(`IRawPointerSource`) | ヒットテストとジェスチャは全オーバーレイで動くが、**入力源はダッシュボードオーバーレイにしか繋がっていない** |
| Phase | フレームワークの機能上の到達点 | NuGet のバージョン番号とは対応しない(既存文書に明記あり。維持する) |

**ヒットテスト / ジェスチャ / ポインタ入力源の3語を分けることが、今回の改稿の核心。**
既存文書は「ジェスチャ・ヒットテストは未実装」と一括りにしているが、実際には
前2つは実装済みで、3つ目だけが非ダッシュボードオーバーレイで未接続(Issue #182)である。

### 固有名詞・API 名(言い換えない)

`Widget` / `Element` / `RenderObject` / `Layer` / `BuildOwner` / `RenderPipeline` /
`WidgetBinding` / `BoxConstraints` / `PointerEvent` / `HitTestResult` / `GestureArena` /
`IRawPointerSource` / `DashboardOverlay` / 各ウィジェット名 / 各 `Render*` クラス名。

---

## Step 3: アウトライン差分

全面的な再構成は行わない。**既存の構成を維持し、事実が誤っている箇所と、
実装されたのに載っていない項目を差し込む。**

### docs/Home.md

- 「ロードマップ(Phase)」表 — Phase 1 と Phase 2 の状況を実態へ更新
- ⚠️ の警告文 — 「Phase 1 完了までユーザー操作は動作しません」を、実態(ダッシュボードでは動く / それ以外は入力源が未接続)へ書き換え
- 「実装状況サマリ」表 — ジェスチャ・ヒットテスト行を分割し、便利ウィジェット行を実態へ更新。Phase 2 で入った表示系ウィジェット群の行を追加
- リポジトリ構成表 — 変更なし

### docs/WidgetSystem.md

- 冒頭の「実装状況」ブロック — 3項目(実装済み / 未実装 / WIP)を Step 2 の4状態語彙へ統一。`Container` の記述の自己矛盾を解消
- Layout 表 — `Container` の行を `✗ internal スタブ` から `△ 部分実装` へ。`Container` の解説文を追加(`Padding` がまだないこと)
- Gesture 表 — 全面差し替え。`GestureDetector` / `RawGestureDetector` / `Listener` / `AbsorbPointer` / `IgnorePointer` / `PointerRegion` の6件を実装済みとして記載し、入力源の制約を注記
- Components 表 — `Icon` / `Image`(Components)が `internal` であることを明記(現在は表に無く、本文の記述だけ)
- 新規節「ジェスチャとヒットテスト」 — `GestureDetector` の使い方と、ダッシュボード以外では入力が届かない制約

### docs/RenderObjects.md

- 「組み込み RenderObject 一覧」— 最も乖離が大きい。Layout 6件 → 19件、Painting 6件 → 6件(内容更新)、Gesture 節を新設(4件)、Animation を Painting から分離
- `RenderSiftedBox` の行を削除(型が存在しない)
- 「MarkNeedsPaint と RepaintBoundary」— 「現在は `RenderView`」を `RenderRepaintBoundary` を含む記述へ
- 新規節「intrinsic 測定」— Phase 2 で入った基盤の説明(コスト特性を含む)

### docs/BuildPipeline.md

- 「未実装の領域」表 — 4行すべて実態へ更新。ウィジェット行の列挙を修正し、ジェスチャ行を分割

### docs/GettingStarted.md

- 「Widget の実装状況」引用ブロック — 列挙を実態へ。`Padding` / `Container` を未実装側から外す
- 「オーバーレイ種別の選び方」表 — 入力が届くのはダッシュボードだけである旨を追記

### docs/Architecture.md

- 「ツリー構造」節の導入文 — 「一部の便利ウィジェットはスタブのまま」を実態へ
- アセンブリ構成表 — `FloatSoda.Hooks` が抜けているので追加

### docs/UILayering.md

- 「境界基準」— 「`Container` 予定」を実装済みへ
- 「デザインシステム」表の「現状」行 — `Button` がまだ押下に反応しない理由を、ヒットテスト未実装ではなく `ButtonBase` の配線待ち(`ButtonBaseState` の TODO)へ訂正
- ロードマップ「0. ジェスチャ・ヒットテスト(前提条件)」— 前提が満たされたことを反映

### docs/TargetUsers.md

- 「作れるものの例」表 — Phase 表記を実態へ。「現在 Phase 1 を開発中」の前置きを更新

### README.md

- 「実装済みの Widget」3表 — Phase 2 で入ったウィジェットを追加
- 未実装スタブの引用行 — 実態へ
- 「開発ステータス」— Phase 表と ⚠️、チェックリストを実態へ

### AGENTS.md(英語)

- "Architecture: Three Trees" の Widget/Element 節 — stale な stub 列挙を実態へ
- "Project Structure" 表 — `src/FloatSoda.Hooks` を追加

### 変更しないファイル

`docs/Animation.md` / `docs/Input.md` / `docs/OVRIntegration.md` / `docs/APIDesign.md` /
`docs/DocumentationComments.md` / `docs/Localization.md` — 実装と照合したが乖離を確認できなかった。
Step 10 のユビキタス言語照合と機械的検査の対象には含める。

---

## Step 4 で実施した差分(参考)

| ファイル | 主な変更 |
|---|---|
| docs/Home.md | Phase 表(1・2 とも進行中へ)、⚠️ 警告の全面書き換え、実装状況サマリに記号の凡例を追加し17行→22行へ |
| docs/WidgetSystem.md | 冒頭ブロックを4状態語彙へ、`Container` 行を `△ 部分実装` へ+解説とサンプル追加、Gesture 表を6件へ差し替え、新規節「ジェスチャとヒットテスト」、Components 表に `internal` 2件を追記 |
| docs/RenderObjects.md | Layout 表 6件→19件、Painting 表に4件追加、Gesture 表(4件)と Animation 表を新設、`RenderSiftedBox` 削除、`RepaintBoundary` の記述を訂正、新規節「intrinsic 測定」 |
| docs/BuildPipeline.md | 「未実装の領域」表を4行→6行で全面差し替え |
| docs/GettingStarted.md | 実装状況ブロックを差し替え、オーバーレイ種別表に「ポインタ入力」列と解説を追加 |
| docs/Architecture.md | ツリー構造の導入文を訂正、アセンブリ表に `FloatSoda.Hooks` を追加 |
| docs/UILayering.md | `Container` を実装済みへ、`Button` が反応しない理由を訂正、ロードマップ 0 節を「充足済み」へ |
| docs/TargetUsers.md | 「作れるものの例」表の Phase 表記を実態へ |
| README.md | Widget 表を3表→4表(入力系を新設)へ拡充、未実装スタブの注記を差し替え、ドキュメント表を8件→14件へ、Phase 表と ⚠️ とチェックリストを実態へ |
| AGENTS.md | 三ツリー節の stale な stub 列挙を差し替え、Project Structure に `src/FloatSoda.Hooks` を追加、docs のページ列挙を補完し Wiki 同期の注意を追記 |

差分規模: 10ファイル、288行追加 / 54行削除。

Step 4 で解消した既存の不整合:

- `docs/WidgetSystem.md` の自己矛盾(冒頭は「`Container` は公開 API」、表は「`✗ internal` スタブ」)
- 「ジェスチャ・ヒットテストは未実装」という記述(4ファイル)— 実際は実装済み
- `docs/RenderObjects.md` の `RenderSiftedBox`(ソースに存在しない型)
- 「スタブ」「WIP」という状態を識別しない語彙(docs/ と README.md から全廃)

内部リンクは全件検証済み。リンク切れとアンカー切れはなし。

---

## 2周目(Step 3〜5)の反映履歴

ユーザーの指示: 「7件すべて埋める」「ただし C-002(`DesktopWindow`)は今回は書かない」。
この2つを合わせ、**C-002 を `見送り`、残り6件を `対応済`** として処理した。

追記した内容(すべて差分。既存の構成は維持):

| 指摘 | 追記先 | 内容 |
|---|---|---|
| C-001 | docs/WidgetSystem.md | 新規小節「表示・非表示の3つのウィジェットの使い分け」。`State` の保持とレイアウトコストの2軸で比較表+3行のサンプル |
| C-003 | docs/WidgetSystem.md | 新規小節「組み込みの `InheritedWidget`」内に `ServiceProvider` の解説とサンプル |
| C-004 | docs/WidgetSystem.md | 同小節に `WindowWidget.Of` の解説。`ScopeType` の固定と、派生型 `Of` が例外を投げる点も記載 |
| C-005 | docs/WidgetSystem.md | 新規小節「認識器を自分で組む(`RawGestureDetector`)」。組み込み認識器2件の表、`Gestures` のサンプル、生成と設定を分ける理由、`GestureArenaManager` の決着ルール2通り |
| C-006 | docs/WidgetSystem.md | 「ヒットテストの振る舞い」節にウィジェット別の既定値表を追加 |
| C-007 | docs/Home.md、README.md | Phase 1・2 の残件表を追加し、マイルストーンが一次情報である旨を明記 |

2周目の差分規模: WidgetSystem.md が +277行、全体で 453行追加 / 53行削除(1周目からの累計)。

2周目で新たに検出し、その場で修正した項目(いずれも1周目・2周目の追記が生んだもので、修正が一意に定まるため停止せず反映):

- `RawGestureDetector` サンプルの `using FloatSoda.Gesture.Recognizers;` — 実際の名前空間は `FloatSoda.Gesture`。削除した
- `GestureArena` の決着ルールの記述が曖昧だった。`GestureArenaManager.Sweep` の実装
  (`src/FloatSoda/Gesture/GestureArena.cs:114`)を読み、「宣言による即時確定」と
  「Sweep 時に最初の登録者が勝つ」の2通りへ書き分けた
- `ServiceProvider` サンプルの `IOscClient` が FloatSoda 提供の型に見える。
  利用側が登録する自前サービスであるとコメントで明示した

判断を記録しておく項目:

- `#### 表示・非表示の3つのウィジェットの使い分け` は Layout 節で唯一の h4 になる。
  他のウィジェット解説が地の文であるため見出しの粒度が不均一になるが、
  **3ウィジェット横断の比較という別種の情報**であり、見出しがないと比較表が地の文へ埋もれる。
  読者の主要な読み手が LLM であることを踏まえ、走査しやすさを優先して見出しを残した

検証結果: ID アンカー残存0件。`docs/` の内部リンクとアンカーは全件解決。

---

## 指摘

## C-020 [S6] 対応済 / 軽微 — 「実装済みの Widget」表に部分実装の `Container` が入っている

**該当**(README.md「実装済みの Widget」レイアウト系):
> | `Container` | 配置・装飾・寸法・変換をまとめて指定する合成ウィジェット（`Padding` の合成は未対応） |

**問題**: 見出しは「実装済みの Widget」だが、この行だけ未対応の機能を括弧で断っている。
表の他の行は無条件に使えるものばかりなので、読者は括弧書きを見落とすと `Padding` を探しに行く。
**修正案**: 見出しを「使える Widget」に変えるか、`Container` の行だけ注記を表の外へ出す。
docs 側(`WidgetSystem.md`)は `△ 部分実装` の記号で区別できているので、README でも記号を borrow するのが素直。

---

## C-019 [S6] 対応済 / 軽微 — 修飾が三重に重なった一文

**該当**(docs/GettingStarted.md「Widget の実装状況」):
> 公開 API から除外されている(`internal` の)未実装ウィジェットは、スクロール系の `ListView` / `GridView` / `SingleChildScrollView` と、`Components.Image` / `Components.Icon` です。

**問題**: 「公開 API から除外されている」「(`internal` の)」「未実装」が同じことを3回言っている(B-6 重言)。
主語が長くなり、述語の列挙にたどり着くまでが遠い。
**修正案**: 「`internal` のため公開 API から使えないのは、スクロール系の … と … です。」

---

## C-018 [S6] 対応済 / 軽微 — 「型で分岐してください」がサンプルの前後で2回

**該当**(docs/WidgetSystem.md「WindowWidget — 自分が載っているウィンドウを知る」):
> オーバーレイ種別で表示を変えたい場合は型で分岐してください。
> (コードサンプル)
> 種別を確かめたいだけなら上の型分岐を使ってください。

**問題**: 同じ指示を、サンプルを挟んで2回書いている。後者は「派生型の `Of` は例外を投げる」の
補足として置いたが、直前の指示の繰り返しになっている。
**修正案**: 後者を「派生型の `Of` は、ルートが別の種別だと `InvalidOperationException` を投げます。」で止める。

---

## C-017 [S6] 見送り / 軽微 — 新規サンプルに `using` が無く、同ページの他サンプルと不揃い

**該当**(docs/WidgetSystem.md「WindowWidget — 自分が載っているウィンドウを知る」):
> ```csharp
> var window = WindowWidget.Of(context);
> Widget caption = new Text(window.Title);
> ```

**問題**: このページの他のサンプルはすべて `using` から始まっている(`StatelessWidget` 節、
`Builder` 節、`ListenableBuilder` 節など)。この2つの短いサンプルだけ省略しており、
コピーしてすぐ動かせるという他サンプルの前提が崩れる。
**修正案**: 断片であることが明らかな2〜3行のサンプルは `using` を省く、という方針を採るなら
このままでよい。その場合は `ServiceProvider` 節のサンプル(`using` あり)と粒度を揃える必要がある。
どちらかに寄せる。

---

## C-016 [S6] 対応済 / 軽微 — 「など」で残り2件を隠している

**該当**(docs/Architecture.md ツリー構造):
> 残る未実装はスクロール系(`ListView` など)と画像・アイコンです

**問題**: `GridView` / `SingleChildScrollView` が「など」に隠れている(B-3)。
読者が「使えないもの」を数える文脈なので、開いた列挙は向かない。
**修正案**: 「スクロール系(`ListView` / `GridView` / `SingleChildScrollView`)」と書き切る。

---

## C-015 [S6] 対応済 / 中 — 冒頭の実装状況ブロックが走査しづらい

**該当**(docs/WidgetSystem.md 冒頭):
> - **✓ 実装済み:** `StatelessWidget` / `StatefulWidget` / … 入力系の `GestureDetector` / `Listener` / `AbsorbPointer` / `IgnorePointer` / `PointerRegion` も動作します(→ [ジェスチャとヒットテスト](#ジェスチャとヒットテスト))。

**問題**: 1項目が300字を超え、その中に4つの括弧付き列挙が入っている(A-1・A-5)。
「自分が使いたいウィジェットが使えるか」を確かめる目的で開いた読者が、
地の文を最後まで読まないと判定できない。
このページの読者は主に LLM であり、走査に向かない形は誤読の確率を直接上げる。
**修正案**: 4項目の箇条書きを、状態を1列目に置いた小さな表へ組み替える。
個々のウィジェット名は下の一覧が持っているので、冒頭は「どのカテゴリがどの状態か」に絞れる。

---

## C-014 [S6] 対応済 / 中 — 決着ルールを2つ挙げた直後の例が、片方しか使っていない

**該当**(docs/WidgetSystem.md「認識器を自分で組む(`RawGestureDetector`)」):
> 2. **誰も宣言しないままポインタが上がったら、最初に登録された認識器が勝つ。**
> …
> タップとパンを両方登録した場合、指をほとんど動かさずに離せば1のルートでタップが、動かせば同じく1のルートでパンが勝ちます。

**問題**: 2つのルートを並べておきながら、例では両方とも1を通ると書いている。
読者は「では2はいつ起きるのか」を抱えたまま次へ進む(C-3 単位ごとに完結させる)。
実際、組み込みの `TapGestureRecognizer` は指を離した時点で `Accepted` を宣言し
(`src/FloatSoda/Gesture/Recognizers/TapGestureRecognizer.cs:68`)、
`PanGestureRecognizer` は閾値を超えた時点で宣言する(同 `PanGestureRecognizer.cs:52`)ため、
**2のルートは自作の認識器を混ぜたときにしか効かない。**
**修正案**: 2の説明に「組み込みの2つはどちらも自分で宣言するため、このルートは自作の認識器を
登録したときの保険になる」と補う。例文の「同じく1のルートで」という書き方も、
その補足があれば自然に読める。

---

## C-013 [S6] 対応済 / 中 — `Container` の解説がレイアウト節の末尾にある

**該当**(docs/WidgetSystem.md Layout 節):
> `Container` は、`Align` / `DecoratedBox` / `SizedBox` / `Transform` の組み合わせを1つのウィジェットにまとめた合成ウィジェットです。

**問題**: `Container` は最も使用頻度が高い合成ウィジェットだが、解説は
`ConstraintsTransformBox` / `OverflowBox` / `IntrinsicWidth` といった特殊用途の解説を
すべて越えた末尾に置かれている(F-9 重要度順に反する)。
表の行は中ほどにあるため、表から解説までの距離も遠い。
**修正案**: `Padding` / `Stack` の解説群の近く、遅くとも `ConstraintsTransformBox` の前へ移す。

---

## C-012 [S6] 対応済 / 軽微 — マイルストーンへの誘導が同じ節に2回

**該当**(docs/Home.md「ロードマップ(Phase)」):
> 各 Phase の詳細スコープは [GitHub マイルストーン](...) を参照してください。
> …
> 一覧と進捗は [GitHub マイルストーン](...) が一次情報です。この表は要約なので、着手前にマイルストーンを確認してください。

**問題**: 節の導入と残件表の直後で、同じリンクへ同じ趣旨で2回誘導している。
**修正案**: 導入側の1文を残し、残件表の後は「この表は要約です」だけにする。

---

## C-011 [S6] 対応済 / 中 — Phase 表の状況欄と残件表が同じ情報を2回書いている

**該当**(docs/Home.md「ロードマップ(Phase)」):
> | Phase 1 | 入力基盤(HitTest / Pointer / Gesture) | 🚧 進行中(残るのは非ダッシュボードオーバーレイへのポインタ接続) |
> …
> | Phase 1 | 非ダッシュボードオーバーレイへのポインタ接続(コントローラーレイ経路) |

**問題**: Phase 1 の残件が、Phase 表の状況欄と残件表で重複している。
Phase 2 も状況欄が「残件は下記」と言いながら「レイアウト・描画・入力系は一巡」まで書いており、
2つの表の役割が分かれていない。
C-007 で残件表を足した結果として生まれた重複であり、**役割の違いによる重複ではない**(片方を削っても担保は残る)。
**修正案**: Phase 表の状況欄を `🚧 進行中` / `未着手` だけに戻し、内訳は残件表に一本化する。
そのほうが7行の Phase 表が読みやすくなり、残件表の存在理由もはっきりする。

---

## C-010 [S6] 対応済 / 中 — 記号の凡例が、表の半分の行に当てはまらない

**該当**(docs/Home.md「実装状況サマリ」):
> | ✓ 実装済み | `public` で、意図した動作をする |
> | ✗ 未実装 | 型は存在するが `internal` で、公開 API から使えない |

**問題**: 凡例は状態を**型の可視性**で定義しているが、下の表には型ではない行が多くある。
「ヒットテスト(座標 → RenderObject の特定)」「ジェスチャ認識」「ポインタ入力源」
「intrinsic 測定」「UI3層構成」「レイヤーツリーとレンダースレッド分離」は機能や仕組みであって、
`public` / `internal` という尺度が当たらない。
特に「ポインタ入力源 | △ 部分実装(ダッシュボードオーバーレイのみ)」は、
凡例の「`public` だが機能の一部が未完成」では説明できない(型の話ではなく接続範囲の話)。
**なぜ直すか**: 凡例は読み方を固定するために置いたのに、当てはまらない行が半分あると
かえって「この行はどの意味なのか」を考えさせる。
**修正案**: 凡例の定義から型の可視性を外し、機能単位の言い方へ一般化する。
たとえば「✗ 未実装 = 公開 API から使えない」。型の可視性という判定基準は
`WidgetSystem.md` のウィジェット一覧側だけで使う。

---

## C-009 [S6] 対応済 / 重大 — 「下の一覧にあるものがすべて使用可能」が一覧と矛盾する

**該当**(docs/WidgetSystem.md 冒頭「✓ 実装済み」):
> レイアウト系(`Padding`, `Align`, `Flex`, `Stack`, `Wrap`, `Expanded`, `AspectRatio`, `FittedBox` ほか)と描画系(`ColoredBox`, `DecoratedBox`, `Opacity`, `Transform`, `Clip*`)のウィジェットは下の[一覧](#組み込みウィジェット一覧)にあるものがすべて使用可能です。

**問題**: 参照先の Layout 表には `Container`(`△ 部分実装`)と
`ListView` / `GridView` / `SingleChildScrollView`(`✗ 未実装(internal)`)が含まれている。
「一覧にあるものがすべて使用可能」は事実に反する。
**なぜ直すか**: 読者の主要な読み手は LLM で、この一文を根拠に `ListView` を使ったコードを書くと
`internal` のためコンパイルが通らない。**読後に取ってほしい行動(コンパイルが通るコードを書く)を
直接損なう。** 2行下の「✗ 未実装」項目に正しい情報があるため `致命` ではないが、
矛盾を残したまま出すと4状態語彙を導入した意味がなくなる。
**修正案**: 「下の一覧で `✓` が付いているものが使用可能です」へ変える。
一覧が状態列を持つようになったので、冒頭でウィジェット名を数え上げる必要はない。C-015 の組み替えと同時に直すのが早い。

---

## C-008 [S5] 見送り / 軽微 — Animation.md・Localization.md の「未実装」が Step 2 の4状態語彙と揃っていない

**該当**(docs/Animation.md 冒頭、docs/Localization.md §4):
> **未実装:** `Tween<T>` / `CurvedAnimation` などのアニメーション合成 …

**問題**: Step 2 で「未実装」を「型は存在するが `internal`」の意味に固定したため、
これらの箇所と語義がずれている。
**判断**: 見送り。これらが指しているのは**型そのものが存在しない機能**(Step 2 の語彙では `✗ 未着手`)であり、
ウィジェットの可視性の話ではない。同じ「未実装」でも文脈が「公開 API の可視性」ではなく
「機能の有無」であり、読者が取り違える余地がない。語彙を揃えるために
`✗ 未着手` へ機械的に置換すると、かえって「型はあるのか」という疑問を生む。

---

## C-007 [S5] 対応済 / 軽微 — Phase 2 の残件列挙が不完全

**該当**(docs/Home.md ロードマップ表、README.md 開発ステータス表):
> 🚧 進行中(レイアウト・描画系は一巡。画像・アイコン・`CustomPaint` が残件)

**不足している情報**: Phase 2 のオープン Issue は7件あり、上記のほかに
`DefaultTextStyle`(#170)、`ViewMetrics`(#173)、`Container` への `Padding` 合成(#196)、
サンプル整備(#188)が残っている。
**なぜ必要か**: 読者がこの一文から「あと少しで Phase 2 が終わる」と読み取るが、実際の残件はもう少し広い。
ただし Phase の詳細は GitHub マイルストーンへのリンクが既にあるため、影響は小さい。
**追加すべき内容**: 「画像・アイコン・`CustomPaint`・`DefaultTextStyle` が残件」へ拡張するか、
列挙をやめて「残件はマイルストーンを参照」へ寄せるかのどちらか。

---

## C-006 [S5] 対応済 / 中 — `Listener` と `PointerRegion` で `Behaviour` の既定値が違う理由

**該当**(docs/WidgetSystem.md「ヒットテストの振る舞い」節):
> | `DeferToChild` | 子がヒットしたときだけ自身もヒットする(既定値) |

**不足している情報**: `Listener` の既定は `DeferToChild` だが、`PointerRegion` の既定は `Opaque` である。
表は「既定値」を `DeferToChild` とだけ書いており、ウィジェットによって既定が違うことが読み取れない。
**なぜ必要か**: 読者(LLM)が `PointerRegion` に `Behaviour` を指定せずに書いたとき、
`Listener` と同じ挙動を期待して食い違う。ホバー領域は子の空白を含めたいので `Opaque` が既定という
設計意図も伝わらない。
**追加すべき内容**: 表の「既定値」注記をウィジェット別に分けるか、
`PointerRegion` は `Opaque` が既定である旨を1文添える。

---

## C-005 [S5] 対応済 / 中 — `RawGestureDetector` の使い方が示されていない

**該当**(docs/WidgetSystem.md Gesture 表):
> | `RawGestureDetector` | ✓ | 独自の `GestureRecognizer` を登録して認識器の組み合わせを自分で決める | `Gestures`, `Behaviour`, `Child` (必須) |

**不足している情報**: `Gestures` は `Dictionary<Type, GestureRecognizerFactory>` であり、
表の1行からは書き方が導けない。`GestureArena` による認識器の競合解決にも触れていない。
`TapGestureRecognizer` / `PanGestureRecognizer` という2つの組み込み認識器の存在も未記載。
**なぜ必要か**: `GestureDetector` で足りないケース(長押し、ダブルタップ)に読者が当たったとき、
`RawGestureDetector` が受け皿だと分かっても書き方が分からず止まる。
**追加すべき内容**: `Gestures` の型と最小サンプル、組み込み認識器2件の一覧。
`GestureArena` の役割は1〜2文で足りる。

---

## C-004 [S5] 対応済 / 中 — `WindowWidget.Of(context)` によるウィンドウ情報の取得が未記載

**該当**(docs/WidgetSystem.md、docs/GettingStarted.md — 追加位置を特定できないためアンカーなし)

**不足している情報**: `WindowWidget` は `InheritedWidget` であり、`WindowWidget.Of(context)` と
派生型の `DashboardWindow.Of(context)` / `DesktopWindow.Of(context)` で祖先のウィンドウ定義を
取得できる。この経路がどのドキュメントにも書かれていない。
**なぜ必要か**: ウィジェット側から自分がどのオーバーレイに載っているかを知る唯一の公開手段であり、
オーバーレイ種別によって表示を変えたい場合の入口になる。
**追加すべき内容**: `WidgetSystem.md` の InheritedWidget 節に、実際に使える
`InheritedWidget` の実例として `WindowWidget` と `ServiceProvider` を挙げる。

---

## C-003 [S5] 対応済 / 中 — `ServiceProvider` が未記載

**該当**(docs/WidgetSystem.md InheritedWidget 節 — 追加位置は節末)

**不足している情報**: `ServiceProvider`(`public record ServiceProvider : InheritedWidget`)は
`IServiceProvider` をウィジェットツリーへ公開し、`ServiceProvider.Of(context)` で解決できる。
FloatSoda は Generic Host 統合を前提にしているのに、ビルド中に DI コンテナへ到達する方法が
どこにも書かれていない。
**なぜ必要か**: 「OSC 送信」「フレンド監視」のようなサービスをウィジェットから呼ぶのは、
想定ユーザーの主要ユースケース(→ [TargetUsers](docs/TargetUsers.md))そのものである。
`State` にサービスを渡す手段が分からないと、読者はコンストラクタ注入できない `record` の前で止まる。
**追加すべき内容**: `ServiceProvider.Of(context)` の1サンプル。祖先に無い場合は
`InvalidOperationException` になることも書く。

---

## C-002 [S5] 見送り / 重大 — `DesktopWindow` がどのドキュメントにも存在しない

**該当**(docs/GettingStarted.md「オーバーレイ種別の選び方」節、README.md「ウィンドウ系」表 —
いずれも追加位置は既存の表)

**不足している情報**: `DesktopWindow`(`public record DesktopWindow : WindowWidget`)は、
描画結果を開発機のデスクトップへ GLFW の可視ウィンドウとしてミラー表示する。
`FloatSodaApp.CreateWindow` は `OverlayWindow` 系と `DesktopWindow` の2系統を受け付けるが、
ドキュメントは前者しか説明していない。
**なぜ必要か**: HMD をかぶらずに UI を目視確認できる唯一の手段であり、
「AI にコードを書かせて動作を確認する」という想定ワークフローの検証コストを大きく変える。
存在を知らない読者は、レイアウトの確認のたびに HMD をかぶることになる。
**追加すべき内容**: `DesktopWindow` の1行サンプルと、次の制約。
**`Initialize()` は `OVRApplication` を無条件に生成するため、`DesktopWindow` だけを使う場合でも
SteamVR の起動は必要**(`src/FloatSoda/FloatSodaApp.cs:202` で確認)。
「SteamVR 不要のプレビュー」と誤読されると、読者は起動できずに詰まる。

**判断が必要な点**: `DesktopWindow` を「公開 API として案内してよい機能」と位置づけるかどうか。
XML コメントは「目視確認・デバッグ用途を主眼とする」としており、
Phase 6 の Storybook(#141)と役割が重なる。案内する場合、どのページに置くかで扱いが変わる。

- `GettingStarted.md` に置く → 最初から使えるデバッグ手段として案内する
- `Architecture.md` か新規ページに置く → 開発者向けの内部機能にとどめる

---

## C-001 [S5] 対応済 / 中 — `Visibility` と `Offstage` の使い分けが読み取れない

**該当**(docs/WidgetSystem.md Layout 表):
> | `Offstage` | ✓ | 子をレイアウトしたまま描画・ヒットテストから除外 | `IsOffstage`, `Child` |
> | `Visibility` | ✓ | `Visible`に応じて必須の`Child`と`Replacement`を切り替え。非表示子の状態保持は行わない | `Visible`, `Child` (必須), `Replacement` |

**不足している情報**: 2つのウィジェットの説明はそれぞれ正確だが、
「どちらを選ぶか」の判断材料がない。`Offstage` は子の `State` とレイアウト結果を保つ一方、
`Visibility` は非表示時に子をツリーから外すため `State` が失われる。
`IndexedStack` も「全子をレイアウトしたまま1つだけ表示」で同じ用途に見える。
**なぜ必要か**: 表示/非表示は最も頻出する要求であり、選択を誤ると
「非表示から戻したらスクロール位置やカウンタがリセットされた」という形で後から効く。
**追加すべき内容**: 3ウィジェットの比較を短い表か3行の箇条書きで。
判断軸は「状態を保つか」「レイアウトコストを払うか」の2つで足りる。
