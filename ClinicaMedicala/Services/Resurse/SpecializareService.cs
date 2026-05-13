using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Resurse;

public class SpecializareService : ISpecializareService
{
    private readonly ISpecializareRepository _repo;

    public SpecializareService(ISpecializareRepository repo)
    {
        _repo = repo;
    }

    public Task<IEnumerable<Specializare>> GetAllAsync() => _repo.GetAllOrderedAsync();
    public Task<IEnumerable<Specializare>> GetActiveAsync() => _repo.GetActiveAsync();
    public Task<Specializare?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public async Task<Specializare> CreeazaAsync(string nume, string? descriere)
    {
        if (string.IsNullOrWhiteSpace(nume))
            throw new InvalidOperationException("Numele specializării este obligatoriu.");

        var n = nume.Trim();

        if (await _repo.NumeExistaAsync(n))
            throw new InvalidOperationException($"Există deja o specializare cu numele „{n}”.");

        var spec = new Specializare { Nume = n, Descriere = descriere?.Trim(), Activ = true };
        await _repo.AddAsync(spec);
        await _repo.SaveChangesAsync();
        return spec;
    }

    public async Task<bool> ToggleActivAsync(int id)
    {
        var s = await _repo.GetByIdAsync(id);
        if (s == null) return false;

        s.Activ = !s.Activ;
        _repo.Update(s);
        await _repo.SaveChangesAsync();
        return true;
    }
}
