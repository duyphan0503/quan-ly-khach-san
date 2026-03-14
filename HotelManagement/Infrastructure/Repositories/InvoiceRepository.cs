using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context) => _context = context;

    public async Task<List<Invoice>> GetAllAsync()
        => await _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Guest)
            .Include(i => i.Booking).ThenInclude(b => b.Room)
            .Include(i => i.InvoiceDetails)
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

    public async Task<Invoice?> GetByIdAsync(int id)
        => await _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Guest)
            .Include(i => i.Booking).ThenInclude(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);

    public async Task<Invoice?> GetByBookingIdAsync(int bookingId)
        => await _context.Invoices
            .Include(i => i.InvoiceDetails)
            .Where(i => i.BookingId == bookingId)
            .OrderBy(i => i.Status == InvoiceStatus.Pending ? 0 : 1)
            .ThenByDescending(i => i.InvoiceDate)
            .FirstOrDefaultAsync();

    public async Task AddAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteDetailAsync(InvoiceDetail detail)
    {
        _context.InvoiceDetails.Remove(detail);
        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalRevenueAsync()
        => await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        => await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid
                     && i.InvoiceDate.Year == year
                     && i.InvoiceDate.Month == month)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

    public async Task<List<decimal>> GetMonthlyRevenueChartAsync(int year)
    {
        var revenueByMonth = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid && i.InvoiceDate.Year == year)
            .GroupBy(i => i.InvoiceDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => (decimal?)x.GrandTotal) ?? 0 })
            .ToListAsync();

        var result = new List<decimal>(12);
        for (int month = 1; month <= 12; month++)
        {
            var match = revenueByMonth.FirstOrDefault(x => x.Month == month);
            result.Add(match?.Total ?? 0m);
        }
        return result;
    }

    public async Task<string> GenerateInvoiceNumberAsync()
    {
        var today = DateTime.Now;
        var prefix = $"HD{today:yyyyMM}";
        var count = await _context.Invoices
            .CountAsync(i => i.InvoiceNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D4}"; // VD: HD20260301
    }
}
