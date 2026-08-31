# 確認済みの Flutter 移植差異

**この台帳が、確認済みの FloatSoda ↔ Flutter 差異の正典。** そもそも差異がいつ許されるかを
定める設計原則は [`docs/APIDesign.md`](../../../../docs/APIDesign.md)(「判断原則: Flutter 由来の
observable behavior に差異を作らない」)にあり、このファイルは個別のエントリを保持する。
差異をここ以外の場所に記録すると記録が分裂する — 必ずここに追記すること。

このファイルは軸B 列挙の**出発点**も兼ねる。完全なリストではない — Codex に渡して再発見ではなく
拡張をさせ、実行のたびに新たに確定したエントリを追記する。

突き合わせ用の Flutter クローン: `~/code_reading/flutter_reference`。`flutter-widget-source`
スキルで、ウィジェットから Widget / Element / RenderObject の実装を特定できる。

エントリは、**Status** 行に断りがない限りソース検証済み。未確認のエントリは列挙のための
手がかりであって、確定した知見ではない。

各エントリには、判断が下り次第ラベルが付く: **deliberate**(設計判断 — 未文書なら `docs/` の
ギャップになる)/ **not ported**(未移植。Issue を立てる)/ **port mistake**(実装されているが
誤り — 最も価値の高いカテゴリ)。

## エントリのテンプレート

最初の5フィールドは、`docs/APIDesign.md` がすべての意図的差異に要求するもの(Flutter の挙動 /
FloatSoda の挙動 / 理由 / 差異を固定するテスト / 利用者向け docs)。
**`Test` の無い差異は、次の移植で静かに巻き戻される** — `deliberate` エントリの未設定の `Test` は、
完了した記録ではなく未処理のタスクとして扱うこと。

```markdown
## N. <差異の一行要約>

- **FloatSoda**: 何をするか。`src/…:line` の根拠つき。
- **Flutter**: Flutter が何をするか。対応するファイルつき。
- **Why**: なぜ差異が必要か。ラベルが `deliberate` になったら必須。
- **Test**: 差異を戻すと失敗するテストのファイル + テストメソッド名。未設定なら `— (not set)`。
- **Docs**: 利用者から観測できる場合、`docs/` のページやサンプルの `## Flutterとの違い` 節。
  利用者から見えないなら `— (not set)`。
- **Observation**: `HEADLESS` か `VR` — 差異が実際に観測できる場所。
- **Label**: deliberate / not ported / port mistake / unlabelled。
```

以下でラベルが `unlabelled` のままのエントリには `Why` を書いていない: まだ判断されていない
エントリには、記録すべき合意された理由が存在しないため。

---

## 1. Widget の等価性が同一性ではなく構造的

- **FloatSoda**: `src/FloatSoda/Elements/Element.cs:173` の `child.Widget == newWidget`。`Widget` は
  `abstract record` なので `==` は**値の等価性**。構造的に同一の新しいウィジェットは更新全体を
  短絡させる — `Update()` が走らないため、`didUpdateWidget` 相当は発火せず、サブツリーは
  再構築されない。
- **Flutter**: 同じ位置のチェックは同一性比較(`Widget` は `==` をオーバーライドしない)。
  そのため新しいインスタンスは必ず `canUpdate` → `child.update(newWidget)` → `didUpdateWidget`
  に到達する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled — `AGENTS.md` は意図的な fast path として説明しているが、
  `didUpdateWidget` への帰結はどこにも文書化されていない。

## 2. inactive-element プールが無い

- **FloatSoda**: `src/FloatSoda/Elements/ComponentElement.cs:164` に明記されている — 再活性化
  プールが無いため `Deactivate` は終端(unmount と等価)。キー付きサブツリーを別の親へ移動すると
  `State` が破棄される。
- **Flutter**: `BuildOwner._inactiveElements` が deactivate された要素を保持し、同一フレーム内で
  再活性化できる。`finalizeTree()` が残りを unmount する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: unlabelled

## 3. `GlobalKey` が未実装

- **FloatSoda**: `src/` 配下のどこにも出現しない。
- **Flutter**: ツリーをまたぐ `State` アクセスと reparenting がこれに依存する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: not ported(#2 と対 — 再活性化がその前提)

## 4. InheritedWidget の登録キーが実行時型ではなく `ScopeType`

- **FloatSoda**: `src/FloatSoda/Widgets/InheritedWidget.cs:20` が `ScopeType` を定義(既定は
  `GetType()`、オーバーライド可)。`src/FloatSoda/Widgets/WindowWidget.cs:32` がこれを
  `typeof(WindowWidget)` にオーバーライドし、3つの具象ウィンドウ種別すべてが基底型で登録され、
  具象型が変わっても子孫が依存を保てるようにしている。
- **Flutter**: `runtimeType` がキー。`dependOnInheritedWidgetOfExactType<T>()` は完全一致。
- **Why**: 具象ウィンドウ型が差し替えられても子孫は `WindowWidget` への依存を保つ必要があるため、
  3つのウィンドウ種別すべてを基底型で登録する(issue #90 がこれに依存)。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`
