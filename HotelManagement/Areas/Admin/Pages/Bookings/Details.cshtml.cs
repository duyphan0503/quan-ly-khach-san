using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Bookings;

[Authorize(Roles = "Manager,Receptionist")]
public class DetailsModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IInvoiceService _invoiceService;
    private readonly IServiceService _serviceService;
    private readonly IGuestService _guestService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DetailsModel(
        IBookingService bookingService,
        IInvoiceService invoiceService,
        IServiceService serviceService,
        IGuestService guestService,
        UserManager<ApplicationUser> userManager)
    {
        _bookingService = bookingService;
        _invoiceService = invoiceService;
        _serviceService = serviceService;
        _guestService = guestService;
        _userManager = userManager;
    }

    public Booking Booking { get; set; } = default!;
    public Invoice? DraftInvoice { get; set; }
    public List<Service> ActiveServices { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _bookingService.GetByIdAsync(id.Value);
        if (booking == null) return NotFound();

        Booking = booking;

        // Fallback avatar: nếu Guest chưa có AvatarUrl nhưng đã liên kết UserId thì lấy từ AspNetUsers.
        if (Booking.Guest != null &&
            string.IsNullOrWhiteSpace(Booking.Guest.AvatarUrl) &&
            !string.IsNullOrWhiteSpace(Booking.Guest.UserId))
        {
            var linkedUser = await _userManager.FindByIdAsync(Booking.Guest.UserId);
            if (!string.IsNullOrWhiteSpace(linkedUser?.AvatarUrl))
            {
                Booking.Guest.AvatarUrl = linkedUser.AvatarUrl;
                await _guestService.UpdateAsync(Booking.Guest);
            }
        }

        if (Booking.Status == BookingStatus.CheckedIn)
        {
            DraftInvoice = await _invoiceService.GetByBookingIdAsync(Booking.Id);
            ActiveServices = await _serviceService.GetActiveAsync();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostConfirmAsync(int id)
    {
        var (success, msg) = await _bookingService.UpdateStatusAsync(id, nameof(BookingStatus.Confirmed));
        if (success) TempData["SuccessMessage"] = msg;
        else TempData["ErrorMessage"] = msg;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCancelAsync(int id)
    {
        var (success, msg) = await _bookingService.CancelAsync(id);
        if (success) TempData["SuccessMessage"] = msg;
        else TempData["ErrorMessage"] = msg;

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCheckInAsync(int id)
    {
        var (success, msg) = await _bookingService.UpdateStatusAsync(id, nameof(BookingStatus.CheckedIn));
        if (success)
        {
            // Auto create draft invoice
            await _invoiceService.GetOrCreateDraftInvoiceAsync(id);
            TempData["SuccessMessage"] = "Đã nhận phòng thành công! Hóa đơn nháp đã được tạo.";
        }
        else
        {
            TempData["ErrorMessage"] = msg;
        }
        
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostAddServiceAsync(int id, int serviceId, int quantity)
    {
        if (quantity <= 0)
        {
            TempData["ErrorMessage"] = "Số lượng phải lớn hơn 0.";
            return RedirectToPage(new { id });
        }

        var (success, msg) = await _invoiceService.AddServiceToInvoiceAsync(id, serviceId, quantity);
        if (success)
        {
            TempData["SuccessMessage"] = msg;
        }
        else
        {
            TempData["ErrorMessage"] = msg;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveServiceAsync(int id, int detailId)
    {
        var draft = await _invoiceService.GetByBookingIdAsync(id);
        if (draft != null)
        {
            var (success, msg) = await _invoiceService.RemoveInvoiceDetailAsync(draft.Id, detailId);
            if (success) TempData["SuccessMessage"] = msg;
            else TempData["ErrorMessage"] = msg;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCheckOutAsync(int id)
    {
        // Simple checkout with default values (0 discount, 0 tax, Cash)
        var (success, msg, _) = await _invoiceService.FinalizeInvoiceAsync(id, 0, 0, PaymentMethod.Cash);
        if (success) TempData["SuccessMessage"] = "Thanh toán và trả phòng thành công!";
        else TempData["ErrorMessage"] = msg;

        return RedirectToPage(new { id });
    }
}
