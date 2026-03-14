using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly AppDbContext _context;

    // ── Compiled Query for Performance ──
    private static readonly Func<AppDbContext, int, int, Task<int>> _guestCountMonthlyQuery = 
        EF.CompileAsyncQuery((AppDbContext context, int month, int year) =>
            context.Bookings
                .Where(b => b.CheckIn.Month == month && b.CheckIn.Year == year && b.Status != BookingStatus.Cancelled)
                .Select(b => b.GuestId)
                .Distinct()
                .Count());

    public BookingRepository(AppDbContext context) => _context = context;

    public async Task<List<Booking>> GetAllAsync()
        => await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room!)
                .ThenInclude(r => r.RoomType)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<Booking?> GetByIdAsync(int id)
        => await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(b => b.CreatedByUser)
            .Include(b => b.Invoices)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Booking booking)
    {
        var existingBooking = _context.Bookings.Local.FirstOrDefault(b => b.Id == booking.Id)
            ?? await _context.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);

        if (existingBooking is null)
        {
            return;
        }

        // Chỉ cập nhật scalar fields, tránh attach graph gây conflict tracking.
        existingBooking.GuestId = booking.GuestId;
        existingBooking.RoomId = booking.RoomId;
        existingBooking.CreatedByUserId = booking.CreatedByUserId;
        existingBooking.BookingGroupCode = booking.BookingGroupCode;
        existingBooking.CheckIn = booking.CheckIn;
        existingBooking.CheckOut = booking.CheckOut;
        existingBooking.NumberOfGuests = booking.NumberOfGuests;
        existingBooking.Status = booking.Status;
        existingBooking.TotalAmount = booking.TotalAmount;
        existingBooking.Notes = booking.Notes;
        existingBooking.CreatedAt = booking.CreatedAt;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking is not null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Booking>> GetRecentAsync(int count = 10)
        => await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room!)
                .ThenInclude(r => r.RoomType)
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .Take(count)
            .ToListAsync();

    public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime checkIn, DateTime checkOut, int excludeBookingId = 0)
    {
        return !await _context.Bookings
            .AnyAsync(b =>
                b.RoomId == roomId &&
                b.Id != excludeBookingId &&
                b.Status != BookingStatus.Cancelled &&
                b.Status != BookingStatus.CheckedOut &&
                b.CheckIn < checkOut &&
                b.CheckOut > checkIn);
    }

    public async Task<int> GetGuestCountThisMonthAsync()
    {
        var now = DateTime.Now;
        // Using Compiled Query for massive speedup in heavy datasets
        return await _guestCountMonthlyQuery(_context, now.Month, now.Year);
    }

    public async Task<int> GetGuestCountByMonthAsync(int year, int month)
    {
        if (month is < 1 or > 12)
        {
            return 0;
        }

        return await _guestCountMonthlyQuery(_context, month, year);
    }

    public async Task<int> GetOccupiedRoomCountAtAsync(DateTime atTime)
    {
        var snapshotTime = atTime;

        return await _context.Bookings
            .Where(b => b.Status != BookingStatus.Cancelled
                        && b.CheckIn <= snapshotTime
                        && b.CheckOut > snapshotTime)
            .Select(b => b.RoomId)
            .Distinct()
            .CountAsync();
    }

    public async Task<int> GetOccupiedRoomNightsAsync(DateTime periodStart, DateTime periodEnd)
    {
        var start = periodStart.Date;
        var end = periodEnd.Date;

        if (end <= start)
        {
            return 0;
        }

        return await _context.Bookings
            .Where(b => b.Status != BookingStatus.Cancelled
                        && b.CheckOut > start
                        && b.CheckIn < end)
            .SumAsync(b => EF.Functions.DateDiffDay(
                b.CheckIn < start ? start : b.CheckIn,
                b.CheckOut > end ? end : b.CheckOut));
    }

    public async Task<List<Booking>> GetByStatusAsync(string status)
    {
        if (!Enum.TryParse<BookingStatus>(status, out var bookingStatus))
            return [];

        return await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room!)
                .ThenInclude(r => r.RoomType)
            .Where(b => b.Status == bookingStatus)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> GetTodayNewBookingsCountAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);
        return await _context.Bookings
            .CountAsync(b => b.CreatedAt >= today && b.CreatedAt < tomorrow);
    }

    public async Task<int> GetPendingCheckinCountAsync()
    {
        return await _context.Bookings
            .CountAsync(b => b.Status == BookingStatus.Confirmed);
    }

    public async Task<Dictionary<string, int>> GetRoomTypeDistributionAsync()
    {
        var distribution = await _context.Bookings
            .Include(b => b.Room)
            .ThenInclude(r => r.RoomType)
            .Where(b => b.Room != null && b.Room.RoomType != null)
            .GroupBy(b => b.Room!.RoomType!.Name)
            .Select(g => new { RoomType = g.Key, Count = g.Count() })
            .ToListAsync();

        return distribution.ToDictionary(x => x.RoomType, x => x.Count);
    }

    public async Task<List<Booking>> GetByRoomIdAsync(int roomId, int count = 5)
    {
        return await _context.Bookings
            .Include(b => b.Guest)
            .Where(b => b.RoomId == roomId)
            .OrderByDescending(b => b.CheckIn)
            .Take(count)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Booking>> GetByGroupCodeAsync(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            return [];
        }

        return await _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room)
                .ThenInclude(r => r.RoomType)
            .Where(b => b.BookingGroupCode == groupCode)
            .OrderBy(b => b.Room.RoomNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Booking>> SearchAsync(string? query, string? status)
    {
        var dbQuery = _context.Bookings
            .Include(b => b.Guest)
            .Include(b => b.Room!)
                .ThenInclude(r => r.RoomType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            dbQuery = dbQuery.Where(b =>
                b.Id.ToString().Contains(query) ||
                (b.Guest != null && b.Guest.FullName != null && b.Guest.FullName.ToLower().Contains(query)) ||
                (b.Guest != null && b.Guest.CCCD != null && b.Guest.CCCD.Contains(query)) ||
                (b.Room != null && b.Room.RoomNumber != null && b.Room.RoomNumber.ToLower().Contains(query)));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<BookingStatus>(status, out var bookingStatus))
        {
            dbQuery = dbQuery.Where(b => b.Status == bookingStatus);
        }

        return await dbQuery
            .AsNoTracking()
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }
}
