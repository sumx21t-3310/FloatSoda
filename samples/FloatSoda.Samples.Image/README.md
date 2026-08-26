# Image

## これは何か

`Image` は、`ImageProvider` から読み込んだ画像を自身の領域へ描画するウィジェットです。Flutter の `Image` に対応します。

読み込みは非同期に行われます。**完了するまでの間と、読み込みに失敗した場合は `Child` だけが描画されます。** 読み込み失敗はアプリケーションを停止させず、`OnError` で通知されます。

## 使い方

### 画像を表示する

`Provider` に画像の読み込み方法を渡します。ローカルファイルなら `FileImageProvider` です。

```csharp
using ImageWidget = FloatSoda.Widgets.Paint.Image;

var provider = new FileImageProvider(ImagePath);

Widget image = new ImageWidget { Provider = provider };
```

このサンプルは名前空間が `FloatSoda.Samples.Image` で型名 `Image` と衝突するため、`ImageWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Image { ... }` と書けます。

### 領域への収め方を指定する

`Fit` は、画像を自身の領域へどう収めるかを決めます。既定は `BoxFit.Contain` で、縦横比を維持したまま領域内へ収めます。

`BoxFit.Fill` を指定すると縦横比を無視して領域いっぱいへ引き伸ばします。

```csharp
new ImageWidget
{
    Provider = provider,
    // 既定のBoxFit.Containでは縦横比が維持され、CONTAINカードと見分けがつかない。
    // このカードは「領域いっぱいへ引き伸ばす」比較対象なのでFillを明示する。
    Fit = BoxFit.Fill,
}
```

`BoxFit.Cover` のように画像の一部だけを使う値でも、`Image` は描画元の矩形を切り取るため**領域外へはみ出しません**。領域そのものを切り抜きたい場合は `FittedBox` と `ClipBehavior` を組み合わせます。

```csharp
new ColoredBox
{
    Color = new Color(40, 47, 64),
    Child = new FittedBox
    {
        Fit = fit,
        ClipBehavior = Clip.HardEdge,
        Child = new ImageWidget { Provider = provider }
    }
};
```

### 画像の上に子を重ねる

`Child` に渡したウィジェットは画像の上に描画されます。

```csharp
new ImageWidget
{
    Provider = provider,
    Fit = BoxFit.Fill,
    Child = new Center
    {
        Child = Label("CHILD ON TOP", 20, new Color(255, 111, 97), 700)
    }
}
```

`Child` は読み込み中と読み込み失敗時にも描画されるため、プレースホルダーとしても機能します。

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Fit` の既定値 | `BoxFit.Contain`。領域より小さい画像も拡大される | `fit` は null 許容で、`paintImage` が `BoxFit.scaleDown` として解決する。領域より小さい画像は拡大されない |
| 画像の上への重ね描き | `Child` を持つ | `Image` は子を持たない。`Stack` で重ねる |
| 読み込み元の指定 | `Provider` プロパティ1本 | `Image.asset` / `Image.network` / `Image.file` などの名前付きコンストラクタ |
| 読み込み失敗時 | `Child` を描画し、`OnError` で通知する | `errorBuilder` で代替ウィジェットへ差し替えられる |

## 実行

**HMD を PC へ接続し、SteamVR を起動してから**、リポジトリルートで実行します。
FloatSoda は表示先によらず起動時に OpenVR を初期化するため、デスクトップ表示でも HMD の接続が必要です。

デスクトップウィンドウへ表示する(HMD を被る必要はない):

```powershell
dotnet run --project samples/FloatSoda.Samples.Image -- --desktop
```

SteamVR ダッシュボードのタブとして表示する:

```powershell
dotnet run --project samples/FloatSoda.Samples.Image
```

## 関連

- 動作確認の手順: [checklist.md](checklist.md)
- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [docs/RenderObjects.md](../../docs/RenderObjects.md) — `RenderImage` の契約
