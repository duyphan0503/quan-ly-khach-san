using HotelManagement.Core.Models;

namespace HotelManagement.Application.Services.Interfaces;

public interface IRoomService
{
    Task<List<Room>> GetAllAsync();
    Task<Room?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CreateAsync(Room room);
    Task<(bool Success, string Message)> UpdateAsync(Room room);
    Task<(bool Success, string Message)> DeleteAsync(int id);
    Task<List<RoomType>> GetRoomTypesAsync();
    Task<RoomType?> GetRoomTypeByIdAsync(int id);
    Task<(bool Success, string Message)> CreateRoomTypeAsync(RoomType roomType);
    Task<(bool Success, string Message)> UpdateRoomTypeAsync(RoomType roomType);
    Task<(bool Success, string Message)> DeleteRoomTypeAsync(int id);
    Task<int> GetAvailableCountAsync();
    Task<int> GetTotalCountAsync();
    Task<List<RoomType>> GetFeaturedRoomTypesAsync(int count);
    Task<List<Room>> GetAvailableRoomsAsync(DateTime? checkIn, DateTime? checkOut, int? guests, string? typeName);
}
