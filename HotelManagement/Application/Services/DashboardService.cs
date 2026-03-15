using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Application.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace HotelManagement.Application.Services;

/// <summary>
/// Tổng hợp dữ liệu dashboard từ nhiều repository và tính các chỉ số biến động theo tháng.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IRoomRepository _roomRepo;
    private readonly IBookingRepository _bookingRepo;
    private readonly IInvoiceRepository _invoiceRepo;

    /// <summary>
    /// Khởi tạo lớp DashboardService và nạp các dependency cần thiết.
    /// </summary>
    public DashboardService(
        IRoomRepository roomRepo,
        IBookingRepository bookingRepo,
        IInvoiceRepository invoiceRepo)
    {
        _roomRepo = roomRepo;
        _bookingRepo = bookingRepo;
        _invoiceRepo = invoiceRepo;
    }

    /// <summary>
    /// Thực hiện nghiệp vụ của miền dashboard.
    /// </summary>
    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        // Chuẩn hóa mốc thời gian để tính KPI tháng hiện tại vs tháng trước.
        var now = DateTime.Now;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1);
        var previousMonthStart = currentMonthStart.AddMonths(-1);
        var previousMonthEnd = currentMonthStart;

        var totalRevenue = await _invoiceRepo.GetTotalRevenueAsync();
        var monthlyRevenueThisMonth = await _invoiceRepo.GetMonthlyRevenueAsync(now.Year, now.Month);
        var totalRooms = await _roomRepo.GetTotalCountAsync();
        var previousMonthRevenue = await _invoiceRepo.GetMonthlyRevenueAsync(previousMonthStart.Year, previousMonthStart.Month);

        var occupiedRoomsNow = await _bookingRepo.GetOccupiedRoomCountAtAsync(now);
        var occupiedRoomsYesterday = await _bookingRepo.GetOccupiedRoomCountAtAsync(now.AddDays(-1));
        var availableRooms = Math.Max(0, totalRooms - occupiedRoomsNow);
        var previousAvailableRooms = Math.Max(0, totalRooms - occupiedRoomsYesterday);

        var guestsThisMonth = await _bookingRepo.GetGuestCountThisMonthAsync();
        var guestsPreviousMonth = await _bookingRepo.GetGuestCountByMonthAsync(previousMonthStart.Year, previousMonthStart.Month);

        var occupiedRoomNightsCurrentMonth = await _bookingRepo.GetOccupiedRoomNightsAsync(currentMonthStart, currentMonthEnd);
        var occupiedRoomNightsPreviousMonth = await _bookingRepo.GetOccupiedRoomNightsAsync(previousMonthStart, previousMonthEnd);

        var recentBookings = await _bookingRepo.GetRecentAsync(8);
        var monthlyRevenue = await _invoiceRepo.GetMonthlyRevenueChartAsync(now.Year);
        var todayNewBookingsCount = await _bookingRepo.GetTodayNewBookingsCountAsync();
        var pendingCheckinCount = await _bookingRepo.GetPendingCheckinCountAsync();
        var roomTypeDistribution = await _bookingRepo.GetRoomTypeDistributionAsync();

        var currentMonthCapacity = totalRooms * DateTime.DaysInMonth(currentMonthStart.Year, currentMonthStart.Month);
        var previousMonthCapacity = totalRooms * DateTime.DaysInMonth(previousMonthStart.Year, previousMonthStart.Month);

        // Công suất phòng = room-nights đã sử dụng / room-nights khả dụng.
        var occupancyRate = currentMonthCapacity > 0
            ? Math.Round((double)occupiedRoomNightsCurrentMonth / currentMonthCapacity * 100, 1)
            : 0d;

        var previousMonthOccupancyRate = previousMonthCapacity > 0
            ? Math.Round((double)occupiedRoomNightsPreviousMonth / previousMonthCapacity * 100, 1)
            : 0d;

        var monthlyRevenueChangePercent = previousMonthRevenue > 0
            ? Math.Round((monthlyRevenueThisMonth - previousMonthRevenue) / previousMonthRevenue * 100, 1)
            : monthlyRevenueThisMonth > 0 ? 100 : 0;

        return new DashboardViewModel
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenueThisMonth = monthlyRevenueThisMonth,
            MonthlyRevenueChangePercent = monthlyRevenueChangePercent,
            OccupancyRate = occupancyRate,
            OccupancyRateChange = Math.Round(occupancyRate - previousMonthOccupancyRate, 1),
            TotalGuestsThisMonth = guestsThisMonth,
            TotalGuestsChange = guestsThisMonth - guestsPreviousMonth,
            AvailableRooms = availableRooms,
            TotalRooms = totalRooms,
            AvailableRoomsChange = availableRooms - previousAvailableRooms,
            TodayNewBookingsCount = todayNewBookingsCount,
            PendingCheckinCount = pendingCheckinCount,
            RecentBookings = recentBookings,
            MonthlyRevenue = monthlyRevenue,
            RoomTypeDistribution = roomTypeDistribution
        };
    }
}

