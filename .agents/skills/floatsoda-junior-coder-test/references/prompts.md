# Junior-coder task prompts

These are the task prompts handed to the junior subagent. Voice = FloatSoda persona #1: a VRChatter
who vibe-codes personal tools and barely writes code themselves. Keep the voice casual and goal-first,
never framework-jargon-y — that realism is part of the test.

Every prompt must carry the same three guardrails (fill in the docs URL and scratch path):

- **Read docs first**: `https://github.com/sumx21t-3310/FloatSoda/wiki` — read `Home / GettingStarted /
  WidgetSystem / OVRIntegration` before writing anything.
- **Black box**: use ONLY the docs; do not open/read/grep anything under `src/`; if the docs don't cover
  it, say so instead of guessing.
- **Constraints**: C# only; no Unity scenes or prefabs; use the exact API/NuGet names from the docs, never
  invented ones; report which docs pages you actually used and anywhere you were unsure.

Pick difficulty by what you want to measure. Swap the theme freely (the three canonical persona wants are:
FaceEmo expression switcher over OSC, a VRChat photo album, a friend-online toast notifier).

---

## 🟢 easy — does the getting-started path work at all

> FloatSodaで、SteamVRの視界にカードを1枚だけ出すツールを作って。
> カードには好きな絵文字っぽいタイトルとテキストを表示。
> まずdocsを読んでから、C#だけで完結させて（Unityのシーン/prefab禁止）。

Measures: init → one overlay on screen. If this stumbles, `GettingStarted` has a hole.

---

## 🎯 main — stateful, dynamic UI on implemented widgets (the sweet spot)

> FloatSodaを使って、VRChatの友達がオンラインになったら、オーバーレイの隅に
> 「〇〇 がオンラインになりました」というトーストを数秒間ふわっと出すツールを作って。
> ・オンライン情報はダミーでいい（数秒ごとに適当な名前を流すモックでOK）
> ・視界に常駐する形で（ダッシュボードじゃなく）
> ・トーストは自動で消える。複数来たら縦に積む
> ・見た目は角丸・半透明でいい感じに

Measures: `StatefulWidget`/`SetState`, keyed list diffing, animation, and — critically — **dynamic child
add/remove**, which is where real render-tree bugs live. This is the run that found the two known bugs.

---

## 🔴 hard — pushes into known gaps on purpose

> FloatSodaで、VRChatのアバター表情をワンタップで切り替えるオーバーレイパネルを作って。
> ボタンを3〜4個並べて、押したらOSCで表情パラメータを送る（OSC送信はConsole出力のダミーでいい）。
> ダッシュボードオーバーレイで、ボタンは押した見た目のフィードバックが欲しい。

Measures: multi-button layout + interaction + **tap/hit-testing (currently unimplemented)**. The point is
to watch the failure *mode*: does the model hallucinate a `GestureDetector`/`Button` API, correctly report
that the docs don't cover input, or invent a workaround? Each outcome says something different about the docs.

---

## 🎯 targeted — new-API PR gate

For the new-API PR gate, don't reuse a canned task — **write one that cannot be completed without the new
API**, so the junior is forced to discover and use it. The whole point is to measure whether the *new*
surface is usable from the *updated* docs alone, so the black-box rule and the "docs only" pointer matter
even more here. Hand it the **PR branch's** docs.

Keep the task realistic (still the VRChatter voice) but shaped so the new API is on the critical path — not
mentioned by name (that would leak the answer), but unavoidable by function.

> **Phase 2 exception — read this first.** While Phase 2 is running, every gate PR ports one named Flutter
> widget, and the task for those runs is **required** to come from that widget's Flutter documentation, not
> from an invented persona scenario. See "🧩 Phase 2" at the bottom of this file; the template below applies
> to non-Phase-2 API additions.

**Template:**

> FloatSodaで、〔新APIを使わないと自然には解けない、ペルソナ①らしい小道具〕を作って。
> ・〔新APIの機能を必要とする具体的な要件を1〜2個〕
> （残りは固定枠：docs必読 / ブラックボックス / C#のみ・prefab禁止 / 推測API禁止 / 自己報告）

**Worked example** — PR adds a `Padding` widget:

