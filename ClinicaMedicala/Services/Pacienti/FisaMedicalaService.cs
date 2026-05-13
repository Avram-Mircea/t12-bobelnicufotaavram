using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Pacienti;

public class FisaMedicalaService : IFisaMedicalaService
{
    private readonly IFisaMedicalaRepository _repo;

    public FisaMedicalaService(IFisaMedicalaRepository repo)
    {
        _repo = repo;
    }

    public Task<FisaMedicala?> GetByPacientIdAsync(int pacientId)
        => _repo.GetByPacientIdAsync(pacientId);

    public async Task CreateOrUpdateAsync(FisaMedicala fisa)
    {
        var existing = await _repo.GetByPacientIdAsync(fisa.PacientId);

        if (existing == null)
        {
            fisa.DataCreare = DateTime.UtcNow;
            fisa.LastUpdated = DateTime.UtcNow;
            await _repo.AddAsync(fisa);
        }
        else
        {
            existing.IstoricBoliCronice = fisa.IstoricBoliCronice;
            existing.AntecedenteFamiliale = fisa.AntecedenteFamiliale;
            existing.GrupaDeRisc = fisa.GrupaDeRisc;
            existing.LastUpdated = DateTime.UtcNow;
            await _repo.UpdateAsync(existing);
        }
    }

    public Task UpdateAsync(FisaMedicala fisa) => _repo.UpdateAsync(fisa);
}
