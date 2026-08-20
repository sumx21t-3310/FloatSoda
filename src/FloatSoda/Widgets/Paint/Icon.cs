using FloatSoda.Core;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Widgets.Paint;

/// <summary>アイコンフォントのグリフを正方形の領域へ表示します。</summary>
/// <param name="Data">表示するアイコンのコードポイントとフォント。</param>
public sealed record Icon(IconData Data) : StatelessWidget
{
    /// <summary>表示するアイコンのコードポイントとフォントを取得します。</summary>
    /// <exception cref="ArgumentNullException">値が<see langword="null"/>です。</exception>
    public IconData Data { get; init; } = Data ?? throw new ArgumentNullException(nameof(Data));

    /// <summary>アイコンの一辺の長さを取得します。</summary>
    public double Size { get; init; } = 24;

    /// <summary>アイコンの色を取得します。</summary>
    public Color Color { get; init; } = new Color(0, 0, 0);

    /// <inheritdoc/>
    public override Widget Build(IBuildContext context)
    {
        return new SizedBox
        {
            Width = Size,
            Height = Size,
            Child = new Center
            {
                Child = new RichText
                {
                    Text = new TextSpan(char.ConvertFromUtf32(Data.CodePoint))
                    {
                        Style = new TextStyle
                        {
                            Font = Data.Font,
                            FontSize = Size,
                            Color = Color
                        }
                    }
                }
            }
        };
    }
}
