using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

public class ProgramareRepository : IProgramareRepository
{
    private readonly ApplicationDbContext _context;

    public ProgramareRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Programare>> GetProgramariByMedicId(int medicId)
    {
        return await _context.Programari
            .Include(p => p.Pacient)
            .Where(p => p.MedicId == medicId)
            .OrderBy(p => p.DataStart)
            .ToListAsync();
    }
}