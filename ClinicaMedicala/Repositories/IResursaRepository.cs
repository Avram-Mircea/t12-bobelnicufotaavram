using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IResursaRepository : IGenericRepository<Resursa>
{
    // Verificări de unicitate pre-save (mesaje prietenoase înainte ca DB să arunce)
    Task<bool> DenumireExistaAsync(string denumire, int? idIgnorat = null);
    Task<bool> NumarInventarExistaAsync(string numarInventar, int? idIgnorat = null);

    // Listă filtrată (folosită la Index). doarActive=true filtrează doar resursele active (calendarul).
    Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null, bool? doarActive = null);

    // Resurse cu adevărat disponibile pentru programări la o dată dată (default: azi).
    // Combină: Activ + Stare bună + Revizie validă + Fără perioadă activă de mentenanță.
    Task<IEnumerable<Resursa>> GetDisponibileAsync(DateTime? laData = null);

    // Count rapid pentru dashboard
    Task<int> NumarCuRevizieRestantaAsync();
}
