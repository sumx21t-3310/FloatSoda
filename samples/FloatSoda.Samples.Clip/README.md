# Clip

## これは何か

クリップ系ウィジェットは、子の描画を自身の領域の形で切り抜きます。FloatSoda には4種類あります。

| ウィジェット | 切り抜く形 |
|---|---|
| `ClipRect` | 自身の領域の矩形 |
| `ClipRoundRect` | `BorderRadius` で角を丸めた矩形 |
| `ClipOval` | 領域に内接する楕円 |
| `ClipCustomPath` | `CustomClipper<SKPath>` が返す任意のパス |

**Flutter とは名前が2つ違います。** `ClipRRect` ではなく `ClipRoundRect`、`ClipPath` ではなく `ClipCustomPath` です。

## 使い方

### 矩形で切り抜く

`ClipRect` は自身の領域からはみ出した子を隠します。

```csharp
new ClipRect { Child = Overflowing() }
```

切り抜きの効果を見るには、領域より大きい子を置く必要があります。**`SizedBox` を入れ子にするだけでは足りません。**`RenderConstrainedBox` が `AdditionalConstraints.Enforce(Constraints)` を使うため親の制約が優先され、`150` を超えられないからです。親より大きい制約を子へ渡すには `OverflowBox` を使います。

```csharp
private static Widget Overflowing() => new SizedBox
{
    Width = 150,
    Height = 150,
    Child = new OverflowBox
    {
        MinWidth = 190,
        MaxWidth = 190,
        MinHeight = 110,
        MaxHeight = 110,
        Child = new ColoredBox { Color = new Color(124, 205, 255) }
    }
};
```

### 角を丸める

`ClipRoundRect` は `BorderRadius` で角の丸みを決めます。

```csharp
new ClipRoundRect
{
    BorderRadius = BorderRadius.Circular(28),
    Child = Overflowing()
}
```

**`BorderRadius` は `init` プロパティではなくフィールドとして宣言されています。** これは宣言上の差で、オブジェクト初期化子での書き方は他のウィジェットと同じです。

**マウント済みのウィジェットのフィールドへ代入して見た目を変えることはできません。** 再ビルドも再描画も走らないためです。値を変えるときは、他のウィジェットと同じく新しい `ClipRoundRect` を構築してツリーを更新してください。

### 楕円で切り抜く

`ClipOval` は領域に内接する楕円で切り抜きます。領域が正方形なら真円になります。

```csharp
new ClipOval { Child = Overflowing() }
```

### 切り抜きの品質を変える

`ClipBehavior` は切り抜きの縁の処理を決めます。既定は `Antialias` で、曲線の縁が滑らかになります。`HardEdge` はアンチエイリアスを行いません。

```csharp
new ClipOval
{
    ClipBehavior = ClipMode.HardEdge,
    Child = Overflowing()
}
```

このサンプルは名前空間が `FloatSoda.Samples.Clip` で列挙型 `Clip` と衝突するため、`ClipMode` というエイリアスを使っています。名前空間が衝突しないアプリでは `Clip.HardEdge` と書けます。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 角丸クリップの名前 | **`ClipRoundRect`** | `ClipRRect` |
| パスクリップの名前 | **`ClipCustomPath`** | `ClipPath` |
| `BorderRadius` の宣言 | `ClipRoundRect` では **public フィールド**(他のウィジェットは `init` プロパティ) | `borderRadius` は `final` フィールド |
| 既定の `ClipBehavior` | すべて `Clip.Antialias` | `ClipRect` は `Clip.hardEdge`、`ClipRRect` / `ClipOval` / `ClipPath` は `Clip.antiAlias` |
| 超楕円クリップ | 無し | `ClipRSuperellipse` がある |

切り抜きの形と `ClipBehavior` の意味そのものは同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Clip -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Clip
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Image](../FloatSoda.Samples.Image) — `FittedBox` と組み合わせた画像の切り抜き
