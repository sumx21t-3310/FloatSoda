← [Home](Home.md)

# API Design Guidelines

このドキュメントは、本フレームワークのコンポーネントAPIを設計・実装する際の規約とベストプラクティスを定めます。

## 1. 基本設計哲学

本フレームワークは、Flutterのウィジェットツリーモデルを参考にしています。**宣言的UI**、**イミュータブルな構成**、**ツリー構造による合成**を設計の中心に置きます。

C# のオブジェクト初期化子構文を活用し、マークアップに近い読みやすいUIコードを実現します。

### 判断原則: 概念とツリーの語彙は Flutter に、その表現手段は C# に従う

Flutter を参考にする範囲と、C# の慣習を優先する範囲を次のように使い分けます。

- **UIドメインの語彙の層** — Flutter に準拠します。ウィジェット名、レイアウト用語（`MainAxisAlignment`, `CrossAxisAlignment`）、`Child` / `Children` などの木構造の語彙は Flutter の名前をそのまま使います。Flutter を参照する利用者やコード生成AIが自然に書く形を、そのまま正解とするためです。
- **言語機構・実装パターンの層** — C# として自然な API や慣習があれば、そちらを優先します。Dart の言語制約に由来する実装パターン（例: `addListener` / `removeListener`）は直訳せず、C# の言語機構（`event`, `init`, `required`, `record struct`, `extension` プロパティなど）で同じ意図を表現します。

本ドキュメントの個別ルールの多くは、この原則の適用例です。リスナーパターンではなく `event`（セクション3.6）、`is null` パターン（3.5）、ジオメトリの `record struct`（8）、単位リテラルの拡張プロパティ（7.5）、`init` / `required` によるイミュータビリティと必須値の表現（4・5）などが該当します。

ただし、C# の一般慣習を常に優先するわけではありません。フレームワークの読みやすさを優先して意図的に逸脱する場合は、その理由を明記します（例: 兄弟ウィジェットの同一ファイル配置は StyleCop SA1402 から意図的に外れる — セクション2.5）。

### 判断原則: .NET が標準で提供する機構を再実装しない

一つ前の原則の後半（表現手段は C# に従う）を、判定可能な手順にしたものです。「C# として自然か」という判断は主観に流れやすいため、次の順で判定します。

1. その機構と **同等のもの** を .NET が標準で提供していないか確認します。
2. 提供されていれば、それを使います。Flutter 側に対応する独自実装があっても移植しません。
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

最後の行で `Channel<T>` と reader ループの組み合わせだけを挙げているのは、**「投入順に1件ずつ」まで満たせる標準機構が他にない**からです。似て見える機構でも、保証する契約が異なります。

| 機構 | 保証すること | 保証しないこと |
|---|---|---|
| `Channel<T>` と単一の reader ループ | 投入順での取り出し。ループが各要素の処理を `await` し切ってから次を読むことで1件ずつになる | — |
| `Task` とスレッドプール | — | 排他実行も順序も保証しない |
| `SemaphoreSlim(1, 1)` | 排他実行 | **待機の順序**。先に待った側が先に通るとは限らない |
| `ConcurrentExclusiveSchedulerPair.ExclusiveScheduler` | 同期的なタスク本体どうしの排他 | **`await` をまたぐ排他**（継続は別タスクとして再スケジュールされる）と順序 |

`Channel<T>` が保証するのは**投入順での取り出し**までです。1件ずつの処理は reader 側の書き方で決まります。`SingleReader = true` は「呼び出し側が単一リーダーを守る」ことを前提にした最適化指示であり、複数リーダーや並行処理を禁止しません。**単一の reader ループが各要素の処理を `await` し切ってから次を読む**形にして初めて、直列化が成立します。producer が複数ある場合、ここでの「投入順」は enqueue が成立した順を指します。

`IOTaskRunner` のように「投入順に1件ずつ」を契約しているものを、排他だけを保証する機構へ置き換えると、順序が逆転して壊れます。**要件が排他だけなのか、順序まで含むのかを先に確定させてください。**

#### 「同等」の判定

この原則の難しさは「同等」の判定にあります。**形が似ているだけで契約が違うものを同等と見なすと、逆向きに壊れます。** 同等かどうかは API の形ではなく、**満たすべき契約が一致するか** で判定します。

非同等と判定した例として、画像キャッシュに `Microsoft.Extensions.Caching.Memory` を使えるか検討した場合を挙げます。LRU と容量上限は満たせますが、**「参照中のエントリは容量に関係なく退避しない」という動的な契約**を表現できません。

