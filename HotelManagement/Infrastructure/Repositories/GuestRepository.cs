using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

/// <summary>
/// Repository thao tác dữ liệu khách hàng và các truy vấn tìm kiếm/phân trang.
/// </summary>
public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Khởi tạo lớp GuestRepository và nạp các dependency cần thiết.
    /// </summary>
    public GuestRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Lấy toàn bộ dữ liệu guest.
    /// </summary>
    public async Task<List<Guest>> GetAllAsync()
        => await _context.Guests
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    /// <summary>
    /// Lấy thông tin guest theo mã định danh.
    /// </summary>
    public async Task<Guest?> GetByIdAsync(int id)
        => await _context.Guests
            .Include(g => g.Bookings)
                .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(g => g.Id == id);

    /// <summary>
    /// Tạo mới dữ liệu guest.
    /// </summary>
    public async Task AddAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật thông tin dữ liệu guest.
    /// </summary>
    public async Task UpdateAsync(Guest guest)
    {
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Xóa dữ liệu guest theo tiêu chí được truyền vào.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest is not null)
        {
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Kiểm tra sự tồn tại của guest theo điều kiện xác định.
    /// </summary>
    public async Task<bool> ExistsCCCDAsync(string cccd, int excludeId = 0)
        => await _context.Guests
            .AnyAsync(g => g.CCCD == cccd && g.Id != excludeId);

    /// <summary>
    /// Kiểm tra sự tồn tại của guest theo điều kiện xác định.
    /// </summary>
    public async Task<bool> ExistsPhoneAsync(string phoneNumber, int excludeId = 0)
        => await _context.Guests
            .AnyAsync(g => g.PhoneNumber == phoneNumber && g.Id != excludeId);

    /// <summary>
    /// Tìm kiếm guest theo các bộ lọc đầu vào.
    /// </summary>
    public async Task<Guest?> SearchByPhoneOrCCCDAsync(string query)
        // Tìm chính xác theo phone/CCCD để phục vụ auto-fill nhanh tại quầy lễ tân.
        => await _context.Guests
            .FirstOrDefaultAsync(g => g.PhoneNumber == query || g.CCCD == query);
    /// <summary>
    /// Lấy dữ liệu guest theo điều kiện chỉ định.
    /// </summary>
    public async Task<Guest?> GetByUserIdAsync(string userId)
        => await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);

    /// <summary>
    /// Lấy toàn bộ dữ liệu guest.
    /// </summary>
    public async Task<List<Guest>> GetAllByUserIdAsync(string userId)
        => await _context.Guests
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<(List<Guest> Items, int TotalCount)> GetPagedAsync(string? searchQuery, int pageIndex, int pageSize)
    {
        // IQueryable giữ truy vấn ở DB, chỉ materialize sau cùng để tối ưu hiệu năng.
        var query = _context.Guests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(g =>
                g.FullName.ToLower().Contains(lowerQuery) ||
                (g.PhoneNumber != null && g.PhoneNumber.Contains(searchQuery)) ||
                (g.CCCD != null && g.CCCD.Contains(searchQuery)) ||
                (g.Email != null && g.Email.ToLower().Contains(lowerQuery)));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
