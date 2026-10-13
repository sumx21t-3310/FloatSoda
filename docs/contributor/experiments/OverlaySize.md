# オーバーレイの大きさは何で決まるの?

同じ 340px 四方の絵を、`Dpm` を変えて VR 空間に出します。`Dpm` が 1000 なら幅 34cm、4000 なら手のひらほどの 8.5cm になります。オーバーレイの幅は、絵のピクセル数を `Dpm`(1m あたりのピクセル数)で割って決まるからです。この実験には、SteamVR と HMD が要ります。

> **実機で未確認** — このページの「見えるもの」と「壊して直す」に書いた見え方は、コードから予想したもので、実機ではまだ確かめていません(※要確認)。コードがビルドできることと、コンソールに出る幅の計算だけを確かめています。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意し、SteamVR を起動して HMD をつなぎます。次のコードを、リポジトリのルートに `OverlaySize.cs` という名前で保存します。

```csharp
#:project src/FloatSoda
using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.OVR;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

// 1m あたりのピクセル数。--dpm 4000 のように渡して変える
var dpm = args.Length >= 2 && args[0] == "--dpm" ? float.Parse(args[1]) : 1000f;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddFloatSoda(new FloatSodaOptions { AppKey = new AppKey("FloatSoda.Experiments.OverlaySize") });
using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

// 340px 四方の絵を、Position の既定値 (0, 1, -1) に出す
app.CreateWindow(new WorldSpaceWindow
{
    Title = $"OverlaySize dpm={dpm}",
    Dpm = new Dpm(dpm),
    Size = new SKSize(340, 340),
    Child = new ColoredBox { Color = SKColors.MediumSeaGreen },
});
Console.WriteLine($"幅 = 340px ÷ {dpm} = {new Dpm(dpm).ToMeters(340) * 100} cm");
await host.RunAsync();
```

リポジトリのルートで実行します。

```bash
dotnet run OverlaySize.cs
```

`WorldSpaceWindow` は、VR 空間の決まった位置に置くオーバーレイです。位置は `Position` の既定値 `(0, 1, -1)`(単位は m)を使います。この座標が HMD から見てどこになるかは、SteamVR のルームセットアップで決まる原点によります(※要確認)。確かめたら、ターミナルで Ctrl+C を押して止めます。終わったら、`OverlaySize.cs` を消します。

## 見えるもの

コンソールに、オーバーレイの幅の計算が出ます。

```text
幅 = 340px ÷ 1000 = 34 cm
```

※要確認(実機で未確認): 緑の正方形が、幅 34cm で浮かんで見えます。

確かめる点は次のとおりです。

- 正方形の幅が、およそ 34cm に見えること。コントローラーを横に当てると比べやすい
- 正方形の高さが、幅と同じに見えること

## 壊して直す

いったん止めて、`Dpm` を 4000 にして実行します。4000 は `Dpm.Default`(`Dpm` を指定しないときの値)と同じです。

```bash
dotnet run OverlaySize.cs -- --dpm 4000
```

```text
幅 = 340px ÷ 4000 = 8.5 cm
```

※要確認(実機で未確認): 同じ 340px の絵が、手のひらほどの 8.5cm になります。`--dpm 4000` を外して実行し直すと、34cm に戻ります。

確かめる点は次のとおりです。

- 正方形の幅が、およそ 8.5cm に見えること
- 正方形の色と形は、34cm のときと同じであること

## なぜそうなるか

SteamVR のオーバーレイは、ピクセル数ではなく、VR 空間での幅(m)で大きさが決まります。FloatSoda は、ウィンドウの大きさが決まるたびに、幅のピクセル数を `Dpm` で割った値を、オーバーレイの幅として SteamVR へ渡します(`OverlayWindow.Resize` と `Dpm.ToMeters`)。高さは渡しません。高さは、テクスチャの縦横の比から決まります(※要確認)。

そのため、同じピクセル数の絵でも、`Dpm` を4倍にすると幅は4分の1になります。オーバーレイの種類と大きさの本体は、[プラットフォームの仕組みと理由](../explanation/Platform.md) のオーバーレイの種類の節で説明します。利用者から見た大きさの決め方は、[Window](../../concepts/Window.md) にあります。

## 関連

- [プラットフォームの仕組みと理由](../explanation/Platform.md) — オーバーレイの種類と、幅の渡し方
- [Window](../../concepts/Window.md) — 利用者から見たオーバーレイの種類と大きさ
- [実験の一覧](Home.md)