似た機能として `CacheItemPriority.NeverRemove` がありますが、これは**エントリ登録時に決まる静的な優先度**です。必要なのは、「貸出が1つ以上ある間だけ退避を禁止し、最後の貸出が返された時点で通常の退避対象へ戻す」という、参照カウントに追従して切り替わる契約です。`NeverRemove` と既定値を出し入れして模倣することは理屈のうえでは可能ですが、切り替えの間に退避が走る隙間が残るため、契約を満たしません。

したがって同等とは見なせず、自前で実装したうえで理由を残します。

#### 依存関係の増減とは独立

この原則は「依存関係を増やさない」という意味ではありません。標準機構を使うために依存が一つ増えることも、標準機構で足りるために既存の依存を落とせることも、どちらも正しい結果です。判断の対象は依存の数ではなく、**同じものを二度実装していないか** です。

### 判断原則: Flutter 由来の observable behavior に差異を作らない

最初の原則の前半（語彙は Flutter に従う）を、**挙動**まで広げた原則です。

**Flutter 由来の Widget / RenderObject は、明示された FloatSoda 固有の理由がない限り、Flutter との observable behavior の差異を作りません。**

FloatSoda の docs は、Flutter の語彙で概念を説明します。読者やコード生成 AI は Flutter の挙動を期待して使うため、**差異がバグでも意図的な設計判断でも、利用者が払うコストは同じ**です。

少なくとも次は parity の対象です。

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

この原則の適用開始点である、「何が Flutter 由来か」の判定基準です。

- Flutter に対応する public concept / Widget / RenderObject / lifecycle semantics が存在し、それを FloatSoda へ持ち込むものは **Flutter-derived** として扱い、この原則を適用します。
- Flutter の型やアルゴリズムを内部実装として利用していても、FloatSoda 独自の利用者向け概念や API であれば、その API 自体は **FloatSoda 固有**として扱います (例: オーバーレイ種別)。
- Flutter-derived かどうかの判断が割れる場合は、実装開始前に Issue 上で正典 (対応する Flutter API か、FloatSoda 固有か) を明示します。

#### 理由にならないもの

**「実装しやすい」「こちらの方が安全」「こちらの方が自然」といった理由だけで、独自仕様にしないでください。** これらの理由は差異を正当化しません。Flutter がその挙動を選んだ背景（多くは実際のアプリで発生した事故）を、FloatSoda が再発見する必要はありません。

#### behavioral difference とみなさないもの

C# や .NET として自然な表現への置換は、差異とは見なしません。

- `event`
- `init` / `required`
- `record` / `record struct`
- .NET 標準機構の利用

これは一つ前の2つの原則（表現手段は C# に従う／標準機構を再実装しない）の裏返しです。判断に迷った場合は、**利用者から見た挙動が変わるか**で切り分けてください。`addListener` を `event` に置き換えても、購読と解除という観測可能な挙動は同じです。

#### 既に決めた非移植方針を壊さない

FloatSoda のランタイムや対象ユーザーに存在しない機能について、**既に明示された非移植方針を parity を理由に覆さないでください。** 特に Semantics（次節）との整合性を確認してください。「Flutter にあるから」は、次節の判断を覆す理由になりません。

#### 差異が必要な場合の記録義務

差異が必要と判断した場合は、**次の5点を恒久的に記録します**。

1. **Flutter の挙動**
2. **FloatSoda の挙動**
3. **差異が必要な理由**
4. **差異を固定するテスト**（ファイルとテストメソッド名）
5. **利用者に影響する場合のドキュメント**

記録先は [`known-divergences.md`](../.agents/skills/floatsoda-device-test/references/known-divergences.md) です。このファイルが **FloatSoda と Flutter の確認済み差異を管理する台帳の正典**であり、本ドキュメントは判断原則だけを持ちます。

5 について、利用者から見える差異は台帳に記録するだけでは不十分です。該当する `docs/` のページと、対応するサンプルの `## Flutterとの違い` 節（→ [CONTRIBUTING.md](../CONTRIBUTING.md)）にも記載してください。台帳はコントリビュータ向け、docs とサンプルは利用者向けです。

4 が無い差異は、次の移植で気づかずに戻されてしまいます。**テストで固定されていない差異は、記録されていないのと同じ**と考えてください。

#### 参照の追跡可能性

Flutter を参照して移植や修正を行う場合、可能な範囲で次の情報を **PR から追跡可能にしてください**（推奨）。PR テンプレートの `## Flutter reference` 欄が該当します。

- Flutter version または commit
- 参照した Flutter source
- 参照した Flutter tests
- 関連する Flutter API documentation

ローカル参照クローンは `~/code_reading/flutter_reference` です。対応する Widget / Element / RenderObject の特定には `flutter-widget-source` skill が使えます。

#### 仕様が食い違ったときの優先順位

既存実装を無条件に正しいものとして扱わないでください。**上から順に**確認します。

