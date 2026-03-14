using HotelManagement.Core.Models;

namespace HotelManagement.Application.Services.Interfaces;

public interface IWebsiteSettingsService
{
    Task<WebsiteSettings> GetCurrentAsync();
    Task<(bool Success, string Message)> UpdateAsync(WebsiteSettings settings);
}
