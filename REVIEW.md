# Code Review Guidelines

このドキュメントは、FloatSoda のコードレビューで**何を、どの優先度で見るか**を定めます。人間のレビュアーと、レビューを行うコーディングエージェントの両方に適用します。

役割分担は次のとおりです。このファイルに開発手順や API 設計原則を書かないでください。

| 目的 | 正典 |
|---|---|
| 開発・コントリビューション規約(ブランチ命名 / namespace / skill / PR 運用 / テスト観点) | [CONTRIBUTING.md](CONTRIBUTING.md) |
| API 設計原則・Flutter parity | [docs/APIDesign.md](docs/APIDesign.md) |
| エージェントの入口・実行方針 | [AGENTS.md](AGENTS.md) |

本文中の **必須** は MUST、**推奨** は SHOULD の強さで読んでください。

レビュー出力は**日本語**で書きます(→ [AGENTS.md](AGENTS.md) の Output Language)。コード・識別子・型名・ファイルパスは原文のままにします。

---

## 1. 重要度の順序

指摘は概ね次の順で重要です。上位の問題を差し置いて下位の指摘を並べないでください。

1. **behavioral correctness** — 仕様どおりに動くか
2. **tree lifecycle / state transitions** — mount / update / dispose と状態遷移が壊れていないか
3. **incremental update correctness** — 差分更新(dirty 伝播・再構築範囲)が正しいか
4. **public API consistency** — 既存 API・[docs/APIDesign.md](docs/APIDesign.md) と一貫しているか
5. **test coverage** — behavioral contract を押さえたテストがあるか
6. **documentation consistency** — docs / XML ドキュメントコメントと実挙動が一致しているか
7. **performance** — **concrete impact のあるもののみ**
8. **style / maintainability**

---

## 2. finding の基準

- **concrete failure mode を説明できる問題を優先します**(必須)。「どの入力・状態で、何が壊れるか」を書けない指摘は、書けるようになるまで格下げしてください。
- **subjective な好みだけの指摘をしません**(必須)。規約・契約・失敗経路のいずれにも紐づかない「私ならこう書く」は指摘ではありません。
- **hypothetical な問題を過剰に報告しません**(必須)。「将来こう使われたら壊れるかもしれない」は、その使い方が実際に到達可能であることを示せる場合にのみ挙げます。
- **Issue のスコープ外の改善要求を、安易に blocking にしません**(必須)。気づいた点は「別 Issue 向け」と明示して非 blocking で伝えます(→ [CONTRIBUTING.md](CONTRIBUTING.md) の scope discipline)。
- **既存コードがそうなっているという理由だけで、正しい仕様と判断しません**(必須)。次章の優先順位で確認してください。

blocking にしてよいのは、重要度 1〜5 に該当し、かつ concrete failure mode を説明できる指摘です。6〜8 は原則として非 blocking の提案とします。

---

## 3. 仕様の正典と優先順位

仕様が食い違ったときは、**上から順に**確認します(必須)。

1. **FloatSoda で明示的に定義された差異・設計判断** — [docs/APIDesign.md](docs/APIDesign.md)、[known-divergences.md](.agents/skills/floatsoda-device-test/references/known-divergences.md) の `Label: deliberate` エントリ、`docs/` 各ページの明記。known-divergences.md のそれ以外のラベル(unlabelled / port mistake / not ported)は意図が確定していないため、正典としては扱いません
2. **Flutter 由来機能は、Flutter の仕様・実装・公式テスト** — 1 に該当する記述が無いなら、Flutter が正典です
3. **既存の FloatSoda 実装は根拠になりません** — 実装がそうなっていることは、それが正しいことを意味しません

**古い Issue や既存実装だけを根拠に、新しい挙動を決めないでください**(必須)。Issue が書かれた時点の前提が今も成り立つかを確認します。

Flutter を参照するときのローカルクローンは `~/code_reading/flutter_reference` です。対応する Widget / Element / RenderObject の特定には `flutter-widget-source` skill が使えます。

---

## 4. FloatSoda 固有の不変条件

