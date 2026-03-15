using Microsoft.AspNetCore.Mvc.RazorPages;
using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Areas.Admin.Pages.RoomTypes;

/// <summary>
/// Cung cấp danh sách loại phòng với tìm kiếm, lọc sức chứa và phân trang.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IRoomService _roomService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ truy vấn loại phòng.
    /// </summary>
    public IndexModel(IRoomService roomService)
    {
        _roomService = roomService;
    }

    public List<RoomType> RoomTypes { get; set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public const int PageSize = 5;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? OccupancyFilter { get; set; }

    /// <summary>
    /// Nạp dữ liệu và áp dụng bộ lọc GET trước khi tính thông tin phân trang.
    /// </summary>
    public async Task OnGetAsync()
    {
        var allRoomTypes = (await _roomService.GetRoomTypesAsync())
            .OrderByDescending(r => r.Id)
            .ToList();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var keyword = SearchQuery.Trim().ToLowerInvariant();
            allRoomTypes = allRoomTypes
                .Where(r =>
                    (!string.IsNullOrWhiteSpace(r.Name) && r.Name.ToLowerInvariant().Contains(keyword)) ||
                    (!string.IsNullOrWhiteSpace(r.Description) && r.Description.ToLowerInvariant().Contains(keyword)) ||
                    (!string.IsNullOrWhiteSpace(r.Amenities) && r.Amenities.ToLowerInvariant().Contains(keyword)))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(OccupancyFilter))
        {
            allRoomTypes = OccupancyFilter switch
            {
                "1-2" => allRoomTypes.Where(r => r.MaxOccupancy <= 2).ToList(),
                "3-4" => allRoomTypes.Where(r => r.MaxOccupancy is >= 3 and <= 4).ToList(),
                "5+" => allRoomTypes.Where(r => r.MaxOccupancy >= 5).ToList(),
                _ => allRoomTypes
            };
        }

        TotalCount = allRoomTypes.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        RoomTypes = allRoomTypes
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();
    }
}
