using FloatSoda.Rendering.Layers;
using FloatSoda.Rendering;
using FloatSoda.Testing;
using FloatSoda.Geometrics;
using FloatSoda.RenderObjects;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var layerRenderer = new LayerBitmapRenderer();
var renderObjectRenderer = new RenderObjectBitmapRenderer();
var widgetRenderer = new WidgetBitmapRenderer();

var imageSize = new SKSizeI(1000, 1000);

var layerTree = CreateLayerTree(imageSize.Width, imageSize.Height);

var savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

layerRenderer.Render(layerTree, imageSize).Save(Path.Combine(savePath, "layer_tree_output.png"));

var renderTree = CreateRenderObject(imageSize.Width, imageSize.Width);

renderObjectRenderer.Render(renderTree, imageSize).Save(Path.Combine(savePath, "render_tree_output.png"));

var widgetTree = CreateWidgetTree(imageSize.Width, imageSize.Height);
widgetRenderer.Render(widgetTree, imageSize).Save(Path.Combine(savePath, "widget_tree_output.png"));

return;

ILayer CreateLayerTree(float width, float height)
{
    var root = new ContainerLayer();

    var rect = SKRect.Create(0, 0, width, height);
    var leaf = new PictureLayer();
    var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(rect);

    canvas.DrawRect(rect, new SKPaint { Color = SKColors.WhiteSmoke });

    var parentSize = new SKSize(width, height);
    var childSize = new SKSize(width / 2, height / 2);
    var location = new Alignment().ComputeOffset(parentSize, childSize);

    canvas.DrawRect(SKRect.Create(location, childSize), new SKPaint { Color = SKColors.DarkSeaGreen });

    leaf.Picture = recorder.EndRecording();

    root.Children.Add(leaf);

    return root;
}


RenderBox CreateRenderObject(float width, float height)
{
    return new RenderPositionedBox
    {
        Alignment = Alignment.Center,
        Child = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(width, height),
            Child = new RenderColoredBox
            {
                Color = SKColors.WhiteSmoke,
                Child = new RenderFlex
                {
                    MainAxisSize = MainAxisSize.Min,
                    MainAxisAlignment = MainAxisAlignment.Center,
                    Children =
                    {
                        new RenderClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(20),
                            Child = new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(width / 4,
                                    height / 4),
                                Child = new RenderColoredBox
                                {
                                    Color = SKColors.DarkSeaGreen
                                }
                            }
                        },
                        new RenderClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(20),
                            Child = new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(width / 4,
                                    height / 4),
                                Child = new RenderColoredBox
                                {
                                    Color = SKColors.Tomato
                                }
                            }
                        },
                        new RenderClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(20),
                            Child = new RenderConstrainedBox
                            {
                                AdditionalConstraints = BoxConstraints.Tight(width / 4,
                                    height / 4),
                                Child = new RenderColoredBox
                                {
                                    Color = SKColors.CornflowerBlue
                                }
                            }
                        }
                    }
                },
            },
        }
    };
}

Widget CreateWidgetTree(double width, double height)
{
    return new Align
    {
        Alignment = Alignment.Center,
        Child = new SizedBox
        {
            Width = width,
            Height = height,
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
                                Width = width / 4,
                                Height = height / 4
                            }
                        },
                        new ClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(20),
                            Child = new SizedBox
                            {
                                Child = new ColoredBox { Color = SKColors.Tomato },
                                Width = width / 4,
                                Height = height / 4
                            },
                        },
                        new ClipRoundRect
                        {
                            BorderRadius = BorderRadius.Circular(20),
                            Child = new SizedBox
                            {
                                Child = new ColoredBox { Color = SKColors.CornflowerBlue },
                                Width = width / 4,
                                Height = height / 4
                            },
                        },
                    }
                }
            }
        }
    };
}
