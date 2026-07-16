← [Home](Home.md)

# Getting Started

## 前提条件

- .NET 10 SDK
- SteamVR がインストール済みで、アプリ実行前に起動していること
- OpenVR ランタイム（SteamVR に同梱）

---

## サンプルアプリを動かす（最速）

リポジトリをクローンしたらまずサンプルアプリを起動してフレームワークの動作を確認できます。

```bash
# SteamVR を起動してから実行する
dotnet run --project samples/FloatSoda.Samples.OverlayApp
```

起動すると SteamVR ダッシュボードに、レイアウト、時計、アニメーション、カウンターのデモ用タブが追加されます。左コントローラー追従とワールド座標固定の時計オーバーレイも生成されます(`samples/FloatSoda.Samples.OverlayApp/Program.cs`)。

SteamVR を終了するか `VREvent_Quit` を受信するとアプリも自動終了します。

> サンプルには `StatefulWidget` を使った時計ウィジェット(`WatchWidget.cs`)が含まれており、`SetState()` による毎秒の再ビルドで時刻が更新されます。

---

## 新しいアプリを作る

### 1. プロジェクトを作成する

```bash
dotnet new console -n MyOverlayApp
cd MyOverlayApp
dotnet add reference ../path/to/FloatSoda/src/FloatSoda/FloatSoda.csproj
```

### 2. 最小構成のコードを書く

`Program.cs` を以下のように書き換えます。Widget ベースの書き方が推奨です。

```csharp
using FloatSoda;
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

Widget root = new Center
{
    Child = new ColoredBox
    {
        Color = SKColors.CornflowerBlue,
        Child = new SizedBox { Width = 400, Height = 200 }
    }
};

// オーバーレイのサイズは root ウィジェットのレイアウト結果に自動追従します。
app.CreateWindow(new DashboardWindow { Title = "HelloWorld", Child = root });

await host.RunAsync();
```

```bash
# SteamVR を起動してから実行
dotnet run
```

Window の作成は Host 側で行います。`host.RunAsync()` は SteamVR が終了するまで待機し、SteamVR の終了イベント、Ctrl+C、または Host の停止要求を受けると正常終了します。

> **Widget の実装状況:** `Center`, `Align`, `Column`, `Row`, `Flex`, `ColoredBox`, `SizedBox`, `ConstrainedBox`, `Clip*`, `RichText`, `Text` などは使用可能で、`StatefulWidget` / `InheritedWidget` も動作します。未実装の `Padding`, `Container`, `ListView` などは公開 API から除外されています。`Button` は `FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop` にスケルトン実装がありますが、ジェスチャ・ヒットテスト未実装のため押下操作はまだ動作しません。詳細は [WidgetSystem.md](WidgetSystem.md) を参照。

<details>
<summary>RenderObject レベルの直接操作（低レベル API）</summary>

```csharp
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

var root = new RenderPositionedBox
{
    Child = new RenderConstrainedBox
    {
        AdditionalConstraints = BoxConstraints.Tight(400, 200),
        Child = new RenderColoredBox { Color = SKColors.CornflowerBlue }
    }
};

// CreateWindow の Child は Widget を要求するため、RenderObjectWidget でラップします。
Widget widgetRoot = new RawRootWidget { Root = root };
app.CreateWindow(new DashboardWindow { Title = "LowLevel", Child = widgetRoot });
await host.RunAsync();

// 既存の RenderObject を Widget ツリーのルートへ接続する最小ラッパー。
public sealed record RawRootWidget : SingleChildRenderObjectWidget<RenderPositionedBox>
{
    public required RenderPositionedBox Root { get; init; }

    public override RenderPositionedBox CreateRenderObject() => Root;
}
```
</details>

---

## オーバーレイ種別の選び方

`app.CreateWindow(...)` に渡すウィンドウ定義 `WindowWidget` の種類でオーバーレイ種別を選びます。
`Size` を指定しない場合、オーバーレイのサイズは `Child` ウィジェットのレイアウト結果に追従します
（`Size` を指定するとそのサイズで固定されます）。

| ウィンドウ定義 | オーバーレイ種別 | 位置の管理 |
|---|---|---|
| `DashboardWindow { Title, Child, Size? }` | `DashboardOverlay` | SteamVR ダッシュボードが管理（ユーザーが開くタブ） |
| `WorldSpaceWindow { Title, Child, Size?, Position, Rotation }` | `WorldSpaceOverlay` | ワールド座標で固定（`Vector3 Position`、既定は前方1m・高さ1m） |
| `DeviceTrackedWindow { Title, Child, Size?, Target, Offset, Rotation }` | `DeviceTrackedOverlay` | トラッキングデバイスに追従（`TrackedDevice` 列挙体） |

`Title` は SteamVR 上の表示名（ダッシュボードタブ名など）です。OpenVR のオーバーレイキーは
「エントリアセンブリ名 + `Title` のスネークケース」から自動生成されます
（例: アセンブリ `MyOverlayApp` + `Title = "My Dashboard"` → `my_overlay_app.my_dashboard`）。

```csharp
// ダッシュボード
app.CreateWindow(new DashboardWindow { Title = "MyDashboard", Child = root });

// ワールド座標固定。Position 省略時はプレイエリア中央から前方1m・高さ1m (0, 1, -1)
app.CreateWindow(new WorldSpaceWindow { Title = "MyWorld", Child = root });

// 左コントローラーに追従
app.CreateWindow(new DeviceTrackedWindow { Title = "MyHand", Child = root, Target = TrackedDevice.LeftController });
```

---

## フレームレート設定

`FloatSodaOptions` でフレームレートを制御できます。

```csharp
// 固定 FPS
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda(new FloatSodaOptions
{
    TargetFrameRate = 90
});
```

`TargetFrameRate` を指定しない場合のデフォルトは 60fps です。オーバーレイアプリはシーンアプリではないため、`WaitGetPoses` によるフレーム同期は利用できません。

---

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — 使えるウィジェットの一覧と実装状況
- [OVRIntegration](OVRIntegration.md) — オーバーレイ種別・プロパティ・イベント処理の詳細
- [Architecture](Architecture.md) — フレームワーク内部の全体像
