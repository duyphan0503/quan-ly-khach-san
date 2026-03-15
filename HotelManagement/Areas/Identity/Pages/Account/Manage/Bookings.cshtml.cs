using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Identity.Pages.Account.Manage;

/// <summary>
/// PageModel xử lý luồng tài khoản 'Bookings.cshtml'.
/// </summary>
public class BookingsModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    /// <summary>
    /// Khởi tạo lớp BookingsModel và nạp các dependency cần thiết.
    /// </summary>
    public BookingsModel(IBookingService bookingService, UserManager<ApplicationUser> userManager)
    {
        _bookingService = bookingService;
        _userManager = userManager;
    }

    public List<Booking> MyBookings { get; set; } = new();

    /// <summary>
    /// Xử lý yêu cầu GET để nạp dữ liệu và hiển thị trang.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var allBookings = await _bookingService.GetAllAsync();
        
        // Lọc các đơn đặt phòng thuộc về Guest có cùng email hoặc thông tin cá nhân của User này
        // Ở đây giả định Business logic là lấy booking dựa trên Guest.Email khớp với User.Email
        MyBookings = allBookings
            .Where(b => b.Guest?.Email == user.Email)
            .OrderByDescending(b => b.CreatedAt)
            .ToList();

        return Page();
    }
}
