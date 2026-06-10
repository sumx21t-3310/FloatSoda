# OpenVR インテグレーション

`FloatSoda.OVR` アセンブリは OpenVR API をラップし、型安全なオーバーレイ操作を提供します。

## Application — OpenVR 初期化

`Application` クラスはコンストラクタ呼び出し時に `OpenVR.Init()` を実行します。

```csharp
var app = new Application(ApplicationType.Overlay);
// app.OVRSystem → CVRSystem（低レベル OpenVR API）
// app.Type      → ApplicationType.Overlay
```

`FloatSodaApp.Run()` を呼ぶと内部で自動的に `Application(ApplicationType.Overlay)` が生成されるため、通常は直接インスタンス化する必要はありません。

### ApplicationType

| 値 | 説明 |
|---|---|
| `Overlay` | オーバーレイ専用アプリ（FloatSoda のデフォルト） |
| `Scene` | 3D シーンを描画するアプリ |
| `Background` | SteamVR を起動しないバックグラウンドアプリ |
| `Utility` | ハードウェア不要のユーティリティ（インストーラーなど） |

---

## オーバーレイ種別

```mermaid
classDiagram
    IOverlay <|-- IDashboardOverlay
    IOverlay <|-- IMovableOverlay
    IMovableOverlay <|-- IMovableOverlay~TTransform~
    IDashboardOverlay <|.. DashboardOverlay
    IMovableOverlay~TTransform~ <|.. MovableOverlay~TTransform~
    MovableOverlay~TTransform~ <|-- WorldSpaceOverlay
    MovableOverlay~TTransform~ <|-- DeviceTrackedOverlay
```

| クラス | 位置管理 | `Visibility` | `Transform` |
|---|---|---|---|
| `DashboardOverlay` | SteamVR ダッシュボードが管理 | なし（ダッシュボードに出現） | なし |
| `WorldSpaceOverlay` | ワールド座標で固定 | あり | `WorldOverlayTransform` |
| `DeviceTrackedOverlay` | トラッキングデバイスに追従 | あり | `DeviceTrackedOverlayTransform` |

### DashboardOverlay

```csharp
var identity = new DashboardOverlayIdentity("アプリ名", "ウィンドウ名");
var overlay = new DashboardOverlay(identity);
// overlay.Opacity.Value = 0.9f;
// overlay.WidthInMeters.Value = 1.5f;
```

### WorldSpaceOverlay

```csharp
var identity = new OverlayIdentity("MyApp", "WorldWindow");
var overlay = new WorldSpaceOverlay(identity);

overlay.Visibility.Show();
overlay.Transform.Position = new Vector3(0, 1.5f, -2f);   // メートル単位
overlay.Transform.Rotation = Quaternion.CreateFromYawPitchRoll(0, 0, 0);
```

### DeviceTrackedOverlay

```csharp
var identity = new OverlayIdentity("MyApp", "HandWindow");
var overlay = new DeviceTrackedOverlay(identity);

overlay.Visibility.Show();
overlay.Transform.Target = TrackedDevice.LeftController;
overlay.Transform.Position = new Vector3(0, 0.05f, 0); // コントローラー相対オフセット
```

`TrackedDevice` の値: `LeftController`, `RightController`, `HMD`

---

## オーバーレイプロパティ

すべての `IOverlay` 実装は以下のプロパティ（ケーパビリティオブジェクト）を持ちます。

| プロパティ | 型 | 説明 |
|---|---|---|
| `Opacity` | `OverlayOpacity` | `Value` (0.0–1.0) でアルファを設定 |
| `WidthInMeters` | `OverlayWidthInMeters` | `Value` でワールド幅（メートル）を設定 |
| `Curvature` | `OverlayCurvature` | `Value` で曲率を設定 |
| `Texture` | `OverlayTexture` | `FromTexture_t()` / `FromFile()` でテクスチャを更新 |
| `Flags` | `OverlayFlags` | `[VROverlayFlags.X]` でフラグを読み書き |
| `Visibility` | `OverlayVisibility` | `Show()` / `Hide()` |
| `Transform` | `OverlayTransform` | `Position`, `Rotation` で位置・向きを設定 |

---

## VREventDispatcher

`VREventDispatcher` は `PollEvents()` を呼ぶたびに OpenVR のイベントキューを消費し、登録されたハンドラを呼び出します。

```csharp
var dispatcher = new VREventDispatcher(application.OVRSystem);

dispatcher.Register(EVREventType.VREvent_Quit, (in VREvent_t _) =>
{
    application.OVRSystem.AcknowledgeQuit_Exiting();
    // 終了処理...
});

// メインループ内で毎フレーム呼ぶ
dispatcher.PollEvents();
```

`FloatSodaApp.Run()` は `VREvent_Quit` / `VREvent_ProcessQuit` を自動登録しています。

---

## 例外体系

OpenVR API のエラーはすべて型付き例外に変換されます。

| 例外クラス | 発生タイミング |
|---|---|
| `VRInitializeException` | `Application` 初期化時（SteamVR が起動していないなど） |
| `VROverlayException` | オーバーレイ作成・操作エラー |
| `VRCompositorException` | Compositor 操作エラー |
| `VRInputException` | 入力システムエラー |
| `VRApplicationException` | アプリケーション登録エラー |
| `TrackedPropertyException` | トラッキングプロパティ取得エラー |

`OpenVRExceptionHelper.ThrowIfError()` が各 OpenVR API の戻り値を検査して例外を投げます。ケーパビリティオブジェクト内部で自動的に呼ばれるため、通常は手動で呼ぶ必要はありません。

```csharp
try
{
    using var app = new Application(ApplicationType.Overlay);
    // ...
}
catch (VRInitializeException ex)
{
    Console.Error.WriteLine($"SteamVR 初期化失敗: {ex.Message}");
}
```

---

## Math — Matrix ヘルパー

`FloatSoda.OVR.Math.Matrix` は `System.Numerics.Matrix4x4` と OpenVR の `HmdMatrix34_t` を相互変換するヘルパーです。

```csharp
// Matrix4x4 → HmdMatrix34_t（OverlayTransform.Apply() 内部で使用）
var hmd = matrix4x4.ToHmdMatrix34_t();
OpenVR.Overlay.SetOverlayTransformAbsolute(handle, origin, ref hmd);
```

`OverlayTransform` サブクラスを実装する場合は `GetMatrix()` が `Position` + `Rotation` から `Matrix4x4` を生成するので、`Apply()` で `ToHmdMatrix34_t()` を呼ぶだけで済みます。
