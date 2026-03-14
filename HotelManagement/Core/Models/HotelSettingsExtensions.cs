using System.Globalization;

namespace HotelManagement.Core.Models;

public static class HotelSettingsExtensions
{
    private static readonly TimeSpan DefaultCheckInTime = new(14, 0, 0);
    private static readonly TimeSpan DefaultCheckOutTime = new(12, 0, 0);

    public static TimeSpan GetCheckInTime(this HotelSettings settings)
        => ParseTimeOrDefault(settings.CheckInTime, DefaultCheckInTime);

    public static TimeSpan GetCheckOutTime(this HotelSettings settings)
        => ParseTimeOrDefault(settings.CheckOutTime, DefaultCheckOutTime);

    public static int GetNoShowThresholdHours(this HotelSettings settings)
        => Math.Max(0, settings.NoShowThresholdHours);

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