- **Label**: deliberate(issue #90 が依存)— よって文書化が必要。

## 5. レイアウトの early-return が boundary のみの変更をカバーしない

- **FloatSoda**: `src/FloatSoda/RenderObjects/RenderObject.cs:91-107`。early return は
  `RelayoutBoundary == relayoutBoundary` を要求するため、relayout boundary だけが変わった場合は
  素通りして **`PerformLayout()` が走る**。
- **Flutter**: 制約が不変なら boundary を付け替え、子を掃除して return — `performLayout` は
  走らない。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS`(実機では余分なレイアウトコストとしても現れうる)
- **Label**: unlabelled — port mistake の可能性が高いが、先に確認する価値がある。

## 6. フレームフェーズに Flutter の複数ステージが欠けている

- **FloatSoda**: `src/FloatSoda/Core/WidgetBinding.cs:186-245` — transient コールバック → build →
  layout → paint → `PostRender`。post-frame コールバックが無く(`PostFrame` は `src/` 配下に
  出現しない)、`finalizeTree` も、compositing-bits の flush も、semantics も無い。
- **Flutter**: transient → persistent(build / layout / compositing bits / paint / composite /
  semantics)→ `finalizeTree` → post-frame コールバック。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — 上に挙げた欠落フェーズはすべてヘッドセット無しで観測できる。
- **Label**: unlabelled。移植者が最も頻繁に噛まれるのは `addPostFrameCallback` 相当の欠落。
  semantics は VR オーバーレイではスコープ外というのがありそうな線。
- **列挙時の注意**: 1シナリオが持つ判定は1つ。派生シナリオが、欠落フェーズそのものではなく
  レンダースレッドのタイミングに依存するなら、`VR` として別に列挙する。

## 7. ポインタ入力がフレーム境界に量子化されている

- **FloatSoda**: `FlushPointerEvents()` が `BeginFrame` の先頭で走る
  (`src/FloatSoda/Core/WidgetBinding.cs:234`、実装は `:277`)ため、入力はフレームごとに1回
  処理される。
- **Flutter**: `GestureBinding.handlePointerEvent` はフレームとは独立にディスパッチする。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: `HEADLESS` — 量子化そのものは、1フレーム内に複数のポインタイベントを流し込み、
  まとめてディスパッチされることをアサートすれば検証できる。
- **Label**: unlabelled

## 8. フレームレート低下時の入力遅延または喪失 — **未確認**

- **Status**: ソース未検証。ここにある他のすべてのエントリと違い、これは実装を読んで得たものでは
  なく #7 からの*派生*で、列挙に拾わせるために載せている — 確立した差異としてではない。
  確定済みとして引用しないこと。
- #7 の利用者から見える帰結。**イベントが実際に落ちるのか、単に遅れるだけなのかが未解決の問い**で、
  実際のイベントソースと実際のフレームバジェットに依存する。
- **Test**: — (not set)
- **Docs**: — (not set)
- **Observation**: **`VR`** — SteamVR のイベント配送と実フレームレートが必要。合成イベントキュー
  からは再現できない。
- **Label**: unlabelled — まず #7 を解決する。ヘッドレスの判定で挙動の説明が付くなら、この
  エントリはそちらへ吸収される。

## 9. 既定フォントサイズが 14 ではなく 30

- **FloatSoda**: `FontSize` 未指定時は描画時に 30 へ解決される
  (`src/FloatSoda/Painting/TextStyle.cs` — `DefaultFontSize`。`ToRichTextKitStyle` で適用)。
  4299340「Textのスタイル指定を完成させる」で意図的に導入され、以来 `TextStyle` の既定値と
  `TextPainter` のフォールバックの両方が一貫して 30(現在は `TextStyle` へ一本化済み)。
- **Flutter**: 祖先がサイズを与えない場合のエンジン既定は 14.0
  (`DefaultTextStyle.fallback` の `TextStyle` は `fontSize: null`)。
- **Why**: 14 は HMD のレンズ越しでは小さすぎて読めないため、VR オーバーレイ向けに 30 へ引き上げた。
  **30 は暫定値** — 目視で決めたもので計測に基づかない。確定扱いにする前に、実機の可読性検証
  (device test)で再調整すること。
- **Test**: `tests/FloatSoda.Test/Widgets/TextTest.cs` —
  `ToRichTextKitStyle_全プロパティ未指定_既定値で描画書式を生成する`
- **Docs**: `docs/WidgetSystem.md`(`Text` の既定書式の記載)
- **Observation**: 値の差異そのものは `HEADLESS`。30 が適切な値かどうかの判定は `VR`
  (レンズ越しの可読性)。
- **Label**: deliberate
