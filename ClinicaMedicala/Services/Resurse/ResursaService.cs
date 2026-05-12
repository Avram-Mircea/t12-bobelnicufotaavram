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

    public Task<IEnumerable<Resursa>> CautaAsync(TipResursa? tip = null, StareResursa? stare = null, string? search = null, bool? doarActive = null)
        => _repo.CautaAsync(tip, stare, search, doarActive);

    public Task<Resursa?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<bool> DezactiveazaAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r == null || !r.Activ) return false;

        r.Activ = false;
        _repo.Update(r);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActiveazaAsync(int id)
    {
        var r = await _repo.GetByIdAsync(id);
        if (r == null || r.Activ) return false;

        r.Activ = true;
        _repo.Update(r);
        await _repo.SaveChangesAsync();
        return true;
    }

    public Task<IEnumerable<Resursa>> GetDisponibileAsync(DateTime? laData = null)
        => _repo.GetDisponibileAsync(laData);

    public Task<int> NumarCuRevizieRestantaAsync() => _repo.NumarCuRevizieRestantaAsync();

    // ── Perioade mentenanță (REQ-14) ─────────────────────────────────────────
    public Task<List<PerioadaMentenanta>> GetPerioadeAsync(int resursaId) =>
        _ctx.PerioadeMentenanta
            .Where(p => p.ResursaId == resursaId)
            .OrderByDescending(p => p.Inceput)
            .ToListAsync();

    public async Task<PerioadaMentenanta> AdaugaPerioadaAsync(int resursaId, DateTime inceput, DateTime sfarsit, string? descriere)
    {
        if (sfarsit < inceput)
            throw new InvalidOperationException("Sfârșitul perioadei nu poate fi înainte de început.");

        var resursaExista = await _ctx.Resurse.AnyAsync(r => r.Id == resursaId);
        if (!resursaExista)
            throw new InvalidOperationException("Resursa nu există.");

        // Verifică suprapunerea cu alte perioade existente
        var seSuprapune = await _ctx.PerioadeMentenanta.AnyAsync(p =>
            p.ResursaId == resursaId &&
            p.Inceput <= sfarsit &&
            p.Sfarsit >= inceput);

        if (seSuprapune)
            throw new InvalidOperationException("Perioada se suprapune cu o altă perioadă de mentenanță existentă.");

        var perioada = new PerioadaMentenanta
        {
            ResursaId = resursaId,
            Inceput = inceput.Date,
            Sfarsit = sfarsit.Date,
            Descriere = descriere?.Trim()
        };

        _ctx.PerioadeMentenanta.Add(perioada);
        await _ctx.SaveChangesAsync();
        return perioada;
    }

    public async Task<bool> StergePerioadaAsync(int perioadaId)
    {
        var p = await _ctx.PerioadeMentenanta.FindAsync(perioadaId);
        if (p == null) return false;

        _ctx.PerioadeMentenanta.Remove(p);
        await _ctx.SaveChangesAsync();
        return true;
    }

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
