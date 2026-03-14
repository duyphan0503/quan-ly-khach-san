using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Repositories.Interfaces;

public interface IRoomRepository
{
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int id);
    Task AddAsync(Room room);
    Task UpdateAsync(Room room);
    Task DeleteAsync(int id);
    Task<bool> ExistsRoomNumberAsync(string roomNumber, int excludeId = 0);
    Task<int> GetAvailableCountAsync();
    Task<int> GetTotalCountAsync();
    Task<List<RoomType>> GetAllRoomTypesAsync();
    Task<RoomType?> GetRoomTypeByIdAsync(int id);
    Task AddRoomTypeAsync(RoomType roomType);
    Task UpdateRoomTypeAsync(RoomType roomType);
    Task DeleteRoomTypeAsync(int id);
}
