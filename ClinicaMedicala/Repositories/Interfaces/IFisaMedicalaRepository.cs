using ClinicaMedicala.Models;
using ClinicaMedicala.Data;
using Microsoft.EntityFrameworkCore;

public interface IFisaMedicalaRepository
{
    Task<FisaMedicala?> GetByPacientId(int pacientId);
    Task Add(FisaMedicala fisa);
    Task Update(FisaMedicala fisa);
}