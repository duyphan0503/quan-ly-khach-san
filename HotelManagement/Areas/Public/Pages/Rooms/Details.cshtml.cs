using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages.Rooms;

public class DetailsModel : PageModel
{
    private readonly IRoomService _roomService;

    public DetailsModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public Room Room { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckIn { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? CheckOut { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Guests { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var room = await _roomService.GetByIdAsync(id);

        if (room == null)
        {
            return NotFound();
        }

        Room = room;
        return Page();
    }
}