using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HotelManagement.Areas.Admin.Pages.Guests;

[Authorize(Roles = "Manager,Receptionist")]
public class IndexModel : PageModel
{
    private readonly IGuestService _guestService;

    public IndexModel(IGuestService guestService)
    {
        _guestService = guestService;
    }

    public List<Guest> Guests { get; set; } = new();
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    [BindProperty(SupportsGet = true)]
    public string? SearchQuery { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 5;

    public async Task<IActionResult> OnGetAsync()
    {
        if (PageNumber < 1) PageNumber = 1;

        var result = await _guestService.GetPagedAsync(SearchQuery, PageNumber, PageSize);
        Guests = result.Items;
        TotalCount = result.TotalCount;

        return Page();
    }

    public async Task<IActionResult> OnGetLoadMoreAsync(string? searchQuery, int pageNumber)
    {
        var result = await _guestService.GetPagedAsync(searchQuery, pageNumber, PageSize);
        return Partial("_GuestRows", result.Items);
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var (success, message) = await _guestService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostMergeDuplicatesAsync()
    {
        var guests = await _guestService.GetAllAsync();
        var duplicateGroups = guests
            .Where(g => !string.IsNullOrWhiteSpace(g.UserId))
            .GroupBy(g => g.UserId!)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateGroups.Count == 0)
        {
            TempData["SuccessMessage"] = "Không có hồ sơ khách trùng theo tài khoản để gộp.";
            return RedirectToPage();
        }

        var mergedGroups = 0;
        foreach (var group in duplicateGroups)
        {
            var sample = group
                .OrderBy(g => g.CreatedAt)
                .First();

            var result = await _guestService.ConsolidateGuestProfilesAsync(
                sample.UserId,
                sample.PhoneNumber,
                sample.CCCD,
                sample.Email,
                sample.FullName,
                sample.AvatarUrl);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
                return RedirectToPage();
            }

            mergedGroups++;
        }

        TempData["SuccessMessage"] = $"Đã gộp xong {mergedGroups} nhóm hồ sơ khách bị trùng.";
        return RedirectToPage();
    }
}
