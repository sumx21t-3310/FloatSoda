using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Rendering.Layers;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ConstraintsTransformBoxWidget = FloatSoda.Widgets.Layout.ConstraintsTransformBox;

namespace FloatSoda.Samples.ConstraintsTransformBox;

/// <summary>制約の取り除き方と、軸の維持・クリップ・カスタム変換を確認するサンプルです。</summary>
public sealed record ConstraintsTransformBoxDemo : StatelessWidget
{
    /// <inheritdoc />
    public override Widget Build(IBuildContext context) => new SizedBox
    {
        Width = 840,
        Height = 280,
        Child = new ColoredBox
        {
            Color = new Color(16, 20, 31),
            Child = new Padding
            {
                Spacing = EdgeInsets.All(32),
                Child = new Row
                {
                    Children =
                    {
                        // ConstrainedAxis = Horizontal は幅の制約だけを残す。
                        // 幅220の要求は枠の幅150へ収められる。
                        Cell("幅の制約は残す", new UnconstrainedBox
                        {
                            ConstrainedAxis = Axis.Horizontal,
                            Child = Bar(new Color(255, 111, 97), 220, 40)
                        }),
                        new SizedBox { Width = 24 },

                        // ClipBehavior を指定すると、はみ出しを枠で切り取れる。
                        Cell("HardEdge でクリップ", new UnconstrainedBox
                        {
                            ClipBehavior = Clip.HardEdge,
                            Child = Bar(new Color(255, 209, 102), 220, 40)
                        }),
                        new SizedBox { Width = 24 },

                        // ConstraintsTransformBox には任意の変換を渡せる。
                        // ここでは最大寸法を100へ差し替え、幅220の要求を100で止める。
                        Cell("カスタム変換", new ConstraintsTransformBoxWidget
                        {
                            ConstraintsTransform = Cap100,
                            Child = Bar(new Color(124, 205, 255), 220, 220)
                        }),
                        new SizedBox { Width = 24 },

                        // UnconstrainedBox は両軸の制約を取り除き、子を自然な大きさにする。
                        // 幅220の子は枠からはみ出し、既定(Clip.None)ではそのまま見える。
                        Cell("自然な大きさ", new UnconstrainedBox
                        {
                            Child = Bar(new Color(124, 205, 255), 220, 40)
                        })
                    }
                }
            }
        }
    };

    /// <summary>最大幅と最大高さを100へ差し替えるカスタム変換。</summary>
    private static BoxConstraints Cap100(BoxConstraints constraints) =>
        new(MaxWidth: 100, MaxHeight: 100);

    /// <summary>ラベル付きの枠を作り、その中で制約変換の効果を見せる。</summary>
    private static Widget Cell(string label, Widget child) => new Column
    {
        MainAxisSize = MainAxisSize.Min,
        Children =
        {
            new ColoredBox
            {
                Color = new Color(40, 47, 64),
                Child = new SizedBox
                {
                    Width = 150,
                    Height = 150,
                    Child = child
                }
            },
            new SizedBox { Height = 10 },
            new Text(label)
            {
                Style = new TextStyle { FontSize = 18, Color = new Color(169, 180, 204) }
            }
        }
    };

    /// <summary>固定寸法を要求する帯。</summary>
    private static Widget Bar(Color color, double width, double height) => new ColoredBox
    {
        Color = color,
        Child = new SizedBox { Width = width, Height = height }
    };
}
