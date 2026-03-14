using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Repositories.Interfaces;

public interface IInvoiceRepository
{
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByBookingIdAsync(int bookingId);
    Task AddAsync(Invoice invoice);
    Task UpdateAsync(Invoice invoice);
    Task DeleteDetailAsync(InvoiceDetail detail);
    Task<decimal> GetTotalRevenueAsync();
    Task<decimal> GetMonthlyRevenueAsync(int year, int month);
    Task<List<decimal>> GetMonthlyRevenueChartAsync(int year); // 12 tháng cho Chart.js
    Task<string> GenerateInvoiceNumberAsync();
}
