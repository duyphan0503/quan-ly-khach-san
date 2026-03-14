using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Repositories.Interfaces;

public interface IGuestRepository
{
    Task<List<Guest>> GetAllAsync();
    Task<Guest?> GetByIdAsync(int id);
    Task AddAsync(Guest guest);
    Task UpdateAsync(Guest guest);
    Task DeleteAsync(int id);
    Task<bool> ExistsCCCDAsync(string cccd, int excludeId = 0);
    Task<bool> ExistsPhoneAsync(string phoneNumber, int excludeId = 0);
    Task<Guest?> SearchByPhoneOrCCCDAsync(string query);
    Task<Guest?> GetByUserIdAsync(string userId);
    Task<List<Guest>> GetAllByUserIdAsync(string userId);
    Task<(List<Guest> Items, int TotalCount)> GetPagedAsync(string? searchQuery, int pageIndex, int pageSize);
}
