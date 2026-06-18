using FloatSoda.Common.Layer;
using FloatSoda.Geometrics;
using FloatSoda.Render;
using FloatSoda.Render.Layout;
using FloatSoda.Render.Painting;
using FloatSoda.Samples.PaintingSample;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;
using SkiaSharp;

var imageRenderer = new ImageRenderer();

var imageSize = new SKSizeI(1000, 1000);

var layerTree = CreateLayerTree(imageSize.Width, imageSize.Height);

var savePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

imageRenderer.RenderLayerTree(layerTree, imageSize, Path.Combine(savePath, "layer_tree_output.png"));

var renderTree = CreateRenderObject(imageSize.Width, imageSize.Width);

imageRenderer.RenderObjectTree(renderTree, imageSize, Path.Combine(savePath, "render_tree_output.png"));

var widgetTree = CreateWidgetTree(imageSize.Width, imageSize.Height);
imageRenderer.RenderWidgetTree(widgetTree, imageSize, Path.Combine(savePath, "widget_tree_output.png"));

return;

ILayer CreateLayerTree(float width, float height)
{
    var root = new ContainerLayer();

    var rect = SKRect.Create(0, 0, width, height);
    var leaf = new PictureLayer();
    var recorder = new SKPictureRecorder();
    var canvas = recorder.BeginRecording(rect);

    var paint = new SKPaint
    {
        Color = SKColors.Tomato
    };

    var alignment = new Alignment();

    var parentSize = new SKSize(width, height);
    var childSize = new SKSize(width / 4, height / 4);

    var location = alignment.ComputeOffset(parentSize, childSize);

    canvas.DrawRect(SKRect.Create(location, parentSize), paint);

    leaf.Picture = recorder.EndRecording();

    root.Children.Add(leaf);

    return root;
}


RenderBox CreateRenderObject(float width, float height)
{
    return new RenderPositionedBox
    {
        Child = new RenderConstrainedBox
        {
            AdditionalConstraints = BoxConstraints.Tight(width, height),
            Child = new RenderColoredBox
            {
                Color = SKColors.AliceBlue,
                Child = new RenderPositionedBox
                {
                    Child = new RenderConstrainedBox
                    {
                        AdditionalConstraints = BoxConstraints.Tight(width / 2, height / 2),
                        Child = new RenderColoredBox
                        {
                            Color = SKColors.Tomato
                        }
                    }
                }
            }
        }
    };
}

Widget CreateWidgetTree(double width, double height)
{
    return new Align
    {
        Child = new SizedBox
        {
            Width = width,
            Height = height,
            Child = new ColoredBox
            {
                Color = SKColors.AliceBlue,
                Child = new Align
                {
                    Child = new SizedBox
                    {
                        Width = width / 2,
                        Height = height / 2,
                        Child = new ColoredBox
                        {
                            Color = SKColors.CornflowerBlue
                        }
                    }
                }
            }
        }
    };
}