using HotelManagement.Core.Models;
using HotelManagement.Application.ViewModels;

namespace HotelManagement.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
