# Flex

## これは何か

`Flex` は複数の子を1方向に並べるウィジェットです。Flutter の `Flex` に対応します。

`Row` は `Flex` に `Axis.Horizontal` を、`Column` は `Axis.Vertical` を固定した薄いラッパーです。方向が固定でよければ `Row` / `Column` のほうが読みやすくなります。

並べる方向を**主軸(main axis)**、それと直交する方向を**交差軸(cross axis)**と呼びます。`Row` なら主軸が水平、交差軸が垂直です。

## 使い方

### 方向を指定して並べる

`Direction` に `Axis.Horizontal` か `Axis.Vertical` を渡します。

```csharp
new FlexWidget
{
    Direction = Axis.Vertical,
    MainAxisAlignment = MainAxisAlignment.SpaceBetween,
    Children =
    {
        Bar(new Color(124, 205, 255), 160, 24),
        Bar(new Color(255, 111, 97), 220, 24)
    }
}
```

`Children` は `List<Widget>` で、オブジェクト初期化子の中に直接並べられます。

### 主軸方向の余りを配る

`MainAxisAlignment` は、子を並べた後に残った主軸方向の余白をどう配るかを決めます。

```csharp
new FlexWidget
{
    Direction = Axis.Horizontal,
    MainAxisAlignment = alignment,
    Children =
    {
        Bar(new Color(124, 205, 255), 90, 40),
        Bar(new Color(255, 111, 97), 90, 40),
        Bar(new Color(255, 209, 102), 90, 40)
    }
}
```

`Start` は先頭に寄せ、`Center` は中央にまとめ、`SpaceBetween` は子と子の間に均等に配ります。

### 交差軸方向の揃え方

`CrossAxisAlignment` は、高さ(`Row` の場合)の異なる子をどう揃えるかを決めます。

```csharp
new FlexWidget
{
    Direction = Axis.Horizontal,
    CrossAxisAlignment = alignment,
    Children =
    {
        Bar(new Color(124, 205, 255), 90, 24),
        Bar(new Color(255, 111, 97), 90, 48),
        Bar(new Color(255, 209, 102), 90, 64)
    }
}
```

### 主軸のサイズを縮める

`MainAxisSize` は `Flex` 自身が主軸方向にどこまで広がるかを決めます。既定の `MainAxisSize.Max` では親から与えられた主軸方向いっぱいに広がり、`MainAxisSize.Min` を指定すると子の合計ぶんだけに縮みます。

このサンプルでは扱っていません。使用例は [FloatSoda.Samples.Align](../FloatSoda.Samples.Align) と [FloatSoda.Samples.Clip](../FloatSoda.Samples.Clip) にあります。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 指定できる項目 | `Direction` / `MainAxisAlignment` / `MainAxisSize` / `CrossAxisAlignment` / `VerticalDirection` / `Children` | 上記に加えて `textDirection` / `textBaseline` / `clipBehavior` / `spacing` を取る |
| 子の間隔 | **`spacing` に相当するものがない。** `SizedBox` を子として挟む | `spacing: 8` で一括指定できる |
| はみ出し時の切り抜き | `clipBehavior` に相当するものがない | `clipBehavior` で指定できる |
| ベースライン揃え | `textBaseline` が無いため、`CrossAxisAlignment.Baseline` 相当は使えない | `textBaseline` と組み合わせて使える |
| 子の指定 | `Children` は `List<Widget>` の `init` プロパティ | `children` は位置指定引数 |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Flex -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Flex
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.SizedBox](../FloatSoda.Samples.SizedBox) — 子の間隔の作り方
- [FloatSoda.Samples.Align](../FloatSoda.Samples.Align) — 単一の子の配置
