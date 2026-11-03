← [ガイド](../../Home.md)

# HMD を被らずに確かめる

## どちらの方法を使うか

見た目を確かめるために HMD を被る回数は、デスクトップの窓に出す方法か、画像にして見る方法で減らせます。窓に出す方法は、画面をモニターで見ながらマウスで操作でき、マウスの操作に対する動きも確かめられます。画像にする方法は、ウィジェットを PNG ファイルに書き出すので、SteamVR も HMD も要りません。ただし、書き出すのは描画を1回実行した時点の見た目だけで、動きや操作は確かめられません。

| 方法 | できること | SteamVR と HMD |
| :-- | :-- | :-- |
| 方法1: デスクトップの窓に出す | 画面をモニターで見る。マウスで押す | 起動と接続が要る |
| 方法2: 画像にして比べる | ウィジェットのある時点の見た目を PNG にして、保存した画像を開いて見る。動きと操作は確かめられない | 要らない |

どちらの方法も、HMD の中で動くことの確認の代わりにはなりません。最後は HMD で確かめます。

## 前提条件

- 方法1: `DashboardWindow` で出している起動のコードがあること
- 方法1: SteamVR を起動し、HMD を接続していること
- 方法2: テストしたいウィジェットがあること
- 方法2: プロジェクトに `FloatSoda.Testing` パッケージを足していること

<!-- ※要確認: `FloatSoda.Testing` の取得元(NuGet での公開の有無)と導入コマンド。src/FloatSoda.Testing/FloatSoda.Testing.csproj に PackageId はあるが、NuGet で公開されているかは確かめていない -->

方法1でも HMD の接続が要る理由は、FloatSoda が表示先によらず、起動時に OpenVR を初期化するためです。HMD が未接続だと `Init_HmdNotFound` で起動に失敗します(追跡: #140)。方法1で省けるのは、HMD を頭に付ける手間だけです。

<!-- ※要確認: HMD を接続していない PC で `--desktop` が動かない状況の回避策が今あるか。FloatSodaApp.Initialize() を読み直して確かめる -->

## 方法1: デスクトップの窓に出す

起動のコードで `DashboardWindow` を渡している箇所を、`DesktopWindow` に替えます。`DesktopWindow` は、開発機のデスクトップ上に見える窓として画面を出します。

```csharp
var useDesktop = args.Contains("--desktop");
var hostArgs = args.Where(argument => argument != "--desktop").ToArray();

var builder = Host.CreateApplicationBuilder(hostArgs);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

app.CreateWindow(useDesktop
    ? new DesktopWindow { Title = "MyOverlay", Child = root }
    : new DashboardWindow { Dpm = new Dpm(100), Title = "MyOverlay", Child = root });

await host.RunAsync();
```

`Dpm` は `FloatSoda.Abstractions.Geometries` の名前空間にあります。`--desktop` を付けて起動すると窓に出て、付けないと従来どおりダッシュボードに出ます。`--desktop` はこのアプリで処理する引数なので、`Host.CreateApplicationBuilder` に渡す引数から取り除きます。このコードは、[GestureDetector のサンプル](../../../../samples/Widgets/FloatSoda.Samples.GestureDetector/Program.cs)と同じ形です。

```powershell
dotnet run -- --desktop
```

**できた?** デスクトップに窓が出て、マウスで操作できれば、方法1は完了です。

<!-- ※要確認: `DesktopWindow` の窓のサイズが `Size` の指定どおりになるか。実行して確かめる -->

<details>
<summary>できなかったら</summary>

窓が出ない、または起動時に例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。HMD を接続していない場合は、前提条件のとおり `Init_HmdNotFound` で失敗します。

</details>

デスクトップで動いても、ダッシュボードで動く証明にはなりません。ウィンドウの種類ごとに、ポインターの座標が届く仕組みが違うためです(追跡: #182)。ボタンを押せる画面は、最後に HMD のダッシュボードでも確かめます。

## 方法2: 画像にして比べる

`WidgetBitmapRenderer.Render` にウィジェットと画像のサイズを渡すと、ビルド、レイアウト、ペイントを1回実行して、結果を `SKBitmap` で返します。この方法は SteamVR を使いません。

```csharp
using FloatSoda.Testing;
using SkiaSharp;

var renderer = new WidgetBitmapRenderer();

using var bitmap = renderer.Render(root, new SKSizeI(400, 300));
using var image = SKImage.FromBitmap(bitmap);
using var data = image.Encode(SKEncodedImageFormat.Png, 100);

File.WriteAllBytes("widget.png", data.ToArray());
```

`root` には、HMD の中に出したい画面のウィジェットを渡します。`Render` は1回しか実行しないので、`Image` ウィジェットのように、画像を非同期で読み込む部品は、読み込み前の状態で描かれます。画像まで描きたいときは、`Image` に渡すものと同じプロバイダーを変数に持ち、`Render` の前に `await provider.ResolveAsync()` を実行します。返されたハンドルは、描画が終わるまで手放さないでください。次の例は、`using var` で、スコープの終わりまでハンドルを保持します。

```csharp
using FloatSoda.Core.Providers;

var provider = new FileImageProvider("photo.png");
Widget imageRoot = new Image { Provider = provider };

using var handle = await provider.ResolveAsync();
using var bitmap = renderer.Render(imageRoot, new SKSizeI(400, 300));
```

`FloatSoda.Samples.PaintingSample` も、ウィジェットのツリーを PNG に書き出します。コードは[PaintingSample の Program.cs](../../../../samples/Showcase/FloatSoda.Samples.PaintingSample/Program.cs)にあります。

<!-- ※要確認: `FloatSoda.Testing` が NuGet で配布されているか(csproj に PackageId はある。公開の有無は NuGet で確かめる) -->

画像を期待する画像と自動で比べる方法は、この手順では扱いません。書き出した PNG を開いて、目で見て確かめます。

**できた?** `widget.png` が保存され、開くと、400 × 300 ピクセルの画像の中で、配置、色、文字が意図どおりになっていれば、方法2は完了です。HMD の中での実寸と操作は、HMD で別に確かめます。

<details>
<summary>できなかったら</summary>

画像の中の配置や大きさが思ったとおりでないときは、[レイアウトの考え方](../../5-troubleshoot/layout-model.md)で、大きさの決まり方を確かめます。例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)に進みます。

</details>

## 結果を確かめる

窓または画像で、次の3点を見ます。

- 配置: 部品が思った場所に、思った大きさで並んでいる
- 色: 背景と文字の色が指定どおりに出ている
- 文字: 文字が切れずに、読める大きさで出ている

HMD の中で最後に確かめる項目は、押せるかどうかと、実寸の大きさです。見た目の指定は[見た目を整える](../screen/style-appearance.md)、押せる部品は[ボタンやトグルを組み立てる](../screen/build-buttons-toggles.md)で扱います。

## うまくいかないとき

窓が出ない、または起動で例外が出るときは、[エラーを読んで直す](../../5-troubleshoot/read-errors.md)で原因を切り分けます。
