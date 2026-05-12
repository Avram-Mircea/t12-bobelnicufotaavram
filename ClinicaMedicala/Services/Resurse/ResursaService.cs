using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Resurse;

public class ResursaService : IResursaService
{
    private readonly IResursaRepository _repo;

    public ResursaService(IResursaRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null)
        => _repo.CautaAsync(tip, stare, search);

    public Task<Resursa?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<Resursa> CreeazaAsync(Resursa resursa)
    {
        if (resursa == null) throw new ArgumentNullException(nameof(resursa));

        resursa.Denumire = resursa.Denumire.Trim();
        resursa.NumarInventar = resursa.NumarInventar.Trim();

        if (await _repo.DenumireExistaAsync(resursa.Denumire))
            throw new InvalidOperationException($"Există deja o resursă cu denumirea '{resursa.Denumire}'.");

        if (await _repo.NumarInventarExistaAsync(resursa.NumarInventar))
            throw new InvalidOperationException($"Există deja o resursă cu numărul de inventar '{resursa.NumarInventar}'.");

        // Implicit: resursa e funcțională la creare
        if (resursa.Stare == default) resursa.Stare = StareResursa.Functional;

        // Dacă nu sunt setate, default-ul revizie = azi, scadență revizie = 1 an
        if (resursa.DataUltimaRevizie == default) resursa.DataUltimaRevizie = DateTime.UtcNow.Date;
        if (resursa.DataScadentaRevizie == default) resursa.DataScadentaRevizie = DateTime.UtcNow.Date.AddYears(1);

        await _repo.AddAsync(resursa);
        await _repo.SaveChangesAsync();
        return resursa;
    }
}
