# フレームワーク(三つのツリー): 貢献までの流れ

このチュートリアルでは、Flutter の `FractionalTranslation` を FloatSoda に移植し、テストと層1サンプルを付けて PR を出すところまで進めます。終えると、Flutter の Widget と RenderObject を読んで C# に置き換える流れと、三つのツリーの層の PR に要るものがひととおり分かります。

> ※要確認 — `FractionalTranslation` は、Flutter の `widgets/basic.dart` にあり FloatSoda にまだないウィジェットの中から選んだ題材です。Phase 2 の対象の一覧(#178)には入っておらず、移植を追跡する Issue もありません。取り込むかどうかはオーナーが決めます。

## 作るもの

`FractionalTranslation` は、子を「自分のサイズに対する割合」で平行移動して描くウィジェットです。`Translation = new Offset(0.5, 0)` なら、子を自分の幅の半分だけ右へずらして描きます。レイアウトは変えず、描く位置とポインタが当たる位置だけを動かします。

作るファイルは次のとおりです。

| ファイル | 中身 |
|---|---|
| `src/FloatSoda/RenderObjects/Painting/RenderFractionalTranslation.cs` | 描画とヒットテストを担う RenderObject |
| `src/FloatSoda/Widgets/Paint/FractionalTranslation.cs` | RenderObject を作る Widget |
| `tests/FloatSoda.Test/Widgets/FractionalTranslationTest.cs` | 描画・ヒットテスト・差分更新のテスト |
| `samples/FloatSoda.Samples.FractionalTranslation/` | 層1サンプル |

## 前提

- リポジトリを clone し、`dotnet build` と `dotnet test` が通る状態になっている([CONTRIBUTING.md](../../../../CONTRIBUTING.md) の「開発環境のセットアップ」)
- Flutter のソースを読める(ローカルに clone していなければ、GitHub の `flutter/flutter` で同じファイルを開く)
- サンプルを表示して確かめるために、SteamVR を起動でき、HMD を接続している

三つのツリーの役割は、このチュートリアルでは一文ずつしか説明しません。詳しくは [三つのツリーの Explanation](Explanation.md) にあります。

## Step 1: Issue を立ててブランチを切る

新しい `public` なウィジェットを足すので、先に Issue を立てます。Issue が要る変更の範囲は CONTRIBUTING.md の「Issue が必須な変更」に、コアに受け入れるウィジェットの範囲は「受け入れ範囲」にあります。`FractionalTranslation` は単一の RenderObject の薄いラッパーなので、受け入れ範囲に入ります。

ブランチ名は `<Issue 番号>-<主な領域>-<内容>` の形にします(CONTRIBUTING.md の「ブランチ名」)。`git branch --show-current` で、作ったブランチ名が表示されれば完了です。

## Step 2: Flutter の実装とテストを読む

Flutter では、Widget が `packages/flutter/lib/src/widgets/basic.dart` の `FractionalTranslation`、RenderObject が `packages/flutter/lib/src/rendering/proxy_box.dart` の `RenderFractionalTranslation` です。エージェントで作業するときは、`flutter-widget-source` スキルでこの2つの場所を特定できます。手で探すときは、ローカル参照クローン(`~/code_reading/flutter_reference`)で次のように探します。

```bash
grep -rn "class RenderFractionalTranslation" ~/code_reading/flutter_reference/src/flutter/packages/flutter/lib/src/
```

`RenderFractionalTranslation` を読むと、次の5点が分かります。

- プロパティは `translation`(`Offset`)と `transformHitTests`(`bool`、既定は `true`)の2つ
- `translation` が変わると `markNeedsPaint` を呼び、レイアウトは要求しない
- `paint` は、子を `translation × 自分のサイズ` だけずらした位置に描く
- `hitTestChildren` は、`transformHitTests` が `true` のとき、同じ量だけずらした座標で子を調べる
- `hitTest` をオーバーライドし、自分の領域を確かめずに子だけを調べる(ずらした子が自分の領域の外へ出ても、子に当たるようにするため)

公式テストは `packages/flutter/test/widgets/basic_test.dart` の `group('FractionalTranslation', ...)` にあります。子が領域の中にある場合、一部がはみ出す場合、すべてはみ出す場合のヒットテストと、子のない場合に落ちないことを確かめています。

最後に、Flutter との差異の台帳([`known-divergences.md`](../../../../.agents/skills/floatsoda-device-test-gen/references/known-divergences.md))を開き、同じ仕組みに関わるエントリがないことを確かめます。

## Step 3: RenderObject を書く

`translation` は見た目だけを変えるので、描画系の `src/FloatSoda/RenderObjects/Painting/` に置きます。単一の子を持つので、`RenderProxyBox` を継承します。

```csharp
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Gesture;

namespace FloatSoda.RenderObjects.Painting;

/// <summary>子を、自分のサイズに対する割合で平行移動して描画するRenderObjectです。</summary>
/// <seealso cref="Widgets.Paint.FractionalTranslation"/>
public class RenderFractionalTranslation : RenderProxyBox
{
    private Offset _translation;

    /// <summary>子を移動する量を、自分の幅と高さに対する割合で取得または設定します。</summary>
    /// <remarks>値が変わった場合だけ、描画Dirtyにします。レイアウトは変わりません。</remarks>
    public Offset Translation
    {
        get => _translation;
        set
        {
            if (_translation == value) return;

            _translation = value;
            MarkNeedsPaint();
        }
    }

    /// <summary>ヒットテストにも移動を適用するかを取得または設定します。既定値は<see langword="true"/>です。</summary>
    public bool TransformHitTests { get; set; } = true;

    private Offset PaintOffset => new(Translation.X * Size.Width, Translation.Y * Size.Height);

    /// <summary>自分の領域を確かめずに、子だけをヒットテストします。</summary>
    /// <param name="result">ヒットした対象を追加する結果。</param>
    /// <param name="position">このRenderObjectのローカル座標における判定位置。</param>
    /// <returns>子がヒットした場合は<see langword="true"/>。</returns>
    public override bool HitTest(HitTestResult result, Offset position) => HitTestChildren(result, position);

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position) =>
        result.AddWidthPaintOffset(
            TransformHitTests ? PaintOffset : null,
            position,
            (testResult, transformed) => base.HitTestChildren(testResult, transformed));

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset)
    {
        if (Child is null) return;

        base.Paint(context, offset + PaintOffset);
    }
}
```

Flutter のコードとの違いは次の2点です。

- FloatSoda は Semantics を移植しないので、Flutter の setter にある `markNeedsSemanticsUpdate()` を削った([APIDesign](../../APIDesign.md) の「実装しない API — Semantics」)
- FloatSoda の `RenderObject` に対応するメソッドがないので、`applyPaintTransform` と `debugFillProperties` を移植しなかった

`Paint` と `HitTestChildren` が同じ `PaintOffset` を使っている点が要です。2つがずれると、見えている位置と押せる位置がずれます([三つのツリーの Reference](Reference.md))。

## Step 4: Widget を書く

Widget は、RenderObject を作る `CreateRenderObject` と、既存の RenderObject を書き換える `UpdateRenderObject` だけを持つ `record` です。描画系の Widget は `src/FloatSoda/Widgets/Paint/` に置きます。

```csharp
using FloatSoda.Abstractions.Geometries;
using FloatSoda.RenderObjects.Painting;

namespace FloatSoda.Widgets.Paint;

/// <summary>子を、自分のサイズに対する割合で平行移動して描画します。</summary>
/// <seealso cref="RenderFractionalTranslation"/>
public record FractionalTranslation : SingleChildRenderObjectWidget<RenderFractionalTranslation>
{
    /// <summary>子を移動する量を、自分の幅と高さに対する割合で取得します。</summary>
    public required Offset Translation { get; init; }

    /// <summary>ヒットテストにも移動を適用するかを取得します。既定値は<see langword="true"/>です。</summary>
    public bool TransformHitTests { get; init; } = true;

    /// <inheritdoc/>
    public override RenderFractionalTranslation CreateRenderObject() => new()
    {
        Translation = Translation,
        TransformHitTests = TransformHitTests,
    };

    /// <inheritdoc/>
    public override void UpdateRenderObject(RenderFractionalTranslation renderObject)
    {
        renderObject.Translation = Translation;
        renderObject.TransformHitTests = TransformHitTests;
    }
}
```

`Child` は基底の `SingleChildRenderObjectWidget<T>` が持つので、書きません。プロパティは `init` にし、必須のものに `required` を付けます([APIDesign](../../APIDesign.md) の「2.1 オブジェクト初期化子ファーストの原則」)。

`dotnet build` がエラーなしで通れば完了です。

## Step 5: テストを書く

Step 2 で読んだ Flutter の公式テストに対応するケースと、CONTRIBUTING.md の「テスト観点」のうち当てはまる観点を、テストにします。この題材で当てはまる観点と、対応するテストは次のとおりです。

| 観点 | テスト |
|---|---|
| 代表的な正常系 | 子を幅の半分だけ右に描く |
| 境界と退化ケース | 子がなくても例外を投げない(Flutter の `does not crash at zero area`) |
| 状態遷移 | `TransformHitTests` を `false` にすると、移動前の位置で当たる |
| 差分更新 | `Translation` を変えると再描画だけを要求し、同じ値なら何も要求しない |
| Flutter のヒットテストのケース | 自分の領域の外へ移動した子に当たる。移動前の位置では当たらない |

`tests/FloatSoda.Test/Widgets/FractionalTranslationTest.cs` を作ります。ヒットテストと差分更新は RenderObject を直接組み、描画は `WidgetBitmapRenderer` で確かめます。テストの組み立て方の選び方は、[三つのツリーの HowTo](HowTo.md) の「ヘッドレスでテストする」にあります。

```csharp
using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using FloatSoda.Geometrics;
using FloatSoda.Gesture;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Gesture;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Testing;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Test.Widgets;

public class FractionalTranslationTest
{
    private static readonly WidgetBitmapRenderer Renderer = new();

    // 100×100 の子を Listener で包み、RenderFractionalTranslation の下に置いて、レイアウトと描画まで済ませる
    private static (RenderView View, RenderPipeline Pipeline, RenderPointerListener Listener) BuildTree(
        Offset translation, bool transformHitTests = true)
    {
        var listener = new RenderPointerListener
        {
            Child = new RenderColoredBox
            {
                Child = new RenderConstrainedBox { AdditionalConstraints = BoxConstraints.Tight(100, 100) }
            }
        };
        var view = new RenderView
        {
            Child = new RenderFractionalTranslation
            {
                Translation = translation,
                TransformHitTests = transformHitTests,
                Child = listener,
            }
        };
        var pipeline = new RenderPipeline { OnNeedVisualUpdate = () => { }, RenderView = view };
        view.PrepareInitialFrame();
        pipeline.FlushLayout();
        pipeline.FlushPaint();

        return (view, pipeline, listener);
    }

    private static bool Hits(RenderView view, RenderPointerListener listener, Offset position)
    {
        var result = new HitTestResult();
        view.HitTest(result, position);
        return result.Path.Any(entry => ReferenceEquals(entry.Target, listener));
    }

    [Fact]
    public void Paint_幅の半分だけ右へ移動_子を右半分に描く()
    {
        var widget = new SizedBox
        {
            Width = 100,
            Height = 100,
            Child = new FractionalTranslation
            {
                Translation = new Offset(0.5, 0),
                Child = new ColoredBox { Color = SKColors.Blue },
            }
        };

        using var bitmap = Renderer.Render(widget, new SKSizeI(100, 100));

        Assert.NotEqual(SKColors.Blue, bitmap.GetPixel(25, 50));
        Assert.Equal(SKColors.Blue, bitmap.GetPixel(75, 50));
    }

    [Fact]
    public void HitTest_自分の領域の外へ移動した子を指す_子に当たる()
    {
        var (view, _, listener) = BuildTree(new Offset(1, 1));

        Assert.True(Hits(view, listener, new Offset(150, 150)));
    }

    [Fact]
    public void HitTest_移動前の位置を指す_子に当たらない()
    {
        var (view, _, listener) = BuildTree(new Offset(1, 1));

        Assert.False(Hits(view, listener, new Offset(50, 50)));
    }

    [Fact]
    public void HitTest_TransformHitTestsがfalse_移動前の位置で子に当たる()
    {
        var (view, _, listener) = BuildTree(new Offset(1, 1), transformHitTests: false);

        Assert.True(Hits(view, listener, new Offset(50, 50)));
    }

    [Fact]
    public void Translation_値を変える_再描画だけを要求する()
    {
        var (view, pipeline, _) = BuildTree(new Offset(0, 0));
        var translation = Assert.IsType<RenderFractionalTranslation>(view.Child);

        translation.Translation = new Offset(0.5, 0.5);

        Assert.Empty(pipeline.NodesNeedingLayout);
        Assert.NotEmpty(pipeline.NodesNeedingPaint);
    }

    [Fact]
    public void Translation_同じ値を設定_再描画を要求しない()
    {
        var (view, pipeline, _) = BuildTree(new Offset(0.5, 0.5));
        var translation = Assert.IsType<RenderFractionalTranslation>(view.Child);

        translation.Translation = new Offset(0.5, 0.5);

        Assert.Empty(pipeline.NodesNeedingPaint);
    }

    [Fact]
    public void Render_子がない_例外を投げない()
    {
        using var bitmap = Renderer.Render(
            new FractionalTranslation { Translation = new Offset(0.5, 0.5) },
            new SKSizeI(100, 100));

        Assert.NotNull(bitmap);
    }
}
```

```bash
dotnet test tests/FloatSoda.Test --filter "FullyQualifiedName~FractionalTranslationTest"
```

7件すべて合格すれば完了です。

## Step 6: 層1サンプルを足す

`samples/FloatSoda.Samples.FractionalTranslation/` を作り、`Program.cs`、`FractionalTranslationDemo.cs`、`README.md`、`checks/items.jsonl`、`FloatSoda.Samples.FractionalTranslation.csproj` を置きます。ファイルの役割、`--desktop` の切り替え、README の構成、禁止事項は、CONTRIBUTING.md の「サンプルを追加する場合の規約」に従います。同じ描画系の `samples/FloatSoda.Samples.Transform` が、構成の近い実例です。

`FractionalTranslationDemo.cs` には、目で見て区別できるケースを並べます。たとえば、移動量 `(0, 0)`、`(0.5, 0)`、`(1, 1)` の3つを枠の中に並べると、ずれ方の違いが一目で分かります。`checks/items.jsonl` は手で書かず、`floatsoda-device-test-gen` スキルで `FractionalTranslationDemo.cs` から生成します。README の `## Flutterとの違い` には、Step 3 で削ったメンバーのうち利用者から見えるものを書きます。見えるものがなければ「同等」と1行書きます。作ったプロジェクトは、`FloatSoda.slnx` の `<Folder Name="/samples/">` に登録します。

次のコマンドでウィンドウが開き、3つのケースが Demo に書いたとおりにずれて見えれば完了です。

```bash
dotnet run --project samples/FloatSoda.Samples.FractionalTranslation -- --desktop
```

> ※要確認 — この Step は、書き手が通して動かしていません。Step 3〜5 のコードは、ビルドとテストが通ることを確かめています。

## PR を出す

CONTRIBUTING.md の「ブランチ・PRフロー」にある、CI と同じ3つのコマンドを実行します。

```bash
dotnet build --configuration Release
dotnet test tests/FloatSoda.Rendering.Test --configuration Release --no-build
dotnet test tests/FloatSoda.Test --configuration Release --no-build
```

3つとも成功したら、PR の本文を [`.github/PULL_REQUEST_TEMPLATE.md`](../../../../.github/PULL_REQUEST_TEMPLATE.md) に沿って書きます。この題材で書く内容は次のとおりです。

| 欄 | 書く内容 |
|---|---|
| `## Flutter reference` | 参照した Flutter の commit、`widgets/basic.dart`、`rendering/proxy_box.dart`、`test/widgets/basic_test.dart` |
| `## Breaking changes` | 型を足しただけで、既存の API は変えていないこと |
| `## Tests` | 7件のテストと、Flutter の公式テストとの対応 |
| `## Documentation` | サンプルの README と、ウィジェットを説明する docs のページ |

新しい `public` なウィジェットを足す PR は、マージの前に docs を更新し、ジュニアコーダーテストを通します(CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」)。PR を出す前の確認項目は、CONTRIBUTING.md の「PRを出す前のチェックリスト」にあります。

## 次に読むもの

- [三つのツリーの HowTo](HowTo.md) — 2つ目のウィジェットを移植するときの手順と規約
- [三つのツリーの Reference](Reference.md) — RenderObject とツリーで守る条件
- [三つのツリーの Explanation](Explanation.md) — Widget の変更が描画とヒットテストに届くまでの仕組み
