using ClinicaMedicala.Models;
using ClinicaMedicala.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicaMedicala.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IUtilizatorRepository _utilizatorRepo;
    private readonly IAutentificareRepository _autRepo;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<AuthService> _logger;

    private const int MaxIncercariEsuate = 5;
    private const int IntervalIncercariMinute = 15;

    public AuthService(
        IUtilizatorRepository utilizatorRepo,
        IAutentificareRepository autRepo,
        IPasswordHasher hasher,
        ILogger<AuthService> logger)
    {
        _utilizatorRepo = utilizatorRepo;
        _autRepo = autRepo;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string parola, string? adresaIp, string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(parola))
            return AuthResult.Esec("Email și parolă sunt obligatorii.");

        // Protecție brute-force
        var esuateRecente = await _autRepo.GetEsuateRecenteAsync(email, IntervalIncercariMinute);
        if (esuateRecente.Count() >= MaxIncercariEsuate)
        {
            _logger.LogWarning("Login blocat (brute-force) pentru {Email}", email);
            return AuthResult.Esec($"Cont blocat temporar. Reîncercați în {IntervalIncercariMinute} minute.");
        }

        var utilizator = await _utilizatorRepo.GetByEmailAsync(email);

        if (utilizator == null || !_hasher.Verify(parola, utilizator.ParolaHash))
        {
            if (utilizator != null)
                await LogAutentificareAsync(utilizator.Id, false, adresaIp, userAgent);

            _logger.LogWarning("Login eșuat pentru {Email}", email);
            return AuthResult.Esec("Email sau parolă incorectă.");
        }

        if (!utilizator.StatusCont)
        {
            await LogAutentificareAsync(utilizator.Id, false, adresaIp, userAgent);
            return AuthResult.Esec("Contul este dezactivat. Contactați administratorul.");
        }

        await LogAutentificareAsync(utilizator.Id, true, adresaIp, userAgent);
        _logger.LogInformation("Login reușit: {Email} ({Rol})", utilizator.Email, utilizator.Rol);
        return AuthResult.Ok(utilizator);
    }

    public async Task LogAutentificareAsync(int utilizatorId, bool succes, string? adresaIp, string? userAgent)
    {
        var log = new Autentificare
        {
            UtilizatorId = utilizatorId,
            DataOra = DateTime.UtcNow,
            Succes = succes,
            AdresaIp = adresaIp,
            UserAgent = userAgent
        };

        await _autRepo.AddAsync(log);
        await _autRepo.SaveChangesAsync();
    }
}
