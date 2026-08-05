using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.RenderObjects.Painting;
using FloatSoda.Widgets;
using FloatSoda.Widgets.Paint;

namespace FloatSoda.Test.Widgets;

public class TreeHelperPublicApiTest
{
    [Fact]
    public void PublicMembers_追加した公開API_必須プロパティとSkia非公開を満たす()
    {
        Assert.NotNull(typeof(Builder).GetProperty(nameof(Builder.ChildBuilder))!
            .GetCustomAttribute<RequiredMemberAttribute>());
        Assert.NotNull(typeof(KeyedSubtree).GetProperty(nameof(KeyedSubtree.Child))!
            .GetCustomAttribute<RequiredMemberAttribute>());

        Type[] types =
        [
            typeof(Builder),
            typeof(KeyedSubtree),
            typeof(RepaintBoundary),
            typeof(RenderRepaintBoundary)
        ];
        var exposedTypes = types
            .SelectMany(type => type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .SelectMany(GetSignatureTypes);

        Assert.DoesNotContain(exposedTypes, type =>
            type.Namespace?.StartsWith("SkiaSharp", StringComparison.Ordinal) == true);
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
