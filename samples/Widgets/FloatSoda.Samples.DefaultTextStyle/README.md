# DefaultTextStyle

## これは何か

`DefaultTextStyle` は、配下にある `Text` の既定の書式を、まとめて決めるウィジェットです。Flutter の `DefaultTextStyle` に対応します。

パネル全体の文字を白にしたいとき、`Text` ごとに `Style` を書く代わりに、外側へ1つ置けば済みます。配下の `Text` は、`Style` で指定しなかったプロパティを、最も近い祖先の `DefaultTextStyle` から引き継ぎます。Unity の uGUI でいえば、子の Text コンポーネントへ同じフォント設定を配って回る作業を、親の1か所にまとめる形です。

## 使い方

### サブツリーの既定の書式を決める

`Style` と `Child` を指定します。どちらも必須です。

```csharp
Child = new DefaultTextStyleWidget
{
    Style = new TextStyle
    {
        FontSize = 30,
        Color = new Color(244, 247, 255)
    },
    Child = new Column
    {
        // ...
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.DefaultTextStyle` で型名 `DefaultTextStyle` と衝突するため、`DefaultTextStyleWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new DefaultTextStyle { ... }` と書けます。

配下の `Text` は、`Style` を省略すると祖先の書式(ここでは FontSize 30・白)で描かれます。

```csharp
new Text("Style を省略すると、祖先の書式を使う"),
```

### 一部のプロパティだけを上書きする

`Text` の `Style` に書いたプロパティだけが上書きされ、書かなかったプロパティは祖先から引き継がれます。次の例では色だけが水色に変わり、FontSize は祖先の 30 のまま残ります。

```csharp
new Text("Color だけを指定すると、色だけが変わる")
{
    Style = new TextStyle { Color = new Color(124, 205, 255) }
},
```

太字を足すときも、`FontWeight` だけを書けば済みます。

```csharp
new Text("FontWeight 700 を足すと、太字だけが加わる")
{
    Style = new TextStyle { FontWeight = 700 }
},
```

### 祖先の書式を使わない

`TextStyle` の `Inherit` を `false` にすると、祖先の書式と混ぜずに、その書式だけを使います。指定しなかったプロパティには、祖先の値ではなくフレームワークの既定値が入ります。次の例では `Color` を書いていないため、祖先の白ではなく既定の黒で描かれます。

```csharp
Child = new Text("Inherit = false で、祖先の書式を使わない")
{
    Style = new TextStyle { Inherit = false, FontSize = 24 }
}
```

### 入れ子にする

`DefaultTextStyle` を入れ子にすると、内側のものが外側のものに代わります。`Text` が引き継ぐのは、最も近い祖先の `DefaultTextStyle` だけです。

```csharp
new DefaultTextStyleWidget
{
    Style = new TextStyle
    {
        FontSize = 22,
        Color = new Color(255, 111, 97),
        IsItalic = true
    },
    Child = new Text("内側の DefaultTextStyle が外側に代わる")
}
```

自作のウィジェットから既定の書式を読むときは、`DefaultTextStyle.Of(context)` を使います。祖先に `DefaultTextStyle` がなければ、プロパティがすべて未指定の `TextStyle` を持つ値が返ります。読む例は [FloatSoda.Samples.Builder](../FloatSoda.Samples.Builder) にあります。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 指定できる項目 | `Style` と `Child` のみ | `textAlign` / `softWrap` / `overflow` / `maxLines` / `textWidthBasis` / `textHeightBehavior` も取る |
| 祖先の書式と合成して置く | **未提供** | `DefaultTextStyle.merge` で、祖先の書式に重ねた `DefaultTextStyle` を作れる |
| 祖先がないときの `Of` | 未指定の `TextStyle` を持つ値を返す | `DefaultTextStyle.fallback()` を返す |
| `Text` が引き継ぐ規則 | 同等(`Inherit = false` なら引き継がない) | 同等 |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.DefaultTextStyle -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.DefaultTextStyle
```

## 関連

- [FloatSoda.Samples.Text](../FloatSoda.Samples.Text) — `Text` と `TextStyle` の基本
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
