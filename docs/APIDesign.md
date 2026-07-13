← [Home](Home.md)

# API Design Guidelines

このドキュメントは、本フレームワークのコンポーネントAPIを設計・実装する際の規約とベストプラクティスをまとめたものです。

---

## 1. 基本設計哲学

本フレームワークはFlutterのウィジェットツリーモデルを参考にしており、**宣言的UI**・**イミュータブルな構成**・**ツリー構造による合成**を設計の中心に置きます。

C# のオブジェクト初期化子構文を活用することで、マークアップに近い読みやすいUIコードを実現します。

```csharp
// 推奨: オブジェクト初期化子によるツリー構造
var ui = new Column
{
    Children =
    [
        new Text { Content = "Hello, World!", FontSize = 24 },
        new Button
        {
            Label = "Click me",
            OnClick = HandleClick,
            Style = new ButtonStyle { IsPrimary = true }
        }
    ]
};
```

---

## 2. コンポーネントAPI設計ガイドライン

### 2.1 オブジェクト初期化子ファーストの原則

すべてのコンポーネントは、**コンストラクタ引数を使わず**オブジェクト初期化子だけで完全に構成できるよう設計します。

```csharp
// ✅ 良い例: 初期化子のみで完結
var card = new Card
{
    Title = "タイトル",
    Body = new Text { Content = "本文" },
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

1. **ウィジェットを引数（`Child` / `Children`）に取らない** — 合成ではなく、それ自体が葉になる
2. **データそのものが表示対象** — 文字列・グリフ・画像・数値など「表示される値」を持つ（レイアウト指示の値は該当しない）

```csharp
// ✅ 容認: Text / Icon はデータそのものが表示対象で、子を取らない
new Text("Hello, World!")
new Icon("home")

// 初期化子形式も常に併存する（スタイル等を足すときはこちら）
new Text { Content = "Hello, World!", FontSize = 24 }
```

**該当する例:** `Text(string)`、`Icon(string)`、`Image`（画像プロバイダ1つ）、`ProgressBar(double)` など。
**該当しない例:** `Padding` / `Container`（ウィジェットを取る）、`Spacer`（`size` はレイアウト指示でデータではない）。

**付帯ルール:**

- コンストラクタ引数は**主たる値1つのみ**。スタイルやオプションは従来どおり `init` プロパティで受ける
- オブジェクト初期化子形式を**必ず併存**させる（コンストラクタは糖衣であって、初期化子を置き換えない）
- コンストラクタを持つ末端ウィジェットは、その引数以外に `required` メンバーを**持たない**。C# ではコンストラクタで必須値を埋めるのに `[SetsRequiredMembers]` が必要だが、これは同時に**他の `required` メンバーの検査も無効化**してしまうため（→ [セクション5](#5-デフォルト値の方針)）
- ソースが曖昧なもの（`Image` のパスかプロバイダか等）は、コンストラクタを増やさず静的ファクトリ（`Image.FromFile(path)` など、[セクション7](#7-ファクトリメソッドの方針)）で受ける

**理由:**

- `Text` の本体は文字列そのもの、`Icon` はグリフそのものであり、位置引数の意味が曖昧になりようがない。初期化子ファースト規約が守ろうとしている「引数の意味の自明性」を壊さない。
- 末端ウィジェットは `Children` の深部に大量に現れるため、`new Text("OK")` と書けることの視覚的ノイズ削減効果が最も大きい。
- Flutter が末端ウィジェットを `Text('hello')` / `Icon(Icons.add)` の形で提供しているため、Flutter を参照する利用者・コード生成AIが自然に書く形をそのまま正解にできる。

### 2.2 子要素の表現

単一の子を持つコンポーネントは `Child` プロパティ、複数の子を持つ場合は `Children` プロパティ（`IList<Widget>` 型）を使用します。

```csharp
// 単一の子
new Padding
{
    Insets = EdgeInsets.All(16),
    Child = new Text { Content = "パディングされたテキスト" }
}

