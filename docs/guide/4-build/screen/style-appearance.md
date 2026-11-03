← [ガイド](../../Home.md)

# 見た目を整える

## この手順でできること

文字の色や大きさ、背景、角丸、余白は、すべて C# のコードで指定します。見た目は、C# のウィジェットで指定します。シーンやプレハブを編集する手順は要りません。AI に頼むときも、自分で直すときも、同じコードの同じ場所を直せば、見た目が変わります。

この手順では、4つを順に足して、1枚のカードを作ります。終わると、HMD の中に、角が丸く、白い縁取りの付いた青紫色のカードが出ます。カードの中には、大きな太字のタイトルと、説明の文字が並びます。

1. 文字の色と大きさを指定する
2. 背景色を付けて、角を丸める
3. 余白を取る
4. 画面全体の文字書式を1か所で決める

## 前提条件

- [最初のオーバーレイを作る](../../../tutorials/first-overlay.md)を終え、ウィジェットを `Align` や `SizedBox` で組めること
- ソースの先頭に、`FloatSoda.Painting`、`FloatSoda.Geometrics`、`FloatSoda.Widgets`、`FloatSoda.Widgets.Layout`、`FloatSoda.Widgets.Paint` の `using` があること

## 手順1: 文字の色と大きさを指定する

文字は `Text` ウィジェットで出します。色、大きさ、太さは、`Style` に渡す `TextStyle` で指定します。

```csharp
new Text("タイトル")
{
    Style = new TextStyle
    {
        Color = Colors.WhiteSmoke,
        FontSize = 56,
        FontWeight = 700,
    },
}
```

