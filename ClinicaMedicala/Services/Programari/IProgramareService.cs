using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Programari;

public interface IProgramareService : IGenericService<Programare>
{
    // REQ-22: verifică dacă medicul are disponibilitate în interval
    Task<bool> MedicEsteDisponibilAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // REQ-23: verifică dacă resursa e disponibilă în interval
    Task<bool> ResursaEsteDisponibilaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null);

    // REQ-28..31: preluare programări pentru calendar
    Task<IEnumerable<Programare>> ObtineProgramariPentruCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null);

    // REQ-33: lista programărilor unui medic
    Task<List<Programare>> GetProgramariMedicAsync(int medicId);

    // REQ-49: rating-ul poate fi acordat doar după o consultație finalizată
    Task<bool> HasCompletedConsultationAsync(int pacientId, int medicId);

    // Lista TOATE programările cu navigation properties (pentru Index staff)
    Task<List<Programare>> GetAllWithRelationsAsync();
}
