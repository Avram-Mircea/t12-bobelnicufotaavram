using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Interfaces
{
    public interface IRatingService
    {
        Task<List<Rating>> GetForMedic(int medicId);
        Task Add(Rating rating);
        Task<List<Rating>> GetByMedicId(int medicId);
    }
}
