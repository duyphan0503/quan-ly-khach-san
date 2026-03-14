using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _repo;
    private readonly IBookingRepository _bookingRepo;

    public RoomService(IRoomRepository repo, IBookingRepository bookingRepo)
    {
        _repo = repo;
        _bookingRepo = bookingRepo;
    }

    public Task<List<Room>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Room?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<(bool Success, string Message)> CreateAsync(Room room)
    {
        if (await _repo.ExistsRoomNumberAsync(room.RoomNumber))
            return (false, $"Số phòng \"{room.RoomNumber}\" đã tồn tại trong hệ thống.");

        await _repo.AddAsync(room);
        return (true, $"Đã thêm phòng {room.RoomNumber} thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Room room)
    {
        if (await _repo.ExistsRoomNumberAsync(room.RoomNumber, room.Id))
            return (false, $"Số phòng \"{room.RoomNumber}\" đã được dùng bởi phòng khác.");

        await _repo.UpdateAsync(room);
        return (true, $"Đã cập nhật phòng {room.RoomNumber}.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var room = await _repo.GetByIdAsync(id);
        if (room is null) return (false, "Không tìm thấy phòng.");

        if (room.Status == RoomStatus.Occupied)
            return (false, "Không thể xóa phòng đang có khách.");

        await _repo.DeleteAsync(id);
        return (true, $"Đã xóa phòng {room.RoomNumber}.");
    }

    public Task<List<RoomType>> GetRoomTypesAsync() => _repo.GetAllRoomTypesAsync();

    public Task<RoomType?> GetRoomTypeByIdAsync(int id) => _repo.GetRoomTypeByIdAsync(id);

    public async Task<(bool Success, string Message)> CreateRoomTypeAsync(RoomType roomType)
    {
        await _repo.AddRoomTypeAsync(roomType);
        return (true, $"Đã thêm loại phòng {roomType.Name} thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateRoomTypeAsync(RoomType roomType)
    {
        await _repo.UpdateRoomTypeAsync(roomType);
        return (true, $"Đã cập nhật loại phòng {roomType.Name}.");
    }

    public async Task<(bool Success, string Message)> DeleteRoomTypeAsync(int id)
    {
        var rt = await _repo.GetRoomTypeByIdAsync(id);
        if (rt is null) return (false, "Không tìm thấy loại phòng");
        
        try
        {
            await _repo.DeleteRoomTypeAsync(id);
            return (true, $"Đã xóa loại phòng {rt.Name}.");
        }
        catch (Exception)
        {
            return (false, "Lỗi: Không thể xóa loại phòng này vì đang có phòng thuộc danh mục này.");
        }
    }

    public Task<int> GetAvailableCountAsync() => _repo.GetAvailableCountAsync();

    public Task<int> GetTotalCountAsync() => _repo.GetTotalCountAsync();

    public async Task<List<RoomType>> GetFeaturedRoomTypesAsync(int count)
    {
        var allRoomTypes = await _repo.GetAllRoomTypesAsync();
        return allRoomTypes
            .Take(count)
            .ToList();
    }

    public async Task<List<Room>> GetAvailableRoomsAsync(DateTime? checkIn, DateTime? checkOut, int? guests, string? typeName)
    {
        var allRooms = await _repo.GetAllAsync();
        var query = allRooms.AsQueryable();

        // 1. Luôn loại bỏ phòng bảo trì
        query = query.Where(r => r.Status != RoomStatus.Maintenance);

        // 2. Nếu không cung cấp ngày cụ thể, chỉ hiển thị những phòng thực sự đang rảnh ngay lúc này
        if (!checkIn.HasValue || !checkOut.HasValue)
        {
            query = query.Where(r => r.Status == RoomStatus.Available);
        }

        if (!string.IsNullOrEmpty(typeName))
        {
            query = query.Where(r => r.RoomType != null && r.RoomType.Name.ToLower() == typeName.ToLower());
        }

        if (guests.HasValue)
        {
            query = query.Where(r => r.RoomType != null && r.RoomType.MaxOccupancy >= guests.Value);
        }

        var candidateRooms = query.ToList();

        // 3. Nếu có ngày cụ thể, thực hiện kiểm tra sâu hơn trong lịch sử đặt phòng (Bookings)
        if (checkIn.HasValue && checkOut.HasValue)
        {
            var availableRooms = new List<Room>();
            foreach (var room in candidateRooms)
            {
                if (await _bookingRepo.IsRoomAvailableAsync(room.Id, checkIn.Value, checkOut.Value))
                {
                    availableRooms.Add(room);
                }
            }
            return availableRooms;
        }

        return candidateRooms;
    }
}
