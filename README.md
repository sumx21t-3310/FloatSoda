# FloatSoda: SteamVR Overlay UI Framework

[![NuGet](https://img.shields.io/nuget/v/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/FloatSoda.svg)](https://www.nuget.org/packages/FloatSoda/)
[![CI](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml/badge.svg)](https://github.com/sumx21t-3310/FloatSoda/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/github/license/sumx21t-3310/FloatSoda)](LICENSE)

**FloatSoda** is a UI framework for building SteamVR overlays with a **Flutter-like declarative API** in C# / .NET. It renders via SkiaSharp → OpenGL → OpenVR, and manages multiple overlays (dashboard, world-space, and device-tracked) in a unified way. Currently in alpha — APIs may change without notice.

> 📖 The documentation below is in Japanese. See the [Wiki](https://github.com/sumx21t-3310/FloatSoda/wiki) for details, or check the [minimal example](#最小構成のコード) — the code speaks for itself.

**FloatSoda** は、SteamVR Overlay を **Flutter のような宣言的な書き心地** で作成できるように開発中の UI フレームワークです。SkiaSharp → OpenGL → OpenVR という経路でレンダリングし、複数のオーバーレイを統一的に管理できます。

## 特徴

- **Flutter-like な開発体験**: `StatelessWidget` / `StatefulWidget` による宣言的な UI 構築と `SetState()` による再ビルド
- **差分更新**: `BuildOwner` による Widget の差分ビルドと、dirty フラグによる RenderObject の差分レイアウト・差分ペイント
- **RenderObject ツリー**: Flutter の RenderObject に相当するレイアウト・描画ツリーを実装
- **複数オーバーレイ対応**: ダッシュボード・ワールド座標固定・デバイス追従を同時に管理
- **Skia による描画**: SkiaSharp を使用した高品質なレンダリング
- **スレッドセーフ**: メインスレッドとレンダースレッドをレイヤークローンで分離

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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkiaSharp;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddFloatSoda();

using var host = builder.Build();
var app = host.Services.GetRequiredService<FloatSodaApp>();

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

await host.RunAsync();
```

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

## 実装済みの Widget

**レイアウト系**

| クラス | 説明 |
|---|---|
| `Row` / `Column` / `Flex` | 子を水平・垂直に並べる。`MainAxisAlignment` / `CrossAxisAlignment` を指定可 |
| `Expanded` / `Flexible` / `Spacer` | `Flex` 系の余剰領域を比率で分配する |
| `Wrap` | 主軸が尽きたら次の行(`run`)へ折り返して並べる |
| `Align` / `Center` | 子を `Alignment` で配置 |
| `Padding` | 子の周囲に `EdgeInsets` の余白を取る |
| `Container` | 配置・装飾・寸法・変換をまとめて指定する合成ウィジェット |
| `SizedBox` | `Width` / `Height` で固定サイズを与える |
| `ConstrainedBox` / `LimitedBox` / `ConstraintsTransformBox` / `UnconstrainedBox` | 子へ渡す `BoxConstraints` を加工する |
| `Stack` / `Positioned` / `IndexedStack` | 子を重ねる。`Positioned` で絶対配置、`IndexedStack` で1つだけ表示 |
| `AspectRatio` / `FittedBox` / `RotatedBox` | 比率の維持、`BoxFit` による拡大縮小、90度単位の回転 |
| `FractionallySizedBox` / `OverflowBox` / `SizedOverflowBox` | 親の寸法に対する割合指定と、領域外へのはみ出しを許す配置 |
| `IntrinsicWidth` / `IntrinsicHeight` | 子の自然な寸法へ収縮する（コストが高いので多用しない） |
| `Visibility` / `Offstage` | 表示の切り替えと、レイアウトを保ったままの非表示 |

**描画系**

| クラス | 説明 |
|---|---|
| `ColoredBox` | 矩形を指定色で塗りつぶす |
| `DecoratedBox` | `BoxDecoration` の背景色・角丸・ボーダーを描画 |
| `Opacity` / `Transform` | 不透明度の適用と、`Matrix3x2` による2次元変換 |
| `Image` | `FileImageProvider` でロードした画像を描画 |
| `Text` / `RichText` | テキストを描画（`Text` は `RichText` の簡易ラッパー） |
| `ClipRect` / `ClipRoundRect` / `ClipOval` | 矩形・角丸矩形・楕円でクリップ |
| `ClipCustomPath` | 任意の `SKPath` でクリップ（`CustomClipper<SKPath>` を渡す） |
| `RepaintBoundary` | 子の再描画を独立した合成レイヤー内に閉じ込める |

**入力系**

| クラス | 説明 |
|---|---|
| `GestureDetector` | タップ・パン（ドラッグ）を検知する |
| `Listener` / `PointerRegion` | 生のポインターイベントと、ホバーの出入りを受け取る |
| `AbsorbPointer` / `IgnorePointer` | ヒットテストを止める・素通りさせる |

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
- `ParentDataWidget<T>` — 親レイアウトだけが解釈する子ごとの情報を渡す（`Expanded` / `Positioned` の基盤）

> **まだ使えないもの:** スクロール系の `ListView` / `GridView` / `SingleChildScrollView` と、
> 画像とアイコンには公開APIの `Paint.Image` / `Paint.Icon` を使用します。
> **`Button` などの UI コンポーネントはまだ提供していません。** 用意する予定の3層構成
> （`FloatSoda.UI` / `FloatSoda.UI.Cream` / `FloatSoda.UI.FizzyPop`）は Phase 5 で、
> 現時点では NuGet 未配布・押下も未反応です。ボタンは `GestureDetector` で組み立ててください。
> `Container` は使えますが、`Padding` の合成にはまだ対応していません。余白は `Padding` を入れ子にしてください。

## ドキュメント

入り口は **[docs/Home.md](docs/Home.md)** です(GitHub Wiki にも自動同期されます)。

| ドキュメント | 内容 |
|---|---|
| [docs/Home.md](docs/Home.md) | ドキュメントトップ・全体像・実装状況サマリ |
| [docs/TargetUsers.md](docs/TargetUsers.md) | 想定する3タイプの作り手と読み進め方 |
| [docs/GettingStarted.md](docs/GettingStarted.md) | クイックスタートガイド |
| [docs/Architecture.md](docs/Architecture.md) | アーキテクチャ概要・フレームパイプライン・スレッドモデル |
| [docs/WidgetSystem.md](docs/WidgetSystem.md) | ウィジェット/エレメントシステム・組み込みウィジェット一覧 |
| [docs/UILayering.md](docs/UILayering.md) | UI層の3層パッケージ構成(ヘッドレス / デザインシステム)。設計方針であり未提供 |
| [docs/Animation.md](docs/Animation.md) | AnimationController・Ticker・Curves によるアニメーション |
| [docs/BuildPipeline.md](docs/BuildPipeline.md) | BuildOwner による Widget 差分更新の仕組み |
| [docs/RenderObjects.md](docs/RenderObjects.md) | RenderObject ツリーのリファレンス |
| [docs/OVRIntegration.md](docs/OVRIntegration.md) | OpenVR インテグレーションリファレンス |
| [docs/Input.md](docs/Input.md) | アクション入力(コントローラーのボタン・トリガー・スティック) |
| [docs/APIDesign.md](docs/APIDesign.md) | API 設計規約 |
| [docs/DocumentationComments.md](docs/DocumentationComments.md) | ドキュメントコメント規約 |
| [docs/Localization.md](docs/Localization.md) | ローカライゼーション方針(日本語デフォルト) |

## 開発ステータス

本プロジェクトは現在 **Alpha 段階・Phase 1(入力基盤)と Phase 2(表示系ウィジェット)が並行して進行中** です。簡単なアプリケーションは動作しますが、API は予告なく変更されます。

開発は Phase 単位で進めています。Phase は「フレームワークとして何ができる段階か」を表す機能上の到達点で、NuGet のバージョン番号とは対応しません。バージョンはリリースの通し番号として独立に上がり、同じ Phase 中に複数のバージョンが公開されることがあります(バージョン番号から Phase を推定することはできません。`1.0.0` のみ Phase 7 に対応)。各 Phase の詳細スコープは [GitHub マイルストーン](https://github.com/sumx21t-3310/FloatSoda/milestones) を参照してください。

| Phase | 内容 | 作れるようになるアプリ | 状況 |
|---|---|---|---|
| Phase 1 | 入力基盤(HitTest / Pointer / Gesture) | 操作できるパネル(GestureDetector で完全自作したボタン・トグル) | 🚧 進行中(残件は非ダッシュボードオーバーレイへのポインタ接続) |
| Phase 2 | basic.dart 相当の表示系ウィジェット網羅 | リッチな HUD / 字幕オーバーレイ | 🚧 進行中(レイアウト・描画・入力・画像・アイコンは一巡。残るのは`CustomPaint`・`DefaultTextStyle`・`ViewMetrics`) |
| Phase 3 | スクロールとアニメーションの充実(Tween / 暗黙的アニメーション / 物理シミュレーション) | チャットビューア等のリスト系アプリ | 未着手 |
| Phase 4 | Hooks・テキスト入力・API安定化 | VR 内メモ帳などの入力を伴うアプリ | 未着手 |
| Phase 5 | Cream / FizzyPop デザインシステム完成 | テーマを選べる実用 UI アプリ | 未着手 |
| Phase 6 | DX 向上(Storybook・manifest 自動生成・ライフサイクル) | デスクトップ常駐+VR のハイブリッドツール | 未着手 |
| Phase 7 | 安定版リリース(1.0) | 実用オーバーレイ全般 | 未着手 |

> ⚠️ **ユーザー操作が動くのはダッシュボードオーバーレイだけです。** ヒットテストとジェスチャ認識は実装済みで、
> `GestureDetector` でタップとパンを受け取れます。ただしポインタ座標の供給元(SteamVR のレーザーポインター)が
> ダッシュボードオーバーレイにしか接続されていないため、`WorldSpaceWindow` と `DeviceTrackedWindow` は表示専用です。

- [x] RenderObject ツリー（レイアウト・描画・クリップ・画像・差分更新）
- [x] レイヤーツリー（ContainerLayer / PictureLayer / ClipLayer / OpacityLayer）
- [x] 複数オーバーレイ（ダッシュボード / ワールド座標 / デバイス追従）
- [x] Widget → RenderObject への inflate パイプライン（StatelessWidget / StatefulWidget）
- [x] BuildOwner による Widget 差分ビルド（Key による子リストの差分更新を含む）
- [x] InheritedWidget によるコンテキスト伝播
- [x] ParentDataWidget による親固有レイアウト情報の伝達（Expanded / Positioned）
- [x] アニメーションシステム（AnimationController / Ticker / FadeTransition）
- [x] ヒットテストとジェスチャ認識（GestureDetector / Listener / タップ・パン）
- [ ] 非ダッシュボードオーバーレイへのポインタ接続（コントローラーレイ経路）
- [ ] スクロール（ListView / GridView / SingleChildScrollView）
- [ ] マニフェストファイルの自動生成（検討中）
