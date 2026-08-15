← [Home](Home.md)

# RenderObject ツリー

RenderObject ツリーはレイアウト計算と描画コマンド記録を担う低レベル API です。`FloatSoda` の描画は最終的にすべてこのツリーを通過します。

## 基本契約

```csharp
// 最小限のカスタム RenderBox
public class MyRenderBox : RenderBox
{
    public override void PerformLayout()
    {
        Size = Constraints.Constrain(new SKSize(200, 100)); // 制約内に収める
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
| `Layout(BoxConstraints)` | フレームワークが呼ぶエントリポイント。制約と RelayoutBoundary を判定し、必要なときだけ `PerformLayout()` を呼ぶ(オーバーライド不可) |
| `PerformLayout()` | サブクラスが実装する。`Constraints` を参照して自身の `Size` を決定し、子の `Layout` を呼ぶ |
| `Paint(PaintingContext, Offset)` | `context.Canvas` に Skia 描画コマンドを記録する |
| `Size` | `PerformLayout` で確定したサイズ(`SKSize`) |
| `Constraints` | 直近の `Layout` で親から渡された `BoxConstraints` |

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

## 差分更新(dirty フラグ)

RenderObject は Flutter と同様に **変更があった部分だけを再レイアウト・再ペイント** します。プロパティを変更したら `MarkNeedsLayout()` / `MarkNeedsPaint()` を呼ぶのが契約です([BuildPipeline](BuildPipeline.md) の `UpdateRenderObject` から呼ばれるのが典型)。

### MarkNeedsLayout と RelayoutBoundary

`MarkNeedsLayout()` は自身の `NeedsLayout` を立て、**RelayoutBoundary**(自分のサイズ変更が親に影響しない境界。tight 制約を受けたノードなどが該当)まで親方向に伝播します。境界ノードが `RenderPipeline.NodesNeedingLayout` に登録され、`FlushLayout()` が `Depth` 順に `LayoutWithoutResize()` を呼びます。

### MarkNeedsPaint と RepaintBoundary

`MarkNeedsPaint()` は `IsRepaintBoundary == true` のノードまで親方向に伝播し、そのノードが `RenderPipeline.NodesNeedingPaint` に登録されます。`FlushPaint()` が `PaintingContext.RepaintCompositedChild()` で再記録します。

境界になるのは `RenderView`(ツリーのルート。常に境界)と `RenderRepaintBoundary`(`RepaintBoundary` ウィジェットの実体)です。毎フレーム変化する部分を `RepaintBoundary` で囲むと、その内側の `MarkNeedsPaint()` が祖先へ伝播しなくなり、変化していない周囲を再描画せずに済みます。

いずれの場合も `RenderPipeline.RequestVisualUpdate()` が呼ばれ、`WidgetBinding` に「このフレームは描画が必要」と通知されます。変更がないフレームではレイアウトもペイントも実行されません。

> **Semantics 系の dirty フラグは持ちません:** Flutter の `markNeedsSemanticsUpdate()` に相当する API は FloatSoda では実装していません。理由は [APIDesign § 実装しない API — Semantics](APIDesign.md#実装しない-api--semantics-アクセシビリティツリー) を参照してください。Flutter 本家から RenderObject を移植する際も、semantics 関連のフックは削除します。

## intrinsic 測定

通常のレイアウトは「制約を渡してサイズを受け取る」一方通行です。これに対し intrinsic 測定は、
**制約を渡す前に「子が本来ほしがっているサイズ」を問い合わせる**仕組みです。
`IntrinsicWidth` / `IntrinsicHeight` や `RenderFlex` の一部の配置計算がこれを使います。

`RenderBox` は4つの問い合わせ口を持ちます。

| メソッド | 意味 |
|---|---|
| `GetMinIntrinsicWidth(height)` | この高さで、内容を切り詰めずに描ける最小の幅 |
| `GetMaxIntrinsicWidth(height)` | この高さで、これ以上広げても見た目が変わらない幅 |
| `GetMinIntrinsicHeight(width)` | この幅で、内容を切り詰めずに描ける最小の高さ |
| `GetMaxIntrinsicHeight(width)` | この幅で、これ以上高くしても見た目が変わらない高さ |

サブクラスは対応する `ComputeMinIntrinsicWidth(double)` などを `protected override` で実装します。
`RenderProxyBox` は既定で子へそのまま委譲します。実装しないまま問い合わせを受けると
`NotSupportedException` になります。

> **コストに注意:** intrinsic 測定は通常のレイアウトとは別にツリーを走査します。
> 入れ子にすると走査が掛け算で増え、最悪 O(N²) になります。
> 寸法があらかじめ分かっている場合は `SizedBox` や `ConstrainedBox` を使ってください。

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

## 組み込み RenderObject 一覧

### Layout

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderView` | ルート。ビューポートサイズの tight 制約を子に渡す | `Child`, `Layer` |
| `RenderFlex` | Flex レイアウト(`Row` / `Column` の実体) | `Direction`, `MainAxisAlignment`, `CrossAxisAlignment`, `MainAxisSize`, `Children` |
| `RenderWrap` | 主軸が尽きたら `run` へ折り返す Flex | `Direction`, `Spacing`, `RunSpacing`, `Alignment`, `RunAlignment`, `CrossAxisAlignment`, `Children` |
| `RenderStack` | 子を重ね、非 `Positioned` 子を `Alignment` で配置 | `Alignment`, `Fit`, `Children` |
| `RenderIndexedStack` | `RenderStack` を継承し、`Index` の子だけを描画・ヒットテスト | `Index`, `Alignment`, `Fit`, `Children` |
| `RenderPositionedBox` | 子をアライメントで配置 | `Alignment`, `WidthFactor`, `HeightFactor` |
| `RenderPadding` | 制約を余白分だけ縮小し、子を余白の内側へ配置 | `Padding` (`EdgeInsets`) |
| `RenderConstrainedBox` | 追加の `BoxConstraints` を子に強制 | `AdditionalConstraints` |
| `RenderConstraintsTransformBox` | 親制約を `BoxConstraintsTransform` で変換して子へ渡す | `ConstraintsTransform` (required), `Alignment`, `ClipBehavior` |
| `RenderAspectRatio` | 幅対高さの比率を保った固定寸法を子へ適用 | `AspectRatio` (required) |
| `RenderFittedBox` | 子を自然サイズでレイアウトし、`BoxFit` に従って変換 | `Fit`, `Alignment`, `ClipBehavior` |
| `RenderLimitedBox` | 親の上限が無限の軸だけに最大寸法を適用 | `MaxWidth`, `MaxHeight` |
| `RenderConstrainedOverflowBox` | 親と異なる制約を子へ渡し、領域外への描画を許す(`OverflowBox` の実体) | `MinWidth`, `MaxWidth`, `MinHeight`, `MaxHeight`, `Fit`, `Alignment` |
| `RenderFractionallySizedOverflowBox` | 親の最大寸法に割合を掛けた tight 制約を子へ渡す | `WidthFactor`, `HeightFactor`, `Alignment` |
| `RenderSizedOverflowBox` | 自身は指定サイズを採り、子へは親の元の制約を渡す | `RequestedSize`, `Alignment` |
| `RenderIntrinsicWidth` | 子の最大 intrinsic 幅へ収縮する | `StepWidth` |
| `RenderIntrinsicHeight` | 子の最大 intrinsic 高さへ収縮する | `StepHeight` |
| `RenderRotatedBox` | 90度単位でレイアウト寸法ごと回転する | `QuarterTurns` |
| `RenderOffstage` | 子をレイアウトしたまま描画・ヒットテストから外す | `Offstage` |
| `RenderProxyBox` | レイアウト・ペイントを子に委譲するパススルー基底 | `Child` |

