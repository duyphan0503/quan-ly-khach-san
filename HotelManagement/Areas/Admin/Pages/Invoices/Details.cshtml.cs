using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Invoices;

[Authorize(Roles = "Manager,Receptionist")]
public class DetailsModel : PageModel
{
    private readonly IInvoiceService _invoiceService;

    public DetailsModel(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public Invoice Invoice { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var invoice = await _invoiceService.GetByIdAsync(id.Value);
        if (invoice == null) return NotFound();

        Invoice = invoice;

        return Page();
    }
}
