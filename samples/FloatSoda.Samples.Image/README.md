# Image

## これは何か

`Image` は、`ImageProvider` から読み込んだ画像を自身の領域へ描画するウィジェットです。Flutter の `Image` に対応します。

読み込みは非同期に行われます。**読み込み完了までは `LoadingBuilder`、失敗時は `ErrorBuilder` の返すウィジェットが代わりに表示されます。** どちらも指定しなければ何も表示されません。読み込みに失敗してもアプリケーションは停止しません。

等しい `Provider`(同じパスの `FileImageProvider` など)を使う `Image` 同士は、読み込んだ画像を共有します。

## 使い方

### 画像を表示する

`Provider` に画像の読み込み方法を渡します。ローカルファイルなら `FileImageProvider` です。

```csharp
using ImageWidget = FloatSoda.Widgets.Paint.Image;

var provider = new FileImageProvider(ImagePath);
```

このサンプルは名前空間が `FloatSoda.Samples.Image` で型名 `Image` と衝突するため、`ImageWidget` というエイリアスを使っています。名前空間が衝突しないアプリでは `new Image { ... }` と書けます。

あとは `Provider` を渡すだけです。これが最小の形で、`ImageDemo.BuildFitted` では `FittedBox` の子として使っています。

```csharp
new ImageWidget { Provider = provider }
```

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

### 読み込み中と失敗時の表示を決める

`LoadingBuilder` は読み込み完了まで、`ErrorBuilder` は読み込み失敗時に、画像の代わりに表示するウィジェットを返します。

```csharp
new ImageWidget
{
    Provider = provider,
    LoadingBuilder = (context, progress) => new Center { Child = Label("LOADING", 20, new Color(124, 205, 255), 700) },
    ErrorBuilder = (context, exception) => new Center { Child = Label("LOAD FAILED", 20, new Color(255, 111, 97), 700) }
}
```

`LoadingBuilder` の第2引数 `progress` は、読み込みの進み具合(`ImageLoadingProgress`)です。`FileImageProvider` は進み具合を報告しないため、常に `null` が渡されます。

### 画像の上に子を重ねる

`Image` は子を持ちません。画像の上へウィジェットを重ねるときは `Stack` を使います。

```csharp
new Stack
{
    Fit = StackFit.Expand,
    Children =
    {
        new ImageWidget { Provider = provider, Fit = BoxFit.Fill },
        new Center
        {
            Child = Label("CHILD ON TOP", 20, new Color(255, 111, 97), 700)
        }
    }
}
```

## Flutterとの違い

| 項目 | FloatSoda | Flutter |
|---|---|---|
| `Fit` の既定値 | `BoxFit.Contain`。領域より小さい画像も拡大される | `fit` は null 許容で、`paintImage` が `BoxFit.scaleDown` として解決する。領域より小さい画像は拡大されない |
| 読み込み元の指定 | `Provider` プロパティ1本 | `Image.asset` / `Image.network` / `Image.file` などの名前付きコンストラクタ |
| 読み込み中の表示 | `LoadingBuilder` は読み込み中だけ呼ばれ、画像の代わりに表示するウィジェットを返す | `loadingBuilder` は読み込み完了後も呼ばれ、完成した画像を `child` として受け取って包む |
| 画像の共有 | 借りている `Image` がある間だけ共有し、最後の1つがツリーから外れると解放する | `ImageCache` が LRU で保持し、表示が終わった画像も上限まで残す |

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

- [docs/WidgetSystem.md](../../docs/WidgetSystem.md) — 組み込みウィジェット一覧
- [docs/RenderObjects.md](../../docs/RenderObjects.md) — `RenderImage` の契約
