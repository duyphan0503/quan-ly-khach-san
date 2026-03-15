using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagement.Core.Models;

/// <summary>
/// Chi tiết từng dòng hóa đơn (tiền phòng + dịch vụ)
/// </summary>
public class InvoiceDetail
{
    // Khóa chính chi tiết hóa đơn.
    public int Id { get; set; }

    [Display(Name = "Hóa đơn")]
    public int InvoiceId { get; set; }

    [Display(Name = "Dịch vụ")]
    // Null khi dòng tiền không gắn service cụ thể (ví dụ: tiền phòng).
    public int? ServiceId { get; set; }

    [Required(ErrorMessage = "Mô tả không được để trống")]
    [StringLength(200)]
    [Display(Name = "Mô tả")]
    public string Description { get; set; } = string.Empty;

    [Range(1, 1000)]
    [Display(Name = "Số lượng")]
    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Đơn giá")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Thành tiền")]
    // LineTotal = Quantity * UnitPrice.
    public decimal LineTotal { get; set; }

    // Navigation
    public Invoice Invoice { get; set; } = null!;
    public Service? Service { get; set; }
}
