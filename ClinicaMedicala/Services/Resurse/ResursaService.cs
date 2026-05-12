using ClinicaMedicala.Data;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClinicaMedicala.Services.Resurse;

public class ResursaService : IResursaService
{
    private readonly IResursaRepository _repo;
    private readonly ApplicationDbContext _ctx;

    public ResursaService(IResursaRepository repo, ApplicationDbContext ctx)
    {
        _repo = repo;
        _ctx = ctx;
    }

    public Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null)
        => _repo.CautaAsync(tip, stare, search);

    public Task<Resursa?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<Resursa> CreeazaAsync(Resursa resursa, IEnumerable<int> specializareIds)
    {
        if (resursa == null) throw new ArgumentNullException(nameof(resursa));

        resursa.Denumire = resursa.Denumire.Trim();
        resursa.NumarInventar = resursa.NumarInventar.Trim();

        if (await _repo.DenumireExistaAsync(resursa.Denumire))
            throw new InvalidOperationException($"Există deja o resursă cu denumirea „{resursa.Denumire}”.");

        if (await _repo.NumarInventarExistaAsync(resursa.NumarInventar))
            throw new InvalidOperationException($"Există deja o resursă cu numărul de inventar „{resursa.NumarInventar}”.");

        if (resursa.Stare == default) resursa.Stare = StareResursa.Functional;
        if (resursa.DataUltimaRevizie == default) resursa.DataUltimaRevizie = DateTime.UtcNow.Date;
        if (resursa.DataScadentaRevizie == default) resursa.DataScadentaRevizie = DateTime.UtcNow.Date.AddYears(1);

        // Atașăm specializările alese (DbContext le va recunoaște ca existente prin Id)
        var ids = specializareIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count > 0)
        {
            var specs = await _ctx.Specializari
                .Where(s => ids.Contains(s.Id) && s.Activ)
                .ToListAsync();
            foreach (var s in specs)
                resursa.Specializari.Add(s);
        }

        await _repo.AddAsync(resursa);
        await _repo.SaveChangesAsync();
        return resursa;
    }

    public async Task ActualizeazaAsync(int id,
                                         string denumire,
                                         TipResursa tip,
                                         string numarInventar,
                                         string? locatie,
                                         StareResursa stare,
                                         DateTime dataUltimaRevizie,
                                         DateTime dataScadentaRevizie,
                                         IEnumerable<int> specializareIds)
    {
        // Încărcăm cu navigarea Specializari ca să o putem manipula prin tracking
        var resursa = await _ctx.Resurse
            .Include(r => r.Specializari)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resursa == null)
            throw new InvalidOperationException("Resursa nu mai există.");

        denumire = denumire.Trim();
        numarInventar = numarInventar.Trim();

        if (await _repo.DenumireExistaAsync(denumire, idIgnorat: id))
            throw new InvalidOperationException($"Există deja o altă resursă cu denumirea „{denumire}”.");

        if (await _repo.NumarInventarExistaAsync(numarInventar, idIgnorat: id))
            throw new InvalidOperationException($"Există deja o altă resursă cu numărul de inventar „{numarInventar}”.");

        resursa.Denumire = denumire;
        resursa.Tip = tip;
        resursa.NumarInventar = numarInventar;
        resursa.Locatie = locatie;
        resursa.Stare = stare;
        resursa.DataUltimaRevizie = dataUltimaRevizie;
        resursa.DataScadentaRevizie = dataScadentaRevizie;

        // Înlocuim lista de specializări: golim → adăugăm cele noi
        resursa.Specializari.Clear();

        var ids = specializareIds?.Distinct().ToList() ?? new List<int>();
        if (ids.Count > 0)
        {
            var specs = await _ctx.Specializari
                .Where(s => ids.Contains(s.Id) && s.Activ)
                .ToListAsync();
            foreach (var s in specs)
                resursa.Specializari.Add(s);
        }

        await _ctx.SaveChangesAsync();
    }
}
