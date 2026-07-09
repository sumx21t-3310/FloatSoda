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

起動すると SteamVR ダッシュボードに `FloatSodaDashboard` タブが追加され、中央に 100×100 のカラーボックスが表示されます(`samples/FloatSoda.Samples.OverlayApp/Program.cs`)。

SteamVR を終了するか `VREvent_Quit` を受信するとアプリも自動終了します。

> サンプルには `StatefulWidget` を使った時計ウィジェット(`WatchWidget.cs`)も含まれていますが、`StatefulElement` が未実装のため現在はコメントアウトされています。

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
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();
using var app = builder.Build();

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

app.Run();
```

```bash
# SteamVR を起動してから実行
dotnet run
```

`app.Run()` は SteamVR が終了するまでブロックします。

> **Widget の実装状況:** `Center`, `Align`, `Column`, `Row`, `Flex`, `ColoredBox`, `SizedBox`, `ConstrainedBox`, `Clip*`, `RichText`, `Text` などは使用可能です。`Padding`, `Button`, `StatefulWidget` 系は WIP（`NotImplementedException`）です。詳細は [WidgetSystem.md](WidgetSystem.md) を参照。

<details>
<summary>RenderObject レベルの直接操作（低レベル API）</summary>

```csharp
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();
using var app = builder.Build();

var root = new RenderPositionedBox
{
    Child = new RenderConstrainedBox
    {
        AdditionalConstraints = BoxConstraints.Tight(400, 200),
        Child = new RenderColoredBox { Color = SKColors.CornflowerBlue }
    }
};

// CreateWindow の Child は Widget を要求するため、RenderObject を直接ルートにする場合は
// RenderObjectWidget でラップして渡します。
app.Run();
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

`FloatSodaAppBuilder` でフレームレートを制御できます。

```csharp
// 固定 FPS（デフォルトは IFrameLimiter 実装に依存）
var builder = FloatSodaAppBuilder.CreateDefault();
builder.WithTargetFrameRate(90);

// OpenVR Compositor のタイミングに同期（推奨: WaitGetPoses ベース）
builder.WithOpenVRFrameLimiter();

using var app = builder.Build();
```

VR アプリでは `WithOpenVRFrameLimiter()` を使うと Compositor のリフレッシュレートに同期できます。`WithTargetFrameRate()` を指定しない場合のデフォルトは 30fps です。

---

## 関連ページ

- [WidgetSystem](WidgetSystem.md) — 使えるウィジェットの一覧と実装状況
- [OVRIntegration](OVRIntegration.md) — オーバーレイ種別・プロパティ・イベント処理の詳細
- [Architecture](Architecture.md) — フレームワーク内部の全体像
