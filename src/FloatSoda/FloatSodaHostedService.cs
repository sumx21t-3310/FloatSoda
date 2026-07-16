using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FloatSoda;

internal sealed class FloatSodaHostedService(
    FloatSodaApp app,
    IHostApplicationLifetime applicationLifetime,
    ILogger<FloatSodaHostedService> logger) : IHostedService
{
    private readonly TaskCompletionSource _started =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private readonly TaskCompletionSource _completed =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private Thread? _mainThread;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_mainThread is not null)
        {
            await _started.Task.WaitAsync(cancellationToken);
            return;
        }

        _mainThread = new Thread(Run)
        {
            IsBackground = true,
            Name = "FloatSodaMainThread"
        };

        if (OperatingSystem.IsWindows())
        {
            _mainThread.SetApartmentState(ApartmentState.STA);
        }

        _mainThread.Start();

        using var cancellationRegistration = cancellationToken.Register(app.RequestStop);
        await _started.Task.WaitAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        app.RequestStop();

        return _mainThread is null
            ? Task.CompletedTask
            : _completed.Task.WaitAsync(cancellationToken);
    }

    private void Run()
    {
        try
        {
            app.Run(applicationLifetime.ApplicationStopping, () => _started.TrySetResult());
        }
        catch (OperationCanceledException) when (applicationLifetime.ApplicationStopping.IsCancellationRequested)
        {
            _started.TrySetCanceled(applicationLifetime.ApplicationStopping);
        }
        catch (Exception exception)
        {
            _started.TrySetException(exception);
            logger.LogCritical(exception, "FloatSodaランタイムが異常終了しました");
        }
        finally
        {
            _completed.TrySetResult();
            applicationLifetime.StopApplication();
        }
    }
}
