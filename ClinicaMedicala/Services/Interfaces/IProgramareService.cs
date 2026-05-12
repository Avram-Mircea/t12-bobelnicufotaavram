using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Interfaces
{
    public interface IProgramareService
    {
        Task<List<Programare>> GetProgramariMedic(int medicId);
        Task<bool> HasCompletedConsultation(int pacientId, int medicId);
    }
}
