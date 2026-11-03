# API Design Guidelines

このドキュメントでは、本フレームワークのコンポーネントAPIを設計・実装する際の規約とベストプラクティスを定めます。

## 1. 基本設計哲学

本フレームワークは、Flutterのウィジェットツリーモデルを参考にしています。**宣言的UI**、**イミュータブルな構成**、**ツリー構造による合成**を設計の中心に据えています。

C# のオブジェクト初期化子構文を活用し、マークアップに近く読みやすいUIコードを実現します。

### 判断原則: 概念とツリーの語彙は Flutter に、その表現手段は C# に従う

Flutter を参考にする範囲と、C# の慣習を優先する範囲は、次のように使い分けます。

- **UIドメインの語彙の層** — Flutter に準拠します。ウィジェット名やレイアウト用語（`MainAxisAlignment`, `CrossAxisAlignment`）、`Child` / `Children` などのツリー構造を表す語彙は、Flutter の名前をそのまま使います。Flutter を参照する利用者やコード生成AIが自然に書いたコードを、そのまま正解として扱えるようにするためです。
- **言語機構・実装パターンの層** — C# として自然な API や慣習がある場合は、そちらを優先します。Dart の言語制約に由来する実装パターン（例: `addListener` / `removeListener`）は直訳せず、C# の言語機構（`event`, `init`, `required`, `record struct`, `extension` プロパティなど）を用いて同じ意図を表現します。

本ドキュメントに記載された個別ルールの多くは、この原則を適用したものです。リスナーパターンではなく `event` を採用する（セクション3.6）、`is null` パターンを用いる（3.5）、ジオメトリを `record struct` で表現する（8）、単位リテラルに拡張プロパティを使う（7.5）、`init` / `required` によってイミュータビリティと必須値を表現する（4・5）などが該当します。

ただし、C# の一般的な慣習を常に優先するわけではありません。フレームワークとしての読みやすさを重視し、意図的に慣習から逸脱する場合はその理由を明記します（例: 兄弟ウィジェットの同一ファイル配置は StyleCop SA1402 から意図的に外れる — セクション2.5）。

### 判断原則: .NET が標準で提供する機構を再実装しない

前述の原則の後半（表現手段は C# に従う）を、客観的に判定できる手順に落とし込んだものです。「C# として自然か」という判断は主観に偏りやすいため、次の手順で判定します。

1. その機構と **同等のもの** を .NET が標準で提供していないか確認します。
2. 提供されていれば、それを使います。Flutter 側に該当する独自実装があっても移植しません。
3. なければ自前で実装し、**なぜ同等でないのか** をドキュメントコメントに残します。

ここでの「標準」は厳密な BCL に限らず、.NET が標準として提供する機構（`Microsoft.Extensions.*` を含む）を指します。

適用例:

| Flutter / 独自実装に流れがちな機構 | .NET の同等機構 | 判断 |
|---|---|---|
| `addListener` / `removeListener` | `event` | 移植しない（セクション3.6） |
| `operator==` / `hashCode` の手書き | `record` / `record struct` の値等価性 | 実装しない（セクション8・9） |
| HTTP のリトライ・レート制限 | `Microsoft.Extensions.Http.Resilience`(`DelegatingHandler` として組み込む) | 実装しない。**設定済みの `HttpClient` を呼び出し元から受け取り、ポリシーは呼び出し元の責任とする** |
| HTTP の同時接続数制御 | `SocketsHttpHandler.MaxConnectionsPerServer` | 実装しない |
| 処理を直列化するための専用スレッド | `Channel<T>` と単一の reader ループ | 立てない。専用スレッドはスレッド親和性が要求される場合（GL コンテキスト等）に限る |

上記の表の最後の行で `Channel<T>` と reader ループの組み合わせだけを挙げているのは、**「投入順に1件ずつ」まで満たせる標準機構が他にない**ためです。一見似ている機構であっても、保証する契約が異なります。

| 機構 | 保証すること | 保証しないこと |
|---|---|---|
| `Channel<T>` と単一の reader ループ | 投入順での取り出し。ループが各要素の処理を `await` し切ってから次を読むことで1件ずつになる | — |
| `Task` とスレッドプール | — | 排他実行も順序も保証しない |
| `SemaphoreSlim(1, 1)` | 排他実行 | **待機の順序**。先に待った側が先に通るとは限らない |
| `ConcurrentExclusiveSchedulerPair.ExclusiveScheduler` | 同期的なタスク本体どうしの排他 | **`await` をまたぐ排他**（継続は別タスクとして再スケジュールされる）と順序 |

`Channel<T>` が保証するのは、あくまで**投入順での取り出し**までです。1件ずつ処理されるかどうかは reader 側の書き方で決まります。`SingleReader = true` は「呼び出し側が単一リーダーを守る」ことを前提にした最適化指示に過ぎず、複数リーダーによる読み出しや並行処理を禁止しません。**単一の reader ループが各要素の処理を `await` し切ってから次を読む**形にして初めて、直列化が成立します。なお、producer が複数ある場合、ここでの「投入順」は enqueue が成立した順を指します。

`IOTaskRunner` のように「投入順に1件ずつ」を契約しているものを、排他実行だけを保証する機構へ置き換えると、順序が逆転して破綻します。**要件が排他だけなのか、順序まで含むのかを先に確定させてください。**

#### 「同等」の判定

この原則の難しさは、「同等」の判定にあります。**形が似ているだけで契約が違うものを同等とみなすと、かえって破綻を招きます。** 同等かどうかは API の形ではなく、**満たすべき契約が一致するか** で判定します。

非同等と判定した具体例として、画像キャッシュに `Microsoft.Extensions.Caching.Memory` を使えるか検討した場合を挙げます。LRU と容量上限は満たせますが、**「参照中のエントリは容量に関係なく退避しない」という動的な契約**を表現できません。

似た機能として `CacheItemPriority.NeverRemove` がありますが、これは**エントリ登録時に決まる静的な優先度**です。実際に必要なのは、「貸出が1つ以上ある間だけ退避を禁止し、最後の貸出が返された時点で通常の退避対象へ戻す」という、参照カウントに追従して動的に切り替わる契約です。`NeverRemove` と既定値を出し入れして模倣することは理論上可能ですが、切り替えの合間に退避が走る隙間が残るため、契約を満たせません。

したがって同等とはみなせず、自前で実装したうえでその理由を残します。

#### 依存関係の増減とは独立

この原則は「依存関係を増やさない」という意味ではありません。標準機構を使うために依存が一つ増えることも、標準機構で足りるために既存の依存を削減できることも、どちらも正しい結果です。判断の対象とすべきは依存の数ではなく、**同じものを二度実装していないか** です。

