using ClinicaMedicala.Models;
using ClinicaMedicala.Services.Interfaces;

public class ProgramareService : IProgramareService
{
    private readonly IProgramareRepository _repository;

    public ProgramareService(IProgramareRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Programare>> GetProgramariMedic(int medicId)
    {
        return await _repository.GetProgramariByMedicId(medicId);
    }
}