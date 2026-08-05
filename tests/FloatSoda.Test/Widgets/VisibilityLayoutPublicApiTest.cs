using System.Reflection;
using FloatSoda.RenderObjects.Layout;
using FloatSoda.Widgets.Layout;

namespace FloatSoda.Test.Widgets;

public class VisibilityLayoutPublicApiTest
{
    [Fact]
    public void PublicMembers_Issue169で追加したWidgetAPI_外部描画型を公開しない()
    {
        Type[] types =
        [
            typeof(Offstage),
            typeof(Visibility),
            typeof(IndexedStack),
            typeof(RotatedBox),
            typeof(RenderOffstage),
            typeof(RenderIndexedStack),
            typeof(RenderRotatedBox),
        ];

        var exposedTypes = types
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SelectMany(GetSignatureTypes);

        Assert.DoesNotContain(exposedTypes, type => type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true);
    }

    [Fact]
    public void RenderTypes_Issue169の公開Widget_対応する専用RenderObjectを生成する()
    {
        Assert.IsType<RenderOffstage>(new Offstage().CreateRenderObject());
        Assert.IsType<RenderIndexedStack>(new IndexedStack { Index = null }.CreateRenderObject());
        Assert.IsType<RenderRotatedBox>(new RotatedBox().CreateRenderObject());
    }

    private static IEnumerable<Type> GetSignatureTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
        _ => [],
    };
}
