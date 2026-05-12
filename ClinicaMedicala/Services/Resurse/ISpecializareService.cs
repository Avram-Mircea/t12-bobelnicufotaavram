using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface ISpecializareService
{
    Task<IEnumerable<Specializare>> GetAllAsync();
    Task<IEnumerable<Specializare>> GetActiveAsync();
    Task<Specializare?> GetByIdAsync(int id);
    Task<Specializare> CreeazaAsync(string nume, string? descriere);
    Task<bool> ToggleActivAsync(int id);
}
