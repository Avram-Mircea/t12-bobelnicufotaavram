using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface IReguliConsultatieService
{
    Task<List<ReguliConsultatie>> GetAllAsync();

    // Folosit de validator (REQ-17) la creare programare:
    // returnează true dacă tipul de consultație are nevoie de asistent.
    Task<bool> NecesitaAsistentAsync(TipProgramare tip);

    // Actualizează regulile în bulk (lista vine din form)
    Task ActualizeazaAsync(IEnumerable<(int Id, bool NecesitaAsistent, string? Descriere)> reguli);
}
