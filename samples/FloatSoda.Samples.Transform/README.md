# Transform

## これは何か

`Transform` は、レイアウト後の子の**描画**へ2次元アフィン変換(回転・拡大縮小・平行移動など)を適用するウィジェットです。Flutter の `Transform` に対応します。

変換はレイアウトへ影響しません。子が占有する領域は変換前のままで、描画だけが動きます。兄弟の位置は変わらず、変換後の描画が領域からはみ出してもクリップされません。

## 使い方

### 行列を渡す

`Matrix` に `System.Numerics.Matrix3x2` を渡します。Flutter の `Transform.rotate` / `scale` / `translate` に相当するものは、`Matrix3x2` の静的メソッド(`CreateRotation` / `CreateScale` / `CreateTranslation`)で行列を作って渡します。

```csharp
using TransformWidget = FloatSoda.Widgets.Paint.Transform;

new TransformWidget
{
    Matrix = Matrix3x2.CreateRotation((float)(15 * Math.PI / 180)),
    Child = Marker(new Color(124, 205, 255))
}
```

このサンプルは名前空間が `FloatSoda.Samples.Transform` で型名 `Transform` と衝突するため、`TransformWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Transform { ... }` と書けます。

### 変換原点を決める

既定の原点は子の**左上**です。上の回転では四角が左上の角を軸に振れます。`Alignment` で原点を移せます。`Alignment.Center` ならその場で回ります。

```csharp
new TransformWidget
{
    Matrix = Matrix3x2.CreateRotation((float)(15 * Math.PI / 180)),
    Alignment = Alignment.Center,
    Child = Marker(new Color(255, 111, 97))
}
```

原点を座標で微調整したい場合は `Origin` にローカル座標のオフセットを渡します。

### レイアウトは動かない

平行移動しても、子のレイアウト領域は元の位置のままです。ヒットテストは既定(`TransformHitTests = true`)でポインタ座標を逆変換して子と照合しますが、**判定が行われるのは変換前のレイアウト領域の内側だけ**です。ポインタが当たるのは「変換後の描画」と「レイアウト領域」が重なっている部分で、レイアウト領域の外へ描かれた部分は見えていても当たりません。`GestureDetector` などを重ねる場合は、この範囲を前提にしてください。

```csharp
new TransformWidget
{
    Matrix = Matrix3x2.CreateTranslation(40, 28),
    Child = Marker(new Color(124, 205, 255))
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| 行列の型とプロパティ名 | `Matrix`(`System.Numerics.Matrix3x2`、2次元アフィンのみ) | `transform`(`Matrix4`。遠近を含む3次元変換も表せる) |
| 回転・拡大縮小の指定 | `Matrix3x2` の静的メソッドで行列を作る | `Transform.rotate` / `scale` / `translate` / `flip` の名前付きコンストラクタがある |
| 描画品質 | 指定なし | `filterQuality` で拡大縮小時の補間を指定できる |

変換がレイアウトへ影響しない点、原点の決まり方(`Origin` + `Alignment`、既定は左上)、`TransformHitTests` の意味は Flutter と同等です。

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Transform -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Transform
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [docs/Animation.md](../../docs/Animation.md) — 変換をアニメーションさせる場合
- [FloatSoda.Samples.Opacity](../FloatSoda.Samples.Opacity) — 同じく描画だけに効く不透明度の合成
