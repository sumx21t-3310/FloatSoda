using FloatSoda.Core;
using FloatSoda.RenderObjects;
using FloatSoda.Widgets;

namespace FloatSoda.Testing;

/// <summary>
/// Widgetをヘッドレスでマウントし、フレームを進め、ツリーを調べるテストハーネスです。
/// Flutterの<c>WidgetTester</c>に相当し、<c>pumpWidget</c>、<c>pump</c>、<c>find</c>と同じ書き方でテストを書けます。
/// 特定のテストフレームワークには依存しません。
/// </summary>
public sealed class WidgetTester : IDisposable
{
    // Flutter の _kDefaultTestViewportSize と同じ大きさ。
    private const float DefaultViewportWidth = 800;
    private const float DefaultViewportHeight = 600;

    // ウィンドウを持たないバインディング。FrameScheduler と GestureBinding を本番と同じ形で BuildOwner に渡す。
    private readonly WidgetBinding _binding = new(
        new RenderView(DefaultViewportWidth, DefaultViewportHeight)
    );

    private bool _disposed;

    /// <summary>
    /// ウィジェットをまだマウントしていないテストハーネスを初期化します。
    /// </summary>
    public WidgetTester() => Find = new CommonFinders(() => _binding.RenderViewElement);

    /// <summary>
    /// Elementツリーを探すためのFinderの入口を取得します。
    /// </summary>
    // 挙動:
    // - 作成済みのCommonFindersを返す。呼ぶたびに新しく作らない。
    // - CommonFindersが作るFinderは、Evaluate()のたびにこのtesterの現在のルートElementから探し直す。
    //   そのため、このプロパティはtesterのルートへの参照を持つ。
    public CommonFinders Find { get; }

    /// <summary>
    /// Widgetをルートとしてマウントし、1フレーム回します。
    /// 再び呼ぶと、ルートを新しいWidgetに差し替えます。
    /// </summary>
    /// <param name="widget">ルートとしてマウントするWidget。</param>
    /// <returns>1フレームの処理が終わると完了するタスク。</returns>
    public Task PumpWidget(Widget widget)
    {
        // 挙動:
        // 1. widgetがnullならArgumentNullExceptionを投げる。破棄済みならObjectDisposedExceptionを投げる。
        // 2. 初回: ウィンドウを持たないWidgetBindingのRenderViewへ、widgetをルートとしてマウントする。
        // 3. 2回目以降: 同じルートElementのChildをwidgetに差し替えて再アタッチする。
        //    WidgetのCanUpdateが真ならElementとStateを引き継ぎ、偽なら作り直す。
        // 4. Pump()と同じ1フレームを回す。
        // RenderViewのサイズの既定値は800x600(Flutterの_kDefaultTestViewportSizeと同じ)。

        ArgumentNullException.ThrowIfNull(widget);
        ObjectDisposedException.ThrowIf(_disposed, this);

        _binding.AttachRootWidget(widget);

        return Pump();
    }

    /// <summary>
    /// ビルド、レイアウト、ペイントを1回回します。
    /// </summary>
    /// <returns>1フレームの処理が終わると完了するタスク。</returns>
    /// <exception cref="InvalidOperationException"><see cref="PumpWidget"/>をまだ呼んでいないとき。</exception>
    public Task Pump()
    {
        // 破棄済みの判定を先に行う。マウントせずに破棄したtesterでも、ObjectDisposedExceptionにするため。
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (_binding.RenderViewElement is null)
        {
            throw new InvalidOperationException(
                "PumpWidget を呼ぶ前に Pump が呼ばれました。先に PumpWidget でウィジェットをマウントしてください。"
            );
        }

        // 処理は同期で終わる。await の付け忘れの検出(TestAsyncUtils.guard 相当)は #239 で決める。
        _binding.FlushBuildLayoutAndPaint();
        return Task.CompletedTask;
    }

    /// <summary>
    /// <paramref name="finder"/>に一致したWidgetを返します。
    /// </summary>
    /// <typeparam name="T">取り出すWidgetの型。</typeparam>
    /// <param name="finder">Widgetを探すFinder。</param>
    /// <returns>一致したWidget。</returns>
    /// <exception cref="WidgetAssertionException">一致が0件、または2件以上のとき。</exception>
    public T Widget<T>(Finder finder) where T : Widget
    {
        // 挙動:
        // 1. finder.Evaluate()の結果を数える。
        // 2. 0件なら、探した条件を含むメッセージでWidgetAssertionExceptionを投げる。
        // 3. 2件以上なら、件数を含むメッセージでWidgetAssertionExceptionを投げる。
        // 4. 1件なら、そのElementのWidgetをTにキャストして返す。
        //    Tに合わないときはWidgetAssertionExceptionを投げる。
        // 【未決】WidgetAssertionExceptionの第1引数(type)の使い道。現状は未使用。

        ArgumentNullException.ThrowIfNull(finder);
        ObjectDisposedException.ThrowIf(_disposed, this);

        var elements = finder.Evaluate().ToList();

        if (elements.Count == 0)
        {
            throw new WidgetAssertionException(
                typeof(T),
                $"{finder.Description} が見つかりませんでした。"
            );
        }

        if (elements.Count > 1)
        {
            throw new WidgetAssertionException(
                typeof(T),
                $"{finder.Description} が{elements.Count}件見つかりました。1件に絞り込んでください。"
            );
        }

        var found = elements[0].Widget;
        if (found is not T widget)
        {
            throw new WidgetAssertionException(
                typeof(T),
                $"{finder.Description} は {found?.GetType().Name} で、{typeof(T).Name} ではありません。"
            );
        }

        return widget;
    }

    /// <summary>
    /// マウントしたツリーを破棄します。
    /// </summary>
    public void Dispose()
    {
        // 挙動:
        // 1. 2回目以降の呼び出しは何もしない。
        // 2. ルートの子をnullに差し替えて再アタッチし、BuildOwner.BuildScopeを回す。
        //    これでStateが破棄される(WidgetBitmapRenderer.Renderのfinallyと同じ)。
        // 3. 以後、PumpWidgetとPumpはObjectDisposedExceptionを投げる。

        if (_disposed)
            return;

        // 先に立てる。ツリーを外す途中で例外が出ても、2回目の Dispose で二重に破棄しないため。
        _disposed = true;

        _binding.DetachRootWidget();
    }
}
