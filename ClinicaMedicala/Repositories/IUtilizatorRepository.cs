using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

// Extinde repository-ul generic cu interogări specifice domeniului utilizator
public interface IUtilizatorRepository : IGenericRepository<Utilizator>
{
    Task<Utilizator?> GetByEmailAsync(string email);
    Task<Utilizator?> GetByResetTokenAsync(string token);
    Task<bool> EmailExistaAsync(string email);
    Task<IEnumerable<Utilizator>> GetByRolAsync(Rol rol);
    Task<IEnumerable<Utilizator>> GetActiviAsync();
}