1. **FloatSoda で明示的に定義された差異・設計判断** — 本ドキュメント、台帳、`docs/` の明記
2. **Flutter の仕様・実装・公式テスト** — 1 に該当する記述が無ければ、Flutter を正典とします。
3. **既存の FloatSoda 実装は根拠になりません** — そう実装されていることは、それが正しいことを意味しません。

**古い Issue や既存実装だけを根拠に、新しい挙動を決めないでください。** Issue が書かれた時点の前提が今も成り立つかを確認してください。

### 判断原則: 対象ユーザー・ランタイムに不要な Flutter API は移植しない

Flutter の API がすべて FloatSoda に必要なわけではありません。対象ユーザー (→ [TargetUsers](TargetUsers.md)) の需要がなく、かつ FloatSoda のランタイム (Skia → OpenGL → OpenVR オーバーレイテクスチャ) に受け皿がない機能は移植しません。Flutter 由来のコードからフックだけを先に置くことはせず、削除します。

#### 実装しない API — Semantics (アクセシビリティツリー)

Flutter の `SemanticsNode` / `SemanticsConfiguration` / `PipelineOwner.semanticsOwner` に相当する **並行ツリー** は FloatSoda では実装しません。

理由:

- **出力先に経路がない** — VR オーバーレイテクスチャは、OS ネイティブのアクセシビリティ API (Windows Narrator / UI Automation / macOS VoiceOver など) に接続する自然なフックを持っていません。SteamVR / OpenVR にもスクリーンリーダー統合の仕組みは存在しません。
- **ターゲットユーザーに需要がない** — FloatSoda の3ペルソナ (バイブコーディング VRChatter / Booth 創作者 / uGUI 回避エンジニア) の要件に、アクセシビリティ需要は含まれていません。
- **部分導入は無意味** — `markNeedsSemanticsUpdate()` だけ実装しても、`SemanticsNode` ツリーと `SemanticsOwner` が無ければ実質 no-op になります。本格的に導入する場合は Semantics ツリー全体の設計が必要になり、その時点で改めて設計し直す方が自然です。

したがって、Flutter 本家から移植する際は、以下のような API やプロパティは **持ち込みません**。

- `RenderObject.markNeedsSemanticsUpdate()`
- `describeSemanticsConfiguration(SemanticsConfiguration config)`
- `SemanticsConfiguration` / `SemanticsNode` 系型
- ウィジェット側の `ignoringSemantics` / `excludeSemantics` / `semanticContainer` などのフラグ

将来、VR 空間内でアクセシビリティ情報を表現する自然な経路が見つかった時点で、個別に設計します。それまでは、Flutter 側コードの semantics 関連の呼び出しは移植せずに削除することを規約とします。

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
                BackgroundColor = SKColors.CornflowerBlue
            }
        }
    ]
};
```

## 2. コンポーネントAPI設計ガイドライン

### 2.1 オブジェクト初期化子ファーストの原則

すべてのコンポーネントは、**コンストラクタ引数を使わず**オブジェクト初期化子だけで完全に構成できるよう設計します。

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
- コードがツリー構造として視覚的に読める
- 引数の順序を覚える必要がない
- 将来的なプロパティ追加時に後方互換性を保ちやすい

#### 例外: 末端ウィジェットの単一値コンストラクタ

**末端ウィジェット**に限り、主たる値1つを取るコンストラクタを容認します。末端ウィジェットとは、次の**両方**を満たすものです。

1. **ウィジェットを引数(`Child` / `Children`)に取らない** — 合成ではなく、それ自体が葉になる
2. **データそのものが表示対象** — 文字列・グリフ・画像・数値など「表示される値」を持つ(レイアウトの指示は該当しない)

```csharp
// ✅ 容認: Text はデータそのものが表示対象で、子を取らない
new Text("Hello, World!")
```

**現在の該当例:** `Text(string)`。将来 `Icon` や `ProgressBar` などの末端ウィジェットを追加するときも、この基準を適用します。
**該当しない例:** `Padding` / `Container` (ウィジェットを子に取るため)、`Spacer` (`size` はレイアウトの指示であり、データではないため)。

**付帯ルール:**

- コンストラクタ引数は**主たる値1つのみ**とする。スタイルやオプションは従来どおり `init` プロパティで受ける
- コンストラクタを持つ末端ウィジェットは、その引数以外に `required` メンバーを**持たない**。追加のスタイルやオプションが必要な場合は `init` プロパティで受ける
- 情報源が曖昧なもの(`Image` のパスかプロバイダかなど)はコンストラクタを増やさず、型付きのプロバイダで受ける(現行 API は `new Image { Provider = new FileImageProvider(path) }`。`FileImageProvider` は `FloatSoda.Core.Providers` 名前空間)。将来ショートカットを追加する場合は、[セクション7](#7-ファクトリメソッドの方針)に従って静的ファクトリにする

**理由:**

- `Text` の本体は文字列そのもの、`Icon` はグリフそのものであり、位置引数の意味が曖昧になる余地がない。これにより、初期化子ファースト規約の目的である「引数の意味の自明性」を損なわない。
- 末端ウィジェットは `Children` の深部に大量に現れるため、`new Text("OK")` と書けることによる視覚的なノイズ削減効果が極めて大きい。
- Flutter が末端ウィジェットを `Text('hello')` / `Icon(Icons.add)` の形で提供しているため、Flutter を参考にする利用者やコード生成AIが自然に書く形をそのまま正解にできる。

### 2.2 子要素の表現

単一の子を持つコンポーネントは `Child` プロパティ、複数の子を持つコンポーネントは `Children` プロパティ(`IList<Widget>` 型)を使用します。

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

ツリーが深くなる場合は、**ローカル変数への分割**を推奨します。

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

視覚的な属性はコンポーネント本体に持たせず、専用の `*Style` クラスに分離します。`*Style` レコードや `Button` などのスタイル付きコンポーネントは、デザインシステム層(`FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`)に配置します(→ [UILayering](UILayering.md))。

```csharp
new FloatSoda.UI.Cream.Button
{
    Child = new Text("送信"),
    OnPressed = OnSubmit,
    Style = new FloatSoda.UI.Cream.ButtonStyle
    {
        BackgroundColor = SKColors.CornflowerBlue,
        PressedBackgroundColor = SKColors.RoyalBlue,
        DisabledBackgroundColor = SKColors.LightGray
    }
}
```

### 2.5 ファイル構成 — 兄弟ウィジェットは同じファイルに置く

密接に関連するウィジェット群(**兄弟ウィジェット**)は、1型1ファイルに分割せず、同一のファイルにまとめます。

```csharp
// Flex.cs — Flex とその薄い特殊化をまとめて定義
public sealed record Flex : MultiChildRenderObjectWidget<RenderFlex> { /* ... */ }

