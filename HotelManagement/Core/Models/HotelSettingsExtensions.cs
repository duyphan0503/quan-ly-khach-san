using System.Globalization;

namespace HotelManagement.Core.Models;

/// <summary>
/// Helper chuyển cấu hình string trong HotelSettings thành kiểu mạnh (TimeSpan/DateTime).
/// </summary>
public static class HotelSettingsExtensions
{
    private static readonly TimeSpan DefaultCheckInTime = new(14, 0, 0);
    private static readonly TimeSpan DefaultCheckOutTime = new(12, 0, 0);

    // Parse giờ check-in từ cấu hình, fallback về giá trị mặc định nếu sai format.
    /// <summary>
    /// Thực hiện nghiệp vụ của miền thành phần hiện tại.
    /// </summary>
    public static TimeSpan GetCheckInTime(this HotelSettings settings)
        => ParseTimeOrDefault(settings.CheckInTime, DefaultCheckInTime);

    // Parse giờ check-out từ cấu hình, fallback về giá trị mặc định nếu sai format.
    /// <summary>
    /// Thực hiện nghiệp vụ của miền thành phần hiện tại.
    /// </summary>
    public static TimeSpan GetCheckOutTime(this HotelSettings settings)
        => ParseTimeOrDefault(settings.CheckOutTime, DefaultCheckOutTime);

    // Chặn giá trị âm để tránh lỗi logic worker no-show.
    /// <summary>
    /// Thực hiện nghiệp vụ của miền thành phần hiện tại.
    /// </summary>
    public static int GetNoShowThresholdHours(this HotelSettings settings)
        => Math.Max(0, settings.NoShowThresholdHours);

    // Tính deadline no-show: ngày check-in + giờ check-in + ngưỡng cho phép.
    /// <summary>
    /// Thực hiện nghiệp vụ của miền thành phần hiện tại.
    /// </summary>
    public static DateTime GetNoShowDeadline(this HotelSettings settings, DateTime checkInDate)
        => checkInDate.Date
            .Add(settings.GetCheckInTime())
            .AddHours(settings.GetNoShowThresholdHours());

    private static TimeSpan ParseTimeOrDefault(string? value, TimeSpan fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (TimeSpan.TryParseExact(value, @"hh\:mm", CultureInfo.InvariantCulture, out var parsed) ||
            TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out parsed))
        {
            return parsed;
        }

        return fallback;
    }
}

