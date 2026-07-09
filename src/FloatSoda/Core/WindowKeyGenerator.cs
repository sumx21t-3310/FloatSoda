using System.Reflection;
using System.Text;

namespace FloatSoda.Core;

/// <summary>
/// オーバーレイキーの生成器。「エントリアセンブリ名（プロジェクトの名前空間）+ タイトル」を
/// それぞれスネークケース化し、<c>.</c> でつないだものをキーとする。
/// 例: アセンブリ <c>MyApp.Overlay</c> + タイトル <c>Watch DashBoard</c> → <c>my_app_overlay.watch_dash_board</c>
/// </summary>
internal static class WindowKeyGenerator
{
    private static readonly string Endpoint =
        Assembly.GetEntryAssembly()?.GetName().Name ?? $"overlay_app_{Guid.NewGuid():N}";

    public static string GenerateKey(string title) => $"{ToSnakeCase(Endpoint)}.{ToSnakeCase(title)}";

    /// <summary>
    /// PascalCase・スペース・ドット区切りをスネークケースへ変換する。
    /// </summary>
    internal static string ToSnakeCase(string text)
    {
        var builder = new StringBuilder(text.Length + 8);

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (char.IsUpper(c))
            {
                // 単語境界: 直前が小文字/数字、または大文字連続の末尾（次が小文字）
                var isBoundary = i > 0 &&
                                 (char.IsLower(text[i - 1]) || char.IsDigit(text[i - 1]) ||
                                  (char.IsUpper(text[i - 1]) && i + 1 < text.Length && char.IsLower(text[i + 1])));

                if (isBoundary && builder.Length > 0 && builder[^1] != '_')
                {
                    builder.Append('_');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
            else if (builder.Length > 0 && builder[^1] != '_')
            {
                // スペース・ドット等の区切り文字はアンダースコアに畳む
                builder.Append('_');
            }
        }

        // 末尾の区切りを除去
        if (builder.Length > 0 && builder[^1] == '_')
        {
            builder.Length--;
        }

        return builder.ToString();
    }
}
