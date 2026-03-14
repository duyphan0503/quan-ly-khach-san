namespace HotelManagement.Core.Models;

public class HotelSettings
{
    public int NoShowThresholdHours { get; set; } = 6;
    public string CheckInTime { get; set; } = "14:00";
    public string CheckOutTime { get; set; } = "12:00";
}
