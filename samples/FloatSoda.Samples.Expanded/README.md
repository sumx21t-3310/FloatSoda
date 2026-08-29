# Expanded

## これは何か

`Expanded` は `Row` / `Column` / `Flex` の直接の子として使い、固定寸法の子を並べた後に残る主軸方向の余剰領域を、`Flex` の比率で受け取るウィジェットです。Flutter の `Expanded` に対応します。

同じ仕組みの仲間が2つあります。`Flexible` は割当量を「上限」として子に選ばせ、`Spacer` は割当量をそのまま空白にします。

## 使い方

### 余剰領域を比率で分ける

`Flex`(既定値1)が分配の比率です。`1 : 2 : 1` なら中央の子が2倍の幅を受け取ります。

```csharp
new Row
{
    CrossAxisAlignment = CrossAxisAlignment.Center,
    Children =
    {
        new ExpandedWidget { Child = FlexBar(new Color(124, 205, 255)) },
        new ExpandedWidget { Flex = 2, Child = FlexBar(new Color(255, 111, 97)) },
        new ExpandedWidget { Child = FlexBar(new Color(255, 209, 102)) }
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.Expanded` で型名 `Expanded` と衝突するため、`ExpandedWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Expanded { ... }` と書けます。

### 固定幅の子と混在させる

`Expanded` で包んでいない子が先にレイアウトされ、その残りが余剰領域になります。「固定幅のアイコン + 残り全部のテキスト」のような構成の基本形です。

```csharp
new Row
{
    CrossAxisAlignment = CrossAxisAlignment.Center,
    Children =
    {
        Bar(new Color(124, 205, 255), 220, 40),
        new ExpandedWidget { Child = FlexBar(new Color(255, 111, 97)) }
    }
}
```

### Expanded と Flexible の違い

どちらも余剰領域を比率で受け取りますが、割当量の使い方が違います。

- `Expanded` — 子を割当量**いっぱいに引き伸ばす**(tight)。子が自分で指定した幅は上書きされる。
- `Flexible` — 割当量を**上限**として渡す(既定の `FlexFit.Loose`)。子は割当量以下の好きな大きさを選べる。

このサンプルでは、どちらにも「幅90を要求する子」を入れて結果を比較しています。

```csharp
new ExpandedWidget
{
    Child = new ColoredBox
    {
        Color = new Color(124, 205, 255),
        Child = new SizedBox { Width = 90, Height = 40 }
    }
}
```

`Expanded` の帯は幅90の指定を無視して帯いっぱいに広がり、`Flexible` の帯は幅90のままです。

`Flexible` に `Fit = FlexFit.Tight` を指定すると `Expanded` と同じ動きになります。

### Spacer で空白を入れる

`Spacer` は「何も描かない `Expanded`」です。子同士の間隔を比率で空けたいときに使います。

```csharp
new Row
{
    CrossAxisAlignment = CrossAxisAlignment.Center,
    Children =
    {
        Bar(new Color(124, 205, 255), 90, 40),
        new Spacer(),
        Bar(new Color(255, 111, 97), 90, 40),
        new Spacer { Flex = 2 },
        Bar(new Color(255, 209, 102), 90, 40)
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Flex` に指定できる値 | 1以上。0以下は `ArgumentOutOfRangeException` | `Flexible` は `flex: 0`(固定寸法の子として扱う)も指定できる |
| 型の関係 | `Expanded` と `Flexible` は独立した型 | `Expanded` は `Flexible` の派生クラス |

余剰領域の分配、`FlexFit.Tight` / `Loose` の意味、`Spacer` が `Expanded` + 空の `SizedBox` である点は Flutter と同等です。主軸が無限に伸びる場所(スクロール領域など)へ flex を持つ子を置くとエラーになる点も同じです。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Expanded -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Expanded
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Flex](../FloatSoda.Samples.Flex) — 主軸・交差軸の揃え方