// 複数の子
new Row
{
    MainAxisAlignment = MainAxisAlignment.SpaceBetween,
    Children =
    [
        new Icon { Name = "home" },
        new Text { Content = "ホーム" }
    ]
}
```

### 2.3 ネストの深さと可読性

ツリーが深くなる場合は、**ローカル変数への分割**を推奨します。

```csharp
// ✅ 推奨: 変数に切り出してフラット化
var avatar = new CircleAvatar { ImageUrl = user.AvatarUrl, Size = 40 };
var nameLabel = new Text { Content = user.Name, FontWeight = FontWeight.Bold };
var emailLabel = new Text { Content = user.Email, Color = Colors.Gray };

var userInfo = new Column
{
    Children = [nameLabel, emailLabel]
};

var tile = new Row
{
    Children = [avatar, new SizedBox { Width = 12 }, userInfo]
};
```

### 2.4 スタイルの分離

視覚的な属性はコンポーネント本体ではなく、専用の `*Style` クラスに分離します。`*Style` レコードや `Button` などのスタイル付きコンポーネントはデザインシステム層(`FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`)に属します(→ [UILayering](UILayering.md))。

```csharp
new Button
{
    Label = "送信",
    OnClick = OnSubmit,
    Style = new ButtonStyle
    {
        BackgroundColor = Colors.Blue,
        TextColor = Colors.White,
        BorderRadius = 8,
        Padding = EdgeInsets.Symmetric(horizontal: 16, vertical: 8)
    }
}
```

### 2.5 ファイル構成 — 兄弟ウィジェットは同じファイルに置く

密接に関連するウィジェット群（**兄弟ウィジェット**）は、1型1ファイルに分割せず同一ファイルにまとめます。

```csharp
// Flex.cs — Flex とその薄い特殊化をまとめて定義
public sealed record Flex : MultiChildRenderObjectWidget<RenderFlex> { /* ... */ }

public abstract record FlexWrapper(Axis Direction) : StatelessWidget { /* ... */ }

public sealed record Column() : FlexWrapper(Axis.Vertical);

public sealed record Row() : FlexWrapper(Axis.Horizontal);
```

**兄弟と見なす基準（いずれかを満たす場合）:**

1. 同一ファイル内の共通基底の薄い特殊化である（`Column` / `Row` → `FlexWrapper`）
2. 単独では意味をなさず、必ず対で使う（`Stack` + `Positioned` のような関係）
3. 同じ実装詳細（private ヘルパーや共通の RenderObject）を共有する

**兄弟と見なさない:** 単に同じカテゴリ・同じフォルダに属するというだけの関係。`Padding` と `SizedBox` はどちらも Layout 配下ですが、独立したファイルに分けます。

**ファイル名の規則:**

| 状況 | ファイル名 | 例 |
|---|---|---|
| 基底・代表となるウィジェットがある | 代表のウィジェット名 | `Flex.cs`, `Text.cs`, `Align.cs` |
| 対等なグループで代表が決めがたい | グループの概念名 | `Clip.cs`（`ClipOval` / `ClipRect` / `ClipRoundRect` / `ClipCustomPath`） |

**理由:**

- `Column` / `Row` のような数行のレコードを個別ファイルに分けるより、基底と並べて読めるほうが設計意図（薄いラッパーであること）が伝わる。Flutter の `basic.dart` が関連ウィジェットをまとめているのと同じ発想。
- C# の一般慣習「1型1ファイル」（StyleCop SA1402）からは意図的に逸脱する。本フレームワークのウィジェットは小さな `record` が多く、機械的な分割はファイル数だけを増やして見通しを悪化させるため。

---

## 3. プロパティ命名規則

| 種別 | 規則 | 例 |
|---|---|---|
| コンテンツ系 | 意味のある名詞 | `Content`, `Label`, `Title`, `ImageUrl` |
| 子要素 | `Child` / `Children` | `Child`, `Children` |
| イベントハンドラ | `On` + 動詞 (PascalCase) | `OnClick`, `OnChanged`, `OnSubmit` |
| ブール型フラグ | `Is` / `Has` / `Can` プレフィックス | `IsEnabled`, `HasBorder`, `IsVisible` |
| スタイル | `Style` サフィックス | `TextStyle`, `ButtonStyle` |
| レイアウト | Flutterに準拠した名前 | `MainAxisAlignment`, `CrossAxisAlignment` |

### イベントハンドラの型

```csharp
// 引数なし
public Action? OnClick { get; init; }

