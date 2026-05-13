using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Pacienti;

public interface IFisaMedicalaService
{
    Task<FisaMedicala?> GetByPacientIdAsync(int pacientId);
    Task CreateOrUpdateAsync(FisaMedicala fisa);
    Task UpdateAsync(FisaMedicala fisa);
}
