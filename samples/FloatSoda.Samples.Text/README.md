# Text

## これは何か

`Text` は文字列を1つ表示するウィジェットです。Flutter の `Text` に対応します。

内部では `RichText` を構築する薄いラッパーで、`RichText` が `TextSpan`（文字列とその書式の組）を受け取って段落として描画します。書式を1つだけ適用するなら `Text`、文字列と書式を明示的に組み立てるなら `RichText` を使います。

## 使い方

### 文字列を表示する

表示する文字列はコンストラクタで渡します。FloatSoda のウィジェットとしては例外的に、これは位置指定の引数です。

```csharp
new TextWidget("Style を指定しない既定の表示")
```

このサンプルは名前空間が `FloatSoda.Samples.Text` で型名 `Text` と衝突するため、`TextWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Text("...")` と書けます。

`Style` を省略すると、段落の既定書式で描画されます。

### 書式を指定する

`Style` に `TextStyle` を渡します。指定できるのは `FontSize` / `Color` / `FontWeight` / `IsItalic` / `Font` です。

```csharp
new TextWidget("FontSize 36 / 明るい前景色")
{
    Style = new TextStyle
    {
        FontSize = 36,
        Color = new Color(244, 247, 255)
    }
}
```

`FontWeight` は列挙型ではなく `int` です。太字は 700 を指定します。

```csharp
Style = new TextStyle
{
    FontSize = 32,
    Color = new Color(124, 205, 255),
    FontWeight = 700
}
```

斜体は `IsItalic` で切り替えます。

```csharp
Style = new TextStyle
{
    FontSize = 32,
    Color = new Color(255, 111, 97),
    IsItalic = true
}
```

### RichText を直接使う

`RichText` は `TextSpan` を必須プロパティ `Text` として受け取ります。

```csharp
new RichText
{
    Text = new TextSpan("RichText と TextSpan による表示")
    {
        Style = new TextStyle
        {
            FontSize = 34,
            Color = new Color(169, 180, 204)
        }
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 指定できる項目 | `Data` と `Style` のみ | `textAlign` / `softWrap` / `overflow` / `maxLines` / `textDirection` / `textScaler` なども取る |
| 折り返し・省略の制御 | **未提供**。`softWrap` / `overflow` / `maxLines` に相当するものがない | `overflow: TextOverflow.ellipsis` などで制御できる |
| 書式の指定範囲 | `TextStyle` は `FontSize` / `Color` / `FontWeight` / `IsItalic` / `Font` | `TextStyle` の項目数はこれより大幅に多い |
| フォントウェイト | `int`（太字は `700`） | `FontWeight.w700` の列挙型 |
| `TextSpan` の役割 | 文字列と `Style` を持つ | 同等（子スパンによる部分書式も持つ） |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Text -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Text
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.ColoredBox](../FloatSoda.Samples.ColoredBox) — 背景色の付け方
