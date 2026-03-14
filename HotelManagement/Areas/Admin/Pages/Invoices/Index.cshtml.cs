using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Areas.Admin.Pages.Invoices;

[Authorize(Roles = "Manager,Receptionist")]
public class IndexModel : PageModel
{
    private readonly IInvoiceService _invoiceService;

    public IndexModel(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public List<Invoice> Invoices { get; set; } = new();
    public List<SelectListItem> InvoiceStatusOptions { get; private set; } = new();
    public List<SelectListItem> PaymentMethodOptions { get; private set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public const int PageSize = 5;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public InvoiceStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentMethod? PaymentMethodFilter { get; set; }

    public async Task OnGetAsync()
    {
        InvoiceStatusOptions = new List<SelectListItem>
        {
            new("Chờ thanh toán", InvoiceStatus.Pending.ToString()),
            new("Đã thanh toán", InvoiceStatus.Paid.ToString()),
            new("Đã hoàn tiền", InvoiceStatus.Refunded.ToString())
        };

        PaymentMethodOptions = new List<SelectListItem>
        {
            new("Tiền mặt", PaymentMethod.Cash.ToString()),
            new("Thẻ ngân hàng", PaymentMethod.Card.ToString()),
            new("Chuyển khoản", PaymentMethod.Transfer.ToString())
        };

        var allInvoices = (await _invoiceService.GetAllAsync())
            .OrderByDescending(i => i.InvoiceDate)
            .ToList();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var keyword = SearchQuery.Trim();
            allInvoices = allInvoices
                .Where(i =>
                    (!string.IsNullOrWhiteSpace(i.InvoiceNumber) && i.InvoiceNumber.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (i.Booking?.Guest?.FullName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.Booking?.Guest?.PhoneNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (i.Booking?.Room?.RoomNumber?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();
        }

        if (StatusFilter.HasValue)
        {
            allInvoices = allInvoices
                .Where(i => i.Status == StatusFilter.Value)
                .ToList();
        }

        if (PaymentMethodFilter.HasValue)
        {
            allInvoices = allInvoices
                .Where(i => i.PaymentMethod == PaymentMethodFilter.Value)
                .ToList();
        }

        TotalCount = allInvoices.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        Invoices = allInvoices
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
