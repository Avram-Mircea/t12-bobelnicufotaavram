using ClinicaMedicala.Models;

public interface IFisaMedicalaService
{
    Task<FisaMedicala?> GetByPacientId(int pacientId);
    Task CreateOrUpdate(FisaMedicala fisa);
    Task Update(FisaMedicala fisa);
}