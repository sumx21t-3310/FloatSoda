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

FloatSoda は Flutter の三ツリーモデルをベースに、現在 **RenderObject ツリー** と **レイヤーツリー** が完全実装済みです。ウィジェット/エレメントツリーは `StatelessWidget` / `StatelessElement` が実装済みで、`StatefulWidget` / `StatefulElement` は WIP です。

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

    E -->|"AttachRenderObject\n(StatelessWidget ✓ / StatefulWidget WIP)"| RV
    RB -->|"Paint → PaintingContext"| CL
```

各ツリーの役割:

- **Widget** — 宣言的な UI の設計図。`abstract record` で不変。
- **Element** — Widget と RenderObject を橋渡しするミュータブルなノード。`StatelessElement` は実装済み。`StatefulElement` は WIP。
- **RenderObject** — レイアウト計算（`Layout`）と描画コマンド記録（`Paint`）を担う。
- **Layer** — `Paint` フェーズが生成する合成操作のツリー。クローンしてレンダースレッドに渡す。

---

## レンダリングライフサイクル

```mermaid
sequenceDiagram
    participant Main as メインスレッド
    participant Pipeline as RenderPipeline
    participant RV as RenderView
    participant RO as RenderObject 群
    participant PC as PaintingContext
    participant Layer as レイヤーツリー
    participant RT as レンダースレッド
    participant Renderer as Renderer
    participant GL as GLView (GLFW/OpenGL)
    participant VR as OpenVR Compositor

    loop メインループ (FloatSodaApp.MainLoop)
        Main->>Main: VREventDispatcher.PollEvents()
        Note over Main: VREvent_Quit → CancellationToken をキャンセル

        alt pipeline.NeedsRebuild == true
            Main->>Pipeline: FlushLayout()
            Pipeline->>RV: PerformLayout()
            RV->>RO: Layout(BoxConstraints) [再帰]
            Note over RO: 制約を子へ伝播し<br/>SKSize を親に返す

            Main->>Pipeline: FlushPaint()
            Pipeline->>PC: new PaintingContext(RenderView.Layer, bounds)
            Pipeline->>RV: Paint(context, Offset.Zero)
            RV->>RO: Paint(context, offset) [再帰]
            Note over RO: context.Canvas に Skia ドローコール記録<br/>クリップ等は PushLayer で子 ContainerLayer に分岐
            RO->>PC: Canvas アクセス → StartRecording()
            PC->>Layer: PictureLayer を ContainerLayer に追加
            Pipeline->>PC: StopRecordingIfNeeded()
            Note over PC: SKPictureRecorder.EndRecording()<br/>→ PictureLayer.Picture に保存

            Main->>Layer: Layer.Clone() — スレッドセーフコピー
            Main-->>RT: PostTask(capturedLayer)
        end

        Main->>Main: FrameLimiter.Wait()
    end

    loop レンダースレッド (RenderThreadRunner)
        RT->>RT: ConcurrentQueue からタスクを取り出す
        RT->>Renderer: Render(layer)
        Renderer->>GL: Clear()
        Note over GL: GRContext.ResetContext()<br/>SKSurface.Canvas.Clear(Transparent)
        Renderer->>Layer: layer.Layout(LayerContext)
        Note over Layer: PaintBounds を再帰的に計算
        Renderer->>Layer: layer.Paint(LayerContext)
        Note over Layer: ContainerLayer → PictureLayer の順に<br/>SKCanvas.DrawPicture() で描画<br/>ClipLayer は SaveLayer/ClipXxx を挿入
        Renderer->>GL: Flush()
        Note over GL: SKSurface.Flush() → GRContext.Flush()<br/>→ GL.Flush() で GL テクスチャに書き込み完了
        RT->>VR: Overlay.Texture.FromTexture_t(GL texture handle)
        Note over VR: ETextureType.OpenGL / EColorSpace.Auto
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
