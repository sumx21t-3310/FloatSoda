using FloatSoda.Abstractions.Geometries;
using FloatSoda.Core;
using SkiaSharp;
using Topten.RichTextKit;
using HitTestResult = FloatSoda.Gesture.HitTestResult;

namespace FloatSoda.RenderObjects;

/// <summary>
/// 書式付きテキストを計測して描画するRenderObjectです。
/// </summary>
public class RenderParagraph : RenderBox, IHasMultiChildrenRenderObject
{
    /// <summary>
    /// テキスト内の埋め込み要素に対応する子のコレクションを取得します。
    /// </summary>
    public MultiChildrenCollection<RenderBox> Children { get; }

    void IHasMultiChildrenRenderObject.AddChild(RenderObject child) => Children.AddErased(child);

    bool IHasMultiChildrenRenderObject.RemoveChild(RenderObject child) => Children.RemoveErased(child);

    /// <inheritdoc/>
    public override void SetupParentData(RenderObject child) => child.ParentData = new BoxParentData();

    /// <inheritdoc/>
    public override void Attach(RenderPipeline? owner)
    {
        base.Attach(owner);
        Children.Attach(owner);
    }

    /// <inheritdoc/>
    public override void Detach()
    {
        base.Detach();
        Children.Detach();
    }

    /// <inheritdoc/>
    public override void VisitChildren(Action<RenderObject> visitor) => Children.VisitChildren(visitor);

    /// <inheritdoc/>
    public override void RedepthChildren() => VisitChildren(RedepthChild);

    /// <summary>
    /// 計測および描画する書式付きテキストを取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、内部の計測結果を破棄し、このRenderObjectをLayout Dirtyとしてマークします。
    /// 次のパイプライン更新時にテキストのサイズを再計算し、必要に応じて再描画します。
    /// 値が変更されなかった場合、Dirty状態は変更されません。
    /// </remarks>
    public required TextSpan Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _textPainter.Text = value;
            MarkNeedsLayout();
        }
    }

    private readonly TextPainter _textPainter = new();

    /// <summary>
    /// 子を持たない段落描画用RenderObjectを初期化します。
    /// </summary>
    public RenderParagraph()
    {
        Children = new MultiChildrenCollection<RenderBox>(this);
    }

    /// <inheritdoc/>
    public override void PerformLayout()
    {
        _textPainter.Layout(Constraints.MinWidth, Constraints.MaxWidth);
        Size = Constraints.Constrain(_textPainter.Size);
    }

    /// <inheritdoc/>
    public override void Paint(PaintingContext context, Offset offset) => _textPainter.Paint(context.Canvas, offset);

    /// <inheritdoc/>
    public override bool HitTestSelf(Offset position) => true;

    /// <inheritdoc/>
    public override bool HitTestChildren(HitTestResult result, Offset position) => false;
}

/// <summary>
/// 文字列とその書式を表すテキスト範囲です。
/// </summary>
/// <param name="Text">この範囲に含める文字列。</param>
public record TextSpan(string Text)
{
    /// <summary>
    /// この範囲へ適用する書式を取得します。
    /// </summary>
    /// <remarks>
    /// <c>null</c>の場合、構築時に渡された既定の書式を使用します。
    /// </remarks>
    public Style? Style { get; init; }

    /// <summary>
    /// この範囲の文字列を書式付きテキストブロックへ追加します。
    /// </summary>
    /// <param name="textBlock">文字列を追加するテキストブロック。</param>
    /// <param name="defaultStyle"><see cref="Style"/>が<c>null</c>の場合に使用する書式。</param>
    public void Build(TextBlock textBlock, Style defaultStyle) => textBlock.AddText(Text, Style ?? defaultStyle);
}

/// <summary>
/// 書式付きテキストの計測結果を保持し、レイアウトと描画を行います。
/// </summary>
public class TextPainter
{
    /// <summary>
    /// 計測および描画する書式付きテキストを取得または設定します。
    /// </summary>
    /// <remarks>
    /// 値が変更された場合、保持している計測結果を破棄します。
    /// 次に<see cref="Layout"/>を呼び出したとき、テキストブロックが再構築されます。
    /// 値が変更されなかった場合、保持している計測結果は変更されません。
    /// </remarks>
    public TextSpan? Text
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            MarkNeedsLayout();
        }
    }

    private TextBlock? _textBlock;

    /// <summary>
    /// 最後にレイアウトしたテキストのサイズを取得します。
    /// </summary>
    public SKSize Size => new((float)Width, (float)Height);

    /// <summary>
    /// 最後にレイアウトしたテキストの幅を取得します。
    /// </summary>
    public double Width => _textBlock?.MeasuredWidth ?? 0;

    /// <summary>
    /// 最後にレイアウトしたテキストの高さを取得します。
    /// </summary>
    public double Height => _textBlock?.MeasuredHeight ?? 0;

    private void MarkNeedsLayout() => _textBlock = null;

    private Style CreateDefaultStyle() => new()
    {
        FontSize = 30,
        TextColor = SKColors.Black
    };

    private TextBlock CreateTextBlock()
    {
        var textBlock = new TextBlock();
        Text?.Build(textBlock, CreateDefaultStyle());
        return textBlock;
    }


    /// <summary>
    /// 指定した幅の範囲でテキストを計測します。
    /// </summary>
    /// <param name="minWidth">最小幅。現在の実装では計測に使用されません。</param>
    /// <param name="maxWidth">改行に使用する最大幅。</param>
    public void Layout(double minWidth, double maxWidth)
    {
        _textBlock ??= CreateTextBlock();

        _textBlock.MaxWidth = (float)maxWidth;
        _textBlock.Layout();
    }


    /// <summary>
    /// 最後にレイアウトしたテキストを指定位置へ描画します。
    /// </summary>
    /// <param name="canvas">描画先のキャンバス。</param>
    /// <param name="offset">キャンバス上の描画開始位置。</param>
    public void Paint(SKCanvas canvas, Offset offset) => _textBlock?.Paint(canvas, offset);
}
