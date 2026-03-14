using HotelManagement.Application.Services.Interfaces;
using HotelManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HotelManagement.Areas.Public.Pages.Booking;

public class ConfirmationModel : PageModel
{
    private readonly IBookingService _bookingService;
    private readonly HotelSettings _hotelSettings;

    public ConfirmationModel(IBookingService bookingService, IOptions<HotelSettings> hotelSettings)
    {
        _bookingService = bookingService;
        _hotelSettings = hotelSettings.Value;
    }

    public HotelManagement.Core.Models.Booking Booking { get; set; } = null!;
    public List<HotelManagement.Core.Models.Booking> GroupBookings { get; set; } = new();
    public int TotalGuests { get; set; }
    public decimal TotalAmount { get; set; }
    public string RoomSummary { get; set; } = string.Empty;
    public bool IsGroupBooking => GroupBookings.Count > 1;
    public string CheckInTimeText { get; set; } = "14:00";
    public string CheckOutTimeText { get; set; } = "12:00";
    public int NoShowThresholdHours { get; set; } = 6;
    public DateTime NoShowCancelDeadline { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        Booking = booking;

        if (!string.IsNullOrWhiteSpace(booking.BookingGroupCode))
        {
            GroupBookings = await _bookingService.GetByGroupCodeAsync(booking.BookingGroupCode);
        }

        if (GroupBookings.Count == 0)
        {
            GroupBookings = new List<HotelManagement.Core.Models.Booking> { booking };
        }

        TotalGuests = GroupBookings.Sum(b => b.NumberOfGuests);
        TotalAmount = GroupBookings.Sum(b => b.TotalAmount);
        RoomSummary = string.Join(", ", GroupBookings.Select(b => $"{b.Room?.RoomNumber} ({b.Room?.RoomType?.Name})"));
        CheckInTimeText = _hotelSettings.GetCheckInTime().ToString(@"hh\:mm");
        CheckOutTimeText = _hotelSettings.GetCheckOutTime().ToString(@"hh\:mm");
        NoShowThresholdHours = _hotelSettings.GetNoShowThresholdHours();
        NoShowCancelDeadline = _hotelSettings.GetNoShowDeadline(booking.CheckIn);

        return Page();
    }
}
