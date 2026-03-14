using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Services;

[Authorize(Roles = "Manager")]
public class CreateModel : PageModel
{
    private readonly IServiceService _serviceService;

    public CreateModel(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [BindProperty]
    public ServiceInputModel Input { get; set; } = new();

    public class ServiceInputModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đơn giá.")]
        [Range(0, 1000000000, ErrorMessage = "Đơn giá phải từ 0 trở lên.")]
        public decimal Price { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị tính không được vượt quá 20 ký tự.")]
        public string? Unit { get; set; } = "Lần"; // Ví dụ: Lần, Giờ, Phần, Ly...

        public bool IsActive { get; set; } = true;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var service = new Service
        {
            Name = Input.Name,
            Price = Input.Price,
            Unit = Input.Unit,
            IsActive = Input.IsActive
        };

        var (success, message) = await _serviceService.CreateAsync(service);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return Page();
        }

        TempData["SuccessMessage"] = message;
        return RedirectToPage("./Index");
    }
}
