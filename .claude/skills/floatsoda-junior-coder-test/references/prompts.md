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
