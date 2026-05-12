using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Resurse;

public interface IResursaService
{
    Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null);
    Task<Resursa?> GetByIdAsync(int id);

    // Creează o resursă nouă; validează unicitatea înainte de save.
    // Aruncă InvalidOperationException cu mesaj prietenos dacă denumirea sau
    // numărul de inventar există deja.
    Task<Resursa> CreeazaAsync(Resursa resursa);
}
