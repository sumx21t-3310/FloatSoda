using System.Reflection;
using FloatSoda.Painting;
using FloatSoda.Widgets.Layout;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Test.Widgets;

public class Phase2PaintingPublicApiTest
{
    [Fact]
    public void PublicMembers_Phase2で追加した公開API_SkiaSharp型を公開しない()
    {
        Type[] types =
        [
            typeof(BoxDecoration),
            typeof(Border),
            typeof(BorderSide),
            typeof(DecoratedBox),
            typeof(Opacity),
            typeof(Transform),
            typeof(Container)
        ];

        var exposedTypes = types
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SelectMany(GetSignatureTypes);

        Assert.DoesNotContain(exposedTypes, type => type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true);
    }

    private static IEnumerable<Type> GetSignatureTypes(MemberInfo member) => member switch
    {
        PropertyInfo property => [property.PropertyType],
        FieldInfo field => [field.FieldType],
        MethodInfo method => method.GetParameters().Select(parameter => parameter.ParameterType).Append(method.ReturnType),
        ConstructorInfo constructor => constructor.GetParameters().Select(parameter => parameter.ParameterType),
        _ => []
    };
}
