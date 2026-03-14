using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Rooms;

[Authorize(Roles = "Manager,Receptionist")]
public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;
    private readonly IBookingService _bookingService;

    public DetailsModel(IRoomService roomService, IBookingService bookingService)
    {
        _roomService = roomService;
        _bookingService = bookingService;
    }

    public Room Room { get; set; } = default!;
    public List<Booking> RecentBookings { get; set; } = [];

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
