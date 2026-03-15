using HotelManagement.Core.Models;
using HotelManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Areas.Admin.Pages.Services;

[Authorize(Roles = "Manager")]
/// <summary>
/// Xử lý chỉnh sửa dịch vụ hiện có và thao tác xóa dịch vụ từ trang quản trị.
/// </summary>
public class EditModel : PageModel
{
    private readonly IServiceService _serviceService;

    /// <summary>
    /// Khởi tạo PageModel với dịch vụ nghiệp vụ cho dịch vụ khách sạn.
    /// </summary>
    public EditModel(IServiceService serviceService)
    {
        _serviceService = serviceService;
    }

    [BindProperty]
    public ServiceInputModel Input { get; set; } = new();

    /// <summary>
    /// Mô hình dữ liệu nhập liệu cho màn hình chỉnh sửa dịch vụ.
    /// </summary>
    public class ServiceInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên dịch vụ.")]
        [StringLength(100, ErrorMessage = "Tên dịch vụ không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập đơn giá.")]
        [Range(0, 1000000000, ErrorMessage = "Đơn giá phải từ 0 trở lên.")]
        public decimal Price { get; set; }

        [StringLength(20, ErrorMessage = "Đơn vị tính không được vượt quá 20 ký tự.")]
        public string? Unit { get; set; } = "Lần";

        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Nạp dữ liệu dịch vụ theo id và map sang model form.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int id)
    {
        var service = await _serviceService.GetByIdAsync(id);
        if (service == null)
        {
            return NotFound();
        }

        Input = new ServiceInputModel
        {
            Id = service.Id,
            Name = service.Name,
            Price = service.Price,
            Unit = service.Unit,
            IsActive = service.IsActive
        };

        return Page();
    }

    /// <summary>
    /// Kiểm tra dữ liệu nhập và cập nhật lại thông tin dịch vụ.
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var service = await _serviceService.GetByIdAsync(Input.Id);
        if (service == null)
        {
            return NotFound();
        }

        service.Name = Input.Name;
        service.Price = Input.Price;
        service.Unit = Input.Unit;
        service.IsActive = Input.IsActive;

        var (success, message) = await _serviceService.UpdateAsync(service);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            return Page();
        }

        TempData["SuccessMessage"] = message;
        return RedirectToPage("./Index");
    }

    /// <summary>
    /// Xóa dịch vụ theo id và phản hồi kết quả bằng thông báo TempData.
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var (success, message) = await _serviceService.DeleteAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = message;
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }

        return RedirectToPage("./Index");
    }
}