public abstract record FlexWrapper(Axis Direction) : StatelessWidget { /* ... */ }

public sealed record Column() : FlexWrapper(Axis.Vertical);

public sealed record Row() : FlexWrapper(Axis.Horizontal);
```

**兄弟と見なす基準(以下のいずれかを満たす場合):**

1. 同一ファイル内にある共通基底の薄い特殊化である(`Column` / `Row` → `FlexWrapper`)
2. 単独では意味を持たず、必ず対で使う(`Stack` + `Positioned` のような関係)
3. 同じ実装詳細(private なヘルパーや共通の RenderObject)を共有する

**兄弟と見なさない基準:** 単に同じカテゴリや同じフォルダに属するというだけの関係は該当しません。たとえば `Padding` と `SizedBox` はどちらも Layout に属しますが、独立したファイルに分けます。

**ファイル名の規則:**

| 状況 | ファイル名 | 例 |
|---|---|---|
| 基底・代表となるウィジェットがある | 代表のウィジェット名 | `Flex.cs`, `Text.cs`, `Align.cs` |
| 対等なグループで代表が決めがたい | グループの概念名 | `Clip.cs`(`ClipOval` / `ClipRect` / `ClipRoundRect` / `ClipCustomPath`) |

**理由:**

- `Column` / `Row` のような数行のレコードを個別のファイルに分けるより、基底と並べて読めるほうが設計意図(薄いラッパーであること)が伝わる。これは Flutter の `basic.dart` が関連ウィジェットをまとめているのと同じ発想である。
- C# の一般慣習である「1型1ファイル」(StyleCop SA1402)からは意図的に逸脱する。本フレームワークのウィジェットは小さな `record` が多く、機械的に分割するとファイル数だけが増え、全体の見通しが悪化するためである。

## 3. プロパティ命名規則

| 種別 | 規則 | 例 |
|---|---|---|
| コンテンツ系 | 意味のある名詞 | `Content`, `Label`, `Title`, `ImageUrl` |
| 子要素 | `Child` / `Children` | `Child`, `Children` |
| イベントハンドラ | `On` + 動詞 (PascalCase) | `OnPressed`, `OnChanged`, `OnSubmit` |
| ブール型フラグ | `Is` / `Has` / `Can` プレフィックス | `IsEnabled`, `HasBorder`, `IsVisible` |
| スタイル | `Style` サフィックス | `TextStyle`, `ButtonStyle` |
| レイアウト | Flutterに準拠した名前 | `MainAxisAlignment`, `CrossAxisAlignment` |

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

FloatSoda で定義する型、メンバー、ドキュメントでは、入力デバイスの語彙として「マウス (Mouse)」を使用せず、「ポインター (Pointer)」に統一します。VR ではレーザーポインターが、デスクトップ環境ではマウスが同じ役割を担うため、デバイスに依存しない語彙を採用します(Flutter の `PointerEvent` 系とも一致します)。

```csharp
// ✅ 良い例: FloatSoda が命名するものはすべて Pointer
public interface IRawPointerSource;
public enum PointerButton { Left, Middle, Right }

