using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface IResursaService
{
    Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null);
    Task<Resursa?> GetByIdAsync(int id);

    // Creează o resursă nouă; validează unicitatea înainte de save.
    // SpecializareIds = lista ID-urilor specializărilor care pot folosi resursa.
    Task<Resursa> CreeazaAsync(Resursa resursa, IEnumerable<int> specializareIds);

    // Actualizează o resursă existentă (REQ-10).
    // Înlocuiește lista de specializări asociate cu cea primită.
    // Aruncă InvalidOperationException dacă denumirea/nr. inventar e duplicat
    // pentru altă resursă, sau dacă resursa nu există.
    Task ActualizeazaAsync(int id,
                           string denumire,
                           TipResursa tip,
                           string numarInventar,
                           string? locatie,
                           StareResursa stare,
                           DateTime dataUltimaRevizie,
                           DateTime dataScadentaRevizie,
                           IEnumerable<int> specializareIds);
}
