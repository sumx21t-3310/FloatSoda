# FloatSoda: SteamVR Overlay UI Framework

[![NuGet](https://img.shields.io/nuget/v/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![CI](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml/badge.svg)](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/sumx21t-3310/FloatSoda)](LICENSE)

**FloatSoda** is a UI framework for building SteamVR overlays with a **Flutter-like declarative API** in C# / .NET. It renders via SkiaSharp → OpenGL → OpenVR, and manages multiple overlays (dashboard, world-space, and device-tracked) in a unified way. Currently in alpha — APIs may change without notice.

> 📖 The documentation below is in Japanese. See the [Wiki](https://github.com/sumx21t-3310/FloatSoda/wiki) for details, or check the [minimal example](#最小構成のコード) — the code speaks for itself.

**FloatSoda** は、SteamVR Overlay を **Flutter のような宣言的な書き心地** で作成できるように開発中の UI フレームワークです。SkiaSharp → OpenGL → OpenVR という経路でレンダリングし、複数のオーバーレイを統一的に管理できます。

---

## 特徴

- **Flutter-like な開発体験**: `StatelessWidget` / `StatefulWidget` による宣言的な UI 構築と `SetState()` による再ビルド
- **差分更新**: `BuildOwner` による Widget の差分ビルドと、dirty フラグによる RenderObject の差分レイアウト・差分ペイント
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

SteamVR ダッシュボードにカラーボックスを表示するオーバーレイが起動します。

### 最小構成のコード

```csharp
using FloatSoda;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var builder = FloatSodaAppBuilder.CreateDefault();
using var app = builder.Build();

Widget root = new Align
{
    Child = new SizedBox
    {
        Width = 100,
        Height = 100,
        Child = new ColoredBox
        {
            Color = SKColors.Tomato
        }
    }
};

// ダッシュボードオーバーレイ（サイズは root のレイアウト結果に自動追従）
app.CreateWindow(new DashboardWindow { Title = "MyDashboard", Child = root });

// ワールド座標固定（メートル単位。Position 省略時は前方1m・高さ1m）
// app.CreateWindow(new WorldSpaceWindow { Title = "MyWorld", Child = root });

// デバイス追従
// app.CreateWindow(new DeviceTrackedWindow { Title = "MyHand", Child = root, Target = TrackedDevice.LeftController });

app.Run();
```

---

## レンダリングライフサイクル

```mermaid
sequenceDiagram
    participant Main as メインスレッド
    participant Pipeline as RenderPipeline
    participant RO as RenderObject 群
    participant Layer as レイヤーツリー
    participant RT as レンダースレッド
    participant GL as GLView (OpenGL)
    participant VR as OpenVR Compositor

    loop メインループ
        Main->>Main: VREventDispatcher.PollEvents()
        Main->>Pipeline: FlushLayout()
        Pipeline->>RO: Layout(BoxConstraints) [再帰]
        Note over RO: 制約↓ サイズ↑
        Main->>Pipeline: FlushPaint()
        Pipeline->>RO: Paint(PaintingContext, Offset) [再帰]
        Note over RO: Skia ドローコールを記録<br/>→ PictureLayer に格納
        Main->>Layer: Clone() — スレッドセーフコピー
        Main-->>RT: PostTask(layer)
        Main->>Main: FramePacer.WaitForNextFrame()
    end

    loop レンダースレッド
        RT->>GL: Clear()
        RT->>Layer: Layout(LayerContext)
        RT->>Layer: Paint(LayerContext)
        Note over Layer: DrawPicture / SaveLayer<br/>でレイヤーを合成
        RT->>GL: Flush()
        Note over GL: SKSurface → GRContext → GL テクスチャ
        RT->>VR: Overlay.Texture.FromTexture_t(GL texture handle)
    end
```

> 詳細は [docs/Architecture.md](docs/Architecture.md) を参照。

---

## 実装済みの Widget

**レイアウト系**

| クラス | 説明 |
|---|---|
| `Row` / `Column` / `Flex` | 子を水平・垂直に並べる。`MainAxisAlignment` / `CrossAxisAlignment` を指定可 |
| `Align` / `Center` | 子を `Alignment` で配置 |
| `SizedBox` | `Width` / `Height` で固定サイズを与える |
| `ConstrainedBox` | 子に `BoxConstraints` を付与してサイズを制約 |

**描画系**

| クラス | 説明 |
|---|---|
| `ColoredBox` | 矩形を指定色で塗りつぶす |
| `Image` | `FileImageProvider` でロードした画像を描画 |
| `Text` / `RichText` | テキストを描画（`Text` は `RichText` の簡易ラッパー） |
| `ClipRect` / `ClipRoundRect` / `ClipOval` | 矩形・角丸矩形・楕円でクリップ |
| `ClipCustomPath` | 任意の `SKPath` でクリップ（`CustomClipper<SKPath>` を渡す） |

**ウィンドウ系**

| クラス | 説明 |
|---|---|
| `DashboardWindow` | SteamVR ダッシュボードに表示するオーバーレイ |
| `WorldSpaceWindow` | ワールド座標に固定するオーバーレイ（メートル単位） |
| `DeviceTrackedWindow` | HMD・コントローラー等のデバイスに追従するオーバーレイ |

**自作 Widget の基底クラス**

- `StatelessWidget` — `Build()` をオーバーライドして UI を宣言
- `StatefulWidget<T>` + `State<T>` — `SetState()` で状態変更と再ビルド
- `InheritedWidget` — ツリー下方向へのコンテキスト伝播

> `Container` / `Padding` / `Opacity` / `ListView` / `GridView` / `Button` / `GestureDetector` などは API 定義のみの未実装スタブです。

---

## ドキュメント

入り口は **[docs/Home.md](docs/Home.md)** です(GitHub Wiki にも自動同期されます)。

| ドキュメント | 内容 |
|---|---|
| [docs/Home.md](docs/Home.md) | ドキュメントトップ・全体像・実装状況サマリ |
| [docs/GettingStarted.md](docs/GettingStarted.md) | クイックスタートガイド |
| [docs/Architecture.md](docs/Architecture.md) | アーキテクチャ概要・フレームパイプライン・スレッドモデル |
| [docs/WidgetSystem.md](docs/WidgetSystem.md) | ウィジェット/エレメントシステム |
| [docs/BuildPipeline.md](docs/BuildPipeline.md) | BuildOwner による Widget 差分更新の仕組み |
| [docs/RenderObjects.md](docs/RenderObjects.md) | RenderObject ツリーのリファレンス |
| [docs/OVRIntegration.md](docs/OVRIntegration.md) | OpenVR インテグレーションリファレンス |
| [docs/APIDesign.md](docs/APIDesign.md) | API 設計規約 |

---

## 開発ステータス (v0.1.0 Alpha)

本プロジェクトは現在 **Alpha 段階** です。簡単なアプリケーションは動作しますが、API は予告なく変更されます。

- [x] RenderObject ツリー（レイアウト・描画・クリップ・画像・差分更新）
- [x] レイヤーツリー（ContainerLayer / PictureLayer / ClipLayer / OpacityLayer）
- [x] 複数オーバーレイ（ダッシュボード / ワールド座標 / デバイス追従）
- [x] Widget → RenderObject への inflate パイプライン（StatelessWidget / StatefulWidget）
- [x] BuildOwner による Widget 差分ビルド（Key による子リストの差分更新を含む）
- [x] InheritedWidget によるコンテキスト伝播
- [x] アニメーションシステム（AnimationController / Ticker / FadeTransition）
- [ ] SteamVR のイベント処理と宣言的な入力（ヒットテスト）
- [ ] マニフェストファイルの自動生成（検討中）
