using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AppDbContext _context;

    public RoomRepository(AppDbContext context) => _context = context;

    public async Task<List<Room>> GetAllAsync()
        => await _context.Rooms
            .Include(r => r.RoomType)
            .AsNoTracking()
            .OrderBy(r => r.Floor)
            .ThenBy(r => r.RoomNumber)
            .ToListAsync();

    public async Task<Room?> GetByIdAsync(int id)
        => await _context.Rooms
            .Include(r => r.RoomType)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task AddAsync(Room room)
    {
        _context.Rooms.Add(room);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Room room)
    {
        var existingRoom = await _context.Rooms.FirstOrDefaultAsync(r => r.Id == room.Id);
        if (existingRoom is null)
        {
            return;
        }

        // Chỉ cập nhật scalar fields, tránh attach navigation graph (RoomType) gây conflict tracking.
        existingRoom.RoomNumber = room.RoomNumber;
        existingRoom.Floor = room.Floor;
        existingRoom.Status = room.Status;
        existingRoom.RoomTypeId = room.RoomTypeId;
        existingRoom.ImageUrl = room.ImageUrl;
        existingRoom.Notes = room.Notes;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room is not null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsRoomNumberAsync(string roomNumber, int excludeId = 0)
        => await _context.Rooms
            .AnyAsync(r => r.RoomNumber == roomNumber && r.Id != excludeId);

    public async Task<int> GetAvailableCountAsync()
        => await _context.Rooms.CountAsync(r => r.Status == RoomStatus.Available);

    public async Task<int> GetTotalCountAsync()
        => await _context.Rooms.CountAsync();

    public async Task<List<RoomType>> GetAllRoomTypesAsync()
        => await _context.RoomTypes.AsNoTracking().ToListAsync();

    public async Task<RoomType?> GetRoomTypeByIdAsync(int id)
        => await _context.RoomTypes.FindAsync(id);

    public async Task AddRoomTypeAsync(RoomType roomType)
    {
        _context.RoomTypes.Add(roomType);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRoomTypeAsync(RoomType roomType)
    {
        _context.RoomTypes.Update(roomType);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRoomTypeAsync(int id)
    {
        var roomType = await _context.RoomTypes.FindAsync(id);
        if (roomType is not null)
        {
            _context.RoomTypes.Remove(roomType);
            await _context.SaveChangesAsync();
        }
    }
}
