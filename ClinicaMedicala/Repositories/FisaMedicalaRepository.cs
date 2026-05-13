using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class FisaMedicalaRepository : IFisaMedicalaRepository
{
    private readonly ApplicationDbContext _context;

    public FisaMedicalaRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<FisaMedicala?> GetByPacientIdAsync(int pacientId)
    {
        return await _context.FiseMedicale
            .Include(f => f.Consultatii)
            .FirstOrDefaultAsync(f => f.PacientId == pacientId);
    }

    public async Task AddAsync(FisaMedicala fisa)
    {
        _context.FiseMedicale.Add(fisa);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FisaMedicala fisa)
    {
        _context.FiseMedicale.Update(fisa);
        await _context.SaveChangesAsync();
    }
}
