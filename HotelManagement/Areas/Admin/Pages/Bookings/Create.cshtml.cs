using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace HotelManagement.Areas.Admin.Pages.Bookings;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Xử lý tạo booking mới cho một hoặc nhiều phòng trong cùng một lần đặt.
/// </summary>
public class CreateModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ booking, phòng và khách.
    /// </summary>
    public CreateModel(IBookingService bookingService, IRoomService roomService, IGuestService guestService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
    }

    [BindProperty]
    public Booking Booking { get; set; } = new Booking 
    { 
        CheckIn = DateTime.Today, 
        CheckOut = DateTime.Today.AddDays(1) 
    };

    [BindProperty]
    public List<int> SelectedRoomIds { get; set; } = new();

    public List<Room> AvailableRooms { get; set; } = new();
    public SelectList Guests { get; set; } = default!;

    /// <summary>
    /// Nạp danh sách phòng/khách để hiển thị form tạo booking.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        await LoadSelectListsAsync();
        return Page();
    }

    /// <summary>
    /// Kiểm tra dữ liệu nhập và tạo booking cho các phòng được chọn.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Booking.Room");
        ModelState.Remove("Booking.Guest");
        ModelState.Remove("Booking.Invoices");
        ModelState.Remove("Booking.RoomId");

        if (Booking.GuestId <= 0)
        {
            ModelState.AddModelError("Booking.GuestId", "Vui lòng chọn khách hàng.");
        }
        
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync();
            return Page();
        }

        if (Booking.CheckOut <= Booking.CheckIn)
        {
            ModelState.AddModelError("Booking.CheckOut", "Ngày trả phòng phải sau ngày nhận phòng.");
            await LoadSelectListsAsync();
            return Page();
        }

        if (SelectedRoomIds == null || SelectedRoomIds.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một phòng.");
            await LoadSelectListsAsync();
            return Page();
        }

        var result = await _bookingService.CreateMultipleAsync(
            Booking.GuestId,
            SelectedRoomIds,
            Booking.CheckIn,
            Booking.CheckOut,
            Booking.NumberOfGuests,
            Booking.Status,
            Booking.Notes,
            User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadSelectListsAsync();
        return Page();
    }

    /// <summary>
    /// Nạp dữ liệu lựa chọn cho form (danh sách phòng và khách).
    /// </summary>
    private async Task LoadSelectListsAsync()
    {
        var rooms = await _roomService.GetAllAsync();
        var guests = await _guestService.GetAllAsync();

        AvailableRooms = rooms
            .OrderBy(r => r.RoomType?.Name)
            .ThenBy(r => r.RoomNumber)
            .ToList();
        Guests = new SelectList(guests.Select(g => new { Id = g.Id, Display = $"{g.FullName} ({g.CCCD})" }), "Id", "Display");
    }
}
