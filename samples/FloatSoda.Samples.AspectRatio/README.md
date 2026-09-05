# AspectRatio

## これは何か

`AspectRatio` は、幅と高さが指定した比率になるように子をレイアウトするウィジェットです。Flutter の `AspectRatio` に対応します。

親から受け取った制約の範囲で、幅が有限ならまず幅いっぱいを採り、比率から高さを決めます。その高さが上限を超える場合は、高さを上限に合わせて幅を逆算します。幅が無制約(unbounded)で高さだけ有限な場合 — 水平方向の `Row` の中など — は逆で、高さいっぱいを起点に幅を決めます。

## 使い方

### 比率を指定する

`Ratio` に「幅 ÷ 高さ」を渡します。16:9 なら `16.0 / 9.0` です。

```csharp
new Center
{
    Child = new AspectRatioWidget
    {
        Ratio = 16.0 / 9.0,
        Child = Fill(new Color(124, 205, 255))
    }
}
```

このサンプルは名前空間が `FloatSoda.Samples.AspectRatio` で型名 `AspectRatio` と衝突するため、`AspectRatioWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new AspectRatio { ... }` と書けます。

幅 150 の領域なら、16:9 は `150 x 84`、3:4 は高さ 200 が要りますが上限 150 に収まらないため、高さ 150 から逆算して `112.5 x 150` になります。

### tight 制約下では効かない

**親が寸法を固定していると比率は効きません。** `AspectRatio` は制約の範囲内でしか寸法を選べないため、tight な制約(固定寸法の `SizedBox` の直下など)ではそのまま親の寸法になります。

```csharp
new AspectRatioWidget
{
    Ratio = 16.0 / 9.0,
    Child = Fill(new Color(124, 205, 255))
}
```

このサンプルの最初の3つの枠が `Center` を挟んでいるのは、緩い制約を作って比率を効かせるためです。

また、幅と高さの**両方**が制約されていない場所(無限に広がる領域)では寸法を決められず、`InvalidOperationException` になります。片方だけ無制約なら、有限な側を起点に寸法が決まります。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 比率プロパティの名前 | `Ratio` | `aspectRatio`(C# ではメンバー名が型名 `AspectRatio` と同名にできないための改名) |
| 両軸が無制約の場合 | `InvalidOperationException` | assert で失敗する(debug ビルド) |

寸法の決まり方(幅優先で比率を適用し、制約へ収める)は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.AspectRatio -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.AspectRatio
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.FittedBox](../FloatSoda.Samples.FittedBox) — 子の描画自体を拡大縮小する場合
