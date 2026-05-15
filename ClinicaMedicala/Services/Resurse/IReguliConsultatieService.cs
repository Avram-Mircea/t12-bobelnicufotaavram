using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface IReguliConsultatieService
{
    Task<List<ReguliConsultatie>> GetAllAsync();

    // Asigură existența unui rând (cu default-uri) pentru fiecare TipProgramare
    // din enum. Apelat la deschiderea paginii Admin → Reguli consultații, ca
    // adminul să poată bifa NecesitaAsistent pentru orice tip — chiar și la prima rulare.
    Task<List<ReguliConsultatie>> GetAllAsigurandExistentaAsync();

    // Folosit de validator (REQ-17) la creare programare:
    // returnează true dacă tipul de consultație are nevoie de asistent.
    Task<bool> NecesitaAsistentAsync(TipProgramare tip);

    // Actualizează regulile în bulk (lista vine din form)
    Task ActualizeazaAsync(IEnumerable<(int Id, bool NecesitaAsistent, string? Descriere)> reguli);
}
