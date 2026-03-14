using System.ComponentModel.DataAnnotations;

namespace HotelManagement.Core.Models.Enums;

public enum RoomStatus
{
    [Display(Name = "Trống")]
    Available,

    [Display(Name = "Đang có khách")]
    Occupied,

    [Display(Name = "Bảo trì")]
    Maintenance,

    [Display(Name = "Đã đặt trước")]
    Reserved
}