// ❌ 悪い例: FloatSoda の公開APIに Mouse を持ち込む
public enum MouseButton { Left, Middle, Right }
```

**例外**: 外部 API(OpenVR / GLFW)の固有名詞をそのまま使用する層は対象外とします。`FloatSoda.OVR` の `SetMouseScale` のように、下位 API(`SetOverlayMouseScale`)との対応がわかることに価値がある薄いラッパーでは、元の語彙を維持します。

ドキュメントコメントの説明文でも「マウス」と単独で書かず、「ポインター(レーザーポインター / マウス)」のように記述します(→ [DocumentationComments](DocumentationComments.md))。

## 3.5 null チェックは `is null` / `is not null` を使う

参照の null 判定には `==` / `!=` 演算子ではなく、パターンマッチの `is null` / `is not null` を使用します。

```csharp
// ✅ 推奨
if (Child is null) return;
if (Child is not null) context.PaintChild(Child, offset);

// ❌ 避ける
if (Child == null) return;
if (Child != null) context.PaintChild(Child, offset);
```

**理由:**
- `==` / `!=` はユーザー定義の演算子オーバーロードに解決される可能性があり、意図しない比較ロジックが実行される恐れがある。一方 `is null` は常に参照の同一性(厳密な null 判定)を評価するため、型に依存せず安全である。
- `is null` / `is not null` は、「null かどうかを判定する」という意図を明確に表現できる。
- 等値演算子をオーバーロードする `record` や `record struct`(セクション8・9)が多い本フレームワークでは、特にこの違いが問題になりやすい。

> **補足:** 値型(`record struct` など)の比較や、null 以外の値との比較には従来どおり `==` / `!=` を使用します。本ルールは、**参照の null 判定**に限定した規約です。

## 3.6 リスナーパターンを実装せず `event` を使う

マルチキャストデリゲート(`event`)で表現できる通知は、Flutter 流のリスナーパターン(`AddListener` / `RemoveListener` メソッドやリスナー用インターフェース)を独自に実装せず、C# の `event` として公開します。

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
- Dart の `addListener` / `removeListener` は言語にイベント機構がないために必要な実装だが、C# ではマルチキャストデリゲートが同等の機能を言語レベルで提供している。これにより、リスナーリストの管理や通知中の購読解除の安全性(invocation list のスナップショット)も、独自実装なしで得られる。
- `+=` / `-=` による購読と解除は、C# 開発者にとって最も予測しやすい API である。
- 購読側にインターフェースの実装を強制せず、ラムダ式やメソッド参照を直接渡せる。

**適用範囲の整理:**

| 対象 | 使うもの |
|---|---|
| Widget のコールバック(単一ハンドラを `init` で受ける) | `On` プレフィックスの `Action?` プロパティ(セクション3) |
| 長寿命のミュータブルなオブジェクト(Controller 等)からの通知(購読者が複数・動的に増減) | `event` |

**補足:**
- 通知元の共通抽象が必要な場合も、リスナー側ではなく通知元側のインターフェースに `event` を宣言する(例: `interface IListenable { event Action? Changed; }`)。
- ラッパー型が親の通知をそのまま中継する場合は、カスタムイベントアクセサ(`add` / `remove`)を使って親の `event` に委譲すると、購読の付け替えミスや解除漏れを防げる。
- `Dispose()` ではイベントフィールドに `null` を代入し、購読を破棄する。

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

- すべてのプロパティに**合理的なデフォルト値**を設定し、最小限の記述でコンポーネントを使用できるようにする
- 必須プロパティは `required` キーワードで明示する

```csharp
public record AspectRatio : Widget
{
    public required double Ratio { get; init; }  // 必須（幅 ÷ 高さ。妥当なデフォルトが存在しない）
    public Widget? Child { get; init; }          // null = 子なし
    public Alignment Alignment { get; init; } = Alignment.Center;  // デフォルトあり
}
```

> **末端ウィジェットの必須値は `required` にしない:** 上記の `AspectRatio` はウィジェット(`Child`)を子に取る合成ウィジェットであるため、初期化子と `required` の組み合わせで必須値を表現します。一方 `Text` / `Icon` / `Image` のような**末端ウィジェット**は、必須値をコンストラクタで受けるため `required` を使用しません(→ [セクション2.1 の例外](#例外-末端ウィジェットの単一値コンストラクタ))。両者で必須値の受け取り方が異なる点に注意してください。

## 6. バージョニングと後方互換性

### 6.1 非破壊的変更（マイナーバージョン）

- 新しいプロパティの追加(デフォルト値あり)
- `required` でないプロパティのオプション化

### 6.2 破壊的変更（メジャーバージョン）

- プロパティの削除やリネーム
- プロパティの型変更
- `required` の追加
- **observable behavior の変更** — 既存の正しい利用コードから観測できる挙動の変化。layout、paint / clipping、hit testing、Widget update behavior、invalid / degenerate input handling、Element / state lifecycle semantics など、セクション1の parity 対象リストに挙げた挙動が判定の観点となる。

observable behavior に関しては、**互換性の基準を文書化された契約**(本ドキュメント、docs、Flutter parity)とします。契約から逸脱していた挙動を本来の契約へ戻す修正は、挙動が変化しても破壊的変更ではなくバグ修正として扱います。反対に、契約どおりに動いていた挙動を変更する場合は破壊的変更となります。

### 6.3 廃止予定プロパティの扱い

```csharp
/// <summary>テキストの色を指定します。</summary>
[Obsolete("TextStyle.Color を使用してください。v3.0 で削除予定です。")]
public Color? TextColor { get; init; }
```

### 6.4 Alpha 段階での判定と許容の分離

Alpha 段階であることは、breaking change の**判定**を省略する理由になりません。判定と許容は分けて行います。

1. まず public API / observable behavior に対して、6.1 と 6.2 の基準で breaking change に該当するかを通常どおり判定する
2. そのうえで、Alpha 段階としてその変更を現時点で受け入れるかを別途判断する

「Alpha 版だから breaking change ではない」という扱いはしません。判定の結果は PR の本文に記載し(→ [CONTRIBUTING.md](../CONTRIBUTING.md))、breaking change を伴う Issue には `breaking-change` ラベルを付与します。

## 7. ファクトリメソッドの方針

プライマリコンストラクタ(`record struct` のポジショナル構文)以外でインスタンスを生成する場合は、**静的ファクトリメソッド**を提供します。オブジェクト初期化子だけでは表現しにくい「よく使うプリセット」や「導出パターン」をファクトリメソッドとして定義し、呼び出し側のコードを簡潔に保ちます。

### 7.1 命名規則

| パターン | メソッド名の例 | 用途 |
|---|---|---|
| 全辺・全軸に同じ値 | `All(value)` | `EdgeInsets.All(16)` |
| 軸ごとに指定 | `Symmetric(h, v)` | `EdgeInsets.Symmetric(horizontal: 8)` |
| 一辺・一方向のみ | `Only(...)` | `EdgeInsets.Only(top: 4)` |
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
        PrimaryColor = Colors.Teal300,
        SurfaceColor = Colors.Gray900,
        OnSurfaceColor = Colors.White
    };
}
```

