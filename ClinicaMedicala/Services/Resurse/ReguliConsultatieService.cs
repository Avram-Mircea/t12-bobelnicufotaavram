using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Resurse;

public class ReguliConsultatieService : IReguliConsultatieService
{
    private readonly IReguliConsultatieRepository _repo;

    public ReguliConsultatieService(IReguliConsultatieRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ReguliConsultatie>> GetAllAsync() => _repo.GetAllOrderedAsync();

    public async Task<List<ReguliConsultatie>> GetAllAsigurandExistentaAsync()
    {
        var existente = await _repo.GetAllOrderedAsync();
        var existenteTipuri = existente.Select(r => r.TipProgramare).ToHashSet();

        var tipuriDinEnum = Enum.GetValues<TipProgramare>();
        var lipsa = tipuriDinEnum.Where(t => !existenteTipuri.Contains(t)).ToList();

        if (lipsa.Count > 0)
        {
            foreach (var tip in lipsa)
            {
                await _repo.AddAsync(new ReguliConsultatie
                {
                    TipProgramare = tip,
                    NecesitaAsistent = false,
                    Descriere = null
                });
            }
            await _repo.SaveChangesAsync();
            existente = await _repo.GetAllOrderedAsync();
        }

        return existente;
    }

    public async Task<bool> NecesitaAsistentAsync(TipProgramare tip)
    {
        var regula = await _repo.GetByTipAsync(tip);
        return regula?.NecesitaAsistent ?? false;
    }

    public async Task ActualizeazaAsync(IEnumerable<(int Id, bool NecesitaAsistent, string? Descriere)> reguli)
    {
        foreach (var (id, necesita, descriere) in reguli)
        {
            var existenta = await _repo.GetByIdAsync(id);
            if (existenta == null) continue;

            existenta.NecesitaAsistent = necesita;
            existenta.Descriere = descriere?.Trim();
            _repo.Update(existenta);
        }
        await _repo.SaveChangesAsync();
    }
}
