using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Invoices;

[Authorize(Roles = "Manager,Receptionist")]
public class SplitModel : PageModel
{
    private readonly IInvoiceService _invoiceService;

    public SplitModel(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public Invoice SourceInvoice { get; set; } = default!;

    [BindProperty]
    public SplitInputModel Input { get; set; } = new();

    public class SplitInputModel
    {
        [Required]
        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một dòng chi tiết để tách.")]
        public List<int> SelectedDetailIds { get; set; } = new();
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        if (invoice.InvoiceDetails.Count <= 1 || invoice.Status != Core.Models.Enums.InvoiceStatus.Pending)
        {
            TempData["ErrorMessage"] = "Hóa đơn này không đủ điều kiện để tách chứng từ.";
            return RedirectToPage("./Details", new { id });
        }

        SourceInvoice = invoice;
        Input = new SplitInputModel { InvoiceId = id };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.SelectedDetailIds == null || Input.SelectedDetailIds.Count == 0)
        {
            ModelState.AddModelError("Input.SelectedDetailIds", "Vui lòng chọn ít nhất một dòng chi tiết để tách.");
        }

        if (!ModelState.IsValid)
        {
            var invalidInvoice = await _invoiceService.GetByIdAsync(Input.InvoiceId);
            if (invalidInvoice != null)
            {
                SourceInvoice = invalidInvoice;
            }
            return Page();
        }

        var result = await _invoiceService.SplitInvoiceAsync(
            Input.InvoiceId,
            Input.SelectedDetailIds ?? new List<int>(),
            User?.Identity?.Name);

        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToPage("./Details", new { id = Input.InvoiceId });
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToPage("./Details", new { id = result.NewInvoice?.Id ?? Input.InvoiceId });
    }
}