> FloatSodaで、SteamVRの視界にメッセージカードを出すツールを作って。
> カードの中のテキストは、フチから均等に少し内側に余白を空けて配置したい
> （テキストが縁にべったり付かないように）。余白は上下左右で個別に変えられるとなお良い。

Here "余白" forces the model to find `Padding`/`EdgeInsets`. If it instead reaches for a hardcoded
`SizedBox` dance, hallucinates a `Margin` property, or asks "does the docs have a padding widget?", that's
the finding — the new API wasn't discoverable, or its shape wasn't the intuitive one.

When picking the requirement, target the *function* of the new API, then check the failure against the
four-category triage in `SKILL.md`. A "hallucination" of a cleaner shape than what the PR built is the most
valuable outcome — it's a direct argument to reshape the API before merge.

---

## 🧩 Phase 2 — per-widget gate runs must be derived from Flutter's own docs

**Applies to: the new-API PR gate during Phase 2** — i.e. a run testing a single ported widget (`Wrap`,
`FittedBox`, `CustomPaint`, …), which is every Phase 2 widget PR. For those runs the task is **not** invented:
it **must be derived from that widget's own Flutter documentation**, using `api.flutter.dev`, the widget
catalog sample, or the `flutter-widget-source` skill's pointer to the canonical implementation. Writing a
VRChatter-voice pretext that happens to require the widget is not an accepted substitute here.

The reason it's mandatory rather than merely allowed: Phase 2 ports are 1:1, so Flutter's own description is
the authoritative statement of what the widget must produce. Grounding the task there makes the run a
comparison against a fixed reference instead of against the test author's improvised idea of the widget —
which is what lets results be compared across widgets and across releases.

(Release-gate runs are unaffected — those stay app-shaped and in the persona voice. See the top of this file.)

### How to reshape a sample description into a prompt

The best raw material is the one-sentence prose intro Flutter puts above each `{@tool snippet}` — it states
the intended outcome authoritatively, in one sentence, for every widget. `Wrap`'s reads:

> This example renders some `[Chip]`s representing four contacts in a `[Wrap]` so that they flow across
> lines as necessary.

It cannot be used verbatim: it names the answer (`[Wrap]`) and depends on a Material widget FloatSoda
doesn't have (`[Chip]`, which would surface as a bogus category-ⓐ "hallucinated API" finding). Reshape it:

1. **Drop the meta-frame.** "This example renders…" → a request ("〜を作って").
2. **Delete the bracketed widget name, keep the behavior clause after it.** `[Wrap]` goes; "so that they
   flow across lines as necessary" stays — that clause *is* the spec.
3. **Restate Flutter-only widgets by appearance.** `[Chip]` → 「小さい角丸のラベル」.
4. **Keep concrete counts and data.** "four contacts" → 「4人」. Concrete output is checkable by eye.

Then **top it up from the class doc prose above the sample**, because the one sentence usually only forces
the widget's headline behavior. `Wrap`'s sample sentence forces wrapping but never `Spacing` / `RunSpacing` /
`Alignment`; those come from the paragraphs describing `alignment` / `runSpacing` / `runAlignment`. Cover the
surface the PR under test actually adds.

Worked result for `Wrap`:

> FloatSodaで、連絡先4人の名前を小さい角丸ラベルにして横に並べるパネルを作って。
> 幅に入りきらなくなったら次の行に折り返してほしい。
> ラベル同士の間隔と、行と行の間隔は別々に調整できるようにして。

### The rule that keeps it valid

- **Do not point the junior at Flutter's docs.** The Flutter source is the *test author's* input, not the
  junior's. FloatSoda mirrors Flutter's naming, so a junior reading `api.flutter.dev` would "discover"
  `RunSpacing` from Flutter rather than from FloatSoda's `WidgetSystem.md` — which is exactly the signal
  this gate exists to measure. The black-box docs pointer stays FloatSoda-only.

The VRChatter voice is dropped here; a bare layout request is what you want. What that costs is the realism
of persona #1's phrasing, which is why the **release gate** keeps the persona voice — that's the run
measuring whether a real user's fuzzy request survives the docs, and this one isn't trying to.
