using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Identity.Pages.Account.Manage;

public class BookingsModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly UserManager<ApplicationUser> _userManager;

    public BookingsModel(IBookingService bookingService, UserManager<ApplicationUser> userManager)
    {
        _bookingService = bookingService;
        _userManager = userManager;
    }

    public List<Booking> MyBookings { get; set; } = new();

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
