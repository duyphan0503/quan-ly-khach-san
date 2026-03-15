using HotelManagement.Core.Models;

namespace HotelManagement.Application.ViewModels;

/// <summary>
/// Mô hình dữ liệu phục vụ hiển thị cho miền dashboard.
/// </summary>
public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenueThisMonth { get; set; }
    public decimal MonthlyRevenueChangePercent { get; set; }
    public double OccupancyRate { get; set; }
    public double OccupancyRateChange { get; set; }
    public int TotalGuestsThisMonth { get; set; }
    public int TotalGuestsChange { get; set; }
    public int AvailableRooms { get; set; }
    public int TotalRooms { get; set; }
    public int AvailableRoomsChange { get; set; }
    public int TodayNewBookingsCount { get; set; }
    public int PendingCheckinCount { get; set; }
    public List<Booking> RecentBookings { get; set; } = new();
    public List<decimal> MonthlyRevenue { get; set; } = new(); // 12 phần tử cho Chart.js
    public Dictionary<string, int> RoomTypeDistribution { get; set; } = new();
}
