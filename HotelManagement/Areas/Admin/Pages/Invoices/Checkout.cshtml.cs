using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Invoices;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Điều phối quy trình checkout: dựng hóa đơn nháp, nhận thông tin thanh toán và chốt thu tiền.
/// </summary>
public class CheckoutModel : PageModel
{
    private readonly IInvoiceService _invoiceService;
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Khởi tạo PageModel với các dịch vụ hóa đơn, đặt phòng và người dùng hiện tại.
    /// </summary>
    public CheckoutModel(
        IInvoiceService invoiceService,
        IBookingService bookingService,
        UserManager<ApplicationUser> userManager)
    {
        _invoiceService = invoiceService;
        _bookingService = bookingService;
        _userManager = userManager;
    }

    public Booking Booking { get; set; } = default!;
    public Invoice DraftInvoice { get; set; } = default!;
    public List<Booking> GroupCheckedInBookings { get; set; } = new();
    public bool IsGroupCheckoutAvailable => GroupCheckedInBookings.Count > 1;
    public decimal GroupSubTotal => GroupCheckedInBookings.Sum(b => b.TotalAmount);

    [BindProperty]
    public CheckoutInputModel Input { get; set; } = new();

    /// <summary>
    /// Dữ liệu đầu vào khi xác nhận checkout.
    /// </summary>
    public class CheckoutInputModel
    {
        [Required]
        public int BookingId { get; set; }

        [Display(Name = "Giảm giá (VNĐ)")]
        [Range(0, 1000000000)]
        public decimal Discount { get; set; } = 0;

        [Display(Name = "Thuế (VNĐ)")]
        [Range(0, 1000000000)]
        public decimal Tax { get; set; } = 0;

        [Display(Name = "Phương thức TT")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Thanh toán toàn bộ nhóm phòng")]
        public bool CheckoutWholeGroup { get; set; } = true;
    }

    /// <summary>
    /// Nạp booking đang ở trạng thái CheckedIn và chuẩn bị hóa đơn nháp để hiển thị màn hình thanh toán.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int bookingId)
    {
        var booking = await _bookingService.GetByIdAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.CheckedIn)
        {
            TempData["ErrorMessage"] = "Không tìm thấy phiên đặt phòng hoặc khách chưa Check-in.";
            return RedirectToPage("/Bookings/Index");
        }

        Booking = booking;
        DraftInvoice = await _invoiceService.GetOrCreateDraftInvoiceAsync(bookingId);

        Input = new CheckoutInputModel
        {
            BookingId = bookingId,
            Discount = DraftInvoice.Discount,
            Tax = DraftInvoice.Tax,
            PaymentMethod = DraftInvoice.PaymentMethod,
            CheckoutWholeGroup = true
        };

        await LoadGroupCheckoutAsync(booking);

        return Page();
    }

    /// <summary>
    /// Chốt thanh toán cho một phòng hoặc toàn bộ nhóm phòng cùng mã booking group.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var booking = await _bookingService.GetByIdAsync(Input.BookingId);
            if (booking != null)
            {
                Booking = booking;
                DraftInvoice = await _invoiceService.GetOrCreateDraftInvoiceAsync(Input.BookingId);
                await LoadGroupCheckoutAsync(booking);
            }
            return Page();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        var (success, message, invoices) = await _invoiceService.FinalizeInvoicesAsync(
            Input.BookingId,
            Input.Discount,
            Input.Tax,
            Input.PaymentMethod,
            Input.CheckoutWholeGroup,
            currentUser?.Id
        );

        if (!success)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToPage(new { bookingId = Input.BookingId });
        }

        TempData["SuccessMessage"] = message;
        if (invoices.Count == 1)
        {
            return RedirectToPage("./Details", new { id = invoices[0].Id });
        }

        return RedirectToPage("./Index");
    }

    /// <summary>
    /// Nạp danh sách phòng đang lưu trú cùng BookingGroupCode để hỗ trợ checkout theo nhóm.
    /// </summary>
    private async Task LoadGroupCheckoutAsync(Booking rootBooking)
    {
        GroupCheckedInBookings = new List<Booking>();
        if (string.IsNullOrWhiteSpace(rootBooking.BookingGroupCode))
        {
            return;
        }

        var groupBookings = await _bookingService.GetByGroupCodeAsync(rootBooking.BookingGroupCode);
        GroupCheckedInBookings = groupBookings
            .Where(b => b.Status == BookingStatus.CheckedIn)
            .OrderBy(b => b.Room?.RoomNumber)
            .ToList();
    }
}
