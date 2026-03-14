using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;

namespace HotelManagement.Application.Services.Interfaces;

public interface IBookingService
{
    Task<List<Booking>> GetAllAsync();
    Task<Booking?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CreateAsync(Booking booking);
    Task<(bool Success, string Message, int? PrimaryBookingId)> CreateMultipleAsync(
        int guestId,
        List<int> roomIds,
        DateTime checkIn,
        DateTime checkOut,
        int totalGuests,
        BookingStatus status,
        string? notes,
        string? createdByUserId = null);
    Task<(bool Success, string Message)> UpdateStatusAsync(int bookingId, string newStatus, string? userId = null);
    Task<(bool Success, string Message)> UpdateAsync(Booking booking);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<(bool Success, string Message)> CancelAsync(int bookingId);
    Task<List<Booking>> GetRecentAsync(int count = 10);
    Task<int> GetGuestCountThisMonthAsync();
    Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int excludeBookingId = 0);
    Task<List<Booking>> GetRecentByRoomIdAsync(int roomId, int count = 5);
    Task<List<Booking>> GetByGroupCodeAsync(string groupCode);
    Task<List<Booking>> SearchAsync(string? query, string? status);
    Task<(bool Success, string Message, int CancelledCount)> AutoCancelNoShowAsync(DateTime now);
}
