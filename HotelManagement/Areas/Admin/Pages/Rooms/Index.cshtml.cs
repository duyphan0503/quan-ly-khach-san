using HotelManagement.Core.Models;
using HotelManagement.Core.Models.Enums;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HotelManagement.Areas.Admin.Pages.Rooms
{
    [Authorize(Roles = "Manager,Receptionist")]
    /// <summary>
    /// Cung cấp danh sách phòng cho màn hình quản trị với tìm kiếm và lọc theo trạng thái/loại phòng.
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly IRoomService _roomService;

        /// <summary>
        /// Khởi tạo PageModel với dịch vụ truy vấn phòng.
        /// </summary>
        public IndexModel(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public List<Room> Rooms { get; private set; } = new();
        public List<SelectListItem> RoomTypeOptions { get; private set; } = new();
        public int TotalCount { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public RoomStatus? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RoomTypeId { get; set; }

        /// <summary>
        /// Nạp danh sách phòng và áp dụng các bộ lọc GET cho giao diện.
        /// </summary>
        public async Task OnGetAsync()
        {
            var allRooms = (await _roomService.GetAllAsync())
                .OrderBy(r => r.Floor)
                .ThenBy(r => r.RoomNumber)
                .ToList();

            RoomTypeOptions = (await _roomService.GetRoomTypesAsync())
                .OrderBy(rt => rt.Name)
                .Select(rt => new SelectListItem(rt.Name, rt.Id.ToString()))
                .ToList();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var keyword = SearchQuery.Trim().ToLowerInvariant();
                allRooms = allRooms
                    .Where(r =>
                        (!string.IsNullOrWhiteSpace(r.RoomNumber) && r.RoomNumber.ToLowerInvariant().Contains(keyword)) ||
                        (r.RoomType != null && !string.IsNullOrWhiteSpace(r.RoomType.Name) && r.RoomType.Name.ToLowerInvariant().Contains(keyword)) ||
                        r.Floor.ToString().Contains(keyword))
                    .ToList();
            }

            if (StatusFilter.HasValue)
            {
                allRooms = allRooms
                    .Where(r => r.Status == StatusFilter.Value)
                    .ToList();
            }

            if (RoomTypeId.HasValue)
            {
                allRooms = allRooms
                    .Where(r => r.RoomTypeId == RoomTypeId.Value)
                    .ToList();
            }

            Rooms = allRooms;
            TotalCount = Rooms.Count;
        }
    }
}
