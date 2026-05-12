using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Implementations
{
    public class FisaMedicalaService : IFisaMedicalaService
    {
        private readonly IFisaMedicalaRepository _repo;

        public FisaMedicalaService(IFisaMedicalaRepository repo)
        {
            _repo = repo;
        }

        public Task<FisaMedicala?> GetByPacientId(int pacientId)
            => _repo.GetByPacientId(pacientId);

        public async Task CreateOrUpdate(FisaMedicala fisa)
        {
            var existing = await _repo.GetByPacientId(fisa.PacientId);

            if (existing == null)
            {
                fisa.DataCreare = DateTime.UtcNow;
                fisa.LastUpdated = DateTime.UtcNow;

                await _repo.Add(fisa);
            }
            else
            {
                existing.IstoricBoliCronice = fisa.IstoricBoliCronice;
                existing.AntecedenteFamiliale = fisa.AntecedenteFamiliale;
                existing.GrupaDeRisc = fisa.GrupaDeRisc;

                existing.LastUpdated = DateTime.UtcNow;

                await _repo.Update(existing);
            }
        }

        public async Task Update(FisaMedicala fisa)
        {
            await _repo.Update(fisa);
        }
    }
}
