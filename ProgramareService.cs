using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services;

public class ProgramareService : GenericService<Programare>, IProgramareService
{
    private readonly IProgramareRepository _programareRepository;

    public ProgramareService(IProgramareRepository repository) : base(repository)
    {
        _programareRepository = repository;
    }

    public async Task<bool> MedicEsteDisponibilAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        bool existaSuprapunere = await _programareRepository.ExistaSuprapunereMedicAsync(medicId, dataStart, dataEnd, programareExclusaId);
        return !existaSuprapunere;
    }

    public async Task<bool> ResursaEsteDisponibilaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        bool existaSuprapunere = await _programareRepository.ExistaSuprapunereResursaAsync(resursaId, dataStart, dataEnd, programareExclusaId);
        return !existaSuprapunere;
    }

    public async Task<IEnumerable<Programare>> ObtineProgramariPentruCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null)
    {
        return await _programareRepository.GetProgramariCalendarAsync(start, end, medicId, resursaId);
    }
}
