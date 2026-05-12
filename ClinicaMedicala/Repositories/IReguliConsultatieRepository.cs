using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IReguliConsultatieRepository : IGenericRepository<ReguliConsultatie>
{
    Task<List<ReguliConsultatie>> GetAllOrderedAsync();
    Task<ReguliConsultatie?> GetByTipAsync(TipProgramare tip);
}
