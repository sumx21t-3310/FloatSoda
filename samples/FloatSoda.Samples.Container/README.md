# Container

## これは何か

`Container` は配置・余白・装飾・寸法・変換を1つのウィジェットへまとめた合成ウィジェットです。Flutter の `Container` に対応します。

指定したプロパティに対応するウィジェットだけを、内側から Child → `Align` → `Padding` → `DecoratedBox` → `SizedBox` → `Transform` の順で重ねます。個別のウィジェットを入れ子にするのと同じ結果を、1つのオブジェクト初期化子で書けます。

## 使い方

### 最小形: 色と寸法だけの四角

`Color` と `Width` / `Height` を指定すると、単色の固定寸法の四角になります。

```csharp
new ContainerWidget
{
    Width = 96,
    Height = 96,
    Color = new Color(124, 205, 255)
}
```

このサンプルは名前空間が `FloatSoda.Samples.Container` で型名 `Container` と衝突するため、`ContainerWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Container { ... }` と書けます。

### 装飾を付ける

角丸やボーダーは `BoxDecoration` で指定します。装飾を使う場合、背景色は `Color` プロパティではなく `BoxDecoration.Color` へ指定します。`Color` と `Decoration` を同時に指定すると `InvalidOperationException` になります。

```csharp
new ContainerWidget
{
    Width = 110,
    Height = 110,
    Decoration = new BoxDecoration
    {
        Color = new Color(255, 111, 97),
        BorderRadius = BorderRadius.Circular(16),
        Border = Border.All(new BorderSide
        {
            Color = new Color(255, 209, 102),
            Width = 4
        })
    }
}
```

### 余白を装飾の内側へ入れる

`Padding` は装飾の内側に入ります。子は余白のぶん装飾より小さくなるため、角丸やボーダーと重なりません。

```csharp
new ContainerWidget
{
    Width = 110,
    Height = 110,
    Padding = EdgeInsets.All(16),
    Decoration = new BoxDecoration
    {
        Color = new Color(255, 209, 102),
        BorderRadius = BorderRadius.Circular(16)
    },
    Child = Fill(new Color(124, 205, 255))
}
```

### 子を配置する

`Alignment` を指定すると、`Container` は利用できる領域いっぱいに広がり、子をその中で配置します。

```csharp
new ContainerWidget
{
    Alignment = Alignment.BottomRight,
    Child = new SizedBox
    {
        Width = 40,
        Height = 40,
        Child = Fill(new Color(255, 111, 97))
    }
}
```

### 変換を掛ける

`Transform` はレイアウトが終わったあとの描画へ適用されます。`TransformAlignment` で変換の原点を決めます。

```csharp
new ContainerWidget
{
    Width = 80,
    Height = 80,
    Color = new Color(124, 205, 255),
    Transform = Matrix3x2.CreateRotation(MathF.PI / 8f),
    TransformAlignment = Alignment.Center
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `margin`(外側の余白) | 無し。外側へ余白を付けるには `Padding` で `Container` を包む | `margin` プロパティ |
| `constraints`(追加の制約) | 無し。`ConstrainedBox` で `Container` を包む | `constraints` プロパティ |
| `foregroundDecoration`(前面の装飾) | 無し | `foregroundDecoration` プロパティ |
| `clipBehavior`(子の切り抜き) | 無し | `clipBehavior` プロパティ |

指定したプロパティに対応するウィジェットを内側から重ねる合成順(配置 → 余白 → 装飾 → 寸法 → 変換)は Flutter と同じです。`Color` と `Decoration` の同時指定が例外になる点も同じです。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Container -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Container
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Padding](../FloatSoda.Samples.Padding) — 余白単体の付き方と収縮
- [FloatSoda.Samples.DecoratedBox](../FloatSoda.Samples.DecoratedBox) — 装飾単体の描画