// 値を渡す場合
public Action<string>? OnChanged { get; init; }

// キャンセル可能な非同期処理
public Func<Task>? OnSubmitAsync { get; init; }
```

---

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
- `==` / `!=` はユーザー定義の演算子オーバーロードに解決される可能性があり、意図しない比較ロジックが走ることがある。`is null` は常に参照の同一性（厳密な null 判定）を見るため、型に依存せず安全。
- `is null` / `is not null` は意図が「null かどうか」であることを明確に表現する。
- 等値演算子をオーバーロードする `record` / `record struct`（セクション8・9）が多い本フレームワークでは、特にこの差が問題になりやすい。

> **補足:** 値型（`record struct` 等）の比較や、null 以外の値との比較は従来どおり `==` / `!=` を使います。本ルールは **参照の null 判定** に限定した規約です。

---

## 3.6 リスナーパターンを実装せず `event` を使う

マルチキャストデリゲート（`event`）で表現できる通知は、Flutter流のリスナーパターン（`AddListener` / `RemoveListener` メソッドやリスナー用インターフェース）を独自実装せず、C# の `event` で公開します。

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
- Dart の `addListener` / `removeListener` は言語にイベント機構がないための実装であり、C# ではマルチキャストデリゲートが同じ機能を言語レベルで提供する。リスナーリストの管理・通知中の購読解除の安全性（invocation list のスナップショット）も自前実装なしで手に入る。
- `+=` / `-=` による購読・解除は C# 開発者にとって最も予測可能な API になる。
- 購読側にインターフェース実装を強制せず、ラムダやメソッド参照をそのまま渡せる。

**適用範囲の整理:**

| 対象 | 使うもの |
|---|---|
| Widget のコールバック（単一ハンドラを `init` で受ける） | `On` プレフィックスの `Action?` プロパティ（セクション3） |
| 長寿命のミュータブルなオブジェクト（Controller 等）からの通知（購読者が複数・動的に増減） | `event` |

**補足:**
- 通知元の共通抽象が必要な場合も、リスナー側ではなく通知元側のインターフェースに `event` を宣言する（例: `interface IListenable { event Action? Changed; }`）。
- ラッパー型が親の通知をそのまま中継する場合は、カスタムイベントアクセサ（`add` / `remove`）で親の `event` に委譲すると購読の付け替えや解除漏れを防げる。
- `Dispose()` ではイベントフィールドに `null` を代入して購読を破棄する。

---

## 4. イミュータビリティと `init` アクセサ

すべてのプロパティは原則として `init` アクセサを使い、構築後の変更を不可にします。状態変化はフレームワークの状態管理レイヤーに委ねてください。

```csharp
public record Text : Widget
{
    public string Content { get; init; } = string.Empty;
    public double FontSize { get; init; } = 14;
    public Color Color { get; init; } = Colors.Black;
    public FontWeight FontWeight { get; init; } = FontWeight.Normal;
}
```

---

## 5. デフォルト値の方針

- すべてのプロパティに**合理的なデフォルト値**を設定し、最小限の記述でコンポーネントを使えるようにする
- 必須プロパティは `required` キーワードで明示する

```csharp
public record Image : Widget
{
    public required string Src { get; init; }   // 必須
    public string? Alt { get; init; }           // 省略可
    public double? Width { get; init; }         // null = 自動
    public double? Height { get; init; }        // null = 自動
    public BoxFit Fit { get; init; } = BoxFit.Contain;  // デフォルトあり
}
```

---

## 6. バージョニングと後方互換性

### 6.1 非破壊的変更（マイナーバージョン）

- 新しいプロパティの追加（デフォルト値あり）
- `required` でないプロパティのオプション化

### 6.2 破壊的変更（メジャーバージョン）

- プロパティの削除・リネーム
- プロパティの型変更
- `required` の追加

### 6.3 廃止予定プロパティの扱い

```csharp
/// <summary>テキストの色を指定します。</summary>
[Obsolete("TextStyle.Color を使用してください。v3.0 で削除予定です。")]
public Color? TextColor { get; init; }
```

---

## 7. ファクトリメソッドの方針

プライマリコンストラクタ（`record struct` のポジショナル構文）以外でインスタンスを生成するパターンには、**静的ファクトリメソッド**を提供します。オブジェクト初期化子だけでは表現しにくい「よく使うプリセット」や「導出パターン」をファクトリメソッドとして定義することで、呼び出し側のコードを簡潔に保ちます。

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
- 別の表現形式（座標2点→矩形など）から変換する必要がある
- `with` 式と組み合わせて「ベースに少し手を加える」用途が想定される

### 7.5 単位系値オブジェクトの拡張ファクトリ

角度・密度・物理長など**単位を持つ値オブジェクト**には、`FromXxx` 静的ファクトリ（セクション7.1）に加えて、数値リテラルから直接生成できる**拡張プロパティ**を提供します。C# 14 の `extension` ブロックで定義し、引数なし・`()` なしで単位付きリテラルのように読める書き味を実現します。

```csharp
// ✅ 推奨: 拡張プロパティによる単位付きリテラル
new RotatedBox { Angle = 45.Deg, Child = icon }

