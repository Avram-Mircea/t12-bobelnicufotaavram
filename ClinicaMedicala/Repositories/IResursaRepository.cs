using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IResursaRepository : IGenericRepository<Resursa>
{
    // Verificări de unicitate pre-save (mesaje prietenoase înainte ca DB să arunce)
    Task<bool> DenumireExistaAsync(string denumire, int? idIgnorat = null);
    Task<bool> NumarInventarExistaAsync(string numarInventar, int? idIgnorat = null);

    // Listă filtrată (folosită la Index)
    Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null);
}
