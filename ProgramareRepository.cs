using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Repositories;

public class ProgramareRepository : GenericRepository<Programare>, IProgramareRepository
{
    public ProgramareRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistaSuprapunereMedicAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        return await _dbSet.AnyAsync(p =>
            p.MedicId == medicId &&
            p.Status != StatusProgramare.Anulat_Pacient && p.Status != StatusProgramare.Anulat_Clinica && // Considerăm că statusul anulat nu blochează
            p.DataStart < dataEnd && p.DataEnd > dataStart &&
            (!programareExclusaId.HasValue || p.Id != programareExclusaId.Value));
    }

    public async Task<bool> ExistaSuprapunereResursaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        return await _dbSet.AnyAsync(p =>
            p.ResursaId == resursaId &&
            p.Status != StatusProgramare.Anulat_Pacient && p.Status != StatusProgramare.Anulat_Clinica &&
            p.DataStart < dataEnd && p.DataEnd > dataStart &&
            (!programareExclusaId.HasValue || p.Id != programareExclusaId.Value));
    }

    public async Task<IEnumerable<Programare>> GetProgramariCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null)
    {
        var query = _dbSet
            .Include(p => p.Medic)
            .Include(p => p.Pacient)
            .Include(p => p.Resursa)
            .Where(p => p.DataStart < end && p.DataEnd > start && p.Status != StatusProgramare.Anulat_Pacient && p.Status != StatusProgramare.Anulat_Clinica);

        if (medicId.HasValue)
        {
            query = query.Where(p => p.MedicId == medicId.Value);
        }

        if (resursaId.HasValue)
        {
            query = query.Where(p => p.ResursaId == resursaId.Value);
        }

        return await query.ToListAsync();
    }
}
