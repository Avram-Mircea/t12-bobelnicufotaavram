using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

// REQ-07: istoricul autentificărilor
public interface IAutentificareRepository : IGenericRepository<Autentificare>
{
    Task<IEnumerable<Autentificare>> GetByUtilizatorAsync(int utilizatorId, int top = 50);
    Task<IEnumerable<Autentificare>> GetEsuateRecenteAsync(string email, int minutes = 15);
}
