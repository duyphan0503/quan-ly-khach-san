using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;

namespace HotelManagement.Application.Services.Interfaces;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllAsync();
    Task<Invoice?> GetByIdAsync(int id);
    Task<Invoice?> GetByBookingIdAsync(int bookingId);
    
    Task<Invoice> GetOrCreateDraftInvoiceAsync(int bookingId, string? userId = null);
    
    Task<(bool Success, string Message)> AddServiceToInvoiceAsync(int bookingId, int serviceId, int quantity);
    Task<(bool Success, string Message)> RemoveInvoiceDetailAsync(int invoiceId, int detailId);
    
    Task<(bool Success, string Message, Invoice? Invoice)> FinalizeInvoiceAsync(int bookingId, decimal discount, decimal tax, PaymentMethod method, string? userId = null);
    Task<(bool Success, string Message, List<Invoice> Invoices)> FinalizeInvoicesAsync(
        int bookingId,
        decimal discount,
        decimal tax,
        PaymentMethod method,
        bool checkoutWholeGroup,
        string? userId = null);
    Task<(bool Success, string Message, Invoice? NewInvoice)> SplitInvoiceAsync(
        int sourceInvoiceId,
        List<int> detailIds,
        string? userId = null);
    
    Task<decimal> GetTotalRevenueAsync();
    Task<List<decimal>> GetMonthlyRevenueChartAsync(int year);
}
