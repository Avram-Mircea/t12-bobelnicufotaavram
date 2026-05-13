using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Pacienti;

public class RatingService : IRatingService
{
    private readonly IRatingRepository _repo;

    public RatingService(IRatingRepository repo)
    {
        _repo = repo;
    }

    public Task<List<Rating>> GetByMedicIdAsync(int medicId) => _repo.GetByMedicIdAsync(medicId);
    public Task AddAsync(Rating rating) => _repo.AddAsync(rating);
    public Task<double> GetAverageRatingForMedicAsync(int medicId) =>
        _repo.GetAverageRatingForMedicAsync(medicId);
}
