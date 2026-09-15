# Wrap

## これは何か

`Wrap` は複数の子を主軸方向へ並べ、収まらなくなったら次の行(run)へ折り返すウィジェットです。Flutter の `Wrap` に対応します。

`Row` / `Column` は収まらない子をはみ出させますが、`Wrap` は折り返します。タグの一覧やボタン群など、個数が可変の子を並べる場面に向いています。1行ぶんの並びを **run** と呼び、`Row` と同じく並べる方向が主軸、直交する方向が交差軸です。

## 使い方

### 折り返して並べる

`Children` へ並べるだけで、主軸方向(既定は水平)に収まらない子から次の run へ送られます。`Spacing` は同じ run 内の子同士の間隔、`RunSpacing` は run 同士の間隔です。

```csharp
new WrapWidget
{
    Spacing = 8,
    RunSpacing = 8,
    Children =
    {
        Chip(new Color(124, 205, 255), 90),
        Chip(new Color(255, 111, 97), 60),
        Chip(new Color(255, 209, 102), 120),
        Chip(new Color(124, 205, 255), 70),
        Chip(new Color(255, 111, 97), 100),
        Chip(new Color(255, 209, 102), 80),
        Chip(new Color(124, 205, 255), 110),
        Chip(new Color(255, 111, 97), 50)
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.Wrap` で型名 `Wrap` と衝突するため、`WrapWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Wrap { ... }` と書けます。

### run 内の余りを配る

`Alignment` は、run ごとに残った主軸方向の余白の配り方です。`Row` の `MainAxisAlignment` に相当し、`Start` / `End` / `Center` / `SpaceBetween` / `SpaceAround` / `SpaceEvenly` を指定できます。

```csharp
new WrapWidget
{
    Alignment = WrapAlignment.Center,
    Spacing = 8,
    RunSpacing = 8,
    Children =
    {
        Chip(new Color(124, 205, 255), 90),
        Chip(new Color(255, 111, 97), 60),
        Chip(new Color(255, 209, 102), 120),
        Chip(new Color(124, 205, 255), 70),
        Chip(new Color(255, 111, 97), 100),
        Chip(new Color(255, 209, 102), 80),
        Chip(new Color(124, 205, 255), 110),
        Chip(new Color(255, 111, 97), 50)
    }
}
```

run 全体を交差軸方向へ配る `RunAlignment` も同じ `WrapAlignment` を取ります。こちらは `Wrap` 自身の交差軸に余白がある場合(高さが固定されている場合など)に効きます。

### run 内の交差軸を揃える

`CrossAxisAlignment` は、高さ(水平 `Wrap` の場合)の異なる子を run 内でどう揃えるかです。`Start` / `End` / `Center` を指定できます。

```csharp
new WrapWidget
{
    CrossAxisAlignment = WrapCrossAlignment.Center,
    Spacing = 8,
    RunSpacing = 8,
    Children =
    {
        Box(new Color(124, 205, 255), 80, 32),
        Box(new Color(255, 111, 97), 80, 48),
        Box(new Color(255, 209, 102), 80, 64),
        Box(new Color(124, 205, 255), 80, 32),
        Box(new Color(255, 111, 97), 80, 48)
    }
}
```

`Direction = Axis.Vertical` を指定すると縦に並べて横へ折り返します。このサンプルでは扱っていません。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 指定できる項目 | `Direction` / `Alignment` / `Spacing` / `RunAlignment` / `RunSpacing` / `CrossAxisAlignment` / `VerticalDirection` / `Children` | 上記に加えて `textDirection` / `clipBehavior` を取る |
| 文字方向の影響 | 受けない。主軸の並び順は常に左から右 | `textDirection` で右から左にできる |

折り返しの判定、`Spacing` / `RunSpacing` の効き方、各揃え方の意味は Flutter と同等です。`clipBehavior` はありませんが、Flutter の既定も `Clip.none` のため、既定同士の見た目は一致します。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Wrap -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Wrap
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Expanded](../FloatSoda.Samples.Expanded) — 一列のまま余剰領域を比率で分配する場合
