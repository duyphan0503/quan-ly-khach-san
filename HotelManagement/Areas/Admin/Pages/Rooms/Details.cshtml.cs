using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Rooms;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Hiển thị thông tin chi tiết phòng và lịch sử đặt gần đây của phòng đó.
/// </summary>
public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ phòng và dịch vụ đặt phòng.
    /// </summary>
    public DetailsModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public Room Room { get; set; } = default!;
    public List<Booking> RecentBookings { get; set; } = [];

    /// <summary>
    /// Nạp phòng theo id và truy vấn danh sách booking gần nhất để phục vụ màn hình chi tiết.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var room = await _roomService.GetByIdAsync(id.Value);
        if (room == null) return NotFound();

        Room = room;
        RecentBookings = await _bookingService.GetRecentByRoomIdAsync(Room.Id);
        
        return Page();
    }
}
