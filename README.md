# FloatSoda

[![NuGet](https://img.shields.io/nuget/v/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![CI](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml/badge.svg)](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/sumx21t-3310/FloatSoda)](LICENSE)

FloatSoda is a UI framework for building SteamVR overlays in C# / .NET with a Flutter-like declarative API, and it is currently in alpha. The rest of this README and the [documentation site](https://floatsoda.sumx21t.com/) are written in Japanese; coding agents can read [llms.txt](https://floatsoda.sumx21t.com/llms.txt) instead.

SteamVR のオーバーレイを、Flutter のような宣言的 UI で作る .NET 10 / C# 14 向けのフレームワークです。

SteamVR のオーバーレイは、VR の中に浮かぶ 2D のパネルです。これまでは、パネルを1枚出すだけでも、Unity を丸ごと動かすか、C++ で OpenVR を直接呼ぶ必要がありました。Qt のようなデスクトップ向けの UI を、自分で OpenVR につなぐ方法もあります。

FloatSoda では、「状態がこうなら、画面はこう見える」を C# のコードで書きます。並べ方の計算、描画、SteamVR への受け渡し、状態が変わったときの描き直しは、FloatSoda が受け持ちます。UI はすべてコードなので、シーンもプレハブもありません。変更を diff で追えます。書き方が Flutter と同じ形なので、Flutter を知っている AI にもコードを書かせやすくなっています。AI に書かせるときは、ガイドの [AI に FloatSoda を書かせる](https://floatsoda.sumx21t.com/guide/3-prepare/write-with-llm/)を開いてください。

自分用のツールを作りたい VRChatter、Booth でオーバーレイを売りたいクリエイター、uGUI を避けたいエンジニアに向けて作っています。開発段階は Alpha で、API は予告なく変わります。

まず動かすなら、チュートリアルの[最初のオーバーレイを作る](docs/tutorials/first-overlay.md)へ進んでください。コーディングエージェントには [llms.txt](https://floatsoda.sumx21t.com/llms.txt) を渡します。紹介の全文は[ドキュメントサイト](https://floatsoda.sumx21t.com/)のトップにあります。

## 使い方

次のコードは、SteamVR のダッシュボードに 100×100 の青い四角を出す、最小のオーバーレイです。[`samples/Showcase/FloatSoda.Samples.GettingStarted`](samples/Showcase/FloatSoda.Samples.GettingStarted/Program.cs) の `Program.cs` と同じ中身です。

```csharp
using FloatSoda;
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

Widget root = new Align
{
    Alignment = Alignment.Center,
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox { Color = SKColors.CornflowerBlue },
    },
};

app.CreateWindow(
    new DashboardWindow
    {
        Dpm = new Dpm(100),
        Title = "MyDashboard",
        Child = root,
    }
);
await host.RunAsync();
```

リポジトリを取得していれば、SteamVR を起動してから次のコマンドで同じサンプルを動かせます。

```bash
dotnet run --project samples/Showcase/FloatSoda.Samples.GettingStarted
```

FloatSoda が裏で何をしているかは、ガイドの [FloatSoda の仕組み](docs/guide/4-build/mechanics/how-floatsoda-works.md)にあります。

## できること・できないこと

オーバーレイは、ダッシュボードに出す `DashboardWindow`、ワールド座標に固定する `WorldSpaceWindow`、デバイスに追従する `DeviceTrackedWindow` の3種類を、同時に出せます。並べる・描く・入力を受け取る部品は、ドキュメントサイトの[部品を目的から探す](https://floatsoda.sumx21t.com/reference/find-widgets/)で探せます。開発の予定は、GitHub の[マイルストーン](https://github.com/sumx21t-3310/FloatSoda/milestones)で確かめられます。

使う前に確かめたい主な制約は、次の3つです。

- ポインタの座標が届くのは `DashboardWindow` だけで、ほかの2種類に置いた `GestureDetector` のコールバックは呼ばれない
- `Button` のような UI コンポーネントは NuGet に出しておらず、ボタンは `GestureDetector` で組み立てる
- スクロールする部品(`ListView`、`GridView`、`SingleChildScrollView`)は、まだ使えない

制約の全部と回避の方法は、リファレンスの[できないこと・まだ使えないもの](docs/reference/limitations.md)にまとめています。

## インストール

開発する PC には、次のものが要ります。

| 項目 | 必要なもの |
| :-- | :-- |
| .NET の SDK | 10 |
| SteamVR | 起動できること |
| HMD | SteamVR につないであること |
| OS | Windows(x64) |

用意ができたら、コンソールアプリを作り、NuGet から FloatSoda を足します。

```powershell
dotnet new console -n MyOverlayApp
cd MyOverlayApp
dotnet add package FloatSoda
```

作ったアプリを動かす PC に要るものと、つまずいたときの対処は、リファレンスの[動作環境](docs/reference/requirements.md)とガイドの[開発環境を用意する](docs/guide/3-prepare/set-up-environment.md)にあります。

## ドキュメント

ドキュメントは [floatsoda.sumx21t.com](https://floatsoda.sumx21t.com/) で公開しています。ソースは `docs/` の Markdown です。サイトは次の4つのタブに分かれています。

| タブ | 開くとき |
| :-- | :-- |
| [チュートリアル](docs/tutorials/Home.md) | 手を動かして、最初のオーバーレイを作りたい |
| [ガイド](docs/guide/Home.md) | 目的があり、その手順と仕組みを知りたい |
| [リファレンス](docs/reference/Home.md) | 作例、制約、動作環境、用語、部品を引きたい |
| [コントリビューター](docs/contributor/Home.md) | FloatSoda の開発に加わりたい |

コーディングエージェントには、索引の [llms.txt](https://floatsoda.sumx21t.com/llms.txt) か、全文の [llms-full.txt](https://floatsoda.sumx21t.com/llms-full.txt) を渡します。

## コントリビュート

バグ報告と機能要望は、テンプレートを使って [Issue](https://github.com/sumx21t-3310/FloatSoda/issues) に立ててください。開発の規約は、リポジトリのルートにあります。

| ファイル | 中身 |
| :-- | :-- |
| [CONTRIBUTING.md](CONTRIBUTING.md) | ブランチの名前、namespace、テストの観点、PR の進め方 |
| [API_DESIGN.md](API_DESIGN.md) | API 設計の原則と、Flutter との対応 |
| [REVIEW.md](REVIEW.md) | コードレビューの判断基準 |
| [AGENTS.md](AGENTS.md) | コーディングエージェント向けの入口 |

## ライセンス

FloatSoda のライセンスは [MIT](LICENSE) です。無料で使え、変更、再配布、販売もできます。条件は、配布物に著作権表示と許諾表示を含めることです。FloatSoda は無保証で提供されています。

FloatSoda を使ったアプリを配るときは、次の2つのファイルを配布物に入れます。

- [`LICENSE`](LICENSE): FloatSoda の著作権表示と許諾表示
- [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md): Flutter、OpenVR SDK、OVRSharp のライセンスの全文

SkiaSharp のように、配布物に入るほかのライブラリのライセンスは、それぞれの配布元で確かめてください。上の2つのファイルだけで足りるとは限りません。配るときの手順は、ドキュメントサイトの [Booth で売る準備](https://floatsoda.sumx21t.com/guide/6-distribute/booth-release/)にあります。

この節はライセンスの要約で、法的な判断ではありません。条件の正しい文面は、`LICENSE` の全文です。
