using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface ISpecializareRepository : IGenericRepository<Specializare>
{
    Task<bool> NumeExistaAsync(string nume);
    Task<IEnumerable<Specializare>> GetActiveAsync();
    Task<IEnumerable<Specializare>> GetAllOrderedAsync();
}
