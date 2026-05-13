using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class SpecializareRepository : GenericRepository<Specializare>, ISpecializareRepository
{
    public SpecializareRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> NumeExistaAsync(string nume)
    {
        if (string.IsNullOrWhiteSpace(nume)) return false;
        var n = nume.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(s => s.Nume.ToLower() == n);
    }

    public async Task<IEnumerable<Specializare>> GetActiveAsync() =>
        await _dbSet.Where(s => s.Activ).OrderBy(s => s.Nume).ToListAsync();

    public async Task<IEnumerable<Specializare>> GetAllOrderedAsync() =>
        await _dbSet.OrderByDescending(s => s.Activ).ThenBy(s => s.Nume).ToListAsync();
}
