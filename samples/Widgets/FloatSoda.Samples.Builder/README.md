# Builder

## これは何か

このサンプルは、ウィジェットツリーの組み立てを助ける3つのウィジェットを並べて見せます。いずれも Flutter の同名のウィジェットに対応します。

| ウィジェット | すること |
|---|---|
| `Builder` | その場所の `context` を渡して、子を作る関数を呼ぶ |
| `KeyedSubtree` | 子のサブツリーに `Key` を付ける |
| `RepaintBoundary` | 子を別のレイヤーに描き、描き直す範囲を分ける |

どれも見た目は変えません。変わるのは、`context` から何が見えるか、並べ替えたときに状態がどこへ行くか、描き直しがどこまで広がるかです。

## 使い方

### Builder で、すぐ上のウィジェットを context から読む

`Build` に渡される `context` は、そのウィジェット自身の場所を指します。そのため、同じ `Build` の中で置いた `DefaultTextStyle` のような祖先は、この `context` からは見えません。`Builder` は、自分の場所の `context` を `ChildBuilder` に渡すので、すぐ上に置いたウィジェットを読めます。

サンプルでは、`Build` の中に `DefaultTextStyle`(FontSize 26)を置き、その下で2つの `context` から FontSize を読んでいます。`Build` の `context` では「指定なし」、`Builder` の `context` では「26」になります。

```csharp
new DefaultTextStyle
{
    Style = new TextStyle { FontSize = 26, Color = new Color(255, 209, 102) },
    Child = new Row
    {
        Children =
        {
            // Build の context は DefaultTextStyle より上にあるので、この書式を見つけられない。
            Value($"Build の context: FontSize {DescribeFontSize(context)}"),
            new SizedBox { Width = 32 },

            // Builder が渡す context は DefaultTextStyle の下にあるので、26 を読める。
            new BuilderWidget
            {
                ChildBuilder = innerContext =>
                    Value($"Builder の context: FontSize {DescribeFontSize(innerContext)}")
            }
        }
    }
},
```

`DescribeFontSize` は、`DefaultTextStyle.Of(context)` で既定の書式を読み、FontSize を文字列にする関数です。

```csharp
private static string DescribeFontSize(IBuildContext context) =>
    DefaultTextStyle.Of(context).Style.FontSize?.ToString() ?? "指定なし";
```

このサンプルは名前空間が `FloatSoda.Samples.Builder` で型名 `Builder` と衝突するため、`BuilderWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Builder { ... }` と書けます。

### KeyedSubtree で、並べ替えても状態を子に付けておく

FloatSoda は、作り直したウィジェットを前回のものと突き合わせるとき、型と `Key` が同じなら同じものとみなします。`Key` がなければ、並んだ位置で突き合わせます。そのため、同じ型の `StatefulWidget` を並べ替えると、状態(このサンプルではタップの回数)は位置に残り、色や名前だけが入れ替わります。

サンプルの「左右を入れ替える」を押すと、`Key` なしの行では色だけが入れ替わり、回数は左右の位置に残ります。

```csharp
.. Ordered(
    new TapCounter { Color = Blue, Name = "青" },
    new TapCounter { Color = Red, Name = "赤" })
```

`KeyedSubtree` で包んで `Key` を付けると、並べ替えても `Key` で突き合わせるため、回数も色と一緒に動きます。

```csharp
.. Ordered(
    new KeyedSubtree
    {
        Key = new ValueKey<string>("blue"),
        Child = new TapCounter { Color = Blue, Name = "青" }
    },
    new KeyedSubtree
    {
        Key = new ValueKey<string>("red"),
        Child = new TapCounter { Color = Red, Name = "赤" }
    })
```

`TapCounter` に直接 `Key` を書いても同じ結果になります。`KeyedSubtree` は、ほかの場所で作られたウィジェットを受け取って並べるときのように、子に `Key` を書けない場面で使います。

### RepaintBoundary で、描き直す範囲を分ける

`RepaintBoundary` は、子を別のレイヤーに描きます。親が描き直されても、子が変わっていなければ、子は描き直さずに前回の結果を使います。逆に、子だけが変わったときは、親を描き直さずに済みます。

```csharp
new RepaintBoundary { Child = Swatch("あり") }
```

見た目は変わりません。サンプルでも「なし」と「あり」は同じに見えます。毎フレーム変わる時計やアニメーションを、変わらない部分から切り離すときに使います。ただし、レイヤーが増えるほど合成の手間も増えるため、すべてを包むことは望ましくありません。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Builder` の関数のプロパティ名 | `ChildBuilder` | `builder` |
| `KeyedSubtree` の補助メソッド | **未提供** | `KeyedSubtree.wrap` / `KeyedSubtree.ensureUniqueKeysForList` |
| `RepaintBoundary` の補助メソッド | **未提供** | `RepaintBoundary.wrap` / `RepaintBoundary.wrapAll` |
| `Key` の種類 | `ValueKey<T>` と `UniqueKey` | `ValueKey` / `ObjectKey` / `UniqueKey` / `GlobalKey` |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Builder -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Builder
```

ボタンのタップは、ダッシュボードとデスクトップウィンドウで受け取れます。

## 関連

- [FloatSoda.Samples.DefaultTextStyle](../FloatSoda.Samples.DefaultTextStyle) — `DefaultTextStyle` の使い方
- [FloatSoda.Samples.GestureDetector](../FloatSoda.Samples.GestureDetector) — タップの受け取り方
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
