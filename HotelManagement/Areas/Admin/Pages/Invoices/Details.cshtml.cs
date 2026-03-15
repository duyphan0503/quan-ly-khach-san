using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Invoices;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Hiển thị chi tiết một hóa đơn theo id, bao gồm thông tin khách lưu trú và các dòng chi tiết thanh toán.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IInvoiceService _invoiceService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ truy vấn hóa đơn.
    /// </summary>
    public DetailsModel(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public Invoice Invoice { get; set; } = default!;

    /// <summary>
    /// Nạp hóa đơn theo id; trả về 404 khi thiếu id hoặc không tìm thấy dữ liệu.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var invoice = await _invoiceService.GetByIdAsync(id.Value);
        if (invoice == null) return NotFound();

        Invoice = invoice;

        return Page();
    }
}