### 7.4 ファクトリメソッドを追加すべき判断基準

- 3つ以上のプロパティを毎回同じパターンで設定する組み合わせがある
- ゼロ・空・単位といった「自明な定数」がある
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

- オブジェクト初期化子ベースのマークアップ(セクション1)では、`Angle.FromDegrees(45)` より `45.Deg` のほうが視覚的ノイズが少なく、単位付きリテラルとして自然に読める。
- 従来の拡張メソッドと異なり、C# 14 の `extension` ブロックは**プロパティ**を定義できるため、`45.Deg()` の `()` も不要になる。値を返すだけの純粋変換であり、プロパティのセマンティクスにも合致する。
- 数値型への拡張は、名前空間をインポートしたすべてのコードで数値リテラルの補完候補に現れる。専用名前空間に隔離すると、単位リテラルを使いたいファイルだけがオプトインでき、IntelliSense の汚染を防げる。

**注意点:**

- 拡張ブロックのレシーバー解決には数値の暗黙変換(`int` → `double` 等)が**効かない**。`double` にだけ定義すると `45.Deg`(`int` リテラル)がコンパイルエラーになるため、レシーバー型は必ず規約どおり `double` と `int` の両方に定義する。
- 単位の解釈が自明でない変換(例: `Dpm.FromMillimetersPerPixel` のような逆数系)は拡張プロパティにせず、正準の `FromXxx` のみとする。拡張プロパティは「数値がそのまま単位値になる」変換に限定する。

## 8. ジオメトリオブジェクトには `record struct` を使う

座標・サイズ・余白などのジオメトリ型は `record struct` で定義します。値型のためヒープ割り当てが不要でレイアウト計算時のパフォーマンスに優れ、`record` の等値比較・分解・`with` 式の恩恵を受けられます。

```csharp
// ✅ 推奨
public readonly record struct Size(double Width, double Height);
public readonly record struct Offset(double X, double Y);
public readonly record struct Rect(double X, double Y, double Width, double Height);
public readonly record struct EdgeInsets(double Left, double Top, double Right, double Bottom);
```

