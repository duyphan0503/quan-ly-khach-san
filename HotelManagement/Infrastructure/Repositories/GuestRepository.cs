using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly AppDbContext _context;

    public GuestRepository(AppDbContext context) => _context = context;

    public async Task<List<Guest>> GetAllAsync()
        => await _context.Guests
            .AsNoTracking()
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<Guest?> GetByIdAsync(int id)
        => await _context.Guests
            .Include(g => g.Bookings)
                .ThenInclude(b => b.Room)
            .FirstOrDefaultAsync(g => g.Id == id);

    public async Task AddAsync(Guest guest)
    {
        _context.Guests.Add(guest);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guest guest)
    {
        _context.Guests.Update(guest);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var guest = await _context.Guests.FindAsync(id);
        if (guest is not null)
        {
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsCCCDAsync(string cccd, int excludeId = 0)
        => await _context.Guests
            .AnyAsync(g => g.CCCD == cccd && g.Id != excludeId);

    public async Task<bool> ExistsPhoneAsync(string phoneNumber, int excludeId = 0)
        => await _context.Guests
            .AnyAsync(g => g.PhoneNumber == phoneNumber && g.Id != excludeId);

    public async Task<Guest?> SearchByPhoneOrCCCDAsync(string query)
        => await _context.Guests
            .FirstOrDefaultAsync(g => g.PhoneNumber == query || g.CCCD == query);
    public async Task<Guest?> GetByUserIdAsync(string userId)
        => await _context.Guests.FirstOrDefaultAsync(g => g.UserId == userId);

    public async Task<List<Guest>> GetAllByUserIdAsync(string userId)
        => await _context.Guests
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

    public async Task<(List<Guest> Items, int TotalCount)> GetPagedAsync(string? searchQuery, int pageIndex, int pageSize)
    {
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
