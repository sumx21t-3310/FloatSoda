# Align

## これは何か

`Align` は与えられた領域の中で、子をどの位置に置くかを決めるウィジェットです。Flutter の `Align` に対応します。

`Center` は `Align` に `Alignment.Center` を固定した薄いラッパーで、中央寄せだけならこちらが読みやすくなります。

## 使い方

### 配置位置を指定する

`Alignment` にプリセットを渡します。`TopLeft` / `TopCenter` / `TopRight` / `CenterLeft` / `Center` / `CenterRight` / `BottomLeft` / `BottomCenter` / `BottomRight` の9つがあります。

```csharp
new AlignWidget
{
    Alignment = Alignment.TopLeft,
    Child = Marker(new Color(124, 205, 255))
}
```

このサンプルは名前空間が `FloatSoda.Samples.Align` で型名 `Align` と衝突するため、`AlignWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Align { ... }` と書けます。

`Align` は既定で、親から与えられた領域いっぱいに広がります。そのうえで子をその中に配置します。

### Center を使う

中央寄せだけなら `Center` が使えます。

```csharp
new Center
{
    Child = Marker(new Color(255, 111, 97))
}
```

### 領域を子に合わせて縮める

`WidthFactor` / `HeightFactor` を指定すると、`Align` 自身が「子の寸法 × 係数」を自分の寸法として要求します。子が `48 x 48` で係数が `2.0` なら `96 x 96` です。子の周囲に子と同じだけの余白を持たせたいときに使えます。

**ただし、これは親から緩い制約を受け取っている場合に限ります。** `Align` は算出した寸法を最後に親の制約へ収めるため、親が固定寸法を強制していると係数は効かず、親の寸法まで引き伸ばされます。`SizedBox` で寸法を固定した直下に置いても収縮しないのはこのためです。

このサンプルでは `Center` を挟んで緩い制約を作り、`ColoredBox` で `Align` 自身の範囲が見えるようにしています。

```csharp
new Center
{
    Child = new ColoredBox
    {
        Color = new Color(92, 76, 44),
        Child = new AlignWidget
        {
            Alignment = Alignment.Center,
            WidthFactor = 2.0,
            HeightFactor = 2.0,
            Child = Marker(new Color(255, 209, 102))
        }
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 係数の型 | `WidthFactor` / `HeightFactor` は `double?` | `widthFactor` / `heightFactor` は `double?`（同等） |
| 文字方向の影響 | 受けない | `AlignmentDirectional` を使うと `textDirection` の影響を受ける |

配置と収縮の振る舞いは同等です。係数が親の制約に収められる点も Flutter と同じです。

`Alignment` は `readonly record struct Alignment(float X = 0, float Y = 0)` なので、プリセット以外に `new Alignment(0.5f, -0.25f)` のような任意位置も指定できます。Flutter の `Alignment(x, y)` と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Align -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Align
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Flex](../FloatSoda.Samples.Flex) — 複数の子を並べる場合の揃え方
