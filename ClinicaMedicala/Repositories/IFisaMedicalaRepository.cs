using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IFisaMedicalaRepository
{
    Task<FisaMedicala?> GetByPacientIdAsync(int pacientId);
    Task AddAsync(FisaMedicala fisa);
    Task UpdateAsync(FisaMedicala fisa);
}
