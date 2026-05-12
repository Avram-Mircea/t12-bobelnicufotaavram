using ClinicaMedicala.Models;

public interface IProgramareRepository
{
    Task<List<Programare>> GetProgramariByMedicId(int medicId);
}