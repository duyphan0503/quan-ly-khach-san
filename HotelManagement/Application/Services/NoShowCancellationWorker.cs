using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Application.Services;

/// <summary>
/// Worker nền chạy định kỳ để tự động hủy booking no-show.
/// </summary>
public class NoShowCancellationWorker : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NoShowCancellationWorker> _logger;

    /// <summary>
    /// Khởi tạo lớp NoShowCancellationWorker và nạp các dependency cần thiết.
    /// </summary>
    public NoShowCancellationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<NoShowCancellationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Chạy ngay một vòng khi app khởi động để không phải đợi đến tick kế tiếp.
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(RunInterval);
        while (!stoppingToken.IsCancellationRequested &&
               await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    // Một chu kỳ xử lý no-show.
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
