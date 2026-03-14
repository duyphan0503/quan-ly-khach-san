using HotelManagement.Core.Models;

namespace HotelManagement.Application.Services.Interfaces;

public interface IGuestService
{
    Task<List<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CreateAsync(Guest guest);
    Task<(bool Success, string Message)> UpdateAsync(Guest guest);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<Guest?> SearchByPhoneOrCCCDAsync(string query);
    Task<Guest?> GetByUserIdAsync(string userId);
    Task<List<Guest>> GetAllByUserIdAsync(string userId);
    Task<(bool Success, string Message, Guest? PrimaryGuest)> ConsolidateGuestProfilesAsync(
        string? userId,
        string phone,
        string? cccd = null,
        string? email = null,
        string? fullName = null,
        string? avatarUrl = null);
    Task<(List<Guest> Items, int TotalCount)> GetPagedAsync(string? searchQuery, int pageIndex, int pageSize);
}
