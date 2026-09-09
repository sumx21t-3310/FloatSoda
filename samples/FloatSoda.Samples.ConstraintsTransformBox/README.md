# ConstraintsTransformBox

## これは何か

`ConstraintsTransformBox` は、親から受け取った制約を任意の変換で書き換えてから子へ渡すウィジェットです。Flutter の `ConstraintsTransformBox` に対応します。

`UnconstrainedBox` はそのいちばんよく使う形 — 制約を取り除いて子を自然な大きさにする — を切り出した薄いラッパーです。

**注意して使うウィジェットです。**変換後の制約は親の制約と無関係になれるため、子が枠より大きくなり、はみ出しが起きえます。はみ出しは既定(`ClipBehavior = Clip.None`)でそのまま描かれます。

## 使い方

### 制約を取り除く(UnconstrainedBox)

子は自然な大きさでレイアウトされます。幅 `220` を要求する子は、枠が `150` でも `220` になります。

```csharp
new UnconstrainedBox
{
    Child = Bar(new Color(124, 205, 255), 220, 40)
}
```

### 片方の軸だけ残す

`ConstrainedAxis` に指定した軸の制約は維持されます。`Axis.Horizontal` なら幅の制約が残るため、幅 `220` の要求は枠の幅 `150` へ収められます。

```csharp
new UnconstrainedBox
{
    ConstrainedAxis = Axis.Horizontal,
    Child = Bar(new Color(255, 111, 97), 220, 40)
}
```

### はみ出しを切り取る

`ClipBehavior` を指定すると、はみ出しを自身の枠で切り取ります。

```csharp
new UnconstrainedBox
{
    ClipBehavior = Clip.HardEdge,
    Child = Bar(new Color(255, 209, 102), 220, 40)
}
```

### 任意の変換を書く

`ConstraintsTransformBox` の `ConstraintsTransform` は `BoxConstraints` を受け取って `BoxConstraints` を返すデリゲートです。`Unconstrained` / `WidthUnconstrained` / `MaxWidthUnconstrained` などの定義済み変換が静的メソッドとして用意されているほか、自分で書くこともできます。

```csharp
new ConstraintsTransformBoxWidget
{
    ConstraintsTransform = Cap100,
    Child = Bar(new Color(124, 205, 255), 220, 220)
}
```

```csharp
/// <summary>最大幅と最大高さを100へ差し替えるカスタム変換。</summary>
private static BoxConstraints Cap100(BoxConstraints constraints) =>
    new(MaxWidth: 100, MaxHeight: 100);
```

このサンプルは名前空間が `FloatSoda.Samples.ConstraintsTransformBox` で型名と衝突するため、`ConstraintsTransformBoxWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new ConstraintsTransformBox { ... }` と書けます。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 定義済み変換 | `Unmodified` / `Unconstrained` / `WidthUnconstrained` / `HeightUnconstrained` / `MaxWidthUnconstrained` / `MaxHeightUnconstrained` / `MaxUnconstrained` の静的メソッド7種(同名) | 同じ7種 + debug 用の `debugTransformType` |
| 文字方向の影響 | 受けない | `textDirection` で `alignment` の解決が変わる |
| debug 時のはみ出し表示 | 無し | debug ビルドでは、はみ出しに縞模様のインジケータが描かれる |

制約の変換、`ConstrainedAxis` の意味、`ClipBehavior` の既定が `Clip.None` である点は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.ConstraintsTransformBox -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.ConstraintsTransformBox
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [FloatSoda.Samples.LimitedBox](../FloatSoda.Samples.LimitedBox) — 取り除かれた制約に上限を戻す場合
