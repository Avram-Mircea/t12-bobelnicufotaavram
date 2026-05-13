using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class UtilizatorRepository : GenericRepository<Utilizator>, IUtilizatorRepository
{
    public UtilizatorRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Utilizator?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var emailNorm = email.Trim().ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == emailNorm);
    }

    public async Task<Utilizator?> GetByResetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _dbSet.FirstOrDefaultAsync(u => u.ResetToken == token);
    }

    public async Task<bool> EmailExistaAsync(string email)
    {
        var emailNorm = email.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(u => u.Email.ToLower() == emailNorm);
    }

    public async Task<IEnumerable<Utilizator>> GetByRolAsync(Rol rol)
    {
        return await _dbSet.Where(u => u.Rol == rol).ToListAsync();
    }

    public async Task<IEnumerable<Utilizator>> GetActiviAsync()
    {
        return await _dbSet.Where(u => u.StatusCont).ToListAsync();
    }
}
