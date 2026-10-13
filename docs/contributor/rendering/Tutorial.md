# 描画: 貢献までの流れ

このページでは、描画の層(`FloatSoda.Rendering`)に Layer の種類を1つ足して、PR を出すまでを一本道で進めます。題材は、子の描画結果にシェーダーをマスクとして重ねる `ShaderMaskLayer` です。[#154](https://github.com/sumx21t-3310/FloatSoda/issues/154)(ShaderMask を実装する)が対象に挙げている Layer で、Widget と RenderObject を待たずに単独で作れます。

## 作るもの

最後には、2つのファイルを足したブランチと、それを出した PR ができます。1つはシェーダー、マスクの矩形、ブレンドモードを持つ Layer(`src/FloatSoda.Rendering/Layers/ShaderMaskLayer.cs`)で、もう1つは描画結果と `Clone()` を確かめるテスト(`tests/FloatSoda.Rendering.Test/Layers/ShaderMaskLayerTest.cs`)です。

左から右へ透明になるグラデーションをマスクにすると、赤い四角の右端が透明になる、という結果をテストで確かめます。

## 前提

- .NET 10 SDK が入っていて、リポジトリをクローン済みであること
- `dotnet test tests/FloatSoda.Rendering.Test` が手元で通ること
- [描画: 仕組みと理由](Explanation.md) の「なぜ Clone してからレンダースレッドへ渡すのか」を読んでいること

SteamVR と HMD は要りません。この Tutorial のテストは、CPU 上のビットマップへ描いて確かめます。

## Step 1: Issue を読み、ブランチを切る

[#154](https://github.com/sumx21t-3310/FloatSoda/issues/154) を開き、「対象」と「完了条件」を読みます。この Tutorial で作るのは、対象のうち「`ShaderMaskLayer` または同等の Layer ツリー処理」の部分です。

ブランチ名は、Issue 番号と、Issue の `area:*` ラベルから決めます([CONTRIBUTING.md](../../../CONTRIBUTING.md) の「ブランチ名」)。#154 のラベルは `area:widget` です。

```bash
git switch main
git pull
git switch -c 154-widget-shader-mask-layer
```

`git branch --show-current` が `154-widget-shader-mask-layer` を返せば、このステップは終わりです。

## Step 2: Flutter の ShaderMaskLayer を読む

FloatSoda の Layer は、名前とプロパティを Flutter に合わせます。Flutter の [`ShaderMaskLayer`](https://github.com/flutter/flutter/blob/8a9f61cfd67396fb2f9afc3cd7854035e9cd6fc2/packages/flutter/lib/src/rendering/layer.dart#L2218) を開き、次の3点を確かめます。

- プロパティは `shader`、`maskRect`、`blendMode` の3つである
- シェーダーの原点は `maskRect` の左上である
- `maskRect` は子の位置を動かさない

## Step 3: Layer を書く

`src/FloatSoda.Rendering/Layers/ShaderMaskLayer.cs` を作り、次の内容を書きます。`Paint` は `SaveLayer` で子を一時レイヤーへ描き、その上にシェーダーを塗った矩形を重ねてから `Restore` で描画先へ戻します。

```csharp
using SkiaSharp;

namespace FloatSoda.Rendering.Layers;

/// <summary>
/// 子レイヤーの描画結果へシェーダーをマスクとして合成するレイヤーです。
/// </summary>
public class ShaderMaskLayer : ContainerLayer
{
    /// <summary>
    /// マスクとして合成するシェーダーを取得または設定します。
    /// </summary>
    /// <value>
    /// マスクのシェーダー。<see langword="null"/>の場合はマスクせずに子レイヤーを描画します。
    /// 所有権はこのレイヤーへ移転せず、使用中は呼び出し元が有効な状態に保つ必要があります。
    /// </value>
    public SKShader? Shader { get; set; }

    /// <summary>
    /// シェーダーを適用する矩形を取得または設定します。
    /// </summary>
    /// <value>
    /// 親レイヤーの座標系で表した矩形。シェーダーの原点はこの矩形の左上になります。
    /// </value>
    public SKRect MaskRect { get; set; }

    /// <summary>
    /// シェーダーと子レイヤーの描画結果を合成する方法を取得または設定します。
    /// </summary>
    /// <value>
    /// 合成に使用するブレンドモード。既定値は<see cref="SKBlendMode.Modulate"/>です。
    /// </value>
    public SKBlendMode BlendMode { get; set; } = SKBlendMode.Modulate;

    /// <summary>
    /// 子レイヤーを一時レイヤーへ描画し、シェーダーを合成してから描画先へ戻します。
    /// </summary>
    /// <param name="context">
    /// 子レイヤーの合成先となるキャンバスを保持するコンテキスト。
    /// </param>
    public override void Paint(LayerContext context)
    {
        if (Shader is null)
        {
            base.Paint(context);
            return;
        }

        context.Canvas.SaveLayer();

        base.Paint(context);

        using var paint = new SKPaint { Shader = Shader, BlendMode = BlendMode };
        context.Canvas.Translate(MaskRect.Left, MaskRect.Top);
        context.Canvas.DrawRect(SKRect.Create(MaskRect.Width, MaskRect.Height), paint);

        context.Canvas.Restore();
    }

    /// <summary>
    /// シェーダー、マスク矩形、ブレンドモード、および子レイヤーを保持する新しいレイヤーを作成します。
    /// </summary>
    /// <returns>
    /// 子レイヤーを再帰的に複製した新しいシェーダーマスクレイヤー。
    /// </returns>
    /// <remarks>
    /// シェーダーの所有権は移転せず、複製元と同じシェーダーを参照します。
    /// </remarks>
    public override ILayer Clone()
    {
        var cloned = new ShaderMaskLayer
        {
            Shader = Shader,
            MaskRect = MaskRect,
            BlendMode = BlendMode
        };

        foreach (var child in Children)
        {
            cloned.Children.Add(child.Clone());
        }

        return cloned;
    }
}
```

既定のブレンドモードは、Flutter の `ShaderMask` Widget の既定値(`BlendMode.modulate`)に合わせています。`Shader` が `null` のときに子だけを描く点は、Flutter と違います。Flutter は `shader` が `null` のまま合成すると assert で止めます。この違いを残すかどうかは PR で決め、残すなら [APIDesign](../APIDesign.md) の記録ルールに従います。

`dotnet build src/FloatSoda.Rendering` が成功すれば、このステップは終わりです。

## Step 4: テストを書く

`tests/FloatSoda.Rendering.Test/Layers/ShaderMaskLayerTest.cs` を作ります。テスト名は `MemberName_条件_期待結果` の形にします(CONTRIBUTING.md の「テストの命名」)。描画結果は `LayerBitmapRenderer` でビットマップへ描き、ピクセルの不透明度で判定します。

```csharp
using FloatSoda.Rendering.Layers;
using SkiaSharp;

namespace FloatSoda.Rendering.Test.Layers;

public class ShaderMaskLayerTest
{
    private static readonly LayerBitmapRenderer Renderer = new();
    private static readonly SKSizeI Size = new(100, 100);

    private static PictureLayer MakeRedPicture()
    {
        var rect = SKRect.Create(0, 0, 100, 100);
        using var recorder = new SKPictureRecorder();
        var canvas = recorder.BeginRecording(rect);
        using var paint = new SKPaint { Color = SKColors.Red };
        canvas.DrawRect(rect, paint);
        return new PictureLayer { Picture = recorder.EndRecording() };
    }

    private static SKShader MakeFadeShader() => SKShader.CreateLinearGradient(
        new SKPoint(0, 0),
        new SKPoint(100, 0),
        [SKColors.White, SKColors.Transparent],
        SKShaderTileMode.Clamp);

    [Fact]
    public void Paint_左から右へ透明になるシェーダー_右端ほど透明になる()
    {
        using var shader = MakeFadeShader();
        var layer = new ShaderMaskLayer
        {
            Shader = shader,
            MaskRect = SKRect.Create(0, 0, 100, 100),
            BlendMode = SKBlendMode.DstIn
        };
        layer.Children.Add(MakeRedPicture());

        using var bitmap = Renderer.Render(layer, Size);

        Assert.True(bitmap.GetPixel(5, 50).Alpha > 200);
        Assert.True(bitmap.GetPixel(95, 50).Alpha < 30);
    }

    [Fact]
    public void Paint_Shaderがnull_子をそのまま描画する()
    {
        var layer = new ShaderMaskLayer { MaskRect = SKRect.Create(0, 0, 100, 100) };
        layer.Children.Add(MakeRedPicture());

        using var bitmap = Renderer.Render(layer, Size);

        Assert.Equal(SKColors.Red, bitmap.GetPixel(95, 50));
    }

    [Fact]
    public void Clone_設定済みのレイヤー_すべてのプロパティと子を複製する()
    {
        using var shader = MakeFadeShader();
        var layer = new ShaderMaskLayer
        {
            Shader = shader,
            MaskRect = SKRect.Create(10, 20, 30, 40),
            BlendMode = SKBlendMode.DstIn
        };
        layer.Children.Add(MakeRedPicture());

        var cloned = (ShaderMaskLayer)layer.Clone();

        Assert.Same(shader, cloned.Shader);
        Assert.Equal(layer.MaskRect, cloned.MaskRect);
        Assert.Equal(SKBlendMode.DstIn, cloned.BlendMode);
        Assert.Single(cloned.Children);
        Assert.NotSame(layer.Children[0], cloned.Children[0]);
    }
}
```

テストを実行します。

```bash
dotnet test tests/FloatSoda.Rendering.Test
```

出力の最後の行で、失敗の数が 0 であれば、このステップは終わりです。

## Step 5: Clone の写し漏れでテストが落ちることを確かめる

レンダースレッドへ渡るのは Clone した Layer なので、`Clone()` の写し漏れはそのまま画面に出ます。テストがその漏れを捕まえることを確かめます。

1. `Clone()` の中の `BlendMode = BlendMode` の行を消し、その上の行の末尾のカンマも消す
2. `dotnet test tests/FloatSoda.Rendering.Test` を実行する
3. `Clone_設定済みのレイヤー_すべてのプロパティと子を複製する` が `Assert.Equal() Failure` で落ちることを確かめる
4. 消した行を戻し、もう一度実行して失敗が 0 になることを確かめる

## PR を出す

PR を出す前に、CI と同じコマンドを手元で実行します。コマンドは [CONTRIBUTING.md](../../../CONTRIBUTING.md) の「ブランチ・PRフロー」にあります。3つのコマンドがすべて失敗 0 で終われば、PR を出せます。

PR の本文は `.github/PULL_REQUEST_TEMPLATE.md` に沿って書きます。この PR では、次の3点を本文に書きます。

- #154 のうち Layer の部分だけを扱い、Widget と RenderObject は含めないこと(CONTRIBUTING.md の「PR のスコープ」)
- breaking change の判定(新しい型を足すだけで既存の型と挙動は変えていないこと、基準は [APIDesign](../APIDesign.md) の 6 章)
- `## Documentation` 節の判断([WritingDocumentation](../WritingDocumentation.md) の「3. Documentation Impact Matrix」で決める)

`ShaderMaskLayer` は `public` の型です。CONTRIBUTING.md の「新しい public API を追加する場合の受け入れ条件」は、ウィジェットや `*Style` のように、アプリを作る人が使う API を例に挙げています。Layer を足す PR でジュニアコーダーテストが要るかは ※要確認 です。

レビューでは、REVIEW.md の「Layer」に沿って、`Clone()` がすべてのプロパティを写しているかと、`SKShader` の所有権が XML ドキュメントコメントに書かれているかが確認されます。

## 次に読むもの

- [描画: 作業の手順](HowTo.md) — Layer を足す手順の規約と、`PaintingContext` へ `Push*` を足すとき
- [描画: 守ること](Reference.md) — Layer の不変条件と、まだテストで固定していない条件
- [描画: 仕組みと理由](Explanation.md) — Layer ツリーがレンダースレッドで描かれるまで
