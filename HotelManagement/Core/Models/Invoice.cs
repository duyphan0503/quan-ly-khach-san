using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotelManagement.Core.Models.Enums;

namespace HotelManagement.Core.Models;

/// <summary>
/// Hóa đơn thanh toán — liên kết với Booking
/// </summary>
public class Invoice
{
    public int Id { get; set; }

    [Display(Name = "Đặt phòng")]
    public int BookingId { get; set; }

    [StringLength(20)]
    [Display(Name = "Số hóa đơn")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Display(Name = "Ngày lập hóa đơn")]
    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Tạm tính")]
    public decimal SubTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Thuế")]
    public decimal Tax { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Giảm giá")]
    public decimal Discount { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Tổng cộng")]
    public decimal GrandTotal { get; set; }

    [Display(Name = "Phương thức thanh toán")]
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

    [Display(Name = "Trạng thái")]
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

    [StringLength(450)]
    public string? CreatedByUserId { get; set; }

    // Navigation
    public Booking Booking { get; set; } = null!;
    public ApplicationUser? CreatedByUser { get; set; }
    public ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();
}
