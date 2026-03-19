using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HotelManagement.Application.Services;

/// <summary>
/// Service trung tâm xử lý nghiệp vụ đặt phòng: tạo, cập nhật trạng thái, kiểm tra trùng lịch và auto-cancel no-show.
/// </summary>
public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IRoomRepository _roomRepo;

    private readonly AppDbContext _context;
    private readonly HotelSettings _hotelSettings;

    /// <summary>
    /// Khởi tạo lớp BookingService và nạp các dependency cần thiết.
    /// </summary>
    public BookingService(
        IBookingRepository bookingRepo,
        IRoomRepository roomRepo,
        AppDbContext context,
        IOptions<HotelSettings> hotelSettingsOptions)
    {
        _bookingRepo = bookingRepo;
        _roomRepo = roomRepo;
        _context = context;
        _hotelSettings = hotelSettingsOptions.Value;
    }

    /// <summary>
    /// Lấy toàn bộ dữ liệu booking.
    /// </summary>
    public Task<List<Booking>> GetAllAsync() => _bookingRepo.GetAllAsync();

    /// <summary>
    /// Lấy thông tin booking theo mã định danh.
    /// </summary>
    public Task<Booking?> GetByIdAsync(int id) => _bookingRepo.GetByIdAsync(id);

    public async Task<(bool Success, string Message)> CreateAsync(Booking booking)
    {
        // Rule 1: check-out phải sau check-in.
        if (booking.CheckOut <= booking.CheckIn)
            return (false, "Ngày trả phòng phải sau ngày nhận phòng.");

        // Rule 2: phòng không được trùng lịch trong cùng khoảng thời gian.
        if (!await _bookingRepo.IsRoomAvailableAsync(booking.RoomId, booking.CheckIn, booking.CheckOut))
            return (false, "Phòng đã có lịch đặt trong khoảng thời gian này.");

        var room = await _roomRepo.GetByIdAsync(booking.RoomId);
        if (room is null) return (false, "Không tìm thấy thông tin phòng.");

        int nights = (booking.CheckOut - booking.CheckIn).Days;
        booking.TotalAmount = room.RoomType.BasePrice * nights;
        booking.Status = BookingStatus.Confirmed;
        booking.CreatedAt = DateTime.Now;

        // Transaction đảm bảo booking và trạng thái phòng luôn nhất quán.
        await ExecuteInTransactionAsync(async () =>
        {
            await _bookingRepo.AddAsync(booking);
            room.Status = RoomStatus.Reserved;
            await _roomRepo.UpdateAsync(room);
        });

        return (true, $"Đã tạo đặt phòng thành công. Tổng tiền: {booking.TotalAmount:N0}₫");
    }

    public async Task<(bool Success, string Message, int? PrimaryBookingId)> CreateMultipleAsync(
        int guestId,
        List<int> roomIds,
        DateTime checkIn,
        DateTime checkOut,
        int totalGuests,
        BookingStatus status,
        string? notes,
        string? createdByUserId = null)
    {
        if (roomIds == null || roomIds.Count == 0)
            return (false, "Vui lòng chọn ít nhất một phòng.", null);

        var distinctRoomIds = roomIds.Distinct().ToList();
        if (checkOut <= checkIn)
            return (false, "Ngày trả phòng phải sau ngày nhận phòng.", null);

        if (totalGuests <= 0)
            return (false, "Số lượng khách phải lớn hơn 0.", null);

        if (!string.IsNullOrWhiteSpace(createdByUserId))
        {
            var userExists = await _context.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == createdByUserId);
            if (!userExists)
            {
                createdByUserId = null;
            }
        }

        var allRooms = await _roomRepo.GetAllAsync();
        var selectedRooms = allRooms
            .Where(r => distinctRoomIds.Contains(r.Id))
            .ToList();

        if (selectedRooms.Count != distinctRoomIds.Count)
            return (false, "Một số phòng đã chọn không tồn tại.", null);

        foreach (var room in selectedRooms)
        {
            if (!await _bookingRepo.IsRoomAvailableAsync(room.Id, checkIn, checkOut))
                return (false, $"Phòng {room.RoomNumber} đã có lịch đặt trong khoảng thời gian này.", null);
        }

        var totalCapacity = selectedRooms.Sum(r => r.RoomType.MaxOccupancy);
        if (totalCapacity < totalGuests)
            return (false, $"Tổng sức chứa phòng đã chọn ({totalCapacity}) không đủ cho {totalGuests} khách.", null);

        var groupCode = $"GRP-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        var remainingGuests = totalGuests;
        var bookingsToCreate = new List<Booking>();

        // Phân bổ khách theo phòng có sức chứa lớn trước để tối ưu số phòng dùng.
        foreach (var room in selectedRooms.OrderByDescending(r => r.RoomType.MaxOccupancy))
        {
            if (remainingGuests <= 0) break;

            var allocatedGuests = Math.Min(room.RoomType.MaxOccupancy, remainingGuests);
            remainingGuests -= allocatedGuests;

            bookingsToCreate.Add(new Booking
            {
                GuestId = guestId,
                RoomId = room.Id,
                CheckIn = checkIn,
                CheckOut = checkOut,
                NumberOfGuests = allocatedGuests,
                Notes = notes,
                CreatedByUserId = createdByUserId,
                Status = status,
                CreatedAt = DateTime.Now,
                BookingGroupCode = groupCode,
                TotalAmount = room.RoomType.BasePrice * (checkOut - checkIn).Days
            });
        }

        await ExecuteInTransactionAsync(async () =>
        {
            foreach (var booking in bookingsToCreate)
            {
                await _bookingRepo.AddAsync(booking);

                var room = selectedRooms.First(r => r.Id == booking.RoomId);
                room.Status = status switch
                {
                    BookingStatus.CheckedIn => RoomStatus.Occupied,
                    BookingStatus.CheckedOut => RoomStatus.Available,
                    BookingStatus.Cancelled => RoomStatus.Available,
                    BookingStatus.Pending => RoomStatus.Available,
                    _ => RoomStatus.Reserved
                };
                await _roomRepo.UpdateAsync(room);
            }
        });

        var primaryBookingId = bookingsToCreate
            .OrderBy(b => b.Id)
            .Select(b => (int?)b.Id)
            .FirstOrDefault();

        return (true, $"Đã tạo {bookingsToCreate.Count} đơn đặt phòng trong cùng một lần đăng ký (mã nhóm: {groupCode}).", primaryBookingId);
    }

    public async Task<(bool Success, string Message)> UpdateStatusAsync(int bookingId, string newStatus, string? userId = null)
    {
        var booking = await _bookingRepo.GetByIdAsync(bookingId);
        if (booking is null) return (false, "Không tìm thấy đặt phòng.");

        if (!Enum.TryParse<BookingStatus>(newStatus, out var status))
            return (false, "Trạng thái không hợp lệ.");

        if (booking.Status == status)
            return (true, $"Đơn đặt phòng đã ở trạng thái này.");

        var room = await _roomRepo.GetByIdAsync(booking.RoomId);

        // Trạng thái booking thay đổi kéo theo trạng thái phòng tương ứng.
        await ExecuteInTransactionAsync(async () =>
        {
            booking.Status = status;
            await _bookingRepo.UpdateAsync(booking);

            if (room is not null)
            {
                room.Status = status switch
                {
                    BookingStatus.Confirmed  => RoomStatus.Reserved,
                    BookingStatus.CheckedIn  => RoomStatus.Occupied,
                    BookingStatus.CheckedOut => RoomStatus.Available,
                    BookingStatus.Cancelled  => RoomStatus.Available,
                    BookingStatus.Pending    => RoomStatus.Available,
                    _                        => room.Status
                };
                await _roomRepo.UpdateAsync(room);
            }
        });

        var statusVn = status switch
        {
            BookingStatus.Confirmed  => "Đã xác nhận",
            BookingStatus.CheckedIn  => "Đã nhận phòng",
            BookingStatus.CheckedOut => "Đã trả phòng",
            BookingStatus.Cancelled  => "Đã hủy",
            BookingStatus.Pending    => "Chờ duyệt",
            _                        => newStatus
        };

        return (true, $"Đã cập nhật trạng thái đặt phòng thành '{statusVn}'.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Booking booking)
    {
        if (booking.CheckOut <= booking.CheckIn)
            return (false, "Ngày trả phòng phải sau ngày nhận phòng.");

        if (!await _bookingRepo.IsRoomAvailableAsync(booking.RoomId, booking.CheckIn, booking.CheckOut, booking.Id))
            return (false, "Phòng đã có lịch đặt trong khoảng thời gian này.");

        var existingBooking = await _bookingRepo.GetByIdAsync(booking.Id);
        if (existingBooking == null) return (false, "Không tìm thấy thông tin đặt phòng.");

        var room = await _roomRepo.GetByIdAsync(booking.RoomId);
        if (room == null) return (false, "Không tìm thấy phòng.");
        
        int nights = (booking.CheckOut - booking.CheckIn).Days;
        existingBooking.TotalAmount = room.RoomType.BasePrice * nights;
        existingBooking.GuestId = booking.GuestId;
        existingBooking.RoomId = booking.RoomId;
        existingBooking.CheckIn = booking.CheckIn;
        existingBooking.CheckOut = booking.CheckOut;
        existingBooking.NumberOfGuests = booking.NumberOfGuests;
        existingBooking.Status = booking.Status;
        existingBooking.Notes = booking.Notes;

        await ExecuteInTransactionAsync(async () =>
        {
            await _bookingRepo.UpdateAsync(existingBooking);

            room.Status = existingBooking.Status switch
            {
                BookingStatus.CheckedIn  => RoomStatus.Occupied,
                BookingStatus.CheckedOut => RoomStatus.Available,
                BookingStatus.Cancelled  => RoomStatus.Available,
                _                        => RoomStatus.Reserved
            };
            await _roomRepo.UpdateAsync(room);
        });

        return (true, "Đã cập nhật thông tin đặt phòng.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var booking = await _bookingRepo.GetByIdAsync(id);
        if (booking == null) return (false, "Không tìm thấy thông tin đặt phòng.");

        await ExecuteInTransactionAsync(async () =>
        {
            await _bookingRepo.DeleteAsync(id);

            if (booking.Status == BookingStatus.CheckedIn || booking.Status == BookingStatus.Confirmed)
            {
                var room = await _roomRepo.GetByIdAsync(booking.RoomId);
                if (room != null)
                {
                    room.Status = RoomStatus.Available;
                    await _roomRepo.UpdateAsync(room);
                }
            }
        });
        return (true, "Đã xóa đơn đặt phòng.");
    }

    public Task<(bool Success, string Message)> CancelAsync(int bookingId)
        => UpdateStatusAsync(bookingId, nameof(BookingStatus.Cancelled));

    /// <summary>
    /// Lấy danh sách booking gần đây theo tiêu chí thời gian.
    /// </summary>
    public Task<List<Booking>> GetRecentAsync(int count = 10) => _bookingRepo.GetRecentAsync(count);

    /// <summary>
    /// Thực hiện nghiệp vụ của miền booking.
    /// </summary>
    public Task<int> GetGuestCountThisMonthAsync() => _bookingRepo.GetGuestCountThisMonthAsync();

    /// <summary>
    /// Thực hiện nghiệp vụ của miền booking.
    /// </summary>
    public Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int excludeBookingId = 0)
        => _bookingRepo.IsRoomAvailableAsync(roomId, checkIn, checkOut, excludeBookingId);

    /// <summary>
    /// Lấy danh sách booking gần đây theo tiêu chí thời gian.
    /// </summary>
    public Task<List<Booking>> GetRecentByRoomIdAsync(int roomId, int count = 5)
        => _bookingRepo.GetByRoomIdAsync(roomId, count);

    /// <summary>
    /// Lấy dữ liệu booking theo điều kiện chỉ định.
    /// </summary>
    public Task<List<Booking>> GetByGroupCodeAsync(string groupCode)
        => _bookingRepo.GetByGroupCodeAsync(groupCode);

    /// <summary>
    /// Tìm kiếm booking theo các bộ lọc đầu vào.
    /// </summary>
    public Task<List<Booking>> SearchAsync(string? query, string? status)
        => _bookingRepo.SearchAsync(query, status);

    public async Task<(bool Success, string Message, int CancelledCount)> AutoCancelNoShowAsync(DateTime now)
    {
        var settings = _hotelSettings ?? new HotelSettings();

        var pendingBookings = await _context.Bookings
            .Include(b => b.Room)
            .Where(b =>
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed) &&
                b.CheckIn <= now)
            .ToListAsync();

        // Chọn các booking đã qua deadline no-show theo cấu hình HotelSettings.
        var bookingsToCancel = pendingBookings
            .Where(b => settings.GetNoShowDeadline(b.CheckIn) <= now)
            .ToList();

        if (bookingsToCancel.Count == 0)
        {
            return (true, "Không có booking no-show cần tự động hủy.", 0);
        }

        try
        {
            await ExecuteInTransactionAsync(async () =>
            {
                foreach (var booking in bookingsToCancel)
                {
                    booking.Status = BookingStatus.Cancelled;
                    if (booking.Room is not null)
                    {
                        booking.Room.Status = RoomStatus.Available;
                    }
                }

                await _context.SaveChangesAsync();
            });

            return (true, $"Đã tự động hủy {bookingsToCancel.Count} booking no-show.", bookingsToCancel.Count);
        }
        catch (Exception ex)
        {
            return (false, $"Lỗi khi tự động hủy booking no-show: {ex.Message}", 0);
        }
    }

    private async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}

