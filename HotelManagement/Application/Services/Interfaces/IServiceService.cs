using HotelManagement.Core.Models;

namespace HotelManagement.Application.Services.Interfaces;

public interface IServiceService
{
    Task<List<Service>> GetAllAsync();
    Task<List<Service>> GetActiveAsync();
    Task<Service?> GetByIdAsync(int id);
    Task<(bool Success, string Message)> CreateAsync(Service service);
    Task<(bool Success, string Message)> UpdateAsync(Service service);
    Task<(bool Success, string Message)> DeleteAsync(int id);
}
