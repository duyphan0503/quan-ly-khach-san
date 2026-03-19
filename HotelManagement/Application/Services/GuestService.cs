using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Application.Services;

/// <summary>
/// Service quản lý hồ sơ khách: chuẩn hóa dữ liệu, chống trùng, hỗ trợ gộp profile.
/// </summary>
public class GuestService : IGuestService
{
    private readonly IGuestRepository _repo;
    private readonly AppDbContext _context;

    /// <summary>
    /// Khởi tạo lớp GuestService và nạp các dependency cần thiết.
    /// </summary>
    public GuestService(IGuestRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }

    /// <summary>
    /// Lấy toàn bộ dữ liệu guest.
    /// </summary>
    public Task<List<Guest>> GetAllAsync() => _repo.GetAllAsync();

    /// <summary>
    /// Lấy thông tin guest theo mã định danh.
    /// </summary>
    public Task<Guest?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<(bool Success, string Message)> CreateAsync(Guest guest)
    {
        // Chuẩn hóa đầu vào trước khi validate unique.
        guest.PhoneNumber = NormalizePhone(guest.PhoneNumber);
        guest.CCCD = string.IsNullOrWhiteSpace(guest.CCCD) ? null : guest.CCCD.Trim();

        if (await _repo.ExistsPhoneAsync(guest.PhoneNumber))
            return (false, $"Số điện thoại \"{guest.PhoneNumber}\" đã tồn tại trong hệ thống.");

        if (!string.IsNullOrEmpty(guest.CCCD) && await _repo.ExistsCCCDAsync(guest.CCCD))
            return (false, $"CCCD/Hộ chiếu \"{guest.CCCD}\" đã tồn tại trong hệ thống.");

        guest.CreatedAt = DateTime.Now;
        await _repo.AddAsync(guest);
        return (true, $"Đã thêm khách hàng {guest.FullName} thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Guest guest)
    {
        guest.PhoneNumber = NormalizePhone(guest.PhoneNumber);
        guest.CCCD = string.IsNullOrWhiteSpace(guest.CCCD) ? null : guest.CCCD.Trim();

        if (await _repo.ExistsPhoneAsync(guest.PhoneNumber, guest.Id))
            return (false, $"Số điện thoại \"{guest.PhoneNumber}\" đã được dùng bởi khách khác.");

        if (!string.IsNullOrEmpty(guest.CCCD) && await _repo.ExistsCCCDAsync(guest.CCCD, guest.Id))
            return (false, $"CCCD/Hộ chiếu \"{guest.CCCD}\" đã được dùng bởi khách khác.");

        await _repo.UpdateAsync(guest);
        return (true, $"Đã cập nhật thông tin khách hàng {guest.FullName}.");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var guest = await _repo.GetByIdAsync(id);
        if (guest is null) return (false, "Không tìm thấy khách hàng.");

        if (guest.Bookings.Any())
            return (false, $"Không thể xóa khách hàng {guest.FullName} vì người này đã có lịch sử đặt phòng trong hệ thống.");

        await _repo.DeleteAsync(id);
        return (true, $"Đã xóa khách hàng {guest.FullName} thành công.");
    }

    /// <summary>
    /// Tìm kiếm guest theo các bộ lọc đầu vào.
    /// </summary>
    public async Task<Guest?> SearchByPhoneOrCCCDAsync(string query)
    {
        var safeQuery = query?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(safeQuery))
        {
            return null;
        }

        var byRawQuery = await _repo.SearchByPhoneOrCCCDAsync(safeQuery);
        if (byRawQuery is not null)
        {
            return byRawQuery;
        }

        var normalizedPhone = NormalizePhone(safeQuery);
        if (!string.Equals(normalizedPhone, safeQuery, StringComparison.Ordinal))
        {
            return await _repo.SearchByPhoneOrCCCDAsync(normalizedPhone);
        }

        return null;
    }

    /// <summary>
    /// Lấy dữ liệu guest theo điều kiện chỉ định.
    /// </summary>
    public Task<Guest?> GetByUserIdAsync(string userId) => _repo.GetByUserIdAsync(userId);
    /// <summary>
    /// Lấy toàn bộ dữ liệu guest.
    /// </summary>
    public Task<List<Guest>> GetAllByUserIdAsync(string userId) => _repo.GetAllByUserIdAsync(userId);

    public async Task<(bool Success, string Message, Guest? PrimaryGuest)> ConsolidateGuestProfilesAsync(
        string? userId,
        string phone,
        string? cccd = null,
        string? email = null,
        string? fullName = null,
        string? avatarUrl = null)
    {
        var normalizedPhone = NormalizePhone(phone);
        var normalizedCccd = string.IsNullOrWhiteSpace(cccd) ? null : cccd.Trim();
        var normalizedEmail = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

        // Tìm candidate theo nhiều khóa để bắt trùng mềm (userId/phone/cccd/email).
        var candidates = await _context.Guests
            .Where(g =>
                (!string.IsNullOrWhiteSpace(userId) && g.UserId == userId) ||
                (!string.IsNullOrWhiteSpace(normalizedPhone) && g.PhoneNumber == normalizedPhone) ||
                (!string.IsNullOrWhiteSpace(normalizedCccd) && g.CCCD == normalizedCccd) ||
                (!string.IsNullOrWhiteSpace(normalizedEmail) && g.Email == normalizedEmail))
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();

        if (candidates.Count == 0)
        {
            return (true, "Không có hồ sơ trùng cần gộp.", null);
        }

        var primary = candidates
            .FirstOrDefault(g =>
                !string.IsNullOrWhiteSpace(userId) &&
                g.UserId == userId &&
                (!string.IsNullOrWhiteSpace(normalizedPhone) && NormalizePhone(g.PhoneNumber) == normalizedPhone))
            ?? candidates.FirstOrDefault(g => !string.IsNullOrWhiteSpace(userId) && g.UserId == userId)
            ?? candidates.First();

        var duplicates = candidates.Where(g => g.Id != primary.Id).ToList();

        // Nếu có hồ sơ gắn UserId khác thì không tự gộp để tránh nhập nhầm dữ liệu đa tài khoản.
        var conflicted = duplicates.FirstOrDefault(g =>
            !string.IsNullOrWhiteSpace(g.UserId) &&
            !string.IsNullOrWhiteSpace(userId) &&
            !string.Equals(g.UserId, userId, StringComparison.Ordinal));
        if (conflicted is not null)
        {
            return (false, "Phát hiện hồ sơ trùng đang thuộc tài khoản khác, vui lòng xử lý thủ công.", null);
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (duplicates.Count > 0)
                {
                    var duplicateIds = duplicates.Select(g => g.Id).ToList();

                    // Chuyển toàn bộ lịch sử booking sang hồ sơ chuẩn.
                    var bookings = await _context.Bookings
                        .Where(b => duplicateIds.Contains(b.GuestId))
                        .ToListAsync();
                    foreach (var booking in bookings)
                    {
                        booking.GuestId = primary.Id;
                    }
                    await _context.SaveChangesAsync();

                    _context.Guests.RemoveRange(duplicates);
                    await _context.SaveChangesAsync();
                }

                // Đồng bộ thông tin về hồ sơ chuẩn sau khi đã chuyển lịch sử + xóa hồ sơ trùng.
                if (string.IsNullOrWhiteSpace(primary.UserId))
                {
                    primary.UserId = userId;
                }
                if (!string.IsNullOrWhiteSpace(normalizedPhone))
                {
                    primary.PhoneNumber = normalizedPhone;
                }
                if (string.IsNullOrWhiteSpace(primary.CCCD) && !string.IsNullOrWhiteSpace(normalizedCccd))
                {
                    primary.CCCD = normalizedCccd;
                }
                if (string.IsNullOrWhiteSpace(primary.Email) && !string.IsNullOrWhiteSpace(normalizedEmail))
                {
                    primary.Email = normalizedEmail;
                }
                if (string.IsNullOrWhiteSpace(primary.FullName) && !string.IsNullOrWhiteSpace(fullName))
                {
                    primary.FullName = fullName.Trim();
                }
                if (string.IsNullOrWhiteSpace(primary.AvatarUrl) && !string.IsNullOrWhiteSpace(avatarUrl))
                {
                    primary.AvatarUrl = avatarUrl;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, duplicates.Count > 0 ? $"Đã gộp {duplicates.Count} hồ sơ khách trùng." : "Không có hồ sơ trùng cần gộp.", primary);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public Task<(List<Guest> Items, int TotalCount)> GetPagedAsync(string? searchQuery, int pageIndex, int pageSize)
        => _repo.GetPagedAsync(searchQuery, pageIndex, pageSize);

    private static string NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("84", StringComparison.Ordinal) && digits.Length == 11)
        {
            return $"0{digits[2..]}";
        }

        return digits;
    }
}
