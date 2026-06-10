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

視覚的な属性はコンポーネント本体ではなく、専用の `*Style` クラスに分離します。

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

## 4. イミュータビリティと `init` アクセサ

すべてのプロパティは原則として `init` アクセサを使い、構築後の変更を不可にします。状態変化はフレームワークの状態管理レイヤーに委ねてください。

```csharp
public class Text : Widget
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
public class Image : Widget
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