ファクトリメソッドの定義方針はセクション7を参照してください。`with` 式を使うと、既存の値から一部だけ変えた新しい値を簡潔に作れます。

```csharp
var insets = EdgeInsets.All(16);
var wider = insets with { Left = 32, Right = 32 };
```

**ジオメトリ型に `class` や通常の `struct` を使わない理由:**

| | `record struct` | `class` | `struct` |
|---|---|---|---|
| ヒープ割り当て | なし | あり | なし |
| 等値比較 | 値ベース（自動） | 参照ベース | 手動実装が必要 |
| `with` 式 | ✅ | ✅ | ❌ |
| 分解 (`Deconstruct`) | ✅ | 手動 | 手動 |

## 8.5 実数は `double` を基本とし、Skia型を公開APIに出さない

### 実数型の方針

公開APIに現れる実数(座標・サイズ・角度・比率など)には **`double`** を使います。`float` はプラットフォーム境界(SkiaSharp / OpenVR / OpenGL への受け渡し)でのみ使い、境界での変換はフレームワーク内部で行います。

```csharp
// ✅ 推奨: 公開APIは double
public readonly record struct Angle(double Radians);
public readonly record struct Size(double Width, double Height);

// ❌ 避ける: 公開APIに float を露出
public readonly record struct Angle(float Radians);
```

**理由:**

- C# の浮動小数リテラルはデフォルトで `double` のため、`Rotation = 45.5.Deg` のように接尾辞 `f` なしで書ける。`float` 基準のAPIはマークアップ全体に `f` のノイズを強いる(セクション7.5の単位リテラルと相乗)。
- レイアウト計算や角度から行列への変換といった合成計算は、`double` で保持するほうが誤差が蓄積しにくい。
- Flutterも公開API(`dart:ui` / framework層)はすべて `double` であり、エンジン境界で `float` へ変換している。

**性能に関する補足:** 値オブジェクトのサイズは倍になるが、UIのプロパティ用途では実害はない。SIMD化されたホットパスなど `float` が正当化される箇所は、公開APIではなく内部実装に限定する。

### Skia型を公開APIに出さない

`SKCanvas` / `SKPicture` / `SKSize` / `SKColor` などの SkiaSharp 型は、公開API(Widget のプロパティ、RenderObject の公開メンバー、ジオメトリ型、デザインシステムの Style レコード)に露出させません。描画の語彙はフレームワーク自前の型(`Size`, `Color`, `Paint` 等)で定義し、Skia型への変換は `FloatSoda.Engine` 側の境界で行います。

```csharp
// ✅ 推奨: 自前の語彙型
public record ButtonStyle
{
    public Color BackgroundColor { get; init; } = Colors.White;
}

// ❌ 避ける: Skia型の直接露出
public record ButtonStyle
{
    public SKColor BackgroundColor { get; init; } = SKColors.White;
}
```

**理由:**

- レンダリングバックエンドを実装詳細に保つため。Flutter が framework 層と `dart:ui` の境界を維持していたからこそ、Skia から Impeller への差し替えが可能だった。同じ境界を引くことで、将来のバックエンド変更を公開APIを破壊せずに行える。
- 利用者に SkiaSharp への直接依存を強制しない。

> **移行中の注記:** 既存コードにはこの規約に違反する箇所が残っている(`RenderBox.Size` の `SKSize`、`Angle` / `Dpm` / `Alignment` の `float` など)。段階的な解消は [Issue #131](https://github.com/sumx21t-3310/FloatSoda/issues/131) のロードマップに従う。新規APIはこの規約に従うこと。

## 9. Contextオブジェクトには `record` を使う

テーマ・ロケール・アクセシビリティ設定など、ツリーを通じて伝播するコンテキスト情報は `record`(参照型)で定義します。`with` 式によるコピー変形でスコープごとに一部を上書きでき、等値比較によって再描画の必要性を効率よく判定できます。

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
        PrimaryColor = Colors.Teal300,
        SurfaceColor = Colors.Gray900,
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
    SurfaceColor = Colors.Gray900
};
```

**`record struct` ではなく `record`(参照型)を選ぶ理由:**

コンテキストオブジェクトはツリー全体で共有参照されるため、値型のコピーが多発するとオーバーヘッドになります。参照型の `record` にすると、`with` 式で変形したときだけ新しいインスタンスを生成し、変化のないサブツリーへは同じ参照を引き渡せます。

## 10. ドキュメントコメント規約

ドキュメントコメント(XML ドキュメントコメント)の規約は独立したページに分離しました。適用範囲・契約の書き方・Dirty フラグなどの副作用の明記・関連型への参照・記述言語などは [DocumentationComments](DocumentationComments.md) を参照してください。

要点は次の通りです。

- アクセス修飾子を問わず、原則としてすべての型およびメンバーに記述する(`public` だけでなく `private` も対象)。
- ドキュメントコメントは正式な API Reference の原稿として扱い、役割・契約・副作用を完結させる。使い方・チュートリアルはドキュメントサイト側へ分離する。
- `<example>` は原則として使用しない。

## 11. ネイティブAPIラップの方針

`FloatSoda.OVR` などでネイティブAPI(OpenVR等)をラップする際の規約です。

### 11.1 enum の名前を返すだけの API は高レベルラッパーに追加しない

OpenVR には `GetApplicationsErrorNameFromEnum` / `GetOverlayErrorNameFromEnum` / `GetSceneApplicationStateNameFromEnum` / `GetEventTypeNameFromEnum` など、enum 値を人間が読める名前文字列へ変換するだけの `*NameFromEnum` 系 API が各インターフェースに存在します。**列挙子名を返すだけで、説明文・ローカライズ・プロトコル上の意味を追加で持たないもの**は、`FloatSoda.OVR` の高レベルラッパーに同等のメソッドを追加しません。

```csharp
// ✅ 推奨: 既知の値をログへ出す通常用途は C# の文字列補間で十分
EVRApplicationError err = ...;
logger.Log($"OpenVR error: {err} ({(int)err})");   // "AppKeyAlreadyExists (100)"

