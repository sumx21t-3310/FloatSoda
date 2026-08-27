# ColoredBox

## これは何か

`ColoredBox` は自身の領域を単色で塗りつぶし、その上に子を描画するウィジェットです。Flutter の `ColoredBox` に対応します。

塗りつぶしだけを行う最も軽い手段です。角丸・枠線・グラデーションが必要な場合は `DecoratedBox`、配置や寸法の指定もまとめたい場合は `Container` を使います。

## 使い方

### 領域を塗る

`Color` に色を渡します。

```csharp
new ColoredBoxWidget
{
    Color = new Color(124, 205, 255),
    Child = new SizedBox { Width = 200, Height = 160 }
}
```

このサンプルは名前空間が `FloatSoda.Samples.ColoredBox` で型名 `ColoredBox` と衝突するため、`ColoredBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new ColoredBox { ... }` と書けます。

### サイズの決まり方

`ColoredBox` 自身は寸法を持ちません。**子があれば子のサイズに従い、その背後を塗ります。** つまり「どこまで塗られるか」は子が決めます。

背景を全面に敷きたい場合は、外側で領域を確定させてから包みます。

```csharp
Child = new ColoredBoxWidget
{
    // 子を持つ ColoredBox は子のサイズに従う。ここでは外側の SizedBox が
    // 900 x 420 を与えるので、その全面が塗られる。
    Color = new Color(16, 20, 31),
```

### 重ねる

入れ子にすると内側が外側の上に描かれます。あいだに `Padding` を挟むと、そのぶんだけ外側の色が縁として残ります。

```csharp
new ColoredBoxWidget
{
    Color = new Color(40, 47, 64),
    Child = new Padding
    {
        Spacing = EdgeInsets.All(28),
        Child = new ColoredBoxWidget
        {
            Color = new Color(255, 111, 97),
            Child = new SizedBox { Width = 144, Height = 104 }
        }
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 色の指定 | `Color` は `init` プロパティ | `color` は必須の位置指定引数 |
| 色の型 | `Color(r, g, b)`（0〜255 の整数） | `Color(0xAARRGGBB)` または `Colors.*` |

振る舞い(子のサイズに従い、その背後を塗る)は同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.ColoredBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.ColoredBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.SizedBox](../FloatSoda.Samples.SizedBox) — 塗る範囲を決める寸法指定
