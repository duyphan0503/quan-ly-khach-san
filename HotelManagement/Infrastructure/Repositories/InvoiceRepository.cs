using Microsoft.EntityFrameworkCore;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Infrastructure.Repositories.Interfaces;

namespace HotelManagement.Infrastructure.Repositories;

/// <summary>
/// Repository thao tác hóa đơn, gồm truy vấn doanh thu và sinh mã hóa đơn.
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    /// <summary>
    /// Khởi tạo lớp InvoiceRepository và nạp các dependency cần thiết.
    /// </summary>
    public InvoiceRepository(AppDbContext context) => _context = context;

    /// <summary>
    /// Lấy toàn bộ hóa đơn kèm thông tin booking, khách, phòng và chi tiết hóa đơn,
    /// dùng cho màn hình danh sách quản trị.
    /// </summary>
    public async Task<List<Invoice>> GetAllAsync()
        => await _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Guest)
            .Include(i => i.Booking).ThenInclude(b => b.Room)
            .Include(i => i.InvoiceDetails)
            .AsNoTracking()
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();

    /// <summary>
    /// Lấy chi tiết một hóa đơn theo Id, bao gồm đầy đủ navigation để render trang chi tiết/in hóa đơn.
    /// </summary>
    public async Task<Invoice?> GetByIdAsync(int id)
        => await _context.Invoices
            .Include(i => i.Booking).ThenInclude(b => b.Guest)
            .Include(i => i.Booking).ThenInclude(b => b.Room).ThenInclude(r => r.RoomType)
            .Include(i => i.InvoiceDetails).ThenInclude(d => d.Service)
            .Include(i => i.CreatedByUser)
            .FirstOrDefaultAsync(i => i.Id == id);

    /// <summary>
    /// Lấy hóa đơn theo booking. Nếu booking có nhiều hóa đơn, ưu tiên hóa đơn đang Pending
    /// để service tiếp tục thêm/xóa dịch vụ trước khi finalize.
    /// </summary>
    public async Task<Invoice?> GetByBookingIdAsync(int bookingId)
        => await _context.Invoices
            .Include(i => i.InvoiceDetails)
            .Where(i => i.BookingId == bookingId)
            // Ưu tiên trả về hóa đơn pending trước để service tiếp tục cập nhật.
            .OrderBy(i => i.Status == InvoiceStatus.Pending ? 0 : 1)
            .ThenByDescending(i => i.InvoiceDate)
            .FirstOrDefaultAsync();

    /// <summary>
    /// Thêm mới hóa đơn vào cơ sở dữ liệu.
    /// </summary>
    public async Task AddAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Cập nhật hóa đơn (tổng tiền, trạng thái, thuế, giảm giá...).
    /// </summary>
    public async Task UpdateAsync(Invoice invoice)
    {
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Xóa một dòng chi tiết khỏi hóa đơn (ví dụ: hủy dịch vụ đã thêm nhầm).
    /// </summary>
    public async Task DeleteDetailAsync(InvoiceDetail detail)
    {
        _context.InvoiceDetails.Remove(detail);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Tính tổng doanh thu lũy kế của toàn hệ thống,
    /// chỉ cộng các hóa đơn đã thanh toán (Paid).
    /// </summary>
    public async Task<decimal> GetTotalRevenueAsync()
        => await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

    /// <summary>
    /// Tính doanh thu của một tháng cụ thể trong năm,
    /// dùng cho KPI tháng hiện tại/tháng trước trên dashboard.
    /// </summary>
    public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        => await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Paid
                     && i.InvoiceDate.Year == year
                     && i.InvoiceDate.Month == month)
            .SumAsync(i => (decimal?)i.GrandTotal) ?? 0;

    /// <summary>
    /// Trả về mảng doanh thu 12 tháng của một năm (tháng không có dữ liệu sẽ là 0),
    /// phục vụ biểu đồ cột/đường trên dashboard.
    /// </summary>
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

    /// <summary>
    /// Sinh mã hóa đơn theo quy tắc HDyyyyMMxxxx, tăng dần theo tháng hiện tại.
    /// </summary>
    public async Task<string> GenerateInvoiceNumberAsync()
    {
        // Quy tắc: HD + yyyyMM + số thứ tự 4 chữ số trong tháng.
        var today = DateTime.Now;
        var prefix = $"HD{today:yyyyMM}";
        var count = await _context.Invoices
            .CountAsync(i => i.InvoiceNumber.StartsWith(prefix));
        return $"{prefix}{(count + 1):D4}"; // VD: HD20260301
    }
}
