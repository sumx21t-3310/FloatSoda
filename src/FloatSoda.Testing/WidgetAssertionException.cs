namespace FloatSoda.Testing;

/// <summary>
/// Represents an error raised when a widget assertion fails during widget testing.
/// </summary>
/// This exception is thrown by <see cref="WidgetTester"/> when a lookup does not produce the expected widget.
/// This happens when a <see cref="Finder"/> matches no widgets, matches more than one widget,
/// or matches a single widget whose type is not the requested type.
public class WidgetAssertionException(Type type, string message) : Exception(message);