### Painting

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderColoredBox` | 単色で塗りつぶし、子をその上に描画 | `Color` (`SKColor`) |
| `RenderDecoratedBox` | `BoxDecoration` の背景色・角丸・ボーダーを子の前面または背面へ描画 | `Decoration`, `Position` |
| `RenderOpacity` | `OpacityLayer` を挿入して固定の不透明度を適用 | `Opacity` |
| `RenderTransform` | レイアウト後に `Matrix3x2` の2次元変換を適用 | `Transform`, `Origin`, `Alignment`, `TransformHitTests` |
| `RenderRepaintBoundary` | `IsRepaintBoundary` を `true` にし、再ペイントの伝播をここで止める | `Child` |
| `RenderCustomClip<T>` | クリップ系の抽象基底 | `Clipper`, `ClipBehavior` |
| `RenderClipRect` | 矩形でクリップ | `ClipBehavior` |
| `RenderClipRoundRect` | 角丸矩形でクリップ | `BorderRadius`, `ClipBehavior` |
| `RenderClipOval` | 楕円でクリップ | `ClipBehavior` |
| `RenderClipPath` | カスタム `SKPath` でクリップ | `Clipper` (`CustomClipper<SKPath>`), `ClipBehavior` |

### Gesture

すべて `RenderProxyBox` を継承し、レイアウトと描画には手を加えず、ヒットテストの結果だけを変えます。
対応する Widget 側の説明は [WidgetSystem § ジェスチャとヒットテスト](WidgetSystem.md#ジェスチャとヒットテスト) を参照してください。

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderPointerListener` | ヒットしたポインターイベントをコールバックへ流す | `OnPointerDown`, `OnPointerUp`, `OnPointerMove`, `OnPointerEnter`, `OnPointerExit`, `OnPointerCancel`, `Behaviour` |
| `RenderPointerRegion` | `RenderPointerListener` の派生。ホバー(Enter / Exit)だけを扱う | `OnPointerEnter`, `OnPointerExit`, `Behaviour` |
| `RenderAbsorbPointer` | 自身をヒットさせたうえで、子へのヒットテストを止める | `Absorbing` |
| `RenderIgnorePointer` | 自身と子をヒットテストから外す | `Ignoring` |

