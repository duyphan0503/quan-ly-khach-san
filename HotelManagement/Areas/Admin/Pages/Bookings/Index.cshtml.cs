using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Bookings
{
    [Authorize(Roles = "Manager,Receptionist")]
    /// <summary>
    /// Cung cấp danh sách booking với tìm kiếm/lọc trạng thái và thao tác cập nhật trạng thái nhanh.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IBookingService _bookingService;

        /// <summary>
        /// Khởi tạo PageModel với dịch vụ booking.
        /// </summary>
        public IndexModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        public List<Booking> Bookings { get; private set; } = new();
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }
        public const int PageSize = 5;

        [TempData]
        public string? SuccessMessage { get; set; }
        [TempData]
        public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Nạp danh sách booking, ưu tiên hiển thị đơn Pending trước và áp dụng phân trang.
        /// </summary>
        public async Task OnGetAsync()
        {
            List<Booking> source;

            if (!string.IsNullOrEmpty(Search) || !string.IsNullOrEmpty(Status))
            {
                source = await _bookingService.SearchAsync(Search, Status);
            }
            else
            {
                source = await _bookingService.GetAllAsync();
            }

            source = source
                .OrderBy(b => b.Status == BookingStatus.Pending ? 0 : 1)
                .ThenByDescending(b => b.CreatedAt)
                .ToList();

            TotalCount = source.Count;
            TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            Bookings = source
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        /// <summary>
        /// Chuyển trạng thái booking sang Confirmed.
        /// </summary>
        public async Task<IActionResult> OnPostConfirmAsync(int id)
        {
            var (success, message) = await _bookingService.UpdateStatusAsync(id, nameof(BookingStatus.Confirmed));
            if (success) SuccessMessage = message;
            else ErrorMessage = message;

            return RedirectToPage();
        }

        /// <summary>
        /// Hủy booking theo nghiệp vụ hủy đặt.
        /// </summary>
        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var (success, message) = await _bookingService.CancelAsync(id);
            if (success) SuccessMessage = message;
            else ErrorMessage = message;
            return RedirectToPage();
        }

        /// <summary>
        /// Chuyển trạng thái booking sang CheckedIn.
        /// </summary>
        public async Task<IActionResult> OnPostCheckInAsync(int id)
        {
            var (success, message) = await _bookingService.UpdateStatusAsync(id, nameof(BookingStatus.CheckedIn));
            if (success) SuccessMessage = message;
            else ErrorMessage = message;
            return RedirectToPage();
        }

        /// <summary>
        /// Chuyển trạng thái booking sang CheckedOut.
        /// </summary>
        public async Task<IActionResult> OnPostCheckOutAsync(int id)
        {
            var (success, message) = await _bookingService.UpdateStatusAsync(id, nameof(BookingStatus.CheckedOut));
            if (success) SuccessMessage = message;
            else ErrorMessage = message;
            return RedirectToPage();
        }
    }
}

