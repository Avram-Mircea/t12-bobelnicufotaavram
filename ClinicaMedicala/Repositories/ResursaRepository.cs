using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class ResursaRepository : GenericRepository<Resursa>, IResursaRepository
{
    public ResursaRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> DenumireExistaAsync(string denumire, int? idIgnorat = null)
    {
        if (string.IsNullOrWhiteSpace(denumire)) return false;
        var d = denumire.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(r =>
            r.Denumire.ToLower() == d &&
            (idIgnorat == null || r.Id != idIgnorat));
    }

    public async Task<bool> NumarInventarExistaAsync(string numarInventar, int? idIgnorat = null)
    {
        if (string.IsNullOrWhiteSpace(numarInventar)) return false;
        var n = numarInventar.Trim();
        return await _dbSet.AnyAsync(r =>
            r.NumarInventar == n &&
            (idIgnorat == null || r.Id != idIgnorat));
    }

    public async Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null)
    {
        IQueryable<Resursa> q = _dbSet.Include(r => r.Administrator);

        if (tip.HasValue) q = q.Where(r => r.Tip == tip.Value);
        if (stare.HasValue) q = q.Where(r => r.Stare == stare.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(r =>
                r.Denumire.Contains(s) ||
                r.NumarInventar.Contains(s) ||
                (r.Locatie != null && r.Locatie.Contains(s)));
        }

        return await q.OrderBy(r => r.Tip).ThenBy(r => r.Denumire).ToListAsync();
    }
}
