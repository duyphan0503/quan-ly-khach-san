using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

/// <summary>
/// Dịch vụ khách sạn: giặt ủi, spa, ăn sáng...
/// </summary>
public class Service
{
    // Khóa chính dịch vụ.
    public int Id { get; set; }

    [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
    [StringLength(100)]
    [Display(Name = "Tên dịch vụ")]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    [Display(Name = "Đơn giá (VNĐ)")]
    // Đơn giá chuẩn cho 1 đơn vị tính.
    public decimal Price { get; set; }

    [StringLength(20)]
    [Display(Name = "Đơn vị tính")]
    public string? Unit { get; set; } // lần, phần, giờ

    [Display(Name = "Đang hoạt động")]
    // Dùng ẩn dịch vụ khỏi UI mà không xóa lịch sử hóa đơn cũ.
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
