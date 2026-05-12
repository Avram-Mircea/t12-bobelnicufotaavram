using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaMedicala.Models;
using ClinicaMedicala.Services;

namespace ClinicaMedicala.Services;

public interface IProgramareService : IGenericService<Programare>
{
    // Verifică dacă medicul are disponibilitate în intervalul ales
    Task<bool> MedicEsteDisponibilAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // Verifică dacă resursa ( ex: sală consultație) este disponibilă
    Task<bool> ResursaEsteDisponibilaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // Metodă care înglobează logica pentru a prelua programările dintr-un anumit interval
    Task<IEnumerable<Programare>> ObtineProgramariPentruCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null);
}