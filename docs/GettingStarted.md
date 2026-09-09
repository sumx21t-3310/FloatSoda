← [Home](Home.md)

# Getting Started

## 前提条件

- .NET 10 SDK
- SteamVR がインストール済みで、アプリ実行前に起動していること
- OpenVR ランタイム（SteamVR に同梱）

## サンプルアプリを動かす（最速）

リポジトリをクローンしたらまずサンプルアプリを起動してフレームワークの動作を確認できます。

```bash
# SteamVR を起動してから実行する
dotnet run --project samples/FloatSoda.Samples.OverlayApp
```

起動すると SteamVR ダッシュボードに、レイアウト、時計、アニメーション、カウンターのデモ用タブが追加されます。左コントローラー追従とワールド座標固定の時計オーバーレイも生成されます(`samples/FloatSoda.Samples.OverlayApp/Program.cs`)。

SteamVR を終了するか `VREvent_Quit` を受信するとアプリも自動終了します。

> サンプルには `StatefulWidget` を使った時計ウィジェット(`WatchWidget.cs`)が含まれており、`SetState()` による毎秒の再ビルドで時刻が更新されます。

### 用途別のサンプル一覧

`samples/` には目的の異なる12のプロジェクトがあります。**総合デモと低レベル API のサンプル**、および**ウィジェット1つ(または1グループ)の使い方を示すカタログ型サンプル**に分かれます。

| プロジェクト | 内容 | SteamVR |
|---|---|---|
| `FloatSoda.Samples.OverlayApp` | レイアウト・時計・アニメーション・カウンター・ドラッグの総合デモ。3種のオーバーレイを同時に生成する | 必要 |
| `FloatSoda.Samples.GettingStarted` | 下の「最小構成のコード」とほぼ同じ最小アプリ | 必要 |
| `FloatSoda.Samples.PointerRegion` | ホバー・押下・取り消しの状態を画面に出す入力デモ。`PointerRegion` と `Listener` の挙動を目で確かめられる | 必要 |
| `FloatSoda.Samples.PrimitiveOverlay` | ウィジェット層を使わず、`FloatSoda.OVR` の低レベル API だけでオーバーレイを出す | 必要 |
| `FloatSoda.Samples.PaintingSample` | Widget / RenderObject / Layer の各ツリーを PNG へ書き出す | **不要** |

### ウィジェットカタログ

ウィジェットごとの使い方を示すサンプルです。各ディレクトリの `README.md` がそのままチュートリアルで、`checklist.md` に目視確認の手順があります。`--desktop` を付けるとデスクトップウィンドウへ表示できます。

| プロジェクト | 扱うウィジェット |
|---|---|
| `FloatSoda.Samples.Text` | `Text` / `RichText` / `TextSpan` / `TextStyle` |
| `FloatSoda.Samples.ColoredBox` | `ColoredBox` |
| `FloatSoda.Samples.Align` | `Align` / `Center` |
| `FloatSoda.Samples.SizedBox` | `SizedBox` |
| `FloatSoda.Samples.Flex` | `Flex` / `Row` / `Column` |
| `FloatSoda.Samples.Clip` | `ClipRect` / `ClipRoundRect` / `ClipOval` / `ClipCustomPath` |
| `FloatSoda.Samples.Image` | `Image` / `FileImageProvider` / `BoxFit` |

いずれも SteamVR の起動が必要です(`--desktop` でも OpenVR を初期化するため)。

**`PaintingSample` だけは SteamVR も HMD もいりません。** `FloatSoda.Testing` のヘッドレスレンダラーで
ツリーを画像化し、デスクトップへ `widget_tree_output.png` などを保存します。
レイアウトの結果だけを確かめたいときは、HMD をかぶらずにこれで見られます。

```bash
dotnet run --project samples/FloatSoda.Samples.PaintingSample
```

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

> **Widget の実装状況:** レイアウト系(`Center`, `Align`, `Row`, `Column`, `Padding`, `Container`, `Stack`, `Wrap`, `Expanded`, `AspectRatio` など)、描画系(`ColoredBox`, `DecoratedBox`, `Opacity`, `Transform`, `Clip*`)、入力系(`GestureDetector`, `Listener`)は使用可能で、`StatefulWidget` / `InheritedWidget` も動作します。
> `internal` のため公開 API から使えないのは、スクロール系の `ListView` / `GridView` / `SingleChildScrollView` です。画像とアイコンには描画系の `Paint.Image` / `Paint.Icon` を使用できます。
> **`Button` などの UI コンポーネントはまだ提供していません。** 用意する予定の3層構成(`FloatSoda.UI` / `Cream` / `FizzyPop`)は Phase 5 で、現時点では NuGet 未配布・押下も未反応です(→ [UILayering](UILayering.md#実装状況))。ボタンは `GestureDetector` で組み立ててください(→ [WidgetSystem.md](WidgetSystem.md#押せるボタンを作る))。

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

## オーバーレイ種別の選び方

`app.CreateWindow(...)` に渡すウィンドウ定義 `WindowWidget` の種類でオーバーレイ種別を選びます。
`Size` を指定しない場合、オーバーレイのサイズは `Child` ウィジェットのレイアウト結果に追従します
（`Size` を指定するとそのサイズで固定されます）。

| ウィンドウ定義 | オーバーレイ種別 | 位置の管理 | ポインタ入力 |
|---|---|---|---|
| `DashboardWindow { Title, Child, Size? }` | `DashboardOverlay` | SteamVR ダッシュボードが管理（ユーザーが開くタブ） | ✓ 届く |
| `WorldSpaceWindow { Title, Child, Size?, Position, Rotation }` | `WorldSpaceOverlay` | ワールド座標で固定（`Vector3 Position`、既定は前方1m・高さ1m） | ✗ 届かない |
| `DeviceTrackedWindow { Title, Child, Size?, Target, Offset, Rotation }` | `DeviceTrackedOverlay` | トラッキングデバイスに追従（`TrackedDevice` 列挙体） | ✗ 届かない |

**ポインタ入力が届くのはダッシュボードオーバーレイだけです。** SteamVR はダッシュボード上のレーザーポインターを
マウスイベントとして送ってくるため、FloatSoda はそれをそのままヒットテストへ流せます。
ワールド座標固定とデバイス追従のオーバーレイには、コントローラーレイからポインタ座標を作る経路がまだありません。
これらのウィンドウに `GestureDetector` を置いてもコンパイルは通り、例外も出ませんが、コールバックは呼ばれません。

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

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — 使えるウィジェットの一覧と実装状況
- [OVRIntegration](OVRIntegration.md) — オーバーレイ種別・プロパティ・イベント処理の詳細
- [Architecture](Architecture.md) — フレームワーク内部の全体像
