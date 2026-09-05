# FittedBox

## これは何か

`FittedBox` は、子を自然な大きさでレイアウトしたうえで、その**描画を**自身の領域へ拡大縮小して収めるウィジェットです。Flutter の `FittedBox` に対応します。

制約を変換して子の寸法を変えるのではなく、子はあくまで自然な大きさのまま、描画だけを変形する点が特徴です。収め方は画像の `BoxFit` と同じ語彙(`Contain` / `Cover` / `Fill` / `FitWidth` / `FitHeight` / `None` / `ScaleDown`)で指定します。

## 使い方

### 全体を収める(Contain、既定)

`Fit` の既定は `BoxFit.Contain` で、子全体が収まる範囲で縦横比を維持して最大化します。

```csharp
new FittedBoxWidget
{
    Child = Flag()
}
```

このサンプルは名前空間が `FloatSoda.Samples.FittedBox` で型名 `FittedBox` と衝突するため、`FittedBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new FittedBox { ... }` と書けます。

`Flag()` は拡大縮小が分かるように目印の正方形を置いた `120 x 64` の子です。`150 x 100` の枠に `Contain` で収めると、1.25倍の `150 x 80` になります。

### 引き伸ばす(Fill)

`BoxFit.Fill` は縦横比を無視して領域全体へ引き伸ばします。目印の正方形が縦長に歪むことで確認できます。

```csharp
new FittedBoxWidget
{
    Fit = BoxFit.Fill,
    Child = Flag()
}
```

### 拡大縮小しない(None)

`BoxFit.None` は変形せず、子を自然な大きさのまま `Alignment`(既定は中央)へ置きます。

### 覆う(Cover)とクリップ

`BoxFit.Cover` は領域全体を覆う範囲で縦横比を維持するため、子の描画が領域からはみ出します。**`ClipBehavior` の既定は `Clip.None` で、はみ出しはそのまま見えます。**切り取りたい場合に `Clip.HardEdge` などを指定します。

```csharp
new FittedBoxWidget
{
    Fit = BoxFit.Cover,
    ClipBehavior = Clip.HardEdge,
    Child = Flag()
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Alignment` の指定 | プリセット9種のほか `new Alignment(x, y)` で任意の位置を指定できるが、`textDirection` による方向依存の解決は無い | `AlignmentGeometry` を取り、`AlignmentDirectional` なら `textDirection` で解決される |

`Fit` の各値の意味、子を自然な大きさでレイアウトして描画だけを変形する点、`ClipBehavior` の既定が `Clip.None` である点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.FittedBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.FittedBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.AspectRatio](../FloatSoda.Samples.AspectRatio) — 描画ではなくレイアウト寸法の比率を固定する場合
- [FloatSoda.Samples.Image](../FloatSoda.Samples.Image) — 画像での BoxFit の使用例
