# Contributing to FloatSoda

FloatSoda は現在 **Alpha 段階** です。API は予告なく破壊的に変更されることがあります。この点をご理解の上、コントリビューションをお願いします。

このドキュメントは、**開発・コントリビューション規約の一次情報**です。人間のコントリビューターと、作業を担当するコーディングエージェントの双方に等しく適用されます。本文中の **必須** は MUST、**推奨** は SHOULD に相当するものとして解釈してください。

関連する各規約の置き場所は次のとおりです。同じ規約を重複して書かず、それぞれのドキュメントを参照してください。

| 目的 | 置き場所 |
|---|---|
| コードレビューの判断基準 | [REVIEW.md](REVIEW.md) |
| API 設計原則・Flutter parity | [API_DESIGN.md](API_DESIGN.md) |
| エージェントの入口・実行方針 | [AGENTS.md](AGENTS.md) |
| リリース手順 | [RELEASING.md](RELEASING.md) |
| ドキュメントの置き場所と、変更ごとに直す docs(Documentation Impact Matrix) | [WRITING_DOCUMENTATION.md](WRITING_DOCUMENTATION.md) |
| XML ドキュメントコメント | [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md) |
| 既定の言語(日本語)と例外メッセージ | [LOCALIZATION.md](LOCALIZATION.md) |

---

## 開発環境のセットアップ

- .NET 10 / C# 14 SDK
- SteamVR(サンプルアプリを実行する場合のみ必須)

```bash
# ソリューション全体のビルド
dotnet build

# 全テストの実行
dotnet test

# サンプルアプリの起動(SteamVRを起動してから)
dotnet run --project samples/Showcase/FloatSoda.Samples.OverlayApp
```

---

## Issue とラベル

### Issue が必須な変更

次の変更を行う際は、着手前に対応する Issue を作成してください(必須)。

- public API の追加・変更・削除
- observable behavior の変更
- アーキテクチャ・依存関係・プロジェクト構成に関する設計判断
- Flutter parity / divergence に関する判断
- breaking change を伴う変更

typo の修正、明白なドキュメント修正、CI や管理ファイルの局所的な保守など、設計判断を伴わない軽微な変更に限り Issue の作成を省略できます。判断に迷った場合は Issue を作成してください。

### タイトルとラベル

Issue のタイトルは「何をする Issue なのか」の記述に専念し、種別や領域などの分類は GitHub Labels で表現します(必須)。`feat(Widget):` のような Conventional Commit 風の prefix はタイトルに付けません。

