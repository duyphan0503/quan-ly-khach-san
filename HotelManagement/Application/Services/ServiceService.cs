using HotelManagement.Core.Models;
using HotelManagement.Infrastructure.Repositories.Interfaces;
using HotelManagement.Application.Services.Interfaces;

namespace HotelManagement.Application.Services;

public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;

    public ServiceService(IServiceRepository repo) => _repo = repo;

    public Task<List<Service>> GetAllAsync() => _repo.GetAllAsync();

    public Task<List<Service>> GetActiveAsync() => _repo.GetActiveAsync();

    public Task<Service?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<(bool Success, string Message)> CreateAsync(Service service)
    {
        await _repo.AddAsync(service);
        return (true, $"Đã thêm dịch vụ \"{service.Name}\" thành công.");
    }

    public async Task<(bool Success, string Message)> UpdateAsync(Service service)
    {
        await _repo.UpdateAsync(service);
        return (true, $"Đã cập nhật dịch vụ \"{service.Name}\".");
    }

    public async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var service = await _repo.GetByIdAsync(id);
        if (service is null) return (false, "Không tìm thấy dịch vụ.");

        await _repo.DeleteAsync(id);
        return (true, $"Đã xóa dịch vụ \"{service.Name}\".");
    }
}