// 従来の静的ファクトリも引き続き有効（こちらが正準）
new RotatedBox { Angle = Angle.FromDegrees(45), Child = icon }
```

**定義例:**

```csharp
namespace FloatSoda.Common.Geometries.Units;

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
| レシーバー型 | `double` と `int` の2つ（公開APIが `double` 基準〔セクション8.5〕のため `float` レシーバーは提供しない） |
| 名前空間 | 専用名前空間（`FloatSoda.Common.Geometries.Units` 等）に隔離し、オプトインにする |
| 位置づけ | `FromXxx` 静的ファクトリが正準。拡張プロパティはその糖衣であり、必ず正準ファクトリへ委譲する |

**理由:**

- オブジェクト初期化子ベースのマークアップ（セクション1）の中で、`Angle.FromDegrees(45)` より `45.Deg` のほうが視覚的ノイズが少なく、単位付きリテラルとして自然に読める。
- 従来の拡張メソッドと異なり、C# 14 の `extension` ブロックは**プロパティ**を定義できるため、`45.Deg()` の `()` すら不要になる。値を返すだけの純粋変換であり、プロパティのセマンティクスにも合致する。
- 数値型への拡張は、名前空間をインポートしたすべてのコードで数値リテラルの補完候補に現れる。専用名前空間への隔離により、単位リテラルを使いたいファイルだけがオプトインでき、IntelliSense 汚染を防げる。

**注意点:**

- 拡張ブロックのレシーバー解決には数値の暗黙変換（`int` → `double` 等）が**効かない**。`double` にだけ定義すると `45.Deg`（int リテラル）がコンパイルエラーになるため、レシーバー型は必ず規約どおり `double` と `int` の両方に定義すること。
- 単位の解釈が自明でない変換（例: `Dpm.FromMillimetersPerPixel` のような逆数系）は拡張プロパティにせず、正準の `FromXxx` のみとする。拡張プロパティは「数値がそのまま単位値になる」変換に限定する。

---

## 8. ジオメトリオブジェクトには `record struct` を使う

座標・サイズ・余白などのジオメトリ型は `record struct` で定義します。値型であるため、ヒープ割り当てが不要でレイアウト計算時のパフォーマンスに優れ、かつ `record` の等値比較・分解・`with` 式の恩恵を受けられます。

```csharp
// ✅ 推奨
public readonly record struct Size(double Width, double Height);
public readonly record struct Offset(double X, double Y);
public readonly record struct Rect(double X, double Y, double Width, double Height);
public readonly record struct EdgeInsets(double Left, double Top, double Right, double Bottom);
```

ファクトリメソッドの定義方針はセクション7を参照してください。`with` 式により既存の値から一部だけ変えた新しい値を簡潔に作れます。

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

