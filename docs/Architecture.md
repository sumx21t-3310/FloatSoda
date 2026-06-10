# Architecture

FloatSoda は Flutter のアーキテクチャを参考に設計された VR オーバーレイ UI フレームワークです。SkiaSharp で描画コマンドを記録し、OpenGL テクスチャに焼き付けて OpenVR Compositor に提出することで SteamVR オーバーレイを表示します。

## アセンブリ構成

```mermaid
graph TD
    Common["FloatSoda.Common\nジオメトリ型・レイヤーツリー"]
    Engine["FloatSoda.Engine\nOpenGL・レンダースレッド"]
    OVR["FloatSoda.OVR\nOpenVR ラッパー"]
    Core["FloatSoda\nウィジェット・RenderObject・パイプライン"]

    Common --> Engine
    Common --> Core
    OVR --> Core
    Engine --> Core
```

| アセンブリ | 役割 |
|---|---|
| `FloatSoda.Common` | `Offset`, `Alignment`, `BoxConstraints` などのジオメトリ型。`ILayer` / `ContainerLayer` / `PictureLayer` などのレイヤーツリーインターフェース |
| `FloatSoda.Engine` | `GLView`（OpenGL サーフェス）、`Renderer`（レイヤーツリー → OpenGL FBO）、`RenderThreadRunner`（レンダースレッド管理）、`FrameLimiter` |
| `FloatSoda.OVR` | OpenVR 初期化（`Application`）、オーバーレイ型（`DashboardOverlay` / `WorldSpaceOverlay` / `DeviceTrackedOverlay`）、イベントディスパッチャ、例外体系 |
| `FloatSoda` | ウィジェット/エレメントツリー、RenderObject ツリー、`RenderPipeline`、`FloatSodaApp` / `FloatSodaAppBuilder` |

---

## ツリー構造

FloatSoda は Flutter の三ツリーモデルをベースに、現在 **RenderObject ツリー** と **レイヤーツリー** が実装済みです。ウィジェット/エレメントツリーは WIP です。

```mermaid
graph LR
    subgraph "Widget Tree (WIP)"
        W["Widget\n(immutable record)"]
        E["Element\n(mutable)"]
        W -->|CreateElement| E
    end

    subgraph "RenderObject Tree"
        RV["RenderView"]
        RB["RenderBox\nサブクラス群"]
        RV --> RB
    end

    subgraph "Layer Tree"
        CL["ContainerLayer"]
        PL["PictureLayer\n(SKPicture)"]
        CL --> PL
    end

    E -->|"AttachRenderObject (WIP)"| RV
    RB -->|"Paint → PaintingContext"| CL
```

各ツリーの役割:

- **Widget** — 宣言的な UI の設計図。`abstract record` で不変。
- **Element** — Widget と RenderObject を橋渡しするミュータブルなノード。（現在 `NotImplementedException`）
- **RenderObject** — レイアウト計算（`Layout`）と描画コマンド記録（`Paint`）を担う。
- **Layer** — `Paint` フェーズが生成する合成操作のツリー。クローンしてレンダースレッドに渡す。

---

## フレームパイプライン

```mermaid
sequenceDiagram
    participant Main as メインスレッド
    participant RT as レンダースレッド
    participant VR as OpenVR Compositor

    loop メインループ
        Main->>Main: PollEvents() — VRイベント処理
        Main->>Main: RenderPipeline.FlushLayout()
        Note over Main: RenderView.PerformLayout()<br/>BoxConstraints を子に伝播
        Main->>Main: RenderPipeline.FlushPaint()
        Note over Main: PaintingContext で SKPicture 記録<br/>ContainerLayer ツリーを構築
        Main->>Main: Layer.Clone() — スレッドセーフコピー
        Main-->>RT: PostTask(layer)
        RT->>RT: Renderer.Render(layer)
        Note over RT: LayerContext.Layout/Paint<br/>SkiaSharp → GL FBO
        RT-->>VR: SetOverlayTexture(GL texture handle)
        Main->>Main: FrameLimiter.Wait()
    end
```

---

## スレッドモデル

| スレッド | 所有物 | 通信方法 |
|---|---|---|
| **メインスレッド** | RenderPipeline, Widget/RenderObject ツリー, VREventDispatcher | `RenderThreadRunner.PostTask(Action)` でタスクをキューに積む |
| **レンダースレッド** | OpenGL コンテキスト, `GLView`, `Renderer`, `OverlayWindow` | `ConcurrentQueue<Action>` を処理 |

OpenGL のコンテキストはレンダースレッドが独占します。ウィンドウ作成も `PostTask` 経由でレンダースレッド上で実行されます。

```
メインスレッド
  └─ RenderThreadRunner.PostTask(layer 更新ラムダ)
         │ ConcurrentQueue
         ▼
    レンダースレッド
         └─ OverlayWindow.Update()
              └─ Renderer.Render(layer)
                   └─ GLView.Clear() → layer.Paint() → GLView.Flush()
              └─ SetOverlayTexture(GL texture handle)
```
