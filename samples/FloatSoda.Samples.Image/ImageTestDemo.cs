using FloatSoda.Core.Providers;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.Rendering.Layers;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using ImageWidget = FloatSoda.Widgets.Paint.Image;

namespace FloatSoda.Samples.Image;

/// <summary>画像の読み込み、拡縮、切り抜き、および子要素の重ね描きを実機で確認するサンプルです。</summary>
public sealed record ImageTestDemo : StatelessWidget
{
    private const double CardWidth = 320;
    private const double ImageHeight = 220;

    /// <summary>表示する画像ファイルの絶対パスを取得します。</summary>
    public required string ImagePath { get; init; }

    /// <inheritdoc />
    public override Widget Build(IBuildContext context)
    {
        var provider = new FileImageProvider(ImagePath);

        return new SizedBox
        {
            Width = 1100,
            Height = 620,
            Child = new ColoredBox
            {
                Color = new Color(16, 20, 31),
                Child = new Padding
                {
                    Spacing = EdgeInsets.All(36),
                    Child = new Column
                    {
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Children =
                        {
                            Label("Image / FileImageProvider", 42, new Color(244, 247, 255), 700),
                            new SizedBox { Height = 10 },
                            Label("同じ 1000 x 1000 PNG を3通りで表示", 24, new Color(169, 180, 204)),
                            new SizedBox { Height = 34 },
                            new Row
                            {
                                MainAxisSize = MainAxisSize.Min,
                                CrossAxisAlignment = CrossAxisAlignment.Start,
                                Children =
                                {
                                    BuildCard("DIRECT", "320 x 220 に引き伸ばし", BuildDirect(provider)),
                                    new SizedBox { Width = 28 },
                                    BuildCard("CONTAIN", "比率を維持して全体表示", BuildFitted(provider, BoxFit.Contain)),
                                    new SizedBox { Width = 28 },
                                    BuildCard("COVER", "比率を維持して中央を切り抜き", BuildFitted(provider, BoxFit.Cover)),
                                }
                            },
                            new SizedBox { Height = 30 },
                            Label(
                                "確認: 3枚が表示される / 色が一致する / DIRECTだけ横長 / COVERは上下が切れる",
                                20,
                                new Color(124, 205, 255))
                        }
                    }
                }
            }
        };
    }

    private Widget BuildDirect(ImageProvider provider) => new SizedBox
    {
        Width = CardWidth,
        Height = ImageHeight,
        Child = new ImageWidget
        {
            Provider = provider,
            // 既定のBoxFit.Containでは縦横比が維持され、CONTAINカードと見分けがつかない。
            // このカードは「領域いっぱいへ引き伸ばす」比較対象なのでFillを明示する。
            Fit = BoxFit.Fill,
            Child = new Center
            {
                Child = Label("CHILD ON TOP", 20, new Color(255, 111, 97), 700)
            }
        }
    };

    private static Widget BuildFitted(ImageProvider provider, BoxFit fit) => new SizedBox
    {
        Width = CardWidth,
        Height = ImageHeight,
        Child = new ColoredBox
        {
            Color = new Color(40, 47, 64),
            Child = new FittedBox
            {
                Fit = fit,
                ClipBehavior = Clip.HardEdge,
                Child = new ImageWidget { Provider = provider }
            }
        }
    };

    private static Widget BuildCard(string title, string description, Widget image) => new Column
    {
        MainAxisSize = MainAxisSize.Min,
        CrossAxisAlignment = CrossAxisAlignment.Center,
        Children =
        {
            Label(title, 26, new Color(244, 247, 255), 700),
            new SizedBox { Height = 8 },
            image,
            new SizedBox { Height = 10 },
            Label(description, 18, new Color(169, 180, 204))
        }
    };

    private static Text Label(string text, double size, Color color, int weight = 400) => new(text)
    {
        Style = new TextStyle
        {
            FontSize = size,
            Color = color,
            FontWeight = weight
        }
    };
}
