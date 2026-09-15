# OverflowBox

## これは何か

`OverflowBox` は、自身の寸法とは無関係に、子へ任意の制約(`MinWidth` / `MaxWidth` / `MinHeight` / `MaxHeight`)を渡すウィジェットです。Flutter の `OverflowBox` に対応します。子が自身より大きくなることを許すため、はみ出しが起きえます。はみ出しはクリップされず、そのまま描かれます。

指定しなかった値には、親から受け取った制約がそのまま使われます。

`SizedOverflowBox` は兄弟ウィジェットで、自身の寸法を `Size` で固定しつつ、子には親から受け取った元の制約を渡します。

## 使い方

### 枠より大きい制約を渡す

自身は枠の寸法のまま、子にだけ枠より大きい最大幅を渡します。幅 `190` の子が幅 `150` の枠からはみ出します。

```csharp
new OverflowBoxWidget
{
    MinHeight = 0,
    MaxWidth = 210,
    Child = Bar(new Color(124, 205, 255), 190, 40)
}
```

このサンプルは名前空間が `FloatSoda.Samples.OverflowBox` で型名 `OverflowBox` と衝突するため、`OverflowBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new OverflowBox { ... }` と書けます。

`MinHeight = 0` を指定しているのは、固定寸法の枠の直下では親の最小高さ(`150`)がそのまま子へ渡るためです。外さないと子の高さ `40` が `150` へ引き伸ばされます。

### 自身の寸法の決め方(Fit)

`Fit` の既定は `OverflowBoxFit.Max` で、自身は使える領域いっぱいに広がります。`DeferToChild` にすると子と同じ寸法まで縮みます。

```csharp
new OverflowBoxWidget
{
    Fit = OverflowBoxFit.DeferToChild,
    Child = Bar(new Color(255, 111, 97), 60, 60)
}
```

このサンプルでは `ColoredBox` を背後に敷いて自身の範囲を可視化しています。`Max` では茶色が枠いっぱいに、`DeferToChild` では子の背後だけに見えます。

### 寸法を固定する(SizedOverflowBox)

`SizedOverflowBox` は自身の寸法を `Size` で決め、子には親から受け取った元の制約を渡します。子が指定寸法より大きければ、そのぶんはみ出します。

```csharp
new SizedOverflowBox
{
    Size = new Size(80, 80),
    Child = Bar(new Color(255, 209, 102), 120, 40)
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Alignment` の指定 | プリセット9種のほか `new Alignment(x, y)` で任意の位置を指定できるが、`textDirection` による方向依存の解決は無い | `AlignmentGeometry` を取り、`AlignmentDirectional` なら `textDirection` で解決される |

制約の受け渡し、`OverflowBoxFit.Max` / `DeferToChild` の意味、はみ出しがクリップされない点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.OverflowBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.OverflowBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.FractionallySizedBox](../FloatSoda.Samples.FractionallySizedBox) — 割合で子の寸法を決める場合
- [FloatSoda.Samples.ConstraintsTransformBox](../FloatSoda.Samples.ConstraintsTransformBox) — 制約そのものを変換する場合
