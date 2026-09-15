# SizedBox

## これは何か

`SizedBox` は子に固定の寸法を与えるウィジェットです。Flutter の `SizedBox` に対応します。

子を持たない `SizedBox` は、その寸法ぶんの空白として働きます。`Row` / `Column` の要素間隔はこれで作ります。

## 使い方

### 寸法を固定する

`Width` と `Height` を指定します。

```csharp
new SizedBoxWidget { Width = width, Height = height }
```

このサンプルは名前空間が `FloatSoda.Samples.SizedBox` で型名 `SizedBox` と衝突するため、`SizedBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new SizedBox { ... }` と書けます。

### 余白として使う

子を省略すると、指定した寸法の空白になります。

```csharp
// 子を持たない SizedBox は、そのぶんの空白として働く。
// Row / Column の要素間隔はこれで作る。
new SizedBoxWidget { Width = 40 },
```

`Row` なら `Width`、`Column` なら `Height` を指定します。

### 片方だけ指定する

`Width` と `Height` はどちらも `double?` で、省略できます。省略した軸は親から渡された制約に従います。

```csharp
new SizedBoxWidget
{
    Height = 70,
    Child = new ColoredBox
    {
        Color = new Color(40, 47, 64),
        Child = new Center
        {
            Child = Label("Height = 70 のみ指定")
        }
    }
}
```

高さだけを決めて幅は親いっぱいに広げたい、という場合に使います。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 生成方法 | `Width` / `Height` の `init` プロパティのみ | `SizedBox.expand` / `SizedBox.shrink` / `SizedBox.square` の名前付きコンストラクタがある |
| 領域いっぱいに広げる | 名前付きの手段はない。`Width` / `Height` を省略して親の制約に従わせるか、`Align` などで広げる | `SizedBox.expand()` |
| 最小に縮める | 名前付きの手段はない。`Width = 0, Height = 0` を指定する | `SizedBox.shrink()` |
| 正方形 | `Width` と `Height` に同じ値を書く | `SizedBox.square(dimension: 48)` |

名前付きコンストラクタが無いのは、FloatSoda がオブジェクト初期化子ファーストの設計方針を採っているためです(→ [docs/APIDesign.md](../../docs/APIDesign.md))。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.SizedBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.SizedBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Flex](../FloatSoda.Samples.Flex) — 余白を挟む相手側の並べ方
