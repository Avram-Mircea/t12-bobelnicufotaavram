using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class ReguliConsultatieRepository : GenericRepository<ReguliConsultatie>, IReguliConsultatieRepository
{
    public ReguliConsultatieRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<List<ReguliConsultatie>> GetAllOrderedAsync() =>
        _dbSet.OrderBy(r => r.TipProgramare).ToListAsync();

    public Task<ReguliConsultatie?> GetByTipAsync(TipProgramare tip) =>
        _dbSet.FirstOrDefaultAsync(r => r.TipProgramare == tip);
}
