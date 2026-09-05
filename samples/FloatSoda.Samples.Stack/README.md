# Stack

## これは何か

`Stack` は複数の子を同じ領域へ重ねて配置するウィジェットです。Flutter の `Stack` に対応します。

子はリストの順に下から積まれ、後の子ほど手前に描かれます。子の位置は2通りの方法で決めます。`Positioned` で包んでいない子は `Alignment` の位置へまとめて配置され、`Positioned` で包んだ子は辺からの距離で絶対配置されます。

## 使い方

### 重ねる

`Children` へ並べるだけで重なります。後の子が手前です。

```csharp
new StackWidget
{
    Alignment = Alignment.Center,
    Children =
    {
        Marker(new Color(124, 205, 255), 110),
        Marker(new Color(255, 111, 97), 80),
        Marker(new Color(255, 209, 102), 50)
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.Stack` で型名 `Stack` と衝突するため、`StackWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Stack { ... }` と書けます。

### 配置位置を指定する

`Alignment` は `Positioned` を使わないすべての子に効きます。既定は `Alignment.TopLeft` です。

```csharp
new StackWidget
{
    Alignment = Alignment.BottomRight,
    Children =
    {
        Marker(new Color(124, 205, 255), 90),
        Marker(new Color(255, 111, 97), 50)
    }
}
```

### Positioned で絶対配置する

`Stack` の直接の子を `Positioned` で包むと、辺からの距離(`Left` / `Top` / `Right` / `Bottom`)と寸法(`Width` / `Height`)で位置を決められます。

```csharp
new Positioned
{
    Left = 10,
    Top = 10,
    Child = Marker(new Color(124, 205, 255), 48)
}
```

水平方向は `Left` / `Right` / `Width` のうち2つまで、垂直方向は `Top` / `Bottom` / `Height` のうち2つまで指定できます。3つ同時に指定すると `InvalidOperationException` になります。

### 両端を指定して引き伸ばす

同じ軸の両端(`Left` と `Right`、または `Top` と `Bottom`)を指定すると、子はその間の寸法へ引き伸ばされます。

```csharp
new Positioned
{
    Left = 10,
    Right = 10,
    Top = 59,
    Height = 32,
    Child = Fill(new Color(255, 209, 102))
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| はみ出しの扱い | クリップしない。`Stack` の範囲を超えた子もそのまま描かれる | 既定(`clipBehavior: Clip.hardEdge`)でクリップする。`clipBehavior` プロパティ自体が FloatSoda には無い |
| `Alignment` の指定 | プリセット9種のほか `new Alignment(x, y)` で任意の位置を指定できる。既定は `TopLeft` で、`textDirection` による方向依存の解決は無い | `AlignmentDirectional.topStart` が既定で、`textDirection` の影響を受ける |
| `Positioned` の補助コンストラクタ | 無し | `Positioned.fill` / `Positioned.fromRect` / `Positioned.directional` がある |
| 同一軸に3つ指定した場合 | レイアウト時に `InvalidOperationException` | コンストラクタの `assert` で即座に失敗する(debug ビルド) |

はみ出しの扱いは**見た目に出る差異**です。Flutter で `Stack` の外へ出た子は既定で切り取られますが、FloatSoda ではそのまま見えます。Flutter の `clipBehavior: Clip.none` を指定した状態に相当します。切り取りたい場合は `Stack` を `ClipRect` で包んでください。

`Fit`(`StackFit.Loose` / `Expand` / `Passthrough`)の3値と、`Positioned` を使わない子への効き方は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Stack -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Stack
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.Padding](../FloatSoda.Samples.Padding) — 重ねずに余白で位置を作る場合
