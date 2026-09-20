using System.Reflection;
using System.Runtime.CompilerServices;
using FloatSoda.Animation;
using FloatSoda.Geometrics;

namespace FloatSoda.Test;

public class RecordToStringTest
{
    /// <summary>recordを検査するアセンブリ。型を1つ参照して取得する。</summary>
    public static TheoryData<string> AssemblyNames =>
    [
        typeof(FloatSoda.Core.WidgetBinding).Assembly.GetName().Name!,
        typeof(FloatSoda.Rendering.Layers.ILayer).Assembly.GetName().Name!,
        typeof(FloatSoda.Abstractions.Geometries.Offset).Assembly.GetName().Name!,
        typeof(FloatSoda.Engine.IOTaskRunner).Assembly.GetName().Name!,
        typeof(FloatSoda.OVR.OVRAppInfo).Assembly.GetName().Name!,
        typeof(FloatSoda.Testing.WidgetBitmapRenderer).Assembly.GetName().Name!
    ];

    /// <summary>
    /// recordが自動生成するToString(PrintMembers)は、publicなインスタンスプロパティをすべて出力する。
    /// 自分と同じ系統のrecordを毎回作って返す計算プロパティ(<c>Flipped</c>など)があると、その値を出力するために
    /// 再びToStringを呼び、終わらない。record structではスタックオーバーフローでプロセスごと落ちる。
    /// そういうプロパティを持つrecordには、PrintMembersかToStringの上書きを要求する。
    /// </summary>
    [Theory]
    [MemberData(nameof(AssemblyNames))]
    public void PrintMembers_自分と同じ系統のrecordを返す計算プロパティを持つ_自動生成のままにしていない(string assemblyName)
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == assemblyName);

        var offenders =
            from type in assembly.GetTypes()
            let printMembers = FindPrintMembers(type)
            where printMembers is not null && printMembers.IsDefined(typeof(CompilerGeneratedAttribute))
            // ToStringを自分で書いている型は、PrintMembersを経由しない。
            where type.GetMethod(nameof(ToString), BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly, Type.EmptyTypes)
                ?.IsDefined(typeof(CompilerGeneratedAttribute)) != false
            from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            where property.GetIndexParameters().Length == 0
            where IsComputed(type, property) && IsSameRecordFamily(type, property.PropertyType)
            select $"{type.FullName}.{property.Name}";

        Assert.Empty(offenders);
    }

    [Fact]
    public void ToString_EdgeInsets_各辺の値を含む文字列を返す()
    {
        var text = new EdgeInsets(1, 2, 3, 4).ToString();

        Assert.Equal("EdgeInsets { Left = 1, Top = 2, Right = 3, Bottom = 4 }", text);
    }

    [Fact]
    public void ToString_Curve_例外を投げず型名を含む文字列を返す()
    {
        Assert.StartsWith("LinearCurve", Curves.Linear.ToString());
        Assert.StartsWith("Cubic", new Cubic(0.1, 0.2, 0.3, 0.4).ToString());
    }

    [Fact]
    public void ToString_FlippedCurve_元のカーブを含む文字列を返す()
    {
        var text = new Cubic(0.1, 0.2, 0.3, 0.4).Flipped.ToString();

        Assert.StartsWith("FlippedCurve", text);
        Assert.Contains("Cubic", text);
    }

    [Fact]
    public void Flipped_EdgeInsets_左右と上下を入れ替える()
    {
        Assert.Equal(new EdgeInsets(3, 4, 1, 2), new EdgeInsets(1, 2, 3, 4).Flipped);
    }

    private static MethodInfo? FindPrintMembers(Type type) => type.GetMethod(
        "PrintMembers",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
        [typeof(System.Text.StringBuilder)]);

    /// <summary>自動実装プロパティは保持している値を返すだけなので有限。計算プロパティだけを対象にする。</summary>
    private static bool IsComputed(Type type, PropertyInfo property) =>
        type.GetField($"<{property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) is null;

    /// <summary>プロパティの型がrecordで、宣言した型と継承関係にあるかどうか。interfaceは対象外。</summary>
    private static bool IsSameRecordFamily(Type declaringType, Type propertyType)
    {
        propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (propertyType.IsInterface || !IsRecord(propertyType)) return false;

        return propertyType.IsAssignableFrom(declaringType) || declaringType.IsAssignableFrom(propertyType);
    }

    private static bool IsRecord(Type type)
    {
        for (var current = type; current is not null && current != typeof(object); current = current.BaseType)
        {
            if (FindPrintMembers(current) is not null) return true;
        }

        return false;
    }
}
