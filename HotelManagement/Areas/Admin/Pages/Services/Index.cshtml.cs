using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Services;

[Authorize(Roles = "Manager,Receptionist")]
/// <summary>
/// Cung cấp danh sách dịch vụ với tìm kiếm, lọc trạng thái hoạt động và phân trang.
/// </summary>
public class IndexModel : PageModel
{
    private readonly IServiceService _serviceService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ quản lý dịch vụ.
    /// </summary>
    public IndexModel(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    public List<Service> Services { get; set; } = new();
    public int TotalCount { get; private set; }
    public int TotalPages { get; private set; }
    public const int PageSize = 5;

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActiveFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Nạp danh sách dịch vụ và áp dụng bộ lọc GET trước khi tính phân trang.
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        var allServices = await _serviceService.GetAllAsync();
        List<Service> filteredServices;

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            var keyword = SearchQuery.Trim();
            filteredServices = allServices
                .Where(s =>
                    (!string.IsNullOrWhiteSpace(s.Name) && s.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(s.Unit) && s.Unit.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }
        else
        {
            filteredServices = allServices;
        }

        if (IsActiveFilter.HasValue)
        {
            filteredServices = filteredServices
                .Where(s => s.IsActive == IsActiveFilter.Value)
                .ToList();
        }

        filteredServices = filteredServices
            .OrderByDescending(s => s.Id)
            .ToList();

        TotalCount = filteredServices.Count;
        TotalPages = Math.Max(1, (int)Math.Ceiling((double)TotalCount / PageSize));
        PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

        Services = filteredServices
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        return Page();
    }
}
