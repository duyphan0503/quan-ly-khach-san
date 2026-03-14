using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Application.Services;

public class NoShowCancellationWorker : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NoShowCancellationWorker> _logger;

    public NoShowCancellationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<NoShowCancellationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(RunInterval);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var now = DateTime.Now;

            var result = await bookingService.AutoCancelNoShowAsync(now);
            if (!result.Success)
            {
                _logger.LogWarning("Auto no-show cancel failed: {Message}", result.Message);
                return;
            }

            if (result.CancelledCount > 0)
            {
                _logger.LogInformation(
                    "Auto no-show cancel completed at {Now}. Cancelled {Count} bookings.",
                    now,
                    result.CancelledCount);
            }
        }
        catch (OperationCanceledException)
        {
            // Ignore shutdown cancellation.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while running no-show cancellation worker.");
        }
    }
}
