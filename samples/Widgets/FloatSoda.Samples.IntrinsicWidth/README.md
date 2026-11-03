# IntrinsicWidth

## これは何か

`IntrinsicWidth` は、子の幅を、子が本来ほしい幅(intrinsic の幅)に揃えるウィジェットです。`IntrinsicHeight` は、同じことを高さで行います。どちらも Flutter の同名のウィジェットに対応します。

よく使うのは、長さの違う項目を一番長い項目の幅に揃える場面です。たとえば「戻る」「設定を開く」「終了」の3つのボタンを縦に並べ、全部を「設定を開く」の幅にそろえられます。親の幅いっぱいに広げると横に間延びし、何もしないと幅がばらばらになる、その中間を取ります。

## 使い方

### 縦に並べた項目を、一番長い項目の幅に揃える

`CrossAxisAlignment.Stretch` の `Column` は、子を横方向に親の幅いっぱいまで広げます。このサンプルでは、項目を並べる `Menu()` がその形になっています。

```csharp
private static Widget Menu() => new Column
{
    CrossAxisAlignment = CrossAxisAlignment.Stretch,
    Children =
    {
        MenuItem("戻る"),
        new SizedBox { Height = 8 },
        MenuItem("設定を開く"),
        new SizedBox { Height = 8 },
        MenuItem("終了")
    }
};
```

幅 320 の `SizedBox` に入れると、3つの項目はどれも幅 320 まで広がります。

```csharp
new SizedBox
{
    Width = 320,
    Child = Menu()
},
```

`IntrinsicWidth` で包むと、`Column` の幅が子の intrinsic の幅の最大値に決まります。3つとも、一番長い「設定を開く」の幅になります。

```csharp
new IntrinsicWidthWidget { Child = Menu() },
```

このサンプルは名前空間が `FloatSoda.Samples.IntrinsicWidth` で型名 `IntrinsicWidth` と衝突するため、`IntrinsicWidthWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new IntrinsicWidth { ... }` と書けます。

### 幅をきりのよい値へ切り上げる

`StepWidth` を指定すると、幅をその倍数へ切り上げます。次の例では、幅が 100 の倍数になります。

```csharp
new IntrinsicWidthWidget
{
    StepWidth = 100,
    Child = Menu()
}
```

`StepWidth` には正の有限値か `null` を指定します。`0` は `null` と同じ扱いです。

### 横に並べた項目を、一番高い項目の高さに揃える

`IntrinsicHeight` は、`CrossAxisAlignment.Stretch` の `Row` と組み合わせます。このサンプルの `Card()` は、高さを持たない赤い帯と、2行のテキストを横に並べています。

```csharp
private static Widget Card() => new Row
{
    CrossAxisAlignment = CrossAxisAlignment.Stretch,
    Children =
    {
        // 高さを持たない帯。Stretch によって Row の高さまで伸びる。
        new ColoredBox
        {
            Color = new Color(255, 111, 97),
            Child = new SizedBox { Width = 16 }
        },
        // ...
    }
};
```

高さ 200 の `SizedBox` に入れると、帯もテキストの背景も高さ 200 まで伸びます。`IntrinsicHeight` で包むと、`Row` の高さが2行のテキストの高さに決まり、帯もその高さになります。

```csharp
new IntrinsicHeight { Child = Card() }
```

`Column` や `Row` の中では、子に渡る高さ(または幅)に上限がありません。上限がないまま `Stretch` を指定しても、子は伸びません。`IntrinsicHeight` で包むと上限が決まり、`Stretch` が効くようになります。

### 使うときの注意

`IntrinsicWidth` と `IntrinsicHeight` は、子を配置する前に、子孫の intrinsic の大きさを測ります。入れ子が深いほど測る回数が増え、レイアウトにかかる時間が増えます。幅や高さを数値で決められる場面では、`SizedBox` を使うことが望ましいです。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `IntrinsicWidth` の高さの切り上げ | **未提供**。`StepWidth` だけを取る | `stepHeight` も取る |
| `IntrinsicHeight` の切り上げ | `StepHeight` を取る | 取らない |
| `IntrinsicHeight.StepHeight` に `0` | `ArgumentOutOfRangeException` を投げる | (該当する項目がない) |
| `IntrinsicWidth.StepWidth` に `0` | `null` と同じ扱い | 同等 |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.IntrinsicWidth -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/Widgets/FloatSoda.Samples.IntrinsicWidth
```

## 関連

- [FloatSoda.Samples.Flex](../FloatSoda.Samples.Flex) — `Row` と `Column` の `CrossAxisAlignment`
- [FloatSoda.Samples.SizedBox](../FloatSoda.Samples.SizedBox) — 幅と高さを数値で決める
- [docs/reference/find-widgets.md](../../../docs/reference/find-widgets.md) — 組み込みウィジェット一覧
