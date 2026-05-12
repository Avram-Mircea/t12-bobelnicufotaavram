using ClinicaMedicala.Models;

namespace ClinicaMedicala.Repositories.Interfaces
{
    public interface IRatingRepository
    {
        Task<List<Rating>> GetByMedicId(int medicId);
        Task Add(Rating rating);
        Task<double> GetAverageRatingForMedic(int medicId);
    }
}