RenderObject / Element / Widget / Layer に触れる変更では、次を確認します。該当する変更が無ければ、その項目は飛ばしてかまいません。

### ツリーの所有権とライフサイクル

- **parent / child ownership** — 子の差し替え時に、旧 child が drop され、新 child が adopt されているか。`SingleChildContainer<T>` / `MultiChildrenCollection<T>` を経由せず親子リンクを直接書き換えていないか。片方向だけ張られて `Parent` が古いまま残っていないか。
- **adopt / drop** — 代入のたびに対称に走るか。同じ子を二重に adopt していないか。drop 後に `Parent` が `null` に戻るか。
- **attach / detach** — 子の追加・削除で attach/detach が転送されるか。detach 済みのノードが `RenderPipeline` の dirty リストに残らないか。
- **mount / update / replace / dispose** — `Element.Mount()` / `UpdateChild()` / `InflateWidget()` の経路と、`State.Dispose()` の呼び漏れ。
  **FloatSoda には inactive-element プールが無く、`Deactivate` は終端(= unmount 相当)です。** keyed subtree を別の親へ移すと `State` は破棄されます。Flutter の再活性化を前提にしたコードを移植していないか確認してください(→ known-divergences #2)。
- **keyed update** — `Widget.CanUpdate`(同一 runtime 型 + `Key` 一致)の判定と、`MultiChildRenderObjectElement.UpdateChildren()` の二端差分。Key 付きの子を並べ替えたとき、`State` と RenderObject が意図どおり保持されるか。重複 Key が入り込まないか。

### 差分更新

- **dirty element scheduling** — `MarkNeedsBuild()` → `BuildOwner` の dirty リスト → `BuildScope()` が `Depth` 昇順(親が先)で処理されるか。ビルド中に追加された dirty が同一フレームで拾われるか。
- **`MarkNeedsLayout` / `MarkNeedsPaint`** — 最も近い relayout / repaint boundary まで伝播し、**その境界ノードが** `RenderPipeline.NodesNeedingLayout` / `NodesNeedingPaint` に登録されるか。dirty フラグを立てただけでリスト登録を忘れていないか。
- **relayout / repaint boundary** — 境界の成立条件が正しいか。境界をまたいで伝播が止まるべき箇所で止まっているか。制約が変わらないのに境界だけ変わったケースの扱い(→ known-divergences #5)。
- **不要な rebuild / layout / paint が発生していないこと** — 値が変わっていないのに `MarkNeeds*` を呼んでいないか。`UpdateRenderObject()` が受け取った値を無条件に代入して毎回 dirty 化していないか。レイアウトだけで足りるのに paint まで落としていないか。**これは重要度 3(incremental update correctness)であって、7 の performance ではありません。**

### Layer

- **Layer clone 後に意図しない mutable state の共有がないこと**(必須) — レイヤーツリーは `ILayer.Clone()` してレンダースレッドへ渡します。clone 後に、メインスレッド側と `SKPicture` / 子レイヤーのリスト / `SKPaint` などの**可変オブジェクトを共有していないか**を確認してください。新しい Layer 型を追加した場合、`Clone()` が新しく持たせたフィールドをコピーしているかを必ず見ます。ここを外すとデータレースになり、テストではまず落ちません。

---

## 5. テスト

規約の本文は [CONTRIBUTING.md](CONTRIBUTING.md) の「テスト観点」にあります。レビューでは次を確認します。

- 観点の抜けを見ます — representative normal behavior / boundary・degenerate inputs / invalid inputs / state transitions / tree lifecycle / incremental behavior / regression test。**該当するのに無い観点を指摘します**(必須)。
- テストが **observable behavior / behavioral contract / invariant** を検証しているか(必須)。private フィールドや呼び出し回数をなぞるだけの、実装詳細のミラーになっていないかを見ます。リファクタリングで壊れるが挙動は正しい、というテストは指摘対象です。
- **バグ修正には regression test が必要です**(必須)。元のバグを再現しているか、**修正を戻せば失敗するか**を確認します。修正コードと同時に読んで「このテストは修正前に落ちたはずか」を判断してください。
- Flutter 由来の Widget / RenderObject では、**Flutter 本体の実装分岐・公式テストに対応する重要ケースが押さえられているか**(必須)。FloatSoda 側で想像したケースだけになっていないかを見ます。
- テスト命名が `対象メンバー名_条件_期待結果` に従っているか。**既存テストの一括リネームは求めません**(規約どおり)。

---

## 6. Flutter parity

判断基準の正典は [docs/APIDesign.md](docs/APIDesign.md) の「判断原則: Flutter 由来の observable behavior に差異を作らない」です。レビューでは次を確認します。

- Flutter 由来の Widget / RenderObject に、**明示された理由のない observable behavior の差異が入っていないか**(必須)。対象は property semantics / default values / layout / paint・clipping / hit testing / child handling / Widget update behavior / invalid・degenerate input handling / dirty layout・paint conditions / Element・state lifecycle semantics。
- 「実装しやすい」「こちらの方が安全」「こちらの方が自然」**だけ**を理由にした独自仕様になっていないか(必須)。
- C#/.NET として自然な表現への置換(`event`、`init` / `required`、`record` / `record struct`、.NET 標準機構の利用)は **behavioral difference ではありません**。これを差異として指摘しないでください。
- **差異が必要と判断された場合、記録必須 5 項目が揃っているか**(必須) — Flutter の挙動 / FloatSoda の挙動 / 差異が必要な理由 / 差異を固定するテスト / 利用者に影響する場合のドキュメント。記録先は [known-divergences.md](.agents/skills/floatsoda-device-test/references/known-divergences.md)。
- Semantics ほか、既に明示された**非移植方針を parity を理由に破っていないか**(必須)。
- Flutter を参照した PR で、**Flutter version / commit・参照した source・参照した tests** が PR から追跡できるか(推奨)。

---

## 7. namespace / ディレクトリ

規約は [CONTRIBUTING.md](CONTRIBUTING.md) にあります。レビューでは、**namespace とディレクトリの片方だけが変わっていないか**を確認します(必須)。ファイルを移動したのに namespace が古いまま、あるいは namespace を変えたのにファイルが元のディレクトリに残っている、という差分は指摘対象です。例外が必要な場合、その理由が書かれているかを見ます。

---

## 8. scope discipline

- Issue / PR の目的に**不要な変更が混ざっていないか**(必須) — unrelated refactoring / rename / cleanup / 依存の追加・削除。混ざっている場合は、別 PR への分離を求めます。
- public API または observable behavior を変更する PR で、**breaking change の有無が明示的に判断され、PR 本文に書かれているか**(必須)。
- 逆に、レビュアー自身がスコープ外の改善を要求して PR を膨らませないでください(必須)。指摘は「別 Issue 向け」と明示します。

---

## 9. レビューの層と分担

この基準を適用するレビューは三層あります。**上の層で拾えるものを、下の層で重複報告しないでください**。

| 層 | 主体・タイミング | 主に担う範囲 |
|---|---|---|
| 入口(自動) | CodeRabbit。PR ごとに自動実行。設定は [.coderabbit.yaml](.coderabbit.yaml) | 重要度 4・6・7・8 と、差分内で静的に読み取れる浅い正しさ。コードを実行できないため、失敗の証明はできない |
| 入口(判断) | 人間・エージェントによる PR レビュー | この文書の全章。blocking 判断はここで行う |
| 出口(フェーズ末) | Codex による敵対的監査。手順は [phase2-adversarial-review](.agents/skills/phase2-adversarial-review/SKILL.md) | 重要度 1〜3 と 6章 parity を、フェーズ横断のサーフェスに対して。finding は再現する失敗テストを添えたものだけ受理する。7〜8 は報告しない |

隣接する検証との住み分け:

- SteamVR 実行時にしか観測できない挙動 → `floatsoda-device-test`
- docs / API の発見性・誤誘導 → `floatsoda-junior-coder-test`
