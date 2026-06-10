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

起動すると以下の 3 つのオーバーレイが同時に表示されます。

| オーバーレイ | 種別 | 内容 |
|---|---|---|
| FloatSodaDashboard | ダッシュボード | カラーボックスと画像の縦並び |
| FloatSodaWorldSpace | ワールド座標 (0, 1, -1) | パスクリップ・角丸クリップ・楕円クリップのデモ |
| FloatSodaTracking | 左コントローラー追従 | 3 色ボックスの横並び |

SteamVR を終了するか `VREvent_Quit` を受信するとアプリも自動終了します。

---

## 新しいアプリを作る

### 1. プロジェクトを作成する

```bash
dotnet new console -n MyOverlayApp
cd MyOverlayApp
dotnet add reference ../path/to/FloatSoda/src/FloatSoda/FloatSoda.csproj
```

### 2. 最小構成のコードを書く

`Program.cs` を以下のように書き換えます。

```csharp
using FloatSoda;
using FloatSoda.Geometrics;
using FloatSoda.Render.Layout;
using FloatSoda.Render.Painting;
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

app.CreateDashboardOverlay("HelloWorld", root, 1000, 1000);

app.Run();
```

```bash
# SteamVR を起動してから実行
dotnet run
```

`app.Run()` は SteamVR が終了するまでブロックします。

> **Widget システムについて:** `StatelessWidget` などの宣言的 API は現在 WIP です。`CreateElement()` は `NotImplementedException` を投げるため、現時点では RenderObject を直接組み合わせる上記の方法を使ってください。詳細は [WidgetSystem.md](WidgetSystem.md) を参照。

---

## オーバーレイ種別の選び方

`FloatSodaApp` には 3 種類のオーバーレイ作成メソッドがあります。

| メソッド | オーバーレイ種別 | 位置の管理 |
|---|---|---|
| `CreateDashboardOverlay(name, root, w, h)` | `DashboardOverlay` | SteamVR ダッシュボードが管理（ユーザーが開くタブ） |
| `CreateWorldSpaceOverlay(name, root, w, h, position)` | `WorldSpaceOverlay` | ワールド座標で固定（`Vector3 position`） |
| `CreateTrackingOverlay(name, root, w, h, device)` | `DeviceTrackedOverlay` | トラッキングデバイスに追従（`TrackedDevice` 列挙体） |

```csharp
// ダッシュボード
app.CreateDashboardOverlay("MyDashboard", root, 1000, 1000);

// ワールド座標 (x=0, y=1, z=-1 メートル)
app.CreateWorldSpaceOverlay("MyWorld", root, 1000, 1000, new Vector3(0, 1, -1));

// 左コントローラーに追従
app.CreateTrackingOverlay("MyHand", root, 1000, 1000, TrackedDevice.LeftController);
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

VR アプリでは `WithOpenVRFrameLimiter()` を使うと Compositor のリフレッシュレートに同期できます。
