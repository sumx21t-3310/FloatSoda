using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

namespace FloatSoda.Samples.OverlayApp;

public record StackWidget : StatelessWidget
{
    public double Width { get; init; } = 1000;
    public double Height { get; init; } = 1000;

    public override Widget Build(IBuildContext context)

    {
        return new Align
        {
            Alignment = Alignment.Center,
            Child = new SizedBox
            {
                Width = Width,
                Height = Height,
                Child = new ColoredBox
                {
                    Color = SKColors.WhiteSmoke,
                    Child = new Flex
                    {
                        MainAxisAlignment = MainAxisAlignment.Center,
                        CrossAxisAlignment = CrossAxisAlignment.Center,
                        Children =
                        {
                            new ClipRoundRect
                            {
                                BorderRadius = BorderRadius.Circular(20),
                                Child = new SizedBox
                                {
                                    Child = new ColoredBox { Color = SKColors.DarkSeaGreen },
                                    Width = Width / 4,
                                    Height = Height / 4
                                }
                            },
                            new ClipRoundRect
                            {
                                BorderRadius = BorderRadius.Circular(20),
                                Child = new SizedBox
                                {
                                    Child = new ColoredBox { Color = SKColors.Tomato },
                                    Width = Width / 4,
                                    Height = Height / 4
                                },
                            },
                            new ClipRoundRect
                            {
                                BorderRadius = BorderRadius.Circular(20),
                                Child = new SizedBox
                                {
                                    Child = new ColoredBox { Color = SKColors.CornflowerBlue },
                                    Width = Width / 4,
                                    Height = Height / 4
                                },
                            },
                        }
                    }
                }
            }
        };
    }
}