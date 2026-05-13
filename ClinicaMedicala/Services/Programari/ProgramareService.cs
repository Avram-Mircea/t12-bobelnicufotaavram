using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Programari;

public class ProgramareService : GenericService<Programare>, IProgramareService
{
    private readonly IProgramareRepository _programareRepository;

    public ProgramareService(IProgramareRepository repository) : base(repository)
    {
        _programareRepository = repository;
    }

    public async Task<bool> MedicEsteDisponibilAsync(int medicId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        var existaSuprapunere = await _programareRepository.ExistaSuprapunereMedicAsync(medicId, dataStart, dataEnd, programareExclusaId);
        return !existaSuprapunere;
    }

    public async Task<bool> ResursaEsteDisponibilaAsync(int resursaId, DateTime dataStart, DateTime dataEnd, int? programareExclusaId = null)
    {
        var existaSuprapunere = await _programareRepository.ExistaSuprapunereResursaAsync(resursaId, dataStart, dataEnd, programareExclusaId);
        return !existaSuprapunere;
    }

    public async Task<IEnumerable<Programare>> ObtineProgramariPentruCalendarAsync(DateTime start, DateTime end, int? medicId = null, int? resursaId = null)
    {
        return await _programareRepository.GetProgramariCalendarAsync(start, end, medicId, resursaId);
    }

    public Task<List<Programare>> GetProgramariMedicAsync(int medicId) =>
        _programareRepository.GetProgramariByMedicIdAsync(medicId);

    public async Task<bool> HasCompletedConsultationAsync(int pacientId, int medicId)
    {
        // Statusul Finalizat e sursa unică de adevăr — e setat explicit de medic/staff
        // după ce consultația s-a terminat. Nu mai filtrăm și pe DataEnd (era buggy:
        // dacă medicul finaliza în timpul programării, DataEnd era încă în viitor și
        // verificarea pica + probleme de fus orar UtcNow vs Now).
        var programari = await _programareRepository.GetProgramariByMedicIdAsync(medicId);
        return programari.Any(p =>
            p.PacientId == pacientId &&
            p.Status == StatusProgramare.Finalizat);
    }

    public Task<List<Programare>> GetAllWithRelationsAsync() =>
        _programareRepository.GetAllWithRelationsAsync();
}
