using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Pacienti;

public interface IRatingService
{
    Task<List<Rating>> GetByMedicIdAsync(int medicId);
    Task AddAsync(Rating rating);
    Task<double> GetAverageRatingForMedicAsync(int medicId);
}
