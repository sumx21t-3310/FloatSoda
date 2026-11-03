# Icon

## これは何か

`Icon` は、アイコンフォントの字を1つ、正方形の領域に出すウィジェットです。Flutter の `Icon` に対応します。

アイコンフォントは、家や歯車のような絵を、文字として収めたフォントです。どの字を出すかは、フォントとコードポイント(字の番号)の組である `IconData` で指定します。画像ファイルを用意せずに、色と大きさを変えられるアイコンを出せます。

## 使い方

### 出す字を決める

`IconData` に、コードポイントとフォントを渡します。このサンプルでは、Windows 10 以降に入っているアイコンフォント「Segoe MDL2 Assets」を `SystemFontProvider` で読み込んでいます。

```csharp
private static readonly FontProvider IconFont = new SystemFontProvider("Segoe MDL2 Assets");

private static readonly IconData Home = new(0xE80F, IconFont);
private static readonly IconData Settings = new(0xE713, IconFont);
private static readonly IconData Search = new(0xE721, IconFont);
private static readonly IconData Star = new(0xE734, IconFont);
```

コードポイントは、フォントごとに決まっています。Segoe MDL2 Assets のコードポイントは、Microsoft の Segoe MDL2 Assets のアイコン一覧で調べられます。フォントファイルを自分で配る場合は、`SystemFontProvider` の代わりに `FileFontProvider` にファイルのパスを渡します。

### Icon を置く

`IconData` はコンストラクタで渡します。FloatSoda のウィジェットとしては例外的に、これは位置指定の引数です。

```csharp
Child = new IconWidget(Home)
```

このサンプルは名前空間が `FloatSoda.Samples.Icon` で型名 `Icon` と衝突するため、`IconWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Icon(Home)` と書けます。

`Color` を省略すると、字は黒で描かれます。サンプルでは、明るい背景の上に置いています。

### 大きさを変える

`Size` は正方形の一辺の長さで、字の大きさにも同じ値を使います。既定値は 24 です。サンプルの灰色の背景は、`Icon` が取る領域(`Size` × `Size`)を示しています。

```csharp
Frame(new IconWidget(Star) { Size = 24, Color = new Color(255, 209, 102) }),
new SizedBox { Width = 24 },
Frame(new IconWidget(Star) { Size = 48, Color = new Color(255, 209, 102) }),
new SizedBox { Width = 24 },
Frame(new IconWidget(Star) { Size = 96, Color = new Color(255, 209, 102) })
```

### 色を変える

`Color` で字の色を変えます。

```csharp
new IconWidget(Home) { Size = 48, Color = new Color(124, 205, 255) },
new SizedBox { Width = 24 },
new IconWidget(Settings) { Size = 48, Color = new Color(255, 111, 97) },
```

### 文字と並べる

`Row` に `Icon` と `Text` を並べると、ラベル付きのアイコンになります。

```csharp
new IconWidget(Settings) { Size = 32, Color = new Color(244, 247, 255) },
new SizedBox { Width = 12 },
new Text("設定")
{
    Style = new TextStyle { FontSize = 28, Color = new Color(244, 247, 255) }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 同梱のアイコン | **未提供**。フォントとコードポイントを自分で指定する | Material の `Icons` と Cupertino の `CupertinoIcons` を使える |
| `IconData` のフォントの指定 | `FontProvider`(`SystemFontProvider` / `FileFontProvider`) | `fontFamily` と `fontPackage` の文字列 |
| 祖先からの既定値 | **未提供**。`Size` の既定は 24、`Color` の既定は黒 | `IconTheme` から大きさと色を引き継ぐ |
| 指定できる項目 | `Data` / `Size` / `Color` のみ | `fill` / `weight` / `grade` / `opticalSize` / `shadows` / `semanticLabel` / `textDirection` / `applyTextScaling` / `blendMode` / `fontWeight` も取る |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Icon -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.Icon
```

## 関連

- [FloatSoda.Samples.Text](../FloatSoda.Samples.Text) — `TextStyle` と文字の表示
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
