# FloatSoda: SteamVR Overlay UI Framework (v0.0.0)

**FloatSoda** は、SteamVR Overlay を **Flutter のような宣言的な書き心地** で作成できるように開発中の UI フレームワークです。SkiaSharp → OpenGL → OpenVR という経路でレンダリングし、複数のオーバーレイを統一的に管理できます。

---

## 特徴

- **Flutter-like な開発体験**: 宣言的な UI 構築を目指しています（Widget システムは WIP）
- **RenderObject ツリー**: Flutter の RenderObject に相当するレイアウト・描画ツリーを実装
- **複数オーバーレイ対応**: ダッシュボード・ワールド座標固定・デバイス追従を同時に管理
- **Skia による描画**: SkiaSharp を使用した高品質なレンダリング
- **スレッドセーフ**: メインスレッドとレンダースレッドをレイヤークローンで分離

---

## Getting Started

### 動作環境

- .NET 10 / C# 14
- SteamVR（起動済みであること）
- SkiaSharp / OpenTK / OpenVR

### サンプルアプリの起動

```bash
# SteamVR を起動してから実行する
dotnet run --project samples/FloatSoda.Samples.OverlayApp
```

ダッシュボード・ワールド座標・左コントローラー追従の 3 つのオーバーレイが同時に表示されます。

### 最小構成のコード

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
    Child = new RenderFlex
    {
        Direction = Axis.Vertical,
        MainAxisAlignment = MainAxisAlignment.Center,
        Children =
        [
            new RenderClipRoundRect
            {
                BorderRadius = BorderRadius.Circular(20),
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(300, 300),
                    Child = new RenderColoredBox { Color = SKColors.CornflowerBlue }
                }
            },
            new RenderClipOval
            {
                Child = new RenderConstrainedBox
                {
                    AdditionalConstraints = BoxConstraints.Tight(300, 300),
                    Child = new RenderColoredBox { Color = SKColors.Tomato }
                }
            }
        ]
    }
};

// ダッシュボードオーバーレイ
app.CreateDashboardOverlay("MyDashboard", root, 1000, 1000);

// ワールド座標固定（メートル単位）
// app.CreateWorldSpaceOverlay("MyWorld", root, 1000, 1000, new Vector3(0, 1, -1));

// デバイス追従
// app.CreateTrackingOverlay("MyHand", root, 1000, 1000, TrackedDevice.LeftController);

app.Run();
```

---

## 実装済みの RenderObject

**レイアウト系**

| クラス | 説明 |
|---|---|
| `RenderView` | ルートノード。オーバーレイのサイズを定義 |
| `RenderPositionedBox` | 子を `Alignment` で配置（デフォルト: 中央） |
| `RenderFlex` | Row / Column 相当。`Axis`, `MainAxisAlignment`, `CrossAxisAlignment` を指定可 |
| `RenderConstrainedBox` | 子に `BoxConstraints` を付与してサイズを強制 |

**描画系**

| クラス | 説明 |
|---|---|
| `RenderColoredBox` | 矩形を指定色で塗りつぶす |
| `RenderImage` | `FileImageProvider` でロードした画像を描画 |

**クリップ系**

| クラス | 説明 |
|---|---|
| `RenderClipRect` | 矩形でクリップ |
| `RenderClipRoundRect` | 角丸矩形でクリップ（`BorderRadius` 指定可） |
| `RenderClipPath` | 任意の `SKPath` でクリップ（`CustomClipper<SKPath>` を渡す） |
| `RenderClipOval` | 楕円でクリップ |

---

## ドキュメント

| ドキュメント | 内容 |
|---|---|
| [docs/Architecture.md](docs/Architecture.md) | アーキテクチャ概要・フレームパイプライン・スレッドモデル |
| [docs/GettingStarted.md](docs/GettingStarted.md) | クイックスタートガイド |
| [docs/RenderObjects.md](docs/RenderObjects.md) | RenderObject ツリーのリファレンス |
| [docs/WidgetSystem.md](docs/WidgetSystem.md) | ウィジェット/エレメントシステム（WIP） |
| [docs/OVRIntegration.md](docs/OVRIntegration.md) | OpenVR インテグレーションリファレンス |
| [docs/APIDesign.md](docs/APIDesign.md) | API 設計規約 |

---

## 開発ステータス (v0.0.0 Alpha)

本プロジェクトは現在 **概念実証（PoC）段階** です。API は予告なく変更されます。

- [x] RenderObject ツリー（レイアウト・描画・クリップ・画像）
- [x] レイヤーツリー（ContainerLayer / PictureLayer / ClipLayer / OpacityLayer）
- [x] 複数オーバーレイ（ダッシュボード / ワールド座標 / デバイス追従）
- [x] Widget/Element システムのスキャフォールド
- [ ] Widget → RenderObject への inflate パイプラインの実装
- [ ] SteamVR のイベント処理と宣言的な入力（ヒットテスト）
- [ ] アニメーションシステムの統合
- [ ] マニフェストファイルの自動生成（検討中）
