using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Guests;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Hiển thị chi tiết hồ sơ khách và lịch sử lưu trú gần đây.
/// </summary>
public class DetailsModel : PageModel
{
    private const int DefaultActivityPageSize = 8;
    private readonly IGuestService _guestService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ truy vấn khách.
    /// </summary>
    public DetailsModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    public Guest Guest { get; set; } = default!;
    public List<Booking> ActivityBookings { get; private set; } = new();
    public int ActivityTotalCount { get; private set; }
    public int ActivityPageSize => DefaultActivityPageSize;

    /// <summary>
    /// Nạp thông tin khách và trang đầu tiên của lịch sử booking để hiển thị màn hình chi tiết.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var guest = await _guestService.GetByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        Guest = guest;
        var orderedBookings = guest.Bookings?
            .OrderByDescending(b => b.CreatedAt)
            .ToList() ?? new List<Booking>();

        ActivityTotalCount = orderedBookings.Count;
        ActivityBookings = orderedBookings
            .Take(DefaultActivityPageSize)
            .ToList();

        return Page();
    }

    /// <summary>
    /// Trả dữ liệu JSON cho chức năng "xem thêm" lịch sử hoạt động của khách.
    /// </summary>
    public async Task<IActionResult> OnGetActivityPageAsync(int id, int pageNumber = 1)
    {
        if (pageNumber < 1)
        {
            pageNumber = 1;
        }

        var guest = await _guestService.GetByIdAsync(id);
        if (guest == null)
        {
            return NotFound();
        }

        var orderedBookings = guest.Bookings?
            .OrderByDescending(b => b.CreatedAt)
            .ToList() ?? new List<Booking>();

        var items = orderedBookings
            .Skip((pageNumber - 1) * DefaultActivityPageSize)
            .Take(DefaultActivityPageSize)
            .Select(booking =>
            {
                var (statusClass, statusIcon, statusLabel) = MapBookingStatus(booking.Status);
                return new
                {
                    Id = booking.Id,
                    IdDisplay = booking.Id.ToString("D4"),
                    RoomNumber = booking.Room?.RoomNumber,
                    RoomTypeName = booking.Room?.RoomType?.Name,
                    CheckIn = booking.CheckIn.ToString("dd/MM/yyyy"),
                    CheckOut = booking.CheckOut.ToString("dd/MM/yyyy"),
                    StatusClass = statusClass,
                    StatusIcon = statusIcon,
                    StatusLabel = statusLabel
                };
            })
            .ToList();

        var loadedCount = pageNumber * DefaultActivityPageSize;
        return new JsonResult(new
        {
            Items = items,
            Total = orderedBookings.Count,
            HasMore = loadedCount < orderedBookings.Count
        });
    }

    private static (string StatusClass, string StatusIcon, string StatusLabel) MapBookingStatus(BookingStatus status)
    {
        return status switch
        {
            BookingStatus.Confirmed => ("bg-blue-500/10 text-blue-400 border-blue-500/20 shadow-blue-500/5", "lucide:circle-check", "Đã Xác Nhận"),
            BookingStatus.CheckedIn => ("bg-indigo-500/10 text-indigo-400 border-indigo-500/20 shadow-indigo-500/5", "lucide:key", "Đang Lưu Trú"),
            BookingStatus.CheckedOut => ("bg-emerald-500/10 text-emerald-400 border-emerald-500/20 shadow-emerald-500/5", "lucide:log-out", "Hoàn Thành"),
            BookingStatus.Cancelled => ("bg-rose-500/10 text-rose-400 border-rose-500/20 shadow-rose-500/5", "lucide:x-circle", "Đã Hủy"),
            _ => ("bg-slate-500/10 text-slate-400 border-slate-500/20", "lucide:help-circle", status.ToString())
        };
    }
}

