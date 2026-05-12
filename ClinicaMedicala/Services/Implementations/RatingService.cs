using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories.Interfaces;
using ClinicaMedicala.Services.Interfaces;

namespace ClinicaMedicala.Services.Implementations
{
    public class RatingService : IRatingService
    {
        private readonly IRatingRepository _repo;

        public RatingService(IRatingRepository repo)
        {
            _repo = repo;
        }

        public Task<List<Rating>> GetForMedic(int medicId)
            => _repo.GetByMedicId(medicId);

        public Task Add(Rating rating)
            => _repo.Add(rating);

        public async Task<List<Rating>> GetByMedicId(int medicId)
        {
            return await _repo.GetByMedicId(medicId);
        }

        public Task<double> GetAverageRatingForMedic(int medicId)
            => _repo.GetAverageRatingForMedic(medicId);
    }
}
