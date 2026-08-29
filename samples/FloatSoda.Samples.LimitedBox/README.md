# LimitedBox

## これは何か

`LimitedBox` は、親から上限を受け取らなかった(unbounded な)軸にだけ、`MaxWidth` / `MaxHeight` を上限として適用するウィジェットです。Flutter の `LimitedBox` に対応します。

普段の bounded なレイアウトの中では何もしません。制約が無限になる場所 — 将来のスクロール領域や、制約を取り除く `UnconstrainedBox` の中 — で「無限に広がられては困る」子に安全な上限を与えるためのウィジェットです。

## 使い方

### bounded な場所では何もしない

親が上限を与えている軸では、`MaxWidth` / `MaxHeight` は無視されます。固定寸法の枠の直下に置いても、枠の制約がそのまま子へ渡ります。

```csharp
new LimitedBoxWidget
{
    MaxWidth = 80,
    MaxHeight = 80,
    Child = Fill(new Color(124, 205, 255))
}
```

このサンプルは名前空間が `FloatSoda.Samples.LimitedBox` で型名 `LimitedBox` と衝突するため、`LimitedBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new LimitedBox { ... }` と書けます。

### unbounded な場所で上限になる

`UnconstrainedBox` で制約を取り除いた中に置くと、`MaxWidth` / `MaxHeight` が上限として効きます。幅 `500` を要求する子が `100` で止まります。

```csharp
new UnconstrainedBox
{
    Child = new LimitedBoxWidget
    {
        MaxWidth = 100,
        MaxHeight = 100,
        Child = Bar(new Color(255, 111, 97), 500, 40)
    }
}
```

`LimitedBox` を外すと、子は要求どおりの幅になり枠からはみ出します。このサンプルの3つ目の枠がその比較です。

## Flutterとの違い

同等です。`MaxWidth` / `MaxHeight` の既定値が正の無限大である点、unbounded な軸にだけ効く点は Flutter と同じです。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.LimitedBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.LimitedBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.ConstraintsTransformBox](../FloatSoda.Samples.ConstraintsTransformBox) — 制約を取り除く側のウィジェット
- [FloatSoda.Samples.SizedBox](../FloatSoda.Samples.SizedBox) — bounded な場所で寸法を決める場合
