using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Auth;

public class UtilizatorService : IUtilizatorService
{
    private readonly IUtilizatorRepository _repo;
    private readonly IPasswordHasher _hasher;

    public UtilizatorService(IUtilizatorRepository repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public Task<IEnumerable<Utilizator>> GetAllAsync() => _repo.GetAllAsync();

    public Task<Utilizator?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

    public Task<Utilizator?> GetByEmailAsync(string email) => _repo.GetByEmailAsync(email);

    public Task<IEnumerable<Utilizator>> GetByRolAsync(Rol rol) => _repo.GetByRolAsync(rol);

    public async Task<Utilizator> CreeazaAsync(Utilizator utilizator, string parolaPlain)
    {
        if (utilizator == null) throw new ArgumentNullException(nameof(utilizator));

        // Verificăm unicitatea email-ului explicit (înainte ca DB să arunce constraint violation)
        if (await _repo.EmailExistaAsync(utilizator.Email))
            throw new InvalidOperationException("Există deja un cont cu acest email.");

        utilizator.Email = utilizator.Email.Trim().ToLowerInvariant();
        utilizator.ParolaHash = _hasher.Hash(parolaPlain);
        utilizator.DataCreareCont = DateTime.UtcNow;
        utilizator.StatusCont = true;

        // Rolul trebuie să fie consistent cu tipul concret (defensive check)
        utilizator.Rol = utilizator switch
        {
            Medic => Rol.Medic,
            Pacient => Rol.Pacient,
            Asistent => Rol.Asistent,
            Administrator => Rol.Admin,
            _ => utilizator.Rol
        };

        await _repo.AddAsync(utilizator);
        await _repo.SaveChangesAsync();
        return utilizator;
    }

    public async Task ActualizeazaAsync(Utilizator utilizator)
    {
        // Nu permitem schimbarea parolei prin acest endpoint — folosiți IPasswordResetService
        _repo.Update(utilizator);
        await _repo.SaveChangesAsync();
    }

    public async Task<bool> DezactiveazaAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u == null || !u.StatusCont) return false;

        u.StatusCont = false;
        _repo.Update(u);
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReactiveazaAsync(int id)
    {
        var u = await _repo.GetByIdAsync(id);
        if (u == null || u.StatusCont) return false;

        u.StatusCont = true;
        _repo.Update(u);
        await _repo.SaveChangesAsync();
        return true;
    }
}
