using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IProgramareRepository : IGenericRepository<Programare>
{
    // Verifică dacă medicul are deja o programare care se suprapune cu intervalul dat
    Task<bool> ExistaSuprapunereMedicAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // Verifică dacă resursa (sala/aparatul) are deja o programare care se suprapune cu intervalul dat
    Task<bool> ExistaSuprapunereResursaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // Metodă centralizată pentru aducerea programărilor pentru calendar (cu filtre opționale)
    Task<IEnumerable<Programare>> GetProgramariCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null);
}
