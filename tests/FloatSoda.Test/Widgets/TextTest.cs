using System.Reflection;
using FloatSoda.Core;
using FloatSoda.Core.Providers;
using FloatSoda.Elements;
using FloatSoda.Geometrics;
using FloatSoda.Painting;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Test.Widgets;

public class TextTest
{
    private static (
        RenderPipeline Pipeline,
        RenderView View,
        RenderObjectToWidgetElement<RenderView> Root) Mount(Widget widget)
    {
        var view = new RenderView();
        var pipeline = new RenderPipeline
        {
            OnNeedVisualUpdate = () => { },
            RenderView = view
        };
        view.PrepareInitialFrame();

        var root = new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = widget
        }.AttachToRenderTree(new BuildOwner(() => { }), null);

        return (pipeline, view, root);
    }

    [Fact]
    public void Build_DataとStyleを指定_RichTextとTextSpanへ委譲する()
    {
        var style = new TextStyle
        {
            FontSize = 24,
            Color = new Color(10, 20, 30),
            Font = new SystemFontProvider("Meiryo"),
            FontWeight = 700,
            IsItalic = true
        };

        var richText = Assert.IsType<RichText>(new Text("FloatSoda") { Style = style }.Build(null!));

        Assert.Equal("FloatSoda", richText.Text.Text);
        Assert.Same(style, richText.Text.Style);
    }

    [Fact]
    public void Build_空文字列を指定_空のTextSpanへ委譲する()
    {
        var richText = Assert.IsType<RichText>(new Text(string.Empty).Build(null!));

        Assert.Empty(richText.Text.Text);
        Assert.Null(richText.Text.Style);
    }

    [Fact]
    public void Text_Dataがnull_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new Text(null!));
        Assert.Throws<ArgumentNullException>(() => new Text("valid") { Data = null! });
        Assert.Throws<ArgumentNullException>(() => new TextSpan(null!));
        Assert.Throws<ArgumentNullException>(() => new TextSpan("valid") { Text = null! });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void TextStyle_FontSizeが0以下または非有限値_ArgumentOutOfRangeExceptionを投げる(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextStyle { FontSize = value });
    }

    [Fact]
    public void TextStyle_Fontがnull_ArgumentNullExceptionを投げる()
    {
        Assert.Throws<ArgumentNullException>(() => new TextStyle { Font = null! });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1001)]
    public void TextStyle_FontWeightが範囲外_ArgumentOutOfRangeExceptionを投げる(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TextStyle { FontWeight = value });
    }

    [Fact]
    public void TextStyle_全プロパティを指定_内部描画書式へ値を保持して変換する()
    {
        var style = new TextStyle
        {
            FontSize = 24,
            Color = new Color(10, 20, 30, 40),
            Font = new SystemFontProvider("Meiryo"),
            FontWeight = 700,
            IsItalic = true
        };

        var renderingStyle = style.ToRichTextKitStyle();

        Assert.Equal(24, renderingStyle.FontSize);
        Assert.Equal(new SkiaSharp.SKColor(10, 20, 30, 40), renderingStyle.TextColor);
        Assert.StartsWith("$FloatSoda.Font.", renderingStyle.FontFamily);
        Assert.Equal(700, renderingStyle.FontWeight);
        Assert.True(renderingStyle.FontItalic);
    }

    [Fact]
    public void WidgetUpdate_DataとStyleを変更_同じRenderParagraphへ反映する()
    {
        var initial = new Text("before");
        var (pipeline, view, root) = Mount(initial);
        pipeline.FlushLayout();
        var renderParagraph = Assert.IsType<RenderParagraph>(view.Child);

        var updatedStyle = new TextStyle { FontSize = 48, Color = new Color(255, 0, 0) };
        new RenderObjectToWidgetAdapter
        {
            Container = view,
            Child = new Text("after") { Style = updatedStyle }
        }.AttachToRenderTree(root.Owner!, root);
        root.Owner!.BuildScope();

        Assert.Same(renderParagraph, view.Child);
        Assert.Equal("after", renderParagraph.Text.Text);
        Assert.Same(updatedStyle, renderParagraph.Text.Style);
        Assert.True(renderParagraph.NeedsLayout);
    }

    [Fact]
    public void PublicMembers_Text関連API_SkiaSharpとRichTextKit型を公開しない()
    {
        Type[] types =
        [
            typeof(Text), typeof(RichText), typeof(TextSpan), typeof(TextStyle),
            typeof(FontProvider), typeof(SystemFontProvider), typeof(FileFontProvider), typeof(FontResource)
        ];

        var exposedTypes = types
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SelectMany(GetSignatureTypes);

        Assert.DoesNotContain(exposedTypes, IsRenderingLibraryType);
    }

    private static bool IsRenderingLibraryType(Type type) =>
        type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true ||
        type.Namespace?.StartsWith("Topten.RichTextKit", StringComparison.Ordinal) == true;

    private static IEnumerable<Type> GetSignatureTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
        _ => []
    };
}
