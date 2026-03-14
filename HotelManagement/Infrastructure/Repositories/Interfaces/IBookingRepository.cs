using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Repositories.Interfaces;

public interface IBookingRepository
{
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task DeleteAsync(int id);
    Task<List<Booking>> GetRecentAsync(int count = 10);
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int excludeBookingId = 0);
    Task<int> GetGuestCountThisMonthAsync();
    Task<int> GetGuestCountByMonthAsync(int year, int month);
    Task<int> GetOccupiedRoomCountAtAsync(DateTime atTime);
    Task<int> GetOccupiedRoomNightsAsync(DateTime periodStart, DateTime periodEnd);
    Task<List<Booking>> GetByStatusAsync(string status);
    Task<int> GetTodayNewBookingsCountAsync();
    Task<int> GetPendingCheckinCountAsync();
    Task<Dictionary<string, int>> GetRoomTypeDistributionAsync();
    Task<List<Booking>> GetByRoomIdAsync(int roomId, int count = 5);
    Task<List<Booking>> GetByGroupCodeAsync(string groupCode);
    Task<List<Booking>> SearchAsync(string? query, string? status);
}