### Animation

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderAnimatedOpacity` | `IAnimation<double>` を購読し、値変化フレームのみ再ペイントして不透明度を適用(→ [Animation](Animation.md)) | `Opacity` (`IAnimation<double>`) |

### Content

| クラス | 説明 | 主なプロパティ |
|---|---|---|
| `RenderParagraph` | `RichText` のテキストレイアウト・描画エンジン（Topten.RichTextKit 使用） | `Text` (`TextSpan`) |
| `RenderImage` | `SKImage` を描画 | `Image` (required) |

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

## RenderPipeline

`RenderPipeline` は `WidgetBinding.DrawFrame()` から毎フレーム呼ばれます。dirty なノードのリスト(`NodesNeedingLayout` / `NodesNeedingPaint`)を保持し、`Flush*` で消化します。

```csharp
pipeline.FlushLayout();  // NodesNeedingLayout を Depth 順に LayoutWithoutResize()
pipeline.FlushPaint();   // NodesNeedingPaint を RepaintCompositedChild() で再記録
var layer = pipeline.RenderView.Layer?.Clone(); // スレッドセーフにコピー
```

dirty なノードがないフレームでは何も行われません。初回フレームは `RenderView.PrepareInitialFrame()` がルートを両リストに登録することで全体をレイアウト・ペイントします。

## 関連ページ

- [BuildPipeline](BuildPipeline.md) — Widget 側から RenderObject が更新される流れ
- [Architecture](Architecture.md) — レイヤーツリーとレンダースレッドへの受け渡し
- [WidgetSystem](WidgetSystem.md) — 各 RenderObject に対応する Widget
