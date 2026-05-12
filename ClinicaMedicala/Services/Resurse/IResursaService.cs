using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface IResursaService
{
    Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null, bool? doarActive = null);
    Task<Resursa?> GetByIdAsync(int id);

    // REQ-11: activează/dezactivează administrativ.
    // REQ-12: o resursă dezactivată nu va fi disponibilă în calendar.
    Task<bool> DezactiveazaAsync(int id);
    Task<bool> ActiveazaAsync(int id);

    // Resurse cu adevărat disponibile pentru programări — folosit de Management Programări.
    // Combină Activ + Stare + Revizie + Perioade Mentenanță.
    Task<IEnumerable<Resursa>> GetDisponibileAsync(DateTime? laData = null);

    Task<int> NumarCuRevizieRestantaAsync();

    // ── Perioade mentenanță (REQ-14) ─────────────────────────────────────────
    Task<List<PerioadaMentenanta>> GetPerioadeAsync(int resursaId);
    Task<PerioadaMentenanta> AdaugaPerioadaAsync(int resursaId, DateTime inceput, DateTime sfarsit, string? descriere);
    Task<bool> StergePerioadaAsync(int perioadaId);

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
