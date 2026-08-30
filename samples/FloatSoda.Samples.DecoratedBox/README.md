# DecoratedBox

## これは何か

`DecoratedBox` は、子の背面または前面へボックス装飾(背景色・角丸・ボーダー)を描くウィジェットです。Flutter の `DecoratedBox` に対応します。

装飾の内容は `BoxDecoration` で指定します。装飾は自身のレイアウト寸法いっぱいに描かれるため、寸法は `SizedBox` などで子側が決めます。

## 使い方

### 背景色と角丸

`BoxDecoration.Color` が塗り、`BorderRadius` が四隅の丸めです。`BorderRadius.Circular` で四隅へ同じ半径を指定できます。

```csharp
new DecoratedBoxWidget
{
    Decoration = new BoxDecoration
    {
        Color = new Color(124, 205, 255),
        BorderRadius = BorderRadius.Circular(24)
    },
    Child = new SizedBox { Width = 110, Height = 110 }
}
```

このサンプルは名前空間が `FloatSoda.Samples.DecoratedBox` で型名 `DecoratedBox` と衝突するため、`DecoratedBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new DecoratedBox { ... }` と書けます。

### ボーダー

`Border.All` で四辺へ同じ `BorderSide`(色と太さ)を引きます。辺ごとに変えたい場合は `Border` の `Top` / `Right` / `Bottom` / `Left` を個別に指定します。

```csharp
new DecoratedBoxWidget
{
    Decoration = new BoxDecoration
    {
        Color = new Color(255, 111, 97),
        Border = Border.All(new BorderSide
        {
            Color = new Color(255, 209, 102),
            Width = 6
        })
    },
    Child = new SizedBox { Width = 110, Height = 110 }
}
```

`BorderRadius` と組み合わせると、ボーダーも角丸に沿って描かれます。

### 前面へ描く(Foreground)

`Position` の既定は `DecorationPosition.Background`(子の背面)です。`Foreground` にすると子の前面へ描きます。`Color` の第4引数はアルファ値で、半透明の装飾をかぶせられます。

```csharp
new DecoratedBoxWidget
{
    Position = DecorationPosition.Foreground,
    Decoration = new BoxDecoration
    {
        Color = new Color(255, 209, 102, 140)
    },
    Child = new ColoredBox
    {
        Color = new Color(124, 205, 255),
        Child = new SizedBox { Width = 110, Height = 110 }
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 装飾の型 | 具象型 `BoxDecoration` 固定 | 抽象型 `Decoration` を取り、`ShapeDecoration` 等も渡せる |
| `BoxDecoration` の表現 | 背景色・角丸・ボーダーの3点 | 上記に加えて `gradient` / `image` / `boxShadow` / `shape`(円形)/ `backgroundBlendMode` |
| ボーダーの辺 | `BorderSide` は色と太さのみ(`Width = 0` で非表示) | `style`(none / solid)と `strokeAlign`(線を引く位置)がある |

`Position`(`Background` / `Foreground`)の意味と、装飾が自身の寸法いっぱいに描かれる点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.DecoratedBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.DecoratedBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.ColoredBox](../FloatSoda.Samples.ColoredBox) — 単色の塗りだけで足りる場合
- [FloatSoda.Samples.Clip](../FloatSoda.Samples.Clip) — 子の描画自体を角丸で切り取る場合
