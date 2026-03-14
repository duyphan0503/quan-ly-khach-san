using HotelManagement.Core.Models;

namespace HotelManagement.Infrastructure.Repositories.Interfaces;

public interface IServiceRepository
{
    Task<List<Service>> GetAllAsync();
    Task<List<Service>> GetActiveAsync();
    Task<Service?> GetByIdAsync(int id);
    Task AddAsync(Service service);
    Task UpdateAsync(Service service);
    Task DeleteAsync(int id);
}
