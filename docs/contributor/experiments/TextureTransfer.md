# SteamVR へのテクスチャ転送はどうなっているの?

フレームごとに位置と色が変わる絵を SteamVR のダッシュボードに出し、目標フレームレートを下げます。下げるとオーバーレイがカクつき、戻すとなめらかになります。FloatSoda が SteamVR へ絵を渡すのは、1フレームに多くても1回だからです。この実験には、SteamVR と HMD が要ります。

> **実機で未確認** — このページの「見えるもの」と「壊して直す」に書いた見え方は、コードから予想したもので、実機ではまだ確かめていません(※要確認)。コードがビルドできることだけを確かめています。

## 試すこと

.NET 10 SDK と、FloatSoda のリポジトリの clone を用意し、SteamVR を起動して HMD をつなぎます。次のコードを、リポジトリのルートに `TextureTransfer.cs` という名前で保存します。

```csharp
#:project src/FloatSoda
using FloatSoda;
using FloatSoda.Animation;
using FloatSoda.Elements;
using FloatSoda.OVR;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

// 目標フレームレート。既定は 60。--fps 5 のように渡して下げる
var fps = args.Length >= 2 && args[0] == "--fps" ? int.Parse(args[1]) : 60;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddFloatSoda(new FloatSodaOptions { AppKey = new AppKey("FloatSoda.Experiments.TextureTransfer"), TargetFrameRate = fps });
using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();
app.CreateWindow(new DashboardWindow { Title = $"TextureTransfer {fps}fps", Size = new SKSize(400, 100), Child = new Slider() });
await host.RunAsync();

// 四角が2秒で左端から右端まで動き、フレームごとに色相が少しずつ変わる
record Slider : StatefulWidget<Slider>
{
    public override State<Slider> CreateState() => new SliderState();
}

class SliderState : TickerProviderState<Slider>
{
    TimeSpan _elapsed;
    int _frame;

    public override void InitState() => CreateTicker(elapsed => SetState(() => { _elapsed = elapsed; _frame++; })).Start();

    public override Widget Build(IBuildContext context) => new Stack
    {
        Children =
        [
            new Positioned
            {
                Left = 300 * (_elapsed.TotalSeconds % 2 / 2), Top = 0, Width = 100, Height = 100,
                Child = new ColoredBox { Color = SKColor.FromHsv(_frame * 6 % 360, 80, 100) },
            },
        ],
    };
}
```

リポジトリのルートで実行します。

```bash
dotnet run TextureTransfer.cs
```

HMD をかぶってダッシュボードを開き、`TextureTransfer 60fps` のオーバーレイを選びます。確かめたら、ターミナルで Ctrl+C を押して止めます。終わったら、`TextureTransfer.cs` を消します。

## 見えるもの

※要確認(実機で未確認): 横長のオーバーレイの中を、四角が2秒かけて左端から右端へ動き、左端へ戻ります。四角の色は、フレームごとに色相が少しずつ変わります。目標フレームレートが 60 のときは、動きも色の変わり方もなめらかに見えます。

確かめる点は次のとおりです。

- オーバーレイのタイトルが `TextureTransfer 60fps` であること
- 四角が途切れずに動いて見えること
- 四角の色が、段差なく変わって見えること

## 壊して直す

いったん止めて、目標フレームレートを 5 に下げて実行します。

```bash
dotnet run TextureTransfer.cs -- --fps 5
```

※要確認(実機で未確認): 四角が1秒に5回だけ飛び飛びに動き、色も段ごとに変わります。HMD 側の表示はなめらかなままで、オーバーレイの中身だけがカクつきます。`--fps 5` を外して実行し直すと、なめらかな動きに戻ります。

確かめる点は次のとおりです。

- オーバーレイのタイトルが `TextureTransfer 5fps` であること
- 四角の動きが飛び飛びになること
- 頭を動かしたときの周りの景色は、なめらかなままであること

## なぜそうなるか

FloatSoda は、目標フレームレートに合わせて回るフレームごとに、変化があれば絵を作り直して SteamVR へ渡します。メインスレッドは、Widget の作り直し・レイアウト・描画をして Layer ツリーを作り、レンダースレッドへ渡します。レンダースレッドは、Layer ツリーを OpenGL のテクスチャへ描き、そのテクスチャを `SetOverlayTexture` で SteamVR へ渡します(`OverlayWindow.Present`)。メインスレッドとレンダースレッドの両方が、`FloatSodaOptions.TargetFrameRate` から作った `FramePacer` で、次のフレームまで待ちます。

目標フレームレートを 5 にすると、SteamVR へ渡すテクスチャも1秒に5枚になります。SteamVR は、次のテクスチャが届くまで前のテクスチャを表示し続けるので、オーバーレイの中身は5回しか変わりません(※要確認)。テクスチャ転送と `FramePacer` の本体は、[プラットフォーム: 仕組みと理由](../platform/Explanation.md) のテクスチャ転送と FramePacer の節で説明します。

## 関連

- [プラットフォーム: 仕組みと理由](../platform/Explanation.md) — テクスチャ転送と `FramePacer`、レンダースレッド
- [なぜ Layer ツリーは Clone してからレンダースレッドへ渡すの?](LayerClone.md) — メインスレッドからレンダースレッドへの受け渡し
- [実験の一覧](Home.md)
