using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Public.Pages;

public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;

    public IndexModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public List<RoomType> FeaturedRoomTypes { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Use Service instead of Repository to follow N-Tier architecture
        FeaturedRoomTypes = await _roomService.GetFeaturedRoomTypesAsync(4);
    }
}