### 判断原則: Flutter 由来の observable behavior に差異を作らない

最初の原則の前半（語彙は Flutter に従う）の対象を、**挙動**にまで広げた原則です。

**Flutter 由来の Widget / RenderObject は、明示された FloatSoda 固有の理由がない限り、Flutter との observable behavior に差異を設けません。**

FloatSoda の docs では、概念の説明に Flutter の語彙を用いています。読者やコード生成 AI は Flutter と同じ挙動を期待して利用するため、**その差異がバグによるものか意図的な設計判断によるものかにかかわらず、利用者が負うコストは同じ**です。

少なくとも次の項目は parity の対象です。

- property semantics
- default values
- layout
- paint / clipping
- hit testing
- child handling
- Widget update behavior
- invalid / degenerate input handling
- dirty layout / paint conditions
- Element / state lifecycle semantics

#### Flutter-derived の判定

本原則を適用する前提となる、「何が Flutter 由来か」についての判定基準です。

- Flutter に対応する public concept / Widget / RenderObject / lifecycle semantics が存在し、それを FloatSoda へ持ち込むものは **Flutter-derived** として扱い、本原則を適用します。
- Flutter の型やアルゴリズムを内部実装として利用している場合でも、FloatSoda 独自の利用者向け概念や API であれば、その API 自体は **FloatSoda 固有**として扱います (例: オーバーレイ種別)。
- Flutter-derived かどうかの判断が分かれる場合は、実装を開始する前に Issue 上で基準 (対応する Flutter API か、FloatSoda 固有か) を明示します。

#### 理由にならないもの

**「実装しやすい」「こちらのほうが安全」「こちらのほうが自然」といった理由だけで、独自仕様にしないでください。** こうした理由は差異を正当化する根拠にはなりません。Flutter がその挙動を採用した背景（多くは実際のアプリで発生した事故）を、FloatSoda がわざわざ再発見する必要はありません。

#### behavioral difference とみなさないもの

C# や .NET として自然な表現への置き換えは、差異とはみなしません。

- `event`
- `init` / `required`
- `record` / `record struct`
- .NET 標準機構の利用

これは直前にある2つの原則（表現手段は C# に従う／標準機構を再実装しない）の裏返しです。判断に迷った場合は、**利用者から見た挙動が変わるかどうか**で切り分けてください。たとえば `addListener` を `event` に置き換えても、購読と解除という観測可能な挙動は同じです。

#### 既に決めた非移植方針を壊さない

FloatSoda のランタイムや対象ユーザーに存在しない機能について、**すでに明示された非移植方針を parity を理由に覆さないでください。** 特に Semantics（次節）との整合性を確認してください。「Flutter にあるから」という理由は、次節の判断を覆す根拠にはなりません。

#### 差異が必要な場合の記録義務

差異が必要だと判断した場合は、**次の5点を恒久的に記録します**。

1. **Flutter の挙動**
2. **FloatSoda の挙動**
3. **差異が必要な理由**
4. **差異を固定するテスト**（ファイルとテストメソッド名）
5. **利用者に影響する場合のドキュメント**

記録先は [`known-divergences.md`](.agents/skills/floatsoda-device-test-gen/references/known-divergences.md) です。このファイルが **FloatSoda と Flutter の確認済み差異を管理するマスター台帳**であり、本ドキュメントには判断原則のみを記載します。

5 について、利用者から見える差異は台帳に記録するだけでは不十分です。該当する `docs/` のページと、対応するサンプルの `## Flutterとの違い` 節（→ [CONTRIBUTING.md](CONTRIBUTING.md)）にも記載してください。台帳はコントリビュータ向け、docs やサンプルは利用者向けです。

4 がない差異は、次の移植で気づかないまま元に戻されてしまいます。**テストで固定されていない差異は、記録されていないのと同じ**と考えてください。

#### 参照の追跡可能性

Flutter を参照して移植や修正を行う場合は、可能な範囲で次の情報を **PR から追跡可能にしてください**（推奨）。PR テンプレートの `## Flutter reference` 欄がこれに該当します。

- Flutter version または commit
- 参照した Flutter source
- 参照した Flutter tests
- 関連する Flutter API documentation

ローカルの参照用クローンは `~/code_reading/flutter_reference` です。対応する Widget / Element / RenderObject の特定には `flutter-widget-source` skill が利用できます。

#### 仕様が食い違ったときの優先順位

既存の実装を無条件に正しいものとして扱わないでください。**上から順に**確認します。

1. **FloatSoda で明示的に定義された差異・設計判断** — 本ドキュメント、台帳、`docs/` への明記
2. **Flutter の仕様・実装・公式テスト** — 1 に該当する記述がなければ、Flutter を基準とします。
3. **既存の FloatSoda 実装は根拠になりません** — そう実装されていること自体は、それが正しい根拠にはなりません。

**古い Issue や既存の実装だけを根拠にして、新しい挙動を決めないでください。** Issue が作成された当時の前提が、現在も成り立っているかを確認してください。

### 判断原則: 対象ユーザー・ランタイムに不要な Flutter API は移植しない

Flutter のすべての API が FloatSoda に必要なわけではありません。対象ユーザー (→ [AGENTS.md の Target Users](AGENTS.md#target-users)) における需要がなく、かつ FloatSoda のランタイム (Skia → OpenGL → OpenVR オーバーレイテクスチャ) に受け皿がない機能は移植しません。Flutter 由来のコードからフックだけを先行して配置することはせず、削除します。

#### 実装しない API — Semantics (アクセシビリティツリー)

Flutter の `SemanticsNode` / `SemanticsConfiguration` / `PipelineOwner.semanticsOwner` に相当する**並行ツリー**は、FloatSoda では実装しません。

理由:

- **出力先がない** — VR オーバーレイテクスチャには、OS ネイティブのアクセシビリティ API (Windows Narrator / UI Automation / macOS VoiceOver など) に接続する適切なフックがありません。SteamVR / OpenVR にもスクリーンリーダーと統合する仕組みは存在しません。
- **ターゲットユーザーに需要がない** — FloatSoda の3ペルソナ (バイブコーディング VRChatter / Booth 創作者 / uGUI 回避エンジニア) の要件に、アクセシビリティ需要は含まれていません。
- **部分導入は無意味** — `markNeedsSemanticsUpdate()` だけを実装しても、`SemanticsNode` ツリーと `SemanticsOwner` がなければ実質的に no-op となります。本格的に導入する場合は Semantics ツリー全体の設計が必要となるため、その時点で改めて設計し直すほうが自然です。

したがって、Flutter 本家から移植する際は、以下のような API やプロパティは **持ち込みません**。

- `RenderObject.markNeedsSemanticsUpdate()`
- `describeSemanticsConfiguration(SemanticsConfiguration config)`
- `SemanticsConfiguration` / `SemanticsNode` 系の型
- ウィジェット側の `ignoringSemantics` / `excludeSemantics` / `semanticContainer` などのフラグ

将来、VR 空間内でアクセシビリティ情報を表現する適切な方法が見つかった時点で、個別に設計します。それまでは、Flutter 側コードの semantics 関連の呼び出しは移植せずに削除することを規約とします。

```csharp
// 推奨: オブジェクト初期化子によるツリー構造
var ui = new Column
{
    Children =
    [
        new Text("Hello, World!"),
        new FloatSoda.UI.Cream.Button
        {
            Child = new Text("Click me"),
            OnPressed = HandleClick,
            Style = new FloatSoda.UI.Cream.ButtonStyle
            {
                BackgroundColor = Color.FromArgb(0xFF6495ED)
            }
        }
    ]
};
```

## 2. コンポーネントAPI設計ガイドライン

### 2.1 オブジェクト初期化子ファーストの原則

すべてのコンポーネントは、**コンストラクタ引数を使わず**、オブジェクト初期化子だけで完全に構成できるよう設計します。

```csharp
// ✅ 良い例: 初期化子のみで完結
var card = new Card
{
    Title = "タイトル",
    Body = new Text("本文"),
    Elevation = 4
};

// ❌ 避ける: 複雑なコンストラクタ引数
var card = new Card("タイトル", new Text("本文"), 4);
```

**理由:**
- コードのツリー構造を視覚的に把握しやすいため
- 引数の順序を覚える必要がないため
- 将来プロパティが追加された際にも、後方互換性を保ちやすいため

#### 例外: 末端ウィジェットの単一値コンストラクタ

**末端ウィジェット**に限り、主たる値1つを受け取るコンストラクタを容認します。末端ウィジェットとは、次の**両方**を満たすものです。

1. **ウィジェットを引数(`Child` / `Children`)に取らない** — 他のウィジェットを合成するのではなく、それ自体がツリーの末端(葉)になる
2. **データそのものが表示対象である** — 文字列・グリフ・画像・数値など「表示される値そのもの」を保持している(レイアウトの指示は該当しない)

```csharp
// ✅ 容認: Text はデータそのものが表示対象で、子を取らない
new Text("Hello, World!")
```

**現在該当する例:** `Text(string)`、`Icon(IconData)`。将来 `ProgressBar` などの末端ウィジェットを追加する場合も、この基準を適用します。
**該当しない例:** `Padding` / `Container` (他のウィジェットを子に取るため)、`Spacer` (`size` はレイアウトの指示であり、表示データではないため)。

**付帯ルール:**

- コンストラクタ引数は**主たる値1つのみ**とします。スタイルやオプションは、従来どおり `init` プロパティで受け取ります。
- コンストラクタを持つ末端ウィジェットは、その引数以外に `required` メンバーを**持たない**ようにします。追加のスタイルやオプションが必要な場合は `init` プロパティで受け取ります。
- 情報源が曖昧なもの(`Image` におけるパスかプロバイダかなど)は、コンストラクタを増やさず、型付きプロバイダで受け取ります(現行 API は `new Image { Provider = new FileImageProvider(path) }`。`FileImageProvider` は `FloatSoda.Core.Providers` 名前空間)。将来ショートカットを追加する場合は、[セクション7](#7-ファクトリメソッドの方針)に従って静的ファクトリとして提供します。

**理由:**

- `Text` の本体は文字列そのものであり、`Icon` の本体はグリフそのものであるため、位置引数の意味が曖昧になる余地がありません。これにより、初期化子ファースト規約の目的である「引数の意味の自明性」を損なわずに済みます。
- 末端ウィジェットは `Children` の深い階層に多数出現するため、`new Text("OK")` と簡潔に記述できることによる視覚的なノイズ削減効果が極めて大きくなります。
- Flutter においても末端ウィジェットは `Text('hello')` や `Icon(Icons.add)` の形で提供されているため、Flutter に親しんだ利用者やコード生成AIが自然に書くコードを、そのまま正解として受け入れられます。

### 2.2 子要素の表現

単一の子要素を持つコンポーネントには `Child` プロパティを、複数の子要素を持つコンポーネントには `Children` プロパティ(`IList<Widget>` 型)を使用します。

```csharp
// 単一の子（SizedBox）
new SizedBox
{
    Width = 240,
    Height = 80,
    Child = new Text("固定サイズ内のテキスト")
}

// 複数の子
new Row
{
    MainAxisAlignment = MainAxisAlignment.SpaceBetween,
    Children =
    [
        new Text("ホーム"),
        new Text("設定")
    ]
}
```

### 2.3 ネストの深さと可読性

ツリーのネストが深くなる場合は、**ローカル変数への分割**によるフラット化を推奨します。

```csharp
// ✅ 推奨: 変数に切り出してフラット化
var avatarPlaceholder = new SizedBox { Width = 40, Height = 40 };
var nameLabel = new Text(user.Name);
var emailLabel = new Text(user.Email);

var userInfo = new Column
{
    Children = [nameLabel, emailLabel]
};

var tile = new Row
{
    Children = [avatarPlaceholder, new SizedBox { Width = 12 }, userInfo]
};
```

### 2.4 スタイルの分離

見た目に関する属性はコンポーネント本体に持たせず、専用の `*Style` クラスへ分離します。`*Style` レコードや `Button` などのスタイル付きコンポーネントは、デザインシステム層(`FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`)に配置します(→ [UI 層](docs/contributor/2-understand/ui-layer.md))。

```csharp
new FloatSoda.UI.Cream.Button
{
    Child = new Text("送信"),
    OnPressed = OnSubmit,
    Style = new FloatSoda.UI.Cream.ButtonStyle
    {
        BackgroundColor = Color.FromArgb(0xFF6495ED),
        PressedBackgroundColor = Color.FromArgb(0xFF4169E1),
        DisabledBackgroundColor = Color.FromArgb(0xFFD3D3D3)
    }
}
```

### 2.5 ファイル構成 — 兄弟ウィジェットは同じファイルに置く

密接に関連するウィジェット群(**兄弟ウィジェット**)は、1型1ファイルに細かく分割せず、同一ファイルにまとめます。

```csharp
// Flex.cs — Flex とその薄い特殊化をまとめて定義
public sealed record Flex : MultiChildRenderObjectWidget<RenderFlex> { /* ... */ }

public abstract record FlexWrapper(Axis Direction) : StatelessWidget { /* ... */ }

public sealed record Column() : FlexWrapper(Axis.Vertical);

public sealed record Row() : FlexWrapper(Axis.Horizontal);
```

**兄弟とみなす基準(以下のいずれかを満たす場合):**

1. 同一ファイル内にある共通基底に対する、薄い特殊化である(`Column` / `Row` → `FlexWrapper`)
2. 単独では成立せず、必ず対で使用する(`Stack` + `Positioned` のような関係)
3. 同じ実装詳細(private なヘルパーメソッドや共通の RenderObject)を共有している

**兄弟とみなさない基準:** 単に同じカテゴリや同じフォルダに属しているだけの場合は該当しません。たとえば `Padding` と `SizedBox` はどちらも Layout に属しますが、それぞれ独立したファイルに分割します。

**ファイル名の規則:**

| 状況 | ファイル名 | 例 |
|---|---|---|
| 基底・代表となるウィジェットがある場合 | 代表となるウィジェット名 | `Flex.cs`, `Text.cs`, `Align.cs` |
| 対等なグループで代表を決めにくい場合 | グループを表す概念名 | `Clip.cs`(`ClipOval` / `ClipRect` / `ClipRoundRect` / `ClipCustomPath`) |

**理由:**

- `Column` や `Row` のような数行のレコードを個別のファイルに分けるよりも、基底定義と並べて読めるようにしたほうが、「薄いラッパーである」という設計意図が明確に伝わります。これは、Flutter の `basic.dart` が関連ウィジェットをひとまとめにしている思想と同じです。
- C# の一般的な慣習である「1型1ファイル」(StyleCop SA1402)からは意図的に逸脱しています。本フレームワークのウィジェットには小さな `record` が多く、機械的に分割するとファイル数ばかりが増加し、コードベース全体の見通しがかえって損なわれるためです。

## 3. プロパティ命名規則

| 種別 | 規則 | 例 |
|---|---|---|
| コンテンツ系 | 意味のある名詞 | `Content`, `Label`, `Title`, `ImageUrl` |
| 子要素 | `Child` / `Children` | `Child`, `Children` |
| イベントハンドラ | `On` + 動詞 (PascalCase) | `OnPressed`, `OnChanged`, `OnSubmit` |
| ブール型フラグ | 接頭辞 `Is` / `Has` / `Can` | `IsEnabled`, `HasBorder`, `IsVisible` |
| スタイル | 接尾辞 `Style` | `TextStyle`, `ButtonStyle` |
| レイアウト | Flutter に準拠した名前 | `MainAxisAlignment`, `CrossAxisAlignment` |

### イベントハンドラの型

```csharp
// 引数なし
public Action? OnPressed { get; init; }

// 値を渡す場合
public Action<string>? OnChanged { get; init; }

// キャンセル可能な非同期処理
public Func<Task>? OnSubmitAsync { get; init; }
```

### 入力語彙は「ポインター (Pointer)」で統一する

FloatSoda で定義する型、メンバー、ドキュメントでは、入力デバイスを表す語彙として「マウス (Mouse)」を使用せず、「ポインター (Pointer)」に統一します。VR 環境のレーザーポインターとデスクトップ環境のマウスはどちらも同じ役割を担うため、特定のデバイスに依存しない語彙を採用します(Flutter の `PointerEvent` 系の設計とも一致します)。

```csharp
// ✅ 良い例: FloatSoda が命名するものはすべて Pointer
public interface IRawPointerSource;
public enum PointerButton { Left, Middle, Right }

// ❌ 悪い例: FloatSoda の公開APIに Mouse を持ち込む
public enum MouseButton { Left, Middle, Right }
```

**例外**: 外部 API(OpenVR / GLFW)の固有名詞をそのまま使用する層は対象外とします。たとえば `FloatSoda.OVR` の `SetMouseScale` のように、下位 API(`SetOverlayMouseScale`)との対応関係が明確であることに価値がある薄いラッパーでは、元の語彙を維持します。

ドキュメントコメントの説明文においても、「マウス」と単独で表記せず、「ポインター(レーザーポインター / マウス)」のように記述します(→ [DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md))。

## 3.5 null チェックは `is null` / `is not null` を使う

参照の null 判定には `==` / `!=` 演算子を使用せず、パターンマッチングの `is null` / `is not null` を使用します。

```csharp
// ✅ 推奨
if (Child is null) return;
if (Child is not null) context.PaintChild(Child, offset);

// ❌ 避ける
if (Child == null) return;
if (Child != null) context.PaintChild(Child, offset);
```

**理由:**
- `==` / `!=` 演算子はユーザー定義のオーバーロードに解決される可能性があり、意図しない比較ロジックが実行される恐れがあります。一方、`is null` は常に参照の同一性(厳密な null 判定)を評価するため、型の実装に左右されず安全です。
- `is null` / `is not null` を使用することで、「null かどうかを判定している」というコードの意図を明確に表現できます。
- 等値演算子をオーバーロードする `record` や `record struct`(セクション8・9)を多用する本フレームワークでは、この挙動の差異が特に問題になりやすいためです。

> **補足:** 値型(`record struct` など)同士の比較や、null 以外の値との比較には従来どおり `==` / `!=` を使用します。本ルールは、あくまで**参照の null 判定**に限定した規約です。

## 3.6 リスナーパターンを実装せず `event` を使う

マルチキャストデリゲート(`event`)で表現できる通知には、Flutter 流のリスナーパターン(`AddListener` / `RemoveListener` メソッドやリスナー用インターフェース)を独自に実装せず、C# 標準の `event` を公開します。

```csharp
// ✅ 推奨: event による通知
public class AnimationController
{
    public event Action? Changed;
    public event Action<AnimationStatus>? StatusChanged;
}

// ❌ 避ける: リスナーパターンの独自実装
public interface IAnimationListener { void OnChanged(); }

public class AnimationController
{
    public void AddListener(IAnimationListener listener) { /* ... */ }
    public void RemoveListener(IAnimationListener listener) { /* ... */ }
}
```

**理由:**
- Dart の `addListener` / `removeListener` は言語組み込みのイベント機構がないために必要な設計ですが、C# ではマルチキャストデリゲートが同等の機能を言語レベルで提供しています。これにより、リスナーリストの管理や通知処理中の購読解除に伴う安全性(invocation list のスナップショット)も、独自に実装することなく得られます。
- `+=` / `-=` による購読と解除は、C# 開発者にとって最も予測しやすい API です。
- 購読側に専用インターフェースの実装を強制せず、ラムダ式やメソッド参照を直接渡せます。

**適用範囲の整理:**

| 対象 | 使用するもの |
|---|---|
| Widget のコールバック(単一ハンドラを `init` で受ける) | 接頭辞 `On` を付与した `Action?` プロパティ(セクション3) |
| 長寿命のミュータブルなオブジェクト(Controller 等)からの通知(購読者が複数・動的に増減) | `event` |

**補足:**
- 通知元の共通抽象を定義する場合も、リスナー側ではなく通知元側のインターフェースに `event` を宣言します(例: `interface IListenable { event Action? Changed; }`)。
- ラッパー型が親の通知をそのまま中継する場合は、カスタムイベントアクセサ(`add` / `remove`)を使って親の `event` に委譲することで、購読の付け替えミスや解除漏れを防止できます。
- `Dispose()` ではイベントフィールドに `null` を代入し、すべての購読を破棄します。

## 4. イミュータビリティと `init` アクセサ

すべてのプロパティは原則として `init` アクセサを使用し、オブジェクト構築後の変更を禁止します。状態変化はフレームワークの状態管理レイヤーに委ねてください。

```csharp
public record Text : Widget
{
    public string Content { get; init; } = string.Empty;
    public double FontSize { get; init; } = 14;
    public Color Color { get; init; } = Colors.Black;
    public FontWeight FontWeight { get; init; } = FontWeight.Normal;
}
```

## 5. デフォルト値の方針

- すべてのプロパティに**合理的なデフォルト値**を設定し、最小限の記述でコンポーネントを使用できるようにします。
- 必須プロパティは `required` キーワードで明示します。

```csharp
public record AspectRatio : Widget
{
    public required double Ratio { get; init; }  // 必須（幅 ÷ 高さ。妥当なデフォルトが存在しない）
    public Widget? Child { get; init; }          // null = 子なし
    public Alignment Alignment { get; init; } = Alignment.Center;  // デフォルトあり
}
```

> **コンストラクタを持つ末端ウィジェットの必須値は `required` にしない:** 上記の `AspectRatio` はウィジェット(`Child`)を子に取る合成ウィジェットであるため、オブジェクト初期化子と `required` の組み合わせで必須値を表現します。一方、`Text` / `Icon` のような**コンストラクタを持つ末端ウィジェット**は、必須値をコンストラクタで受けるため `required` を使用しません(→ [セクション2.1 の例外](#例外-末端ウィジェットの単一値コンストラクタ))。`Image` は情報源が曖昧なためコンストラクタを持たず、`Provider` を `required` で受けます。両者で必須値の受け取り方が異なる点に注意してください。

## 6. バージョニングと後方互換性

### 6.1 非破壊的変更（マイナーバージョン）

- 新しいプロパティの追加(デフォルト値あり)
- `required` でないプロパティのオプション化

### 6.2 破壊的変更（メジャーバージョン）

- プロパティの削除やリネーム
- プロパティの型変更
- `required` の追加
- **observable behavior の変更** — 既存の正しい利用コードから観測できる挙動の変化。layout、paint / clipping、hit testing、Widget update behavior、invalid / degenerate input handling、Element / state lifecycle semantics など、セクション1の parity 対象リストに挙げた挙動が判定の観点となります。

observable behavior に関しては、**互換性の基準を文書化された契約**(本ドキュメント、docs、Flutter parity)とします。契約から逸脱していた挙動を本来の契約へ戻す修正は、挙動が変化しても破壊的変更ではなくバグ修正として扱います。反対に、契約どおりに動いていた挙動を変更する場合は破壊的変更となります。

### 6.3 廃止予定プロパティの扱い

```csharp
/// <summary>テキストの色を指定します。</summary>
[Obsolete("TextStyle.Color を使用してください。v3.0 で削除予定です。")]
public Color? TextColor { get; init; }
```

### 6.4 Alpha 段階での判定と許容の分離

Alpha 段階の開発中であることは、breaking change の**判定**を省略する理由になりません。判定と許容は分けて行います。

1. まず public API / observable behavior に対して、6.1 と 6.2 の基準で breaking change に該当するかを通常どおり判定します。
2. そのうえで、Alpha 段階としてその変更を現時点で受け入れるかを別途判断します。

「Alpha 版だから breaking change ではない」という扱いはしません。判定の結果は PR の本文に記載し(→ [CONTRIBUTING.md](CONTRIBUTING.md))、breaking change を伴う Issue には `breaking-change` ラベルを付与します。

## 7. ファクトリメソッドの方針

プライマリコンストラクタ(`record struct` のポジショナル構文)以外でインスタンスを生成する場合は、**静的ファクトリメソッド**を提供します。オブジェクト初期化子だけでは表現しにくい「よく使うプリセット」や「導出パターン」をファクトリメソッドとして定義し、呼び出し側のコードを簡潔に保ちます。

### 7.1 命名規則

| パターン | メソッド名の例 | 用途 |
|---|---|---|
| 全辺・全軸に同じ値を指定 | `All(value)` | `EdgeInsets.All(16)` |
| 軸ごとに指定 | `Symmetric(h, v)` | `EdgeInsets.Symmetric(horizontal: 8)` |
| 一辺・一方向のみ指定 | `Only(...)` | `EdgeInsets.Only(top: 4)` |
| ゼロ・空・デフォルト | `Zero` / `Empty` / `Default` | `EdgeInsets.Zero`, `Size.Empty` |
| 単位値 | `One` / `Unit` | `Size.One` |
| 既存値からの変換・導出 | `From*(...)` | `Rect.FromPoints(a, b)` |
| よく使うプリセット | 意味のある名詞 | `ThemeContext.Dark()`, `ThemeContext.Light()` |
| 単位付きリテラル | 単位名の拡張プロパティ | `45.Deg`, `2000.Dpm`（→ 7.5） |

### 7.2 ジオメトリ型のファクトリ例

```csharp
public readonly record struct EdgeInsets(double Left, double Top, double Right, double Bottom)
{
    public static readonly EdgeInsets Zero = new(0, 0, 0, 0);

    public static EdgeInsets All(double value) => new(value, value, value, value);
    public static EdgeInsets Symmetric(double horizontal = 0, double vertical = 0)
        => new(horizontal, vertical, horizontal, vertical);
    public static EdgeInsets Only(double left = 0, double top = 0, double right = 0, double bottom = 0)
        => new(left, top, right, bottom);
}

public readonly record struct Rect(double X, double Y, double Width, double Height)
{
    public static readonly Rect Empty = new(0, 0, 0, 0);

    public static Rect FromLTWH(double left, double top, double width, double height)
        => new(left, top, width, height);
    public static Rect FromPoints(Offset topLeft, Offset bottomRight)
        => new(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
    public static Rect FromCenter(Offset center, double width, double height)
        => new(center.X - width / 2, center.Y - height / 2, width, height);
}

public readonly record struct Size(double Width, double Height)
{
    public static readonly Size Zero = new(0, 0);
    public static readonly Size Infinite = new(double.PositiveInfinity, double.PositiveInfinity);

    public static Size Square(double side) => new(side, side);
}
```

### 7.3 Contextオブジェクトのファクトリ例

```csharp
public record ThemeContext
{
    public Color PrimaryColor { get; init; } = Colors.Blue;
    public Color SurfaceColor { get; init; } = Colors.White;
    public Color OnSurfaceColor { get; init; } = Colors.Black;
    public TextStyle DefaultTextStyle { get; init; } = new();
    public double BorderRadius { get; init; } = 4;

    // よく使うプリセットをファクトリメソッドで提供
    public static ThemeContext Light() => new();
    public static ThemeContext Dark() => new()
    {
        PrimaryColor = Color.FromArgb(0xFF4DB6AC),
        SurfaceColor = Color.FromArgb(0xFF212121),
        OnSurfaceColor = Colors.White
    };
}
```

### 7.4 ファクトリメソッドを追加すべき判断基準

- 3つ以上のプロパティを毎回同じパターンで設定する組み合わせがある
- ゼロ・空・単位といった「自明な定数」が存在する
- 別の表現形式(座標2点→矩形など)から変換する必要がある
- `with` 式と組み合わせて「ベースに少し手を加える」用途が想定される

### 7.5 単位系値オブジェクトの拡張ファクトリ

角度・密度・物理長など**単位を持つ値オブジェクト**には、`FromXxx` 静的ファクトリ(セクション7.1)に加えて、数値リテラルから直接生成できる**拡張プロパティ**を提供します。C# 14 の `extension` ブロックで定義し、引数なし・`()` なしで単位付きリテラルのように読める書き味を実現します。

```csharp
// ✅ 推奨: 拡張プロパティによる単位付きリテラル
new RotatedBox { Angle = 45.Deg, Child = icon }

// 従来の静的ファクトリも引き続き有効（こちらが正準）
new RotatedBox { Angle = Angle.FromDegrees(45), Child = icon }
```

**定義例:**

```csharp
namespace FloatSoda.Abstractions.Geometries.Units;

public static class AngleUnits
{
    extension(double value)
    {
        /// <summary>度数から <see cref="Angle"/> を生成します。</summary>
        public Angle Deg => Angle.FromDegrees(value);

        /// <summary>ラジアンから <see cref="Angle"/> を生成します。</summary>
        public Angle Rad => Angle.FromRadians(value);
    }

    extension(int value)
    {
        /// <summary>度数から <see cref="Angle"/> を生成します。</summary>
        public Angle Deg => Angle.FromDegrees(value);

        /// <summary>ラジアンから <see cref="Angle"/> を生成します。</summary>
        public Angle Rad => Angle.FromRadians(value);
    }
}
```

**規約:**

| 項目 | 規約 |
|---|---|
| 形式 | 拡張**プロパティ**（`()` なし）。引数なし・副作用なしの純粋変換のみ |
| 命名 | `From` プレフィックスなしの単位名（`Deg`, `Rad`, `Dpm`, `Meters`） |
| レシーバー型 | `double` と `int` の2つ(公開APIが `double` 基準〔セクション8.5〕のため `float` レシーバーは提供しない) |
| 名前空間 | 専用名前空間（`FloatSoda.Abstractions.Geometries.Units` 等）に隔離し、オプトインにする |
| 位置づけ | `FromXxx` 静的ファクトリが正準。拡張プロパティはその糖衣であり、必ず正準ファクトリへ委譲する |

**理由:**

- オブジェクト初期化子ベースのマークアップ(セクション1)では、`Angle.FromDegrees(45)` より `45.Deg` のほうが視覚的ノイズが少なく、単位付きリテラルとして自然に読めるためです。
- 従来の拡張メソッドと異なり、C# 14 の `extension` ブロックは**プロパティ**を定義できるため、`45.Deg()` の `()` も不要になります。値を返すだけの純粋変換であり、プロパティのセマンティクスにも合致しています。
- 数値型への拡張は、名前空間をインポートしたすべてのコードで数値リテラルの補完候補に現れます。専用名前空間に隔離すると、単位リテラルを使いたいファイルだけがオプトインでき、IntelliSense の汚染を防げます。

**注意点:**

- 拡張ブロックのレシーバー解決には数値の暗黙変換(`int` → `double` 等)が**効かない**点に注意してください。`double` にだけ定義すると `45.Deg`(`int` リテラル)がコンパイルエラーになるため、レシーバー型は必ず規約どおり `double` と `int` の両方に定義します。
- 単位の解釈が自明でない変換(例: `Dpm.FromMillimetersPerPixel` のような逆数系)は拡張プロパティにせず、正準の `FromXxx` のみとします。拡張プロパティは「数値がそのまま単位値になる」変換に限定します。

## 8. ジオメトリオブジェクトには `record struct` を使う

座標・サイズ・余白といったジオメトリ型は `record struct` で定義します。値型であるためヒープ割り当てが発生せず、レイアウト計算時のパフォーマンスに優れているほか、`record` による等値比較や分解、`with` 式の恩恵も受けられます。

```csharp
// ✅ 推奨
public readonly record struct Size(double Width, double Height);
public readonly record struct Offset(double X, double Y);
public readonly record struct Rect(double X, double Y, double Width, double Height);
public readonly record struct EdgeInsets(double Left, double Top, double Right, double Bottom);
```

ファクトリメソッドの定義方針についてはセクション7を参照してください。`with` 式を使用すると、既存の値をベースに一部のプロパティのみを変更した新しい値を簡潔に生成できます。

```csharp
var insets = EdgeInsets.All(16);
var wider = insets with { Left = 32, Right = 32 };
```

**ジオメトリ型に `class` や通常の `struct` を採用しない理由:**

| | `record struct` | `class` | `struct` |
|---|---|---|---|
| ヒープ割り当て | なし | あり | なし |
| 等値比較 | 値ベース（自動） | 参照ベース | 手動実装が必要 |
| `with` 式 | ✅ | ✅ | ❌ |
| 分解 (`Deconstruct`) | ✅ | 手動 | 手動 |

### 自分と同じ型を返す計算プロパティには `PrintMembers` を書く

record が自動生成する `ToString()` は、すべての public インスタンスプロパティを出力します。そのため、`EdgeInsets.Flipped` のように自分と同じ型の値を都度生成して返す計算プロパティが存在すると、その値を出力しようとして再帰的に `ToString()` が呼ばれ続け、処理が終了しなくなります。特に `record struct` ではスタックオーバーフローを引き起こし、プロセスごと異常終了します(catch できません)。これは `$"{padding}"` のような文字列補間や、わずか1行のログ出力でも発生します。

このようなプロパティを追加するときは、`PrintMembers` を自前で実装し、出力対象とするメンバーを明示的に選択します。

```csharp
public readonly record struct EdgeInsets(double Left = 0, double Top = 0, double Right = 0, double Bottom = 0)
{
    public EdgeInsets Flipped => new(Right, Bottom, Left, Top);

    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"Left = {Left}, Top = {Top}, Right = {Right}, Bottom = {Bottom}");
        return true;
    }
}
```

`abstract record` の基底型では、`protected virtual bool PrintMembers(StringBuilder builder)` として定義します(`Curve` を参照)。なお、`ToString()` そのものをオーバーライドしても構いません。

この実装漏れは `tests/FloatSoda.Test/RecordToStringTest.cs` によって検出されます。このテストは公開アセンブリ内の record をリフレクションで走査し、該当するプロパティを持つにもかかわらず `PrintMembers` も `ToString` も自動生成のままになっている型が存在する場合に失敗します。

## 8.5 実数は `double` を基本とし、Skia型を公開APIに出さない

### 実数型の方針

公開APIに現れる実数(座標・サイズ・角度・比率など)には **`double`** を使用します。`float` の利用はプラットフォーム境界(SkiaSharp / OpenVR / OpenGL への受け渡し)のみにとどめ、境界での型変換はフレームワーク内部で行います。

```csharp
// ✅ 推奨: 公開APIは double
public readonly record struct Angle(double Radians);
public readonly record struct Size(double Width, double Height);

// ❌ 避ける: 公開APIに float を露出
public readonly record struct Angle(float Radians);
```

**理由:**

- C# の浮動小数点リテラルはデフォルトで `double` とみなされるため、`Rotation = 45.5.Deg` のように接尾辞 `f` なしで記述できます。一方、`float` を基準としたAPIにすると、マークアップ全体の至る所に `f` の記述を強いることになります(セクション7.5の単位リテラルと組み合わせると、この差がさらに大きくなります)。
- レイアウト計算や、角度から行列への変換といった合成計算において、`double` で値を保持したほうが計算誤差が蓄積しにくくなります。
- Flutter でも公開API(`dart:ui` / framework層)はすべて `double` で統一されており、エンジン境界で `float` へ変換する設計が採られています。

**性能に関する補足:** 値オブジェクトのサイズは倍になりますが、UIプロパティの用途では実害はありません。SIMD化されたホットパスなど、`float` の採用が妥当とされる箇所は、公開APIではなく内部実装に限定します。

### Skia型を公開APIに出さない

`SKCanvas`、`SKPicture`、`SKSize`、`SKColor` といった SkiaSharp の型は、公開API(Widget のプロパティ、RenderObject の公開メンバー、ジオメトリ型、デザインシステムの Style レコード)に露出させません。描画に関する語彙はフレームワーク自前の型(`Size`, `Color`, `Paint` 等)として定義し、Skia 型への変換は `FloatSoda.Engine` 側の境界で行います。

```csharp
// ✅ 推奨: 自前の語彙型
public record ButtonStyle
{
    public Color BackgroundColor { get; init; } = Colors.White;
}

// ❌ 避ける: Skia型の直接露出
public record ButtonStyle
{
    public SKColor BackgroundColor { get; init; } = new SKColor(0xFF, 0xFF, 0xFF);
}
```

**理由:**

- レンダリングバックエンドを実装詳細にとどめるためです。Flutter が framework 層と `dart:ui` の境界を維持していたからこそ、Skia から Impeller への差し替えが可能でした。同様の境界を設けることで、将来バックエンドを変更する際にも公開APIを壊すことなく移行できます。
- 利用者に SkiaSharp への直接依存を強制しないためです。

**例外: `Color` と `SKColor` の暗黙変換**

`Color` は、`SKColor` との双方向の暗黙変換を公開しています。`SKColor` から `Color` への変換が提供されているため、SkiaSharp が定義する名前付きの色を、`Color` 型のプロパティへそのまま渡すことができます。現状、samples や tests には SkiaSharp の名前付きの色を渡す記述が多く存在しており(#367 の時点で 34 ファイルに 156 か所)、この変換を削除するとそれらすべてがコンパイルできなくなるため残しています。また、`Color` から `SKColor` への変換は、フレームワーク内部から Skia へ色情報を渡すために使用します。

> **移行中の注記:** 既存コードには本規約に違反する箇所が一部残っています(`RenderBox.Size` の `SKSize`、`Angle` / `Dpm` / `Alignment` の `float` など)。これらは [Issue #131](https://github.com/sumx21t-3310/FloatSoda/issues/131) のロードマップに従って段階的に解消していきます。新規APIはこの規約に従うこと。

## 9. Contextオブジェクトには `record` を使う

テーマやロケール、アクセシビリティ設定のように、ツリーを通じて伝播するコンテキスト情報は `record`(参照型)で定義します。`with` 式によるコピー変形を用いることで、スコープごとに一部の値を容易に上書きできるほか、等値比較によって再描画の要否を効率よく判定できます。

```csharp
public record ThemeContext
{
    public Color PrimaryColor { get; init; } = Colors.Blue;
    public Color SurfaceColor { get; init; } = Colors.White;
    public Color OnSurfaceColor { get; init; } = Colors.Black;
    public TextStyle DefaultTextStyle { get; init; } = new();
    public double BorderRadius { get; init; } = 4;

    public static ThemeContext Light() => new();
    public static ThemeContext Dark() => new()
    {
        PrimaryColor = Color.FromArgb(0xFF4DB6AC),
        SurfaceColor = Color.FromArgb(0xFF212121),
        OnSurfaceColor = Colors.White
    };
}

public record LocaleContext
{
    public required CultureInfo Culture { get; init; }
    public FlowDirection FlowDirection { get; init; } = FlowDirection.LeftToRight;

    public static LocaleContext FromCulture(CultureInfo culture) => new() { Culture = culture };
}
```

サブツリーでテーマを部分的にオーバーライドする例:

```csharp
// 親のコンテキストから一部だけ変えた新しいコンテキストを派生させる
var darkSection = parentTheme with
{
    PrimaryColor = Colors.White,
    SurfaceColor = Color.FromArgb(0xFF212121)
};
```

**`record struct` ではなく `record`(参照型)を選ぶ理由:**

コンテキストオブジェクトはツリー全体で参照を共有して使われるため、値型を採用してコピーが多発するとオーバーヘッドにつながります。参照型の `record` を用いることで、`with` 式で値を変更したときだけ新しいインスタンスが生成され、変更のないサブツリーには同一の参照をそのまま引き渡せます。

## 10. ドキュメントコメント規約

ドキュメントコメント(XML ドキュメントコメント)の規約は、独立した別ページに分離しました。適用範囲、契約の記述方法、Dirty フラグなどの副作用の明記、関連型への参照方法、記述言語などについては、[DOCUMENTATION_COMMENTS.md](DOCUMENTATION_COMMENTS.md) を参照してください。

要点は次の通りです。

- アクセス修飾子にかかわらず、原則としてすべての型およびメンバーに記述する(`public` だけでなく `private` も対象)。
- ドキュメントコメントは正式な API Reference の原稿として扱い、その型やメンバーの役割・契約・副作用の説明をそれ自身で完結させる。具体的な使い方やチュートリアルはドキュメントサイト側へ分離する。
- `<example>` は原則として使用しない。

## 11. ネイティブAPIラップの方針

`FloatSoda.OVR` などにおいて、ネイティブAPI(OpenVR等)をラップする際の規約です。

### 11.1 enum の名前を返すだけの API は高レベルラッパーに追加しない

OpenVR の各インターフェースには、`GetApplicationsErrorNameFromEnum`、`GetOverlayErrorNameFromEnum`、`GetSceneApplicationStateNameFromEnum`、`GetEventTypeNameFromEnum` のように、enum 値を人間が読める名前文字列へ変換するだけの `*NameFromEnum` 系 API が存在します。**単に列挙子名を返すだけであり、説明文やローカライズ、プロトコル上の追加の意味を持たないもの**については、`FloatSoda.OVR` の高レベルラッパーに同等のメソッドを追加しません。

```csharp
// ✅ 推奨: 既知の値をログへ出す通常用途は C# の文字列補間で十分
EVRApplicationError err = ...;
logger.Log($"OpenVR error: {err} ({(int)err})");   // "AppKeyAlreadyExists (100)"

// ❌ 避ける: ネイティブ関数を経由した文字列化を高レベルラッパーとして公開する
public string GetErrorName(EVRApplicationError err)
    => Marshal.PtrToStringAnsi(OpenVR.Applications.GetApplicationsErrorNameFromEnum(err));
```

**理由:**

- 既知の enum 値をログに表示するような一般的な用途には、`enum.ToString()`(および文字列補間)で十分であり、`FloatSoda.OVR` の高レベル API として個別に文字列化メソッドを追加する価値が小さいため。
- `FloatSoda.OVR` のエラー処理モデルは `ThrowIfError()` による例外化を基本としており、エラーは例外と型付きの `ErrorCode` で扱い、メッセージは診断情報として提供する方針をとっているため。文字列化 API を高レベルラッパーとして公開すると、「エラーを文字列で扱うべきである」という誤ったシグナルを与えてしまいます。
- 高レベルラッパーの公開 API にメソッドが存在すると、(特にコード生成 AI などによって)「使うべきもの」と解釈されやすいため。不要な API を増やさないこと自体が誤用の防止につながります。

**注意点:**

- C# の `enum.ToString()` とネイティブ側の `*NameFromEnum` が返す文字列は、完全な一致を**保証しません**(例: ネイティブ側は `VRApplicationError_AppKeyAlreadyExists` であるのに対し、C# バインディングは `AppKeyAlreadyExists` と表現が既に異なります)。また、未定義の将来値に対して `enum.ToString()` は数値文字列(例: `"117"`)を返しますが、ランタイム側の `*NameFromEnum` であればその値の名前を認識できる場合があります。診断上どうしてもネイティブ側の名前が必要な場合は、低レベルバインディング(`OpenVR.Applications.GetApplicationsErrorNameFromEnum` 等)を直接呼び出してください。
- この規約は `FloatSoda.OVR` の**高レベルラッパー**に適用します。`openvr_api.cs` に生成される低レベルバインディングから `*NameFromEnum` を削除するわけではありません。
- 単なる列挙子名にとどまらず、説明文や対処方法、ローカライズ済みの表示文を返す API(例: `VR_GetVRInitErrorAsEnglishDescription` のような記述系 API)はこの規約の対象外です。個別に検討してください。

### 11.2 ライフサイクルを持つ型は単数形、ステートレスなユーティリティは複数形

ネイティブ API のラッパー型を命名する際は、**そのインスタンスが `Init`/`Dispose` のようなライフサイクルを持つかどうか**に応じて、単数形と複数形を使い分けます。

```csharp
// ✅ ライフサイクルを持つ: 単数形 (OVRApplication)
// - コンストラクタで OpenVR.Init()、Dispose() で OpenVR.Shutdown()
// - Info.Key（自分自身のアプリ識別子）を暗黙に使う自己参照系の操作を持つ
public class OVRApplication : IDisposable
{
    public OVRAppInfo Info { get; init; }

    public bool AutoLaunch
    {
        get => OpenVR.Applications.GetApplicationAutoLaunch(Info.Key);
        set => OpenVR.Applications.SetApplicationAutoLaunch(Info.Key, value);
    }
}

// ✅ ステートレスなユーティリティ: 複数形 (OVRApplications)
// - ライフサイクルを持たない static クラス
// - 「自分」ではなく SteamVR のアプリ登録全体を対象にした操作を持ち、対象は毎回引数で明示する
public static class OVRApplications
{
    public static void Launch(string appKey) =>
        OpenVR.Applications.LaunchApplication(appKey).ThrowIfError();
}
```

**判断基準:**

| 型の性質 | 命名 | 例 |
|---|---|---|
| `Init`/`Dispose` のようなライフサイクルを持ち、自分自身の識別子(`Info.Key` 等)を暗黙に使う操作を持つ | 単数形 | `OVRApplication` |
| ライフサイクルを持たない `static` クラスで、操作対象を毎回引数で明示する | 複数形 | `OVRApplications` |

**理由:**

- OpenVR 自身の命名(`IVRSystem` = 単数、`IVRApplications` = 複数)と対応させることで、ラップ元 API との対応関係を型名から読み取れるようにするため。
- `OVRApplication.Launch(appKey)` のように単数形インスタンスから他アプリを起動する形式にすると、「自分」と「他アプリ」を対象とする操作が同じ型に混在してしまい、`this` を参照しない操作なのか自己参照系の操作なのかを型名だけで区別できなくなります。複数形の独立した static クラスへと分離することで、その曖昧さを型レベルで解消するため。
- 将来 `IVROverlay` など他のネイティブインターフェースをラップする際にも、同じ基準(ライフサイクル所有の有無)に基づいて単数形か複数形かを機械的に判断できるようにするため。

## 関連ページ

- [部品を目的から探す](docs/reference/find-widgets.md) — 本規約に基づいて実装された組み込みウィジェット
- [FloatSoda ドキュメント](docs/Home.md) — docs のトップページ
- [CONTRIBUTING.md](CONTRIBUTING.md) — 開発・コントリビューション規約(ブランチ命名、namespace、テスト観点、PR運用)
- [REVIEW.md](REVIEW.md) — コードレビューの判断基準。本ドキュメントの原則をレビュー基準として参照する
