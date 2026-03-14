using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models.Enums;

public enum PaymentMethod
{
    [Display(Name = "Tiền mặt")]
    Cash,

    [Display(Name = "Thẻ ngân hàng")]
    Card,

    [Display(Name = "Chuyển khoản")]
    Transfer
}
