using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IProgramareRepository : IGenericRepository<Programare>
{
    // REQ-22, REQ-27: verifică suprapunerea programărilor pentru medic
    Task<bool> ExistaSuprapunereMedicAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // REQ-23, REQ-27: verifică suprapunerea programărilor pentru resursă
    Task<bool> ExistaSuprapunereResursaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // REQ-28..31: aducerea programărilor pentru calendar cu filtre opționale
    Task<IEnumerable<Programare>> GetProgramariCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null);

    // REQ-33: lista programărilor unui medic (folosit pentru pagina lui)
    Task<List<Programare>> GetProgramariByMedicIdAsync(int medicId);

    // Lista TOATE (cu pacient/medic), incl. cele anulate — pentru Index staff
    Task<List<Programare>> GetAllWithRelationsAsync();
}
