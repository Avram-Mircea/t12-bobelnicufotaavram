using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories;

public interface IRatingRepository
{
    Task<List<Rating>> GetByMedicIdAsync(int medicId);
    Task AddAsync(Rating rating);
    Task<double> GetAverageRatingForMedicAsync(int medicId);
}
