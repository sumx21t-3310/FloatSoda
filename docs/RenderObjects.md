# RenderObject ツリー

RenderObject ツリーはレイアウト計算と描画コマンド記録を担う低レベル API です。`FloatSoda` の描画は最終的にすべてこのツリーを通過します。

## 基本契約

```csharp
// 最小限のカスタム RenderBox
public class MyRenderBox : RenderBox
{
    public override void Layout(BoxConstraints constraints)
    {
        Size = constraints.Constrain(new SKSize(200, 100)); // 制約内に収める
    }

    public override void Paint(PaintingContext context, Offset offset)
    {
        var rect = SKRect.Create(offset.X, offset.Y, Size.Width, Size.Height);
        context.Canvas.DrawRect(rect, new SKPaint { Color = SKColors.Coral });
    }
}
```

| メソッド / プロパティ | 役割 |
|---|---|
| `Layout(BoxConstraints)` | 自身のサイズを決定し、子の `Layout` を呼ぶ |
| `Paint(PaintingContext, Offset)` | `context.Canvas` に Skia 描画コマンドを記録する |
| `Size` | `Layout` で確定したサイズ（`SKSize`） |

---

## 制約フロー

レイアウトは **制約は下へ・サイズは上へ** の原則で動きます。

```
RenderView (tight 制約: ビューポートサイズ)
  └─ RenderFlex
       ├─ RenderConstrainedBox (追加制約を合成)
       │    └─ RenderColoredBox → size を親に返す
       └─ RenderColoredBox → size を親に返す
```

`BoxConstraints` の主なファクトリ:

| ファクトリ | 意味 |
|---|---|
| `BoxConstraints.Tight(w, h)` | 幅・高さを固定 |
| `BoxConstraints.TightFor(width: w)` | 幅だけ固定、高さはフリー |
| `constraints.Loosen()` | min を 0 に緩める（子が自由にサイズを決められる） |
| `constraints.Enforce(other)` | 別の制約で上書き |

---

## PaintingContext とレイヤーツリー

`Paint` の引数 `PaintingContext` は Skia のキャンバスを抽象化したものです。ドローコールを記録し、`PictureLayer`（`SKPicture`）としてレイヤーツリーに蓄積します。

クリッピングやオパシティを挟む場合は `PushClip*` / `PushOpacity` を使います。

```csharp
// クリップレイヤーを挿入してから子を描画
context.PushClipRect(childOffset, clipRect, Clip.Antialias, (ctx, off) =>
{
    child.Paint(ctx, off);
});
```

レイヤーツリーは `ILayer` の階層で構成されます:

| レイヤー | 役割 |
|---|---|
| `ContainerLayer` | 子レイヤーをまとめるノード |
| `PictureLayer` | `SKPicture`（Skia の記録済み描画コマンド）を保持するリーフ |
| `ClipRectLayer` / `ClipRoundRectLayer` / `ClipPathLayer` | 矩形・角丸・パスのクリッピング |
| `OpacityLayer` | アルファ合成 |
| `TransformLayer` | 変換行列を適用 |

---

## 組み込み RenderObject 一覧

### Layout

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderView` | ルート。ビューポートサイズの tight 制約を子に渡す | `Child`, `Layer` |
| `RenderFlex` | Flex レイアウト（Row/Column の実体） | `Direction`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize`, `Children` |
| `RenderPositionedBox` | 子をアライメントで配置 | `Alignment` |
| `RenderConstrainedBox` | 追加の `BoxConstraints` を子に強制 | `AdditionalConstraints` |
| `RenderProxyBox` | レイアウト・ペイントを子に委譲するパススルー基底 | `Child` |

### Painting

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderColoredBox` | 単色で塗りつぶし、子をその上に描画 | `Color` (`SKColor`) |
| `RenderClipRect` | 矩形でクリップ | `ClipBehavior` |
| `RenderClipRoundRect` | 角丸矩形でクリップ | `BorderRadius`, `ClipBehavior` |
| `RenderClipOval` | 楕円でクリップ | `ClipBehavior` |
| `RenderClipPath` | カスタム `SKPath` でクリップ | `Clipper` (`CustomClipper<SKPath>`), `ClipBehavior` |

### Content

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderParagraph` | `RichText` のテキストレイアウト・描画エンジン（Topten.RichTextKit 使用） | `Texts` (`IList<InlineSpan>`) |
| `RenderImage` | `SKImage` を描画 | `Image` (required) |

---

## カスタムクリッパーの実装

`CustomClipper<SKPath>` を継承して `GetClip(SKSize)` でパスを返します。

```csharp
// 下端が波打つ形状のクリッパー（ArcClipper を参照）
public class ArcClipper : CustomClipper<SKPath>
{
    public override SKPath GetClip(SKSize size)
    {
        var path = new SKPath();
        path.LineTo(0f, size.Height - 30);
        path.QuadTo(
            new SKPoint(size.Width / 4, size.Height),
            new SKPoint(size.Width / 2, size.Height));
        path.QuadTo(
            new SKPoint(size.Width * 3 / 4, size.Height),
            new SKPoint(size.Width, size.Height - 30));
        path.LineTo(size.Width, 0);
        path.Close();
        return path;
    }

    public override bool ShouldReclip(CustomClipper<SKPath> oldClipper) => false;
}

// 使用
var clipped = new RenderClipPath
{
    Clipper = new ArcClipper(),
    Child = new RenderConstrainedBox
    {
        AdditionalConstraints = BoxConstraints.Tight(300, 300),
        Child = new RenderColoredBox { Color = SKColors.Tomato }
    }
};
```

---

## RenderPipeline

`RenderPipeline` は毎フレーム `FloatSodaApp.DrawFrame()` から呼ばれます。

```csharp
pipeline.FlushLayout();  // RenderView.PerformLayout() → 子に制約を伝播
pipeline.FlushPaint();   // RenderView.Paint() → PaintingContext でレイヤーツリーを生成
var layer = pipeline.RenderView?.Layer.Clone(); // スレッドセーフにコピー
```

`NeedsRebuild` が `true` のパイプラインだけが再描画されます。`RenderObject` のプロパティが変更されると自動的にセットされます。
