using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Areas.Admin.Pages.Bookings;

[Authorize(Roles = "Manager,Receptionist")]
public class EditModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly IRoomService _roomService;
    private readonly IGuestService _guestService;

    public EditModel(IBookingService bookingService, IRoomService roomService, IGuestService guestService)
    {
        _bookingService = bookingService;
        _roomService = roomService;
        _guestService = guestService;
    }

    [BindProperty]
    public Booking Booking { get; set; } = default!;

    public SelectList Rooms { get; set; } = default!;
    public SelectList Guests { get; set; } = default!;
    public Dictionary<int, decimal> RoomPrices { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null) return NotFound();

        var booking = await _bookingService.GetByIdAsync(id.Value);
        if (booking == null) return NotFound();

        Booking = booking;
        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Booking.Room");
        ModelState.Remove("Booking.Guest");
        ModelState.Remove("Booking.Invoices");
        
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

        var result = await _bookingService.UpdateAsync(Booking);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        ModelState.AddModelError(string.Empty, result.Message);
        await LoadSelectListsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        if (Booking.Id == 0) return NotFound();

        var result = await _bookingService.DeleteAsync(Booking.Id);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = result.Message;
        return RedirectToPage("./Edit", new { id = Booking.Id });
    }

    private async Task LoadSelectListsAsync()
    {
        var rooms = await _roomService.GetAllAsync();
        var guests = await _guestService.GetAllAsync();

        Rooms = new SelectList(rooms.Select(r => new { Id = r.Id, Display = $"Phòng {r.RoomNumber}" }), "Id", "Display");
        Guests = new SelectList(guests.Select(g => new { Id = g.Id, Display = $"{g.FullName}" }), "Id", "Display");

        RoomPrices = rooms.ToDictionary(r => r.Id, r => r.RoomType.BasePrice);
    }
}