---

## 8.5 実数は `double` を基本とし、Skia型を公開APIに出さない

### 実数型の方針

公開APIに現れる実数（座標・サイズ・角度・比率など）は **`double`** を使用します。`float` はプラットフォーム境界（SkiaSharp / OpenVR / OpenGL への受け渡し）でのみ使用し、境界での変換はフレームワーク内部が担います。

```csharp
// ✅ 推奨: 公開APIは double
public readonly record struct Angle(double Radians);
public readonly record struct Size(double Width, double Height);

// ❌ 避ける: 公開APIに float を露出
public readonly record struct Angle(float Radians);
```

**理由:**

- C# の浮動小数リテラルはデフォルトで `double` であるため、`Rotation = 45.5.Deg` のように接尾辞 `f` なしで書ける。`float` 基準のAPIはマークアップ全体に `f` のノイズを強いる（セクション7.5の単位リテラルと相乗）。
- レイアウト計算や角度→行列変換のような合成計算は `double` で保持するほうが誤差が蓄積しにくい。
- Flutterも公開API（`dart:ui` / framework層）はすべて `double` であり、エンジン境界で `float` へ変換している。

**性能に関する補足:** 値オブジェクトのサイズは倍になるが、UIのプロパティ用途では実害はない。SIMD化されたホットパスなど `float` が正当化される箇所は、公開APIではなく内部実装に限定する。

### Skia型を公開APIに出さない

`SKCanvas` / `SKPicture` / `SKSize` / `SKColor` などの SkiaSharp 型は、公開API（Widget のプロパティ、RenderObject の公開メンバー、ジオメトリ型、デザインシステムの Style レコード）に露出させません。描画の語彙はフレームワーク自前の型（`Size`, `Color`, `Paint` 等）で定義し、Skia型への変換は `FloatSoda.Engine` 側の境界で行います。

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

- レンダリングバックエンドを実装詳細に保つため。Flutter が framework 層と `dart:ui` の境界を維持していたからこそ Skia → Impeller の差し替えが可能だった。同じ境界を引くことで、将来のバックエンド変更を公開APIの破壊なしに行える。
- 利用者に SkiaSharp への直接依存を強制しない。

> **移行中の注記:** 既存コードにはこの規約に違反する箇所が残っている（`RenderBox.Size` の `SKSize`、`Angle` / `Dpm` / `Alignment` の `float` など）。段階的な解消は [Issue #131](https://github.com/sumx21t-3310/FloatSoda/issues/131) のロードマップに従う。新規APIはこの規約に従うこと。

---

## 9. Contextオブジェクトには `record` を使う

テーマ・ロケール・アクセシビリティ設定など、ツリーを通じて伝播するコンテキスト情報は `record`（参照型）で定義します。`with` 式によるコピー変形でスコープごとに一部を上書きでき、等値比較によって再描画の必要性を効率よく判定できます。

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

サブツリーでテーマを部分的にオーバーライドする例：

```csharp
// 親のコンテキストから一部だけ変えた新しいコンテキストを派生させる
var darkSection = parentTheme with
{
    PrimaryColor = Colors.White,
    SurfaceColor = Colors.Gray900
};
```

**`record struct` ではなく `record`（参照型）を選ぶ理由:**

コンテキストオブジェクトはツリー全体で共有参照されるため、値型のコピーが多発するとオーバーヘッドになります。参照型の `record` にしておくことで、`with` 式で変形したときだけ新しいインスタンスを生成し、変化のないサブツリーへは同じ参照を引き渡すことができます。

---

## 10. ドキュメントコメント規約

すべての public プロパティには XML ドキュメントコメントを記載します。

```csharp
/// <summary>ボタンに表示するラベルテキスト。</summary>
/// <example>
/// <code>
/// new Button { Label = "送信", OnClick = OnSubmit }
/// </code>
/// </example>
public string Label { get; init; } = string.Empty;
```

---

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — この規約で実装された組み込みウィジェット
- [Home](Home.md) — ドキュメント一覧
