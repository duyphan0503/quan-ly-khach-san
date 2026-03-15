namespace HotelManagement.Core.Models;

/// <summary>
/// Cấu hình vận hành khách sạn đọc từ appsettings (section: HotelSettings).
/// </summary>
public class HotelSettings
{
    // Sau thời điểm check-in + số giờ này, booking chưa check-in sẽ được xem là no-show.
    public int NoShowThresholdHours { get; set; } = 6;

    // Giờ nhận phòng chuẩn (định dạng HH:mm).
    public string CheckInTime { get; set; } = "14:00";

    // Giờ trả phòng chuẩn (định dạng HH:mm).
    public string CheckOutTime { get; set; } = "12:00";
}
