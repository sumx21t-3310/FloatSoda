namespace FloatSoda.Testing;

/// <summary>
/// <see cref="WidgetTester"/>でWidgetを取り出せなかったときに投げられる例外です。
/// </summary>
/// <remarks>
/// <see cref="Finder"/>の一致が0件のとき、2件以上のとき、一致した1件が求めた型でないときに、
/// <see cref="WidgetTester.Widget{T}"/>が投げます。
/// </remarks>
/// <param name="type">取り出そうとしたWidgetの型。</param>
/// <param name="message">探した条件と、失敗した理由を含むメッセージ。</param>
public class WidgetAssertionException(Type type, string message) : Exception(message);
