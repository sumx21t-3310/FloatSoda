← [Home](Home.md)

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

FloatSoda は Flutter の三ツリーモデルをベースに、現在 **RenderObject ツリー** と **レイヤーツリー** が完全実装済みです。ウィジェット/エレメントツリーは `StatelessWidget` / `StatelessElement` と `BuildOwner` による差分ビルドが実装済みで、`StatefulWidget` / `StatefulElement` は WIP です(詳細は [WidgetSystem](WidgetSystem.md) と [BuildPipeline](BuildPipeline.md))。

```mermaid
graph LR
    subgraph "Widget / Element Tree"
        W["Widget\n(immutable record)"]
        E["Element\n(mutable)"]
        BO["BuildOwner\n(dirty list)"]
        W -->|CreateElement| E
        BO -->|"BuildScope() で\ndirty Element を再ビルド"| E
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

    E -->|"CreateRenderObject /\nUpdateRenderObject"| RV
    RB -->|"Paint → PaintingContext"| CL
```

各ツリーの役割:

- **Widget** — 宣言的な UI の設計図。`abstract record` で不変。
- **Element** — Widget と RenderObject を橋渡しするミュータブルなノード。`MarkNeedsBuild()` で dirty になり、`BuildOwner` が次フレームの `BuildScope()` でまとめて再ビルドする。`StatelessElement` は実装済み。`StatefulElement` は WIP。
- **BuildOwner** — dirty な Element のリストを保持し、`Depth` 順(親が先)に再ビルドを実行するスケジューラ。`WidgetBinding` がウィンドウごとに 1 つ保持する。
- **RenderObject** — レイアウト計算(`PerformLayout`)と描画コマンド記録(`Paint`)を担う。`MarkNeedsLayout` / `MarkNeedsPaint` の dirty フラグにより、変更があった部分だけを再レイアウト・再ペイントする。
- **Layer** — `Paint` フェーズが生成する合成操作のツリー。クローンしてレンダースレッドに渡す。

---

## レンダリングライフサイクル

```mermaid
sequenceDiagram
    participant Main as メインスレッド
    participant WB as WidgetBinding
    participant BO as BuildOwner
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

        Main->>WB: DrawFrame() [ウィンドウごと]
        WB->>BO: BuildScope()
        Note over BO: dirty な Element を Depth 順に Rebuild<br/>→ UpdateRenderObject でプロパティ反映

        alt NeedsVisualUpdate == true
            WB->>Pipeline: FlushLayout()
            Pipeline->>RO: LayoutWithoutResize() [dirty ノードのみ・Depth 順]
            Note over RO: 制約を子へ伝播し<br/>SKSize を親に返す

            WB->>Pipeline: FlushPaint()
            Pipeline->>PC: RepaintCompositedChild(node) [dirty ノードのみ]
            RV->>RO: Paint(context, offset) [再帰]
            Note over RO: context.Canvas に Skia ドローコール記録<br/>クリップ等は PushLayer で子 ContainerLayer に分岐
            PC->>Layer: PictureLayer を ContainerLayer に追加
            Note over PC: SKPictureRecorder.EndRecording()<br/>→ PictureLayer.Picture に保存

            WB->>Layer: Layer.Clone() — スレッドセーフコピー
            WB-->>RT: PostRender(window, capturedLayer)
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

---

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — BuildOwner による差分ビルドの詳細
- [RenderObjects](RenderObjects.md) — 差分レイアウト・差分ペイントの仕組み
- [OVRIntegration](OVRIntegration.md) — OpenVR オーバーレイとイベント処理
