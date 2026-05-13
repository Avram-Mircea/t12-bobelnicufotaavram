using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class AutentificareRepository : GenericRepository<Autentificare>, IAutentificareRepository
{
    public AutentificareRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Autentificare>> GetByUtilizatorAsync(int utilizatorId, int top = 50)
    {
        return await _dbSet
            .Where(a => a.UtilizatorId == utilizatorId)
            .OrderByDescending(a => a.DataOra)
            .Take(top)
            .ToListAsync();
    }

    public async Task<IDictionary<int, DateTime>> GetUltimeleLogariReusiteAsync(IEnumerable<int> utilizatorIds)
    {
        var ids = utilizatorIds.ToList();
        if (ids.Count == 0) return new Dictionary<int, DateTime>();

        return await _dbSet
            .Where(a => a.Succes && ids.Contains(a.UtilizatorId))
            .GroupBy(a => a.UtilizatorId)
            .Select(g => new { UtilizatorId = g.Key, Ultima = g.Max(x => x.DataOra) })
            .ToDictionaryAsync(x => x.UtilizatorId, x => x.Ultima);
    }

    // Util pentru detectarea brute-force: numără încercări eșuate recente pentru un email
    public async Task<IEnumerable<Autentificare>> GetEsuateRecenteAsync(string email, int minutes = 15)
    {
        var prag = DateTime.UtcNow.AddMinutes(-minutes);
        var emailNorm = email.Trim().ToLowerInvariant();

        return await _dbSet
            .Include(a => a.Utilizator)
            .Where(a => !a.Succes
                     && a.DataOra >= prag
                     && a.Utilizator.Email.ToLower() == emailNorm)
            .ToListAsync();
    }
}