ラベルは3つの軸で構成します。ラベルの一覧と説明は、[リポジトリの Labels ページ](https://github.com/sumx21t-3310/FloatSoda/labels)が一次情報です。

| 軸 | ラベル | 付け方 |
|---|---|---|
| 種別 | `feature` / `bug` / `design` / `refactor` / `performance` / `documentation` / `tracking` / `release` / `maintenance` | 原則1つ付ける(必須) |
| 領域 | `area:*` | 該当するものを複数付けてよい |
| 補助属性 | `breaking-change` / `flutter-divergence` / `needs-device-test` / `good-first-issue` / `help-wanted` | 必要なものだけ付ける |

**種別ラベルは、実装手段ではなく Issue の主目的に基づいて1つ選びます**(必須)。

- Flutter との差異を修正して既存契約へ戻す → `bug`
- 新しい利用者向け機能のために内部構造も変える → `feature`
- observable behavior を変えずに内部構造だけを整理する → `refactor`
- 性能改善が主目的で API 変更を伴う → `performance`

**領域ラベルはプロジェクト単位ではなくドメイン単位で選びます。** たとえば RenderObject 関連のコードは、物理的な配置が `src/FloatSoda` であっても `area:rendering` を指定します。

優先度ラベル(`priority:*`)は導入しません。実施時期は Milestone、作業順序は Issue の依存関係で管理し、同じ意味を持つ管理軸を重複して作りません。

---

## ブランチ・PRフロー

- `main` へ直接 push せず、必ず PR を経由してマージしてください(必須)。
- PR を作成する前に、CI(`.github/workflows/ci.yml`)と同じ手順をローカル環境で実行し、すべてパスすることを確認してください(必須)。

```bash
dotnet tool restore
dotnet csharpier check .
dotnet build --configuration Release
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Engine.Test --configuration Release --no-build
dotnet test tests/FloatSoda.OVR.Test --configuration Release --no-build
```

バグ報告や機能要望は、`.github/ISSUE_TEMPLATE/` のテンプレートを使って Issue を作成してください。PR の本文は [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md) に沿って記入してください。

### ブランチ名

人間が作成する場合もエージェントが作成する場合も、共通の規則を適用します(必須)。

```text
<issue-number>-<primary-area>-<slug>   基本形
<issue-number>-<slug>                  領域を1つに定めにくいリポジトリ横断・管理系 Issue
<slug>                                 Issue を省略できる小変更(→ Issue が必須な変更)
```

- Issue 番号を先頭に置きます。「Issue #216 の作業ブランチを探す」といった検索を前方一致で行えるようにするためです
- `<primary-area>` は、Issue の `area:*` ラベルのうち主要な領域1つを指定します。Issue 側に複数の領域ラベルがあっても、ブランチ名には1つだけを選びます
- `<slug>` は **lowercase ASCII + ハイフン**で記述します
- `feature` / `bug` などの種別や、`breaking-change` などの補助属性はブランチ名に含めません。分類の一次情報は Issue の Labels です

例:

- `224-widget-fix-flutter-divergences`
- `216-engine-post-task-runner-stop`
- `231-issue-label-migration`(管理系 Issue のため領域を省略)
- `fix-readme-typo`(Issue を省略できる小変更)

**作業を担当した AI エージェントやツールの名前をブランチ名に含めないでください**(必須)。`codex/`、`claude/`、`agent/` のようなプレフィックスは使用しません。**ブランチ名は「誰が変更したか」ではなく、「何を変更するか」を表すもの**です。

この規則は、**今後新しく切るブランチに適用します**。すでに存在するブランチを遡って改名する必要はありません。

### PR のスコープ

**Issue や PR の目的に不要な変更を混入させないでください**(必須)。作業中に関連箇所を見つけた場合でも、以下の変更を同一の PR へ勝手に含めてはいけません。

- 無関係なリファクタリング
- 無関係なリネーム
- 無関係なクリーンアップ
- 無関係な依存関係の追加・削除

これらが必要な場合は、別の Issue や PR として切り出してください。レビューの範囲が肥大化すると、本来の変更が正しいかどうかを判断できなくなるためです。

Issue の目的を成立させるために不可避な前提変更(prerequisite change)は、次の条件を**すべて**満たす場合に限り、同一の PR に含めて構いません。

- その Issue の完了に直接必要であること
- 単独では別の利用者価値や設計目的を持たないこと
- 変更範囲と理由を PR 本文から追跡できること

独立した設計判断や別の利用者価値を持つ変更は、たとえ同じ箇所を修正する場合であっても別の Issue / PR に分割します。

**public API または observable behavior を変更する場合は、breaking change の有無を明示的に判断し、PR 本文に記述してください**(必須)。「無い」と判断した場合も、その旨を明記します。判断基準は [API_DESIGN.md](API_DESIGN.md) のセクション6(バージョニングと後方互換性)にあります。**Alpha 段階であることは、判定を省略してよい理由にはなりません。** breaking と判定した上で、それを現時点で受け入れるかどうかを個別に判断します(→ 同セクション 6.4)。breaking change を伴う Issue には `breaking-change` ラベルを付けてください。

---

## テスト方針

テストには xunit を使用しています。

- `tests/FloatSoda.Test` — ジオメトリ型、RenderObject、Widget のテスト
- `tests/FloatSoda.Rendering.Test` — Layer ツリーのテスト

追加する機能に応じて、対応するテストプロジェクトにテストを追加してください。

```bash
# 特定のテストのみ実行する例
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~AlignmentTest.ComputeOffset"
```

### テストの命名

テストメソッド名は **`対象メンバー名_条件_期待結果`** の形式とし、条件と期待結果は**日本語**で記述します。クラス名とメンバー名にあたる部分は英語のままにします。

```csharp
[Fact]
public void ComputeOffset_親子が同サイズ_原点を返す() { ... }

[Fact]
public void BorderSide_Widthが負_ArgumentOutOfRangeExceptionを投げる() { ... }

[Theory]
[MemberData(nameof(PresetData))]
public void Preset_各プリセット_XYが仕様どおり(Alignment alignment, float x, float y) { ... }
```

| 要素 | 言語 | 理由 |
|---|---|---|
| テストクラス名(`DecoratedBoxTest`) | 英語 | テスト対象の型名のミラー。ファイル名と `--filter` に揃える |
| メソッド名の先頭(対象メンバー名) | 英語 | API をリネームするとき「この API のテストはどこか」を grep で辿れるようにする |
| 条件・期待結果 | 日本語 | `SameSize_ReturnsZero` のような英語圧縮では仕様の粒度が落ちる。日本語なら同じ長さで正確に書ける |
| ヘルパーメソッド・ローカル変数 | 英語 | 通常のコーディング規約に従う |

補足:

- `[Fact(DisplayName = "…")]` は**使いません**。メソッド名と説明の二重管理になり、片方だけ更新される事故を招くためです。
- 既存テストの一括リネームは行いません。新規テストからこの命名規則を適用し、既存のテストコードは別の修正で触る機会に合わせて順次寄せてください。
- この方針は、「ニュートラル = 日本語」とする設計判断([LOCALIZATION.md](LOCALIZATION.md))に基づいています。

### テスト観点

命名規則とは別に、**何を検証するか**を定めた規約です。実装を変更するときも、コードレビューを行うときも、この観点で漏れがないかを確認します。

実装を変更する際は、次のうち**その変更に該当するものをすべて考慮してください**(必須)。該当しない観点まで機械的に埋める必要はありません。

| 観点 | 内容 |
|---|---|
| representative normal behavior | 代表的な正常系。そのAPIが「普通に使われる形」 |
| boundary / degenerate inputs | 境界値と退化ケース。ゼロサイズ、空リスト、子が1つ、無限大の制約など |
| invalid inputs | 不正入力。負数・非有限値・null が、握り潰されず契約どおり例外になるか |
| state transitions | 状態遷移。プロパティ変更の前後、アニメーションの開始・停止、ジェスチャの成立・失敗 |
| tree lifecycle | ツリーのライフサイクル。mount / update / 差し替え / dispose、attach / detach |
| incremental behavior | 差分更新。dirty 化の条件、再構築・再レイアウト・再描画の範囲 |
| regression test | 再発防止。下記参照 |

原則として、**テストは実装詳細をミラーせず、observable behavior・behavioral contract・invariant を検証します**(必須)。private な内部状態や呼び出し回数をなぞるテストは、リファクタリングで壊れやすい一方で挙動の誤りを検知できないため、避けてください。

**バグ修正では regression test を原則必須とします**(必須)。元のバグを再現するテストを作成し、**修正前には失敗し、修正後に成功する**ことを確認してください。修正を元に戻すと失敗するかどうかが判定基準です。

Flutter 由来の Widget / RenderObject では、**FloatSoda 独自にテストケースを想像するだけで済ませないでください**(必須)。Flutter 本体の実装分岐・公式テスト・API contract を確認し、**対応する重要ケースを FloatSoda 側でも検証**します。Flutter が明示的にテストしている退化ケースは、FloatSoda でも同様に壊れます。

- ローカル参照クローン: `~/code_reading/flutter_reference`
- 対応する Widget / Element / RenderObject の特定: `flutter-widget-source` skill

サンプル(`samples/`)は結合テストのシナリオを兼ねています。ウィジェットを追加した場合の扱いについては、[サンプルを追加する場合の規約](#サンプルを追加する場合の規約)を参照してください。

---

## namespace とディレクトリ

**C# の namespace と、プロジェクトルート以下の物理ディレクトリ構造を一致させます**(必須)。

```
src/FloatSoda/Widgets/Layout/Stack.cs   →   namespace FloatSoda.Widgets.Layout;
tests/FloatSoda.Test/RenderObjects/…    →   namespace FloatSoda.Test.RenderObjects;
```

- **namespace を移動する場合は、ファイルも同一の変更内で対応するディレクトリへ移動してください**(必須)。
- **ディレクトリのみ、または namespace のみ単独で変更し、両者の間に不一致を生じさせないでください**(必須)。
- 例外が必要な場合は、その理由を明示してください。

現在、`src/` と `tests/` 配下の全ファイルがこの規則を満たしています。**不一致がゼロである状態を維持してください**。

`samples/` はこの規則の適用例であり、加えてディレクトリ名とプロジェクト名も一致させます(→ [サンプルを追加する場合の規約](#サンプルを追加する場合の規約))。

ファイルスコープ namespace(`namespace X;`)は、`.editorconfig` の `IDE0161` によりビルド時に強制されます。**namespace の移動やリネームを行う際は、テキストの文字列置換よりも IDE の semantic refactoring 機能を優先して使用してください**(推奨。→ [AGENTS.md](AGENTS.md) の Rider MCP)。

---

## コーディングエージェントに任せる範囲

コーディングエージェントに任せてよい範囲は、作業の種類に応じて3段階に分類しています。エージェントを利用するコントリビューターも、作業範囲を自ら判断するエージェントも、次の表に従ってください(必須)。

| 段階 | 作業 | 進め方 |
| :-- | :-- | :-- |
| 任せない | Flutter にない API の実装 | 人間が実装し、DevEx(使い心地)と設計を確かめる |
| 部分的に任せる | バグの修正 | 最初は調査だけを頼む。結果を読んでから、実装まで頼むかを決める |
| 任せてよい | Flutter のウィジェットの移植 | 全面的に任せてよい。Flutter のテストも移植すること |

---

## エージェント向け skill を追加・変更する場合

リポジトリが提供する agent skill の**唯一の一次情報は [`.agents/skills/`](.agents/skills/) です**(必須)。

- **skill の追加や変更は、必ず `.agents/skills/` 配下で行ってください**(必須)。
- `.claude/skills/` は、`.agents/skills/` を直接サポートしない Claude Code のための **compatibility fallback** です。frontmatter と一次情報へのポインタしか持たない **derived artifact であり、直接編集してはいけません**(必須)。
- 特定のエージェント(Claude / Codex / Cursor 等)向けに、**同一 skill の独立したコピーを一次情報として管理しないでください**(必須)。

### エージェント向けファイルの言語

- **[AGENTS.md](AGENTS.md) は英語で記述します**(必須)。全エージェントが毎セッション読み込む入口ファイルであり、英語のほうが指示追従性やトークン消費の節約に有利なためです。
- **それ以外のエージェント向けファイル(`.agents/skills/` 配下のスキル本文や `references/` 等)は日本語で記述します**(必須)。オーナー自身も読み書きするファイルであり、主要なコーディングエージェントは日本語を支障なく解釈できるため、英語で書く実質的なメリットがありません。識別子・型名・ファイルパス・コマンド・コードは英語のままにします。
- `SKILL.md` の frontmatter にある `description` は、エージェントがスキルの発火を判定する材料です。日本語で記述しつつ、**トリガーとなるキーフレーズは日本語と英語の両方を含めてください**(例: 「実機テスト」と "device test")。

新しい skill を追加する手順:

1. `.agents/skills/<name>/SKILL.md` に本体を記述する。参照ファイルは `.agents/skills/<name>/references/` に配置する
2. `.claude/skills/<name>/SKILL.md` に、一次情報と同じ frontmatter と、一次情報へのポインタのみを持ったスタブを配置する(既存のスタブを雛形にしてください)

skill 名は **`<project>-<subject>-<verb>` の順**で命名します(例: `floatsoda-device-test-gen` / `floatsoda-device-test-run`)。大分類の語を先頭に、動詞を末尾に置くことで、対になる skill が一覧で隣り合い、`floatsoda-device-test-` までの入力で補完候補を絞り込めるようになります。skill のディレクトリは階層をネストできない仕様(Claude Code は `.claude/skills/*/SKILL.md` の1階層のみを探索する)のため、分類はディレクトリではなく名前の接頭辞で保持します。

---

## コーディング規約

API 設計の詳細な規約は **[API_DESIGN.md](API_DESIGN.md)** を参照してください。主な要点は次のとおりです。

- コンストラクタ引数は使わず、`init` プロパティのみで構成する(オブジェクト初期化子ファースト)
- 単一の子要素は `Child`、複数の子要素は `Children`(`IList<Widget>`)とする
- ジオメトリ型は `readonly record struct`、Context や Theme 等は `record` で定義する
- `public` プロパティには XML ドキュメントコメントを付与する(**日本語**で記述する。その理由と英語化の方針は [LOCALIZATION.md](LOCALIZATION.md) を参照)
- イベントハンドラーは `Action?` / `Action<T>?` / `Func<Task>?` とし、`On` プレフィックスを付ける
- スタイル属性はコンポーネント本体ではなく、独立した `*Style` record に分離する
- ユーザーに露出する例外・診断メッセージは resx でローカライズする(ニュートラル = 日本語、`en` サテライト。手順は [LOCALIZATION.md](LOCALIZATION.md))

---

## 受け入れ範囲

現段階の FloatSoda は、Flutter の `widgets/basic.dart` に相当する **プリミティブ層のウィジェット**(単一の RenderObject に対する薄いラッパーで、状態やビジネスロジックを持たないもの)のみをコアに受け入れます。

- **対象内の例**: レイアウト系(`Align`, `SizedBox`, `Row`/`Column`, `Padding` 等)、単純な描画・クリップ系ウィジェット
- **対象外(PRをリジェクトする例)**: `Button` や `Card` のような装飾済み・複合ウィジェット、Material/Cupertino 相当のスタイル付きコンポーネント、ビジネスロジックを含むウィジェット

これは、コアが肥大化すると RenderObject / Layer / Widget-Element の三層アーキテクチャの一貫性を保てなくなるための制約です。このようなウィジェットのコントリビューションについては、コア以外の別パッケージやユーザーランドでの実装をご検討ください。

---

## 新しい public API を追加する場合の受け入れ条件

FloatSoda が第一に想定する利用者は、「コードを自分では書かず LLM に書かせる VRChatter」です(→ [AGENTS.md](AGENTS.md#target-users))。そのため public API の合否基準は、**「更新後の docs だけを読んだ LLM が、その API を正しく使えるか」** となります。単に動作する、あるいは XML ドキュメントコメントが付いているだけでは不十分であり、**LLM から見て発見しやすく、誤用しにくい設計であること**まで求められます。

新しい public API(ウィジェット、ビルダーメソッド、オーバーレイ種別、`*Style`、使い方が変わる public プロパティ等)を追加・変更する PR は、マージ前に **ジュニアコーダーテスト** を通過させてください。テストの手順はスキル [`floatsoda-junior-coder-test`](.agents/skills/floatsoda-junior-coder-test/SKILL.md) に記載されています。要点は以下のとおりです。

1. PR ブランチで **docs を先に更新**する(対象 API を説明するページ。`docs/*.md` は公開サイトの一次情報)。
2. **その新しい API を使わないと解けないお題**を用意し、低〜中エフォートのモデルに、更新後の **docs だけ**を渡して実装させる(ソースコード `src/` は見せないブラックボックス検証とする)。
3. モデルの出力結果を確認し、次のように判定する:
   - **正しく使えた** → 合格。
   - **見つけられない / 使い方を誤る** → docs の発見性か API 形状の問題。docs を直すか、API 設計を見直す。
   - **より自然な別の形を“捏造”した** → モデルが実装よりも直感的な API を書いた可能性が高い。**マージ前に API をその直感へ寄せることを強く検討**する(受け入れ前が最も低コストで修正できるため)。

この検証ゲートは、「動作の回帰(xunit)」ではなく「LLM 体験の回帰」を防ぐためのものです。テストに落ちた場合は、実装コードではなく docs や API 形状の側を修正した上で、再度テストを実行してください。

---

## サンプルを追加する場合の規約

`samples/` 配下のサンプルは、使い方の説明であると同時に、**結合テストのシナリオ** でもあります。xunit はジオメトリ・レイアウト・描画の単体テストを担いますが、「docs に書かれたとおりに記述したら、実際に意図どおり表示されるか」までは検証できません。サンプルがその役割を担います。

さらに各サンプルの `README.md` は、将来のドキュメントサイトにおけるページ本文へそのまま転用されます。ページは、**README 全文 + `<Name>Demo.cs` 全文 + XML ドキュメントコメントから生成した API リファレンス表** を連結して自動生成する想定です。**この3つを機械的に合成できる形に保つこと**が、以下の規約を設ける目的です。

### ディレクトリとファイル構成

1つのサンプルにつき1プロジェクトを作成し、`samples/<Group>/FloatSoda.Samples.<Name>/` に配置します。`<Group>` は `Widgets`(ウィジェットごとのカタログ)、`Showcase`(アプリ全体の作例)、`Experiments`(フレームワーク開発者向けの実験)のいずれかです。

| ファイル | 役割 |
|---|---|
| `Program.cs` | エントリーポイントのみ。Host の構築、表示先の切り替え、`CreateWindow`、`RunAsync`。**ウィジェットツリーを書かない** |
| `<Name>Demo.cs` | サンプル本体。`StatelessWidget` / `StatefulWidget` として実装する |
| `README.md` | チュートリアル本文。**そのままドキュメントサイトのページ本文になる** |
| `checks/items.jsonl` | 目視確認手順(JSON Lines、1行1項目)。**結合テストのシナリオそのもの**。[`floatsoda-device-test-gen`](.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルが `<Name>Demo.cs` から機械生成する。サイトへは転用しない |
| `FloatSoda.Samples.<Name>.csproj` | `TargetFramework` を明示する(`samples/Directory.Build.props` は TFM を設定しない) |

**ディレクトリ名・プロジェクト名・ルート名前空間は一致させます。** たとえば `samples/Widgets/FloatSoda.Samples.Wrap/` の場合、プロジェクトファイルは `FloatSoda.Samples.Wrap.csproj`、名前空間は `FloatSoda.Samples.Wrap` となります。`Sample` のようなサフィックスを付け足したり、言い換えたりしないでください。

作成したら、`FloatSoda.slnx` の `<Group>` に対応する `<Folder>`(`/samples/widgets/`、`/samples/showcase/`、`/samples/experiments/`)へ登録します。

参考実装は [`samples/Widgets/FloatSoda.Samples.Image`](samples/Widgets/FloatSoda.Samples.Image) です。

### 表示先の切り替え

サンプルは、`--desktop` 引数によってデスクトップウィンドウへ表示できるようにします。

```csharp
// --desktop を渡すと、SteamVRダッシュボードの代わりにデスクトップウィンドウへ表示する。
// 目視確認をモニタ上で完結させるためのもの(SteamVRの起動自体は必要)。
// Hostの構成バインダーは値を伴わない引数を解釈できないため、渡す前に取り除く。
var useDesktop = args.Contains("--desktop");
var hostArgs = args.Where(argument => argument != "--desktop").ToArray();

var builder = Host.CreateApplicationBuilder(hostArgs);

// ...

app.CreateWindow(useDesktop
    ? new DesktopWindow { Title = "Image Test", Child = demo }
    : new DashboardWindow { Dpm = new Dpm(1000), Title = "Image Test", Child = demo });
```

`DesktopWindow` は `GLFWRawPointerSource` によってマウス入力を受け取れるため、静的なレイアウトや描画の確認だけでなく、ポインタ操作の検証もモニター上で行えます。これは HMD を被る回数を減らすための規約です。

注意点が2つあります。

- **`--desktop` を指定した場合でも、HMD の接続と SteamVR の起動は必要です。** `FloatSodaApp.Initialize()` は表示先によらず OpenVR を初期化するため、HMD 未接続の状態では `Init_HmdNotFound` が発生して起動に失敗します(issue #140)。省略できるのは「被る」ことだけです。
- **デスクトップで動作したことは、ダッシュボードオーバーレイで動作することの証明にはなりません。** ウィンドウ種別ごとに入力経路が異なるためです(issue #182)。`checks/items.jsonl` は表示方法(デスクトップ/ダッシュボード)を区別しません。区別は実機巡回の結果側(`checks/results/<hmd>-<gpu>-<platform>-<mode>-<yyyy-MM-dd>.jsonl` の `<mode>` フィールド)で保持します。ポインタ操作を伴うサンプルは、実機巡回で必ず `dashboard` モードでも確認してください。

### README の構成

```markdown
# <ウィジェット名>

## これは何か        — 対応する Flutter 公式 docs のページに相当する概念説明(1〜2段落)
## 使い方            — 最小の例から段階的に機能を足す
## Flutterとの違い   — 差異があれば明記する。無ければ「同等」と1行書く(節を空にしない)
## 実行              — dotnet run コマンド2種(--desktop / ダッシュボード)
## 関連              — docs/ の該当ページ、関連サンプルへのリンク
```

`## Flutterとの違い` の節は省略しないでください。FloatSoda の docs は Flutter の語彙を用いて概念を説明するため、読者(と LLM)は Flutter と同等の挙動を期待して参照します。**挙動の差異がバグであっても意図的な設計判断であっても、利用者が払うコストは同じ**です。

### checks/items.jsonl の構成

`checks/items.jsonl` は開発者が手書きするものではなく、[`floatsoda-device-test-gen`](.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルが `<Name>Demo.cs` を読み込んで生成するファイルです。目視で判別できるデモケース(色分け・比較ペア・プロパティ値の異なるセルなど)を、1行1項目の JSON Lines で列挙します。

**スキーマ**(1行1項目、JSON Lines):
```json
{"id": "padding-1", "source": "PaddingDemo.cs:30-34", "flutter_ref": "basic.dart:2317", "question": "EdgeInsets.All(16) で四辺の余白が同じ幅に見えているか?", "read_aloud": "最初の青い四角に、左右上下同じ幅の枠があるか見て"}
```
- `id`: `<サンプル名kebab-case>-<連番>`。例: `padding-1`, `padding-2`。
- `source`: 実装の根拠となる `<Demo.csファイル名>:<開始行>-<終了行>`。
- `flutter_ref`: 対応する Flutter 本家実装の `<ファイル>:<行>`(該当する概念クラスの場所)。分からなければ空文字。
- `question`: 何を確認する項目であるかの説明文(見るべき点の説明)。
- `read_aloud`: オーナーが HMD を被ったまま聞き取り、yes/no で回答できる短い口頭指示(20〜40文字程度)。

デスクトップ表示かダッシュボード表示かは `items.jsonl` 側では区別しません。この区別は実機巡回の結果側(`checks/results/<hmd>-<gpu>-<platform>-<mode>-<yyyy-MM-dd>.jsonl` の `<mode>` フィールド、`desktop` または `dashboard`)で保持します。新規サンプルを追加する際も、既存サンプルが変更された(`<Name>Demo.cs` の行番号がずれた、ケースが増減した)際の再生成時も、[`floatsoda-device-test-gen`](.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルを使用してください。

### 禁止事項

ドキュメントサイトへの機械的な合成を成立させるため、以下の事項を守ってください。

- **README に架空のコードを書かないこと。** コードブロックは `<Name>Demo.cs` に実在する記述の抜粋とします。読みやすさのために `Child =` のような外側の文脈を省くのは構いませんが、**Demo に無い API 呼び出しやプロパティを書かないでください**。README とコードの乖離を構造的に防ぐためです。
- **`<Name>Demo.cs` を単体で読んで完結させること。** サンプル間で共有するヘルパーライブラリを作成しないでください。サンプルコードはサイトに転載され、読者がそのままコピーして利用するため、外部依存があると成立しません。`--desktop` 判定を行う数行のコードも、各 `Program.cs` にそのままコピーして配置します。
- **README に API リファレンス表を手書きしないこと。** リファレンス表は XML ドキュメントコメントから生成します。手書きしてしまうと、サンプルの数だけ同じ表が二重管理になります。

---

## アーキテクチャを理解する

FloatSoda は RenderObject ツリー / Layer ツリー / Widget-Element ツリーの三層構造を持ちます。コードを読み始める前に、まず **[docs/Home.md](docs/Home.md)** から目を通すことを推奨します。

Widget/Element 層では、`StatelessWidget` / `StatefulWidget` / `InheritedWidget` とそれぞれの Element、`BuildOwner` による差分ビルド、`MultiChildRenderObjectElement` のキー付き二端リスト差分まで実装済みです。また、レイアウト・描画・入力のウィジェット(`Padding`, `Container`, `Stack`, `Wrap`, `Opacity`, `Transform`, `GestureDetector`, `Listener` 等)や、ジェスチャ・ヒットテストも実装されています。

未実装の領域は、スクロール系ウィジェット(`ListView`, `GridView`, `SingleChildScrollView`)です。また、現状ではポインタ座標がダッシュボードオーバーレイにしか届かないため、`WorldSpaceWindow` / `DeviceTrackedWindow` では `GestureDetector` のコールバックが発火しません(issue #182)。これらの領域に貢献する場合は、事前に設計方針を Issue 等ですり合わせることを推奨します。

---

## ドキュメントを更新する場合の注意

ドキュメントの公開先は、公開サイト(<https://floatsoda.sumx21t.com>)です。サイトは `docs/*.md` をソースとして、`.github/workflows/docs-site.yml` によりビルドされます(`website/`)。そのため、ドキュメントの変更は必ず `docs/` 配下のファイルに対して行ってください。なお、GitHub Wiki への同期は #319 で停止したため、Wiki は更新されません。

## ドキュメントのトンマナ

この節では、各種ドキュメントにおける文体と、利用者向け docs のトーン＆マナー(語り口)を定義します。「文体」の規則は、`docs/` 配下、サンプルの README、Issue および PR 本文のすべてに適用します。「文体」より後ろの小節(「どう見られたいか」から「足してよい表現」まで)は、利用者向けの docs(`docs/tutorials/`、`docs/guide/`、`docs/reference/`)にのみ適用します。コントリビューター向けの docs(`docs/contributor/`)は、「文体」の規則のみを守ります。

各ページの設計シートからはこの節を参照し、設計シートに同じ内容を書き写さないでください。

### 文体

必須は「〜こと」、推奨は「〜が望ましい」で書き分けます。

- 散文は敬体で書くこと
- 箇条書きと表のセルは常体で書くこと。箇条書きは1項目を1文または体言止めにすること。2文以上になる場合は散文へ移すこと
- 条件と制約は箇条書きで示し、必須は「〜こと」、推奨は「〜が望ましい」で終えること
- 規約は、読者に求める行動として書くこと。禁止そのものが要点である場合のみ禁止形を使い、続けて代わりの行動を示すこと
- 英語の慣用句は直訳せず、日本語の言い回しに置き換えること。技術用語(deploy、revert、worktree)は英語かカタカナのまま書くこと
- 手順は「〜する」の形とし、実行する人の視点で書くこと。作業の経緯や結果は、手順とは別の節に置くこと
- チェックリストの項目は、「はい」か「いいえ」で答えられる1つの事実にすること

XML ドキュメントコメントと例外メッセージは、この節ではなくそれぞれの規約に従います。

### どう見られたいか

利用者向けの docs は、読み手に「自分の欲しい UI も、これなら簡単に形にできそう」と感じてもらえる状態を目指します。書き終えたページは、下の「失敗のセリフ」に当てはめて読み直してください。当てはまるセリフがあれば、「読み手」の小節に戻って書き直します。

| 読み手にこう言われたら成功 | 読み手にこう言われたら失敗 |
| :-- | :-- |
| 使ってみようかな | 結局これって何? |
| 作りたかった UI が、思ったより少ないコードで書けた | なんか難しそうな物作ってますね |
| Flutter みたいに書けるから、迷わずすぐ形になった | なんかめんどくさそう |
| シンプルなのに、見た目もちゃんときれい | シンプルって言うけど、結局ボイラープレートが多い |
| | 簡単に書けるけど、凝ったことをしようとすると詰む |

### 読み手

利用者向け docs の読み手は、[AGENTS.md](AGENTS.md) の Target Users に記載されている3つのペルソナです。この小節では、ペルソナに共通する性質と用語の扱いについて補足します。ペルソナとこの小節の記述が食い違って見える場合は、ペルソナに従ってください。

- UI を自作したい、VR 好きの個人開発者。VRChat 向けのツールを作り、Booth で販売したい
- 「めんどくさそう」と感じるしきい値が低い。準備や手順が多く見えた時点で離脱する
- 入り口が2つある。Unity だけで C# を書いてきた人と、React や Flutter のような宣言的 UI を触ってきた人

専門用語は、それを知らない側の読み手が知っている言葉に結びつけること。Unity の開発者にはシーンやプレハブ、宣言的 UI の開発者には React や Flutter の言葉に結びつけます。ほかの分野の用語を比喩として借りず、そのまま説明すること。定義どおりに使う用語(不変条件、冪等)は、そのまま使って構いません。

### 語彙

次の表の左の語を書くときは、右の言い換えを使うこと。

| 語 | 言い換え | 適用する範囲 |
| :-- | :-- | :-- |
| 実現する | 形にする | 利用者向けの docs |
| 簡単に〜できます | コードや手順の短さで示す。示せなければ「書けます」にとどめる | 利用者向けの docs |
| 正典 | 一次情報、置き場所 | すべての文書 |
| 担保する | 保証する | すべての文書 |
| Litmus test | 判定基準 | すべての文書 |
| 経路 | 読ませ先、手順、流れ。「入力経路」のように信号の道筋を指す使い方は残す | すべての文書 |
| 直交する | 無関係。`Wrap` の交差軸のように幾何の定義どおりの使い方は残す | すべての文書 |

利用者向けの docs では、「形にする」「書ける」「ウィジェット」「組み合わせる」を使うことが望ましいです。「すべての文書」の行は Issue #262 で定めました。機械による検査は Issue #260 で扱います。

### 人柄を守る条件

利用者向けの docs は、次の4つの条件を満たすこと。

| 人柄 | 条件 |
| :-- | :-- |
| こだわって作っているが、何なのかが分かりにくくない | 各見出しの最初の段落で、その見出しが扱うものが何なのかを書く |
| 中の仕組みは深いが、難しそうには見せない | 入口のページ(チュートリアル、「FloatSoda とは」)では、C# と .NET のような技術の中の区別を持ち出さない |
| できることは多いが、準備は多くない | 準備の手順の数を、見出しの数で見えるようにする |
| シンプルだが、凝ったことをしても行き詰まらない | 基本の手順のあとに、応用(関連する How-to か API リファレンス)へのリンクを置く |

### 足してよい表現

利用者向けの解説ページでは、次の2つの表現を足して構いません。どちらも1つの見出しにつき1つまでとします。

- 読み手の「こんな〇〇が欲しい」を言葉にする一文。その見出しで「何なのか」を書いた文のあとに置く
- 作るものの具体例(VRChat 向けのツールなど)。ページの材料に書かれている例だけを使う

---

## PRを出す前のチェックリスト

- [ ] `dotnet build` が通る
- [ ] 該当するテストプロジェクトにテストを追加・更新した
- [ ] `dotnet test` が通る
- [ ] 追加した `public` プロパティに XML ドキュメントコメントを付けた
- [ ] ドキュメントの変更は `docs/*.md` に対して行った
- [ ] 散文は敬体、箇条書きは常体で書いた(→ [ドキュメントのトンマナ](#ドキュメントのトンマナ) の「文体」)
- [ ] プリミティブ層(basic.dart相当)を超える複合ウィジェットを追加していない
- [ ] **新しいウィジェットを追加した場合**、サンプル(README + `checks/items.jsonl`)を追加した(→ [サンプルを追加する場合の規約](#サンプルを追加する場合の規約))。`items.jsonl` は [`floatsoda-device-test-gen`](.agents/skills/floatsoda-device-test-gen/SKILL.md) スキルで生成する
- [ ] **新しい public API を追加した場合**、docs を更新し、ジュニアコーダーテスト([`floatsoda-junior-coder-test`](.agents/skills/floatsoda-junior-coder-test/SKILL.md))を通した