`FontWeight` は数値で、太字は 700 です。指定を省くと、大きさは 30、色は黒になります。色は `Colors.WhiteSmoke` のような名前のほか、`new Color(124, 205, 255)` のように、赤、緑、青の値でも指定できます。ほかの書き方は[色の指定](#色の指定)にまとめています。

日本語の文字が出ないときは、`Font` にフォントの名前を指定します。`SystemFontProvider` は、`FloatSoda.Core.Providers` の名前空間にあります。

```csharp
Style = new TextStyle { Font = new SystemFontProvider("Meiryo") }
```

<!-- ※要確認: 日本語のフォントの指定が必要になる条件と、指定したフォントで日本語が出ること。samples/Widgets/FloatSoda.Samples.Text で確かめる。実機で確かめる -->

**できた?** HMD の中で、文字の色と大きさが変わっていれば、手順1は完了です。

<!-- ※要確認: HMD の中で文字の色と大きさが変わること。実機で確かめる -->

## 手順2: 背景色を付けて、角を丸める

背景と角丸は、`DecoratedBox` と `BoxDecoration` でまとめて指定します。`BoxDecoration` には、背景の色(`Color`)、角の丸み(`BorderRadius`)、縁取り(`Border`)を渡せます。

```csharp
new DecoratedBox
{
    Decoration = new BoxDecoration
    {
        Color = Colors.SlateBlue,
        BorderRadius = BorderRadius.All(Radius.Circular(24)),
        Border = Border.All(new BorderSide { Color = Colors.White, Width = 4 }),
    },
    Child = new Text("タイトル"),
}
```

角丸が要らず、単色の背景だけでよいときは、`ColoredBox { Color = ... }` が短く書けます。子の中身ごと角丸で切り抜きたいときは、`ClipRoundRect` を使います。

**できた?** 文字の後ろに色が付き、角が丸くなっていれば、手順2は完了です。

<!-- ※要確認: DecoratedBox の角丸と縁取りが HMD で期待どおりに描かれること。コードのコンパイルは確かめた。表示は実機で確かめる -->

## 手順3: 余白を取る

文字と背景の縁のあいだの余白は、`Padding` と `EdgeInsets` で取ります。次のコードの `decoration` には、手順2で作った `BoxDecoration` を渡します。`Padding` の余白は、`Spacing` に渡します。`DecoratedBox` の `Child` を `Padding` で包みます。

```csharp
new DecoratedBox
{
    Decoration = decoration,
    Child = new Padding
    {
        Spacing = EdgeInsets.Symmetric(vertical: 24, horizontal: 48),
        Child = new Text("タイトル"),
    },
}
```

四方向に同じ余白を取るなら `EdgeInsets.All(24)`、縦と横で変えるなら `EdgeInsets.Symmetric(vertical: 24, horizontal: 48)` です。

**できた?** 文字が背景の縁に張り付かず、余白が空いていれば、手順3は完了です。

## 手順4: 画面全体の文字書式を1か所で決める

`DefaultTextStyle` で画面の外側を包むと、子孫の `Text` すべてに、同じ文字書式が渡ります。文字が複数ある画面で、色や大きさを1か所で揃えられます。次のコードの `card` は、手順3までに組んだカードのウィジェットです。

```csharp
new DefaultTextStyle
{
    Style = new TextStyle { Color = Colors.WhiteSmoke, FontSize = 36 },
    Child = card,
}
```

`Text` の `Style` を個別に指定すると、その指定が優先されます。優先順位は、`Text` に書いた指定、`DefaultTextStyle`、既定値の順です。指定は項目ごとに引き継がれるので、タイトルだけ大きくしたいときは、そのタイトルの `Text` にだけ `FontSize = 56` を書きます。色は `DefaultTextStyle` の値になります。

ここまでを合わせたカード全体は、次のとおりです。

```csharp
public record Card : StatelessWidget
{
    public override Widget Build(IBuildContext context) => new DefaultTextStyle
    {
        Style = new TextStyle { Color = Colors.WhiteSmoke, FontSize = 36 },
        Child = new Align
        {
            Alignment = Alignment.Center,
            Child = new DecoratedBox
            {
                Decoration = new BoxDecoration
                {
                    Color = Colors.SlateBlue,
                    BorderRadius = BorderRadius.All(Radius.Circular(24)),
                    Border = Border.All(new BorderSide { Color = Colors.White, Width = 4 }),
                },
                Child = new Padding
                {
                    Spacing = EdgeInsets.Symmetric(vertical: 24, horizontal: 48),
                    Child = new Column
                    {
                        Children =
                        {
                            new Text("タイトル") { Style = new TextStyle { FontSize = 56, FontWeight = 700 } },
                            new Text("説明の文章"),
                        },
                    },
                },
            },
        },
    };
}
```

組み込みの、見た目をまとめて切り替える仕組み(テーマやデザインシステム)は、今はありません(追跡: #219)。この手順では、色や文字書式を、使う場所のコードで指定します。

起動のコードで、ウィンドウの `Child` を `new Card()` に置き換えます。

**できた?** 複数の文字の書式が、1か所の指定で揃っていれば、手順4は完了です。

## 結果を確かめる

HMD で、画面全体が指定どおりかを見ます。

- カードの背景色、角丸、縁取りが指定どおりに出ている
- タイトルが説明より大きく、太く出ている
- 文字が背景の縁に張り付いていない

HMD を被らずに確かめたいときは、[HMD を被らずに確かめる](../input/test-without-hmd.md)に進みます。見た目を整えたら、次は[アニメーションを付ける](add-animation.md)に進みます。

## うまくいかないとき

見た目が指定と違う、または文字が出ないときは、症状に合うページに進みます。大きさや位置が思ったとおりでない場合は[レイアウトの考え方](../../5-troubleshoot/layout-model.md)、例外が出る場合は[エラーを読んで直す](../../5-troubleshoot/read-errors.md)です。

## 色の指定

色は、`FloatSoda.Geometrics` の名前空間にある `Color` 型で指定します。`ColoredBox`、`Container`、`TextStyle`、`BoxDecoration`、`BorderSide` の `Color` は、どれもこの型です。作り方は次のとおりで、どれで作っても同じ `Color` 型の値になります。

| 書き方 | 例 | 向いている場面 |
| :-- | :-- | :-- |
| `Colors.名前` | `Colors.Tomato` | CSS の名前付きの色で指定する。`Colors.Transparent` は透明な黒 |
| `new Color(r, g, b)` | `new Color(124, 205, 255)` | 赤、緑、青を、それぞれ 0〜255 の整数で指定する |
| `new Color(r, g, b, a)` | `new Color(124, 205, 255, 128)` | 4つ目に不透明度を 0〜255 で足す |
| `Color.FromArgb(0xAARRGGBB)` | `Color.FromArgb(0xFF2196F3)` | 16進数の整数で指定する。Flutter の `Color(0xFF2196F3)` と同じ並び |
| `Color.FromArgb(a, r, g, b)` | `Color.FromArgb(255, 33, 150, 243)` | 不透明度を先頭にして、成分を 0〜255 で渡す |
| `Color.FromRgbo(r, g, b, opacity)` | `Color.FromRgbo(255, 0, 0, 0.5)` | 不透明度を 0.0〜1.0 の小数で指定する。範囲の外か `NaN` を渡すと `ArgumentOutOfRangeException` |
| `Color.Parse("#RRGGBB")` | `Color.Parse("#2196F3")` | ウェブカラーの文字列で指定する。`#RGB`、`#RGBA`、`#RRGGBB`、`#RRGGBBAA` を受け付け、`#` は省略できる。大文字と小文字は区別しない |

不透明度 `Alpha` は、0 で完全な透明、255 で完全な不透明です。`new Color(r, g, b)` のように不透明度を省くと、255 になります。作った色の不透明度だけを変えるときは、`color with { Alpha = 128 }` と書きます。

16進数で不透明度を書くときは、並び順に気を付けます。`Color.Parse` の8桁は、不透明度が末尾の `#RRGGBBAA` です。`Color.FromArgb` の整数は、不透明度が先頭の `0xAARRGGBB` です。たとえば、`Color.Parse("#FF000080")` と `Color.FromArgb(0x80FF0000)` は、どちらも同じ半透明の赤になります。

`new Color()` と `default(Color)` は、全成分が 0 の透明な黒です。この色で塗っても、画面には何も見えません。`new Color(...)` で作るときは、赤、緑、青の3つを必ず渡します。
