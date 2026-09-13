# Codex enumeration brief

The goal definition handed to `codex-runner` for step 1. Fill the bracketed slots, refresh the
known-issue list from `gh issue list --state open`, and paste the contents of
`known-divergences.md` into the axis B section so Codex extends the list instead of rediscovering it.

**Before handing the filled brief to Codex, grep it for `[` and confirm no bracketed slot survives.**
A brief still carrying `[PASTE …]` makes Codex re-enumerate what is already known, which silently
inflates both the scenario count and the HMD run.

Keep the two hard rules intact when editing: **enumeration only, no code**, and **every `file:line`
must come from reading the file**. Both exist because they've been the failure modes.

---

```markdown
# GOAL

FloatSoda(.NET 10 / C# 14 の SteamVR オーバーレイUIフレームワーク)について、テストが存在せず
壊れうるシナリオが、2つの軸で網羅的に列挙され、1件ずつ検証レイヤーの判定つきで文書化されている
状態にする。

軸A: SteamVR ランタイムが実際に稼働していないと検出できない不具合。
軸B: Flutter からの移植差異に由来する挙動差(FloatSoda は Flutter の Widget/Element/RenderObject
三層ツリーを模しており、docs も Flutter 語彙で説明しているため、利用者は Flutter の挙動を期待して書く)。

**コードは一切書かない。テストも実装も追加しない。列挙と分析だけを行う。**

# SUCCESS CRITERIA

- 成果物が `[SCRATCHPAD]/codex-scenarios.md` に Markdown で書き出されている
  (このファイルの新規作成のみ書き込みを許可する)
- 軸A・軸B それぞれについてシナリオが列挙され、各シナリオが以下の項目をすべて備えている:
  - シナリオ名(英数字の識別子。`--scenario` 引数に渡せる形)
  - 壊れる理由の仮説(1〜3文)
  - 関与ファイルと行番号(`src/...` の実在するパス:行。推測で書かない)
  - 検証レイヤーの判定: `HEADLESS` か `VR` か、およびその根拠1文
  - 再現/操作手順
  - 期待結果(合否の判定基準)
  - 軸Bのシナリオには、対応する Flutter 本家実装のファイルパス
    (`~/code_reading/flutter_reference` 内)を併記する
- 判定基準は次のとおり。`HEADLESS` = Widget / Element / RenderObject / Layer の範囲で再現でき、
  xunit または `src/FloatSoda.Testing` のビットマップレンダラで観測できる。`VR` = OpenVR ランタイム、
  GLコンテキスト、実デバイスのトラッキング、SteamVR のイベント配送のいずれかが介在しないと再現しない
- 末尾に、シナリオを `HEADLESS` / `VR` 別に数えた集計表がある
- 下記「既知として除外するもの」に該当するシナリオが、新規シナリオとして重複して挙がっていない
  (言及して「既知」と印をつけるのは可)

# CONSTRAINTS

- sandbox: workspace-write(ただし書き込んでよいのは上記 `codex-scenarios.md` 1ファイルのみ。
  リポジトリ `[REPO]` 配下の**追跡対象ファイルは一切変更しない**)
- 実装・テストの追加や修正をしない。`dotnet build` / `dotnet test` を実行してもよいが、
  結果を根拠に使うだけでコードは変えない。上の「変更しない」は追跡対象ファイルに対する制約で、
  `bin/` `obj/` などのビルド生成物には及ばない。テスト結果は
  `--results-directory <リポジトリ外の一時ディレクトリ>` を指定してリポジトリ外へ出す
- ファイル:行 を挙げるときは必ず実ファイルを読んで確認する。存在しない行番号やそれらしいAPI名を
  推測で書かない。確認できなかったものは「未確認」と明記する
- 出力は日本語で書く(識別子・型名・ファイルパス・コードは英語のまま)
- 網羅性を優先する。1つのシナリオを深掘りするより、抜けのない列挙を重視する

# CONTEXT

- 作業ディレクトリ: `[REPO]`
- Flutter 本家のクローン: `[FLUTTER_REFERENCE]`
- プロジェクト規約は `AGENTS.md` にある(出力は日本語、など)
- 環境は Windows。PowerShell と git-bash が使える

## 現状のテストカバレッジ(調査済みの事実)

- テストは 84 ファイルあるが、`WidgetBinding` を通して build → layout → paint を一気通貫させて
  いるのは 2 ファイルだけ: `tests/FloatSoda.Test/Core/PointerInputIntegrationTest.cs` と
  `tests/FloatSoda.Test/Core/WidgetBindingTest.cs`
- 残りはウィジェット / RenderObject の単体テスト
- `src/FloatSoda.OVR` と `src/FloatSoda.Engine` にはテストがほぼ無い
  (`tests/FloatSoda.Test/Engine/` に ThreadRunner / IOTaskRunner のテストがあるのみ)

## 軸A の探索範囲(起点。ここに限定しなくてよい)

| 領域 | 関与ファイル |
|---|---|
| オーバーレイのライフサイクル | `src/FloatSoda.OVR/Overlay/Overlay.cs`, `Overlay/Capability.cs` |
| コントローラーポインタの実座標 | `src/FloatSoda.OVR/Overlay/ControllerPointerSystem.cs`, `src/FloatSoda.OVR/PointerData.cs` |
| デバイス追従(接続/切断/スリープ) | `src/FloatSoda.OVR/Overlay/TrackedDeviceResolver.cs` |
| SteamVR イベント(ダッシュボード開閉/Quit/再起動) | `src/FloatSoda.OVR/SystemEventDispatcher.cs` |
| アクション入力とマニフェスト | `src/FloatSoda.OVR/Input/VRInputUpdater.cs`, `Input/ActionManifestWriter.cs` |
| GLコンテキスト / レンダースレッド / フレームペーシング | `src/FloatSoda.Engine/` 全般 |

## 軸B の探索範囲(起点)

`src/FloatSoda/Elements/Element.cs`, `Elements/BuildOwner.cs`, `Elements/ComponentElement.cs`,
`Elements/RenderObjectElement.cs`, `src/FloatSoda/RenderObjects/RenderObject.cs`,
`src/FloatSoda/Core/WidgetBinding.cs`, `src/FloatSoda/Gesture/`。

すでに確認済みの差異は以下。**これらは「確認済み」として扱い、同じものを新規に挙げ直さなくてよい。
ここを起点に、まだ挙がっていない差異を洗い出すことが仕事**:

[PASTE known-divergences.md の各項目を1行ずつ要約して貼る]

## 既知として除外するもの(GitHub issue 化済み)

新規シナリオとして重複して挙げないこと。参照するのは可。

[PASTE 最新の除外リスト。SKILL.md の "Known-issue exclusion list" を gh で更新してから貼る]

なお `src/FloatSoda/FloatSodaApp.cs` の `FloatSodaApp.MainLoop` は、タスク処理・イベント処理・描画の
3つの `catch (Exception)` ブロックそれぞれで `break` するため、例外が出るとアプリごと終了する。
**ただし、これも他の `file:line` と同じ扱いにする。** 現物を読んで実装が今もそうなっていることを
確認できた場合にだけ事実として使い、確認できなければ「未確認」と明記する。この文書に書かれた
行番号は再利用せず、実行時に現在の行番号を確認すること。
```

---

## After Codex reports

Do not relay the list as-is. Spot-check the cited `file:line` first — a scenario built on a misread
line costs an HMD session. Then route per `SKILL.md` step 2, and tell the owner which citations you
verified and which you did not.

**A scenario whose citation you could not verify does not enter routing, and never reaches the HMD.**
Hold it in a separate "unverified" list until the citation and the behaviour are confirmed. The point
of the enumeration is to spend the headset on things that are real.