// ❌ 避ける: ネイティブ関数を経由した文字列化を高レベルラッパーとして公開する
public string GetErrorName(EVRApplicationError err)
    => Marshal.PtrToStringAnsi(OpenVR.Applications.GetApplicationsErrorNameFromEnum(err));
```

**理由:**

- 既知の enum 値をログへ表示する通常用途には `enum.ToString()`(および文字列補間)で十分であり、`FloatSoda.OVR` の高レベル API として別の文字列化メソッドを追加する価値は小さい。
- `FloatSoda.OVR` のエラーモデルは `ThrowIfError()` による例外化であり、エラーは例外と型付きの `ErrorCode` で扱い、メッセージは診断情報として提供する。文字列化 API を高レベルラッパーとして公開すると、「エラーは文字列で扱う」という誤ったシグナルになる。
- 高レベルラッパーの公開 API に存在するものは、「使うべきもの」と解釈される(特にコード生成 AI)。不要な API を増やさないことが誤用の防止になる。

**注意点:**

- C# の `enum.ToString()` とネイティブ側の `*NameFromEnum` が返す文字列は、完全一致を**保証しません**(例: ネイティブ側は `VRApplicationError_AppKeyAlreadyExists`、C# バインディングは `AppKeyAlreadyExists` と表現が既に異なります)。また、未定義の将来値に対して `enum.ToString()` は数値文字列(例: `"117"`)を返しますが、ランタイム側の `*NameFromEnum` はその値の名前を認識できる場合があります。診断上どうしてもネイティブ側の名前が必要な場合は、低レベルバインディング(`OpenVR.Applications.GetApplicationsErrorNameFromEnum` 等)を直接呼び出してください。
- この規約は `FloatSoda.OVR` の**高レベルラッパー**に適用します。`openvr_api.cs` に生成される低レベルバインディングから `*NameFromEnum` を削除するわけではありません。
- 単なる列挙子名ではなく、説明文・対処方法・ローカライズ済みの表示文を返す API(例: `VR_GetVRInitErrorAsEnglishDescription` のような記述系 API)はこの規約の対象外です。個別に検討してください。

### 11.2 ライフサイクルを持つ型は単数形、ステートレスなユーティリティは複数形

ネイティブ API のラッパー型を命名する際、**そのインスタンスが `Init`/`Dispose` のようなライフサイクルを持つかどうか**で単数形と複数形を使い分けます。

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

- OpenVR 自身の命名(`IVRSystem` = 単数、`IVRApplications` = 複数)と対応させることで、ラップ元の API との対応関係を名前から読み取れる。
- `OVRApplication.Launch(appKey)` のように単数形インスタンスから他アプリを起動する形にすると、「自分」と「他アプリ」の操作が同じ型に混在し、`this` を参照しない操作か自己参照系の操作かを型名だけで区別できない。複数形の独立した静的クラスに分離することで、その曖昧さを型レベルで解消する。
- 将来 `IVROverlay` など他のネイティブインターフェースをラップする際にも、同じ基準(ライフサイクル所有の有無)で単数形か複数形かを機械的に判断できる。

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — この規約で実装された組み込みウィジェット
- [Home](Home.md) — ドキュメント一覧
- [CONTRIBUTING.md](../CONTRIBUTING.md) — 開発・コントリビューション規約(ブランチ命名、namespace、テスト観点、PR運用)
- [REVIEW.md](../REVIEW.md) — コードレビューの判断基準。本ドキュメントの原則をレビュー基準として参照する
