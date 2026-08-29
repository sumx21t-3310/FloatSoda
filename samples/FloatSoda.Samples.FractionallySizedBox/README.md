# FractionallySizedBox

## これは何か

`FractionallySizedBox` は、親から使える最大寸法に対する**割合**で子の寸法を決めるウィジェットです。Flutter の `FractionallySizedBox` に対応します。

「親の半分の幅」のような相対指定ができます。係数を指定しなかった軸は、親の制約がそのまま子へ渡ります。

## 使い方

### 割合で寸法を決める

`WidthFactor` / `HeightFactor` に 0 以上の係数を渡します。親の幅 `150` に `WidthFactor = 0.5` なら子の幅は `75` です。

```csharp
new FractionallySizedBoxWidget
{
    WidthFactor = 0.5,
    Child = Fill(new Color(124, 205, 255))
}
```

このサンプルは名前空間が `FloatSoda.Samples.FractionallySizedBox` で型名と衝突するため、`FractionallySizedBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new FractionallySizedBox { ... }` と書けます。

### 配置を指定する

割合で小さくなった子は、既定では中央に置かれます。`Alignment` で位置を変えられます。

```csharp
new FractionallySizedBoxWidget
{
    WidthFactor = 0.5,
    HeightFactor = 0.5,
    Alignment = Alignment.TopLeft,
    Child = Fill(new Color(255, 209, 102))
}
```

### 1 を超える係数

係数は 1 を超えてもかまいません。子は親より大きくなり、はみ出した部分は**クリップされずそのまま描かれます**。

```csharp
new FractionallySizedBoxWidget
{
    WidthFactor = 1.4,
    HeightFactor = 0.3,
    Child = Fill(new Color(124, 205, 255))
}
```

## Flutterとの違い

同等です。係数未指定の軸に親の制約がそのまま渡る点、1 超の係数によるはみ出しがクリップされない点、既定の配置が中央である点は Flutter と同じです。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.FractionallySizedBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.FractionallySizedBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.SizedBox](../FloatSoda.Samples.SizedBox) — 絶対値で寸法を決める場合
- [FloatSoda.Samples.OverflowBox](../FloatSoda.Samples.OverflowBox) — 親と無関係な制約を渡す場合
