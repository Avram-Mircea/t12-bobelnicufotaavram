using System.Security.Cryptography;
using ClinicaMedicala.Repositories;

namespace ClinicaMedicala.Services.Auth;

public class PasswordResetService : IPasswordResetService
{
    private readonly IUtilizatorRepository _utilizatorRepo;
    private readonly IPasswordHasher _hasher;

    // Token valabil 1 oră — suficient pentru flow normal, suficient de scurt pentru securitate
    private const int TokenValiditateMinute = 60;

    public PasswordResetService(IUtilizatorRepository utilizatorRepo, IPasswordHasher hasher)
    {
        _utilizatorRepo = utilizatorRepo;
        _hasher = hasher;
    }

    public async Task<string?> SolicitaResetAsync(string email)
    {
        var utilizator = await _utilizatorRepo.GetByEmailAsync(email);

        // Nu dezvăluim dacă email-ul există — întoarcem null silențios
        if (utilizator == null || !utilizator.StatusCont)
            return null;

        var token = GenereazaTokenSecurizat();
        utilizator.ResetToken = token;
        utilizator.ResetTokenExpires = DateTime.UtcNow.AddMinutes(TokenValiditateMinute);

        _utilizatorRepo.Update(utilizator);
        await _utilizatorRepo.SaveChangesAsync();

        // În producție: aici se trimite email cu link-ul către /Auth/ResetPassword?token={token}
        // Pentru moment, întoarcem token-ul ca să poată fi afișat în dev
        return token;
    }

    public async Task<bool> ReseteazaParolaAsync(string token, string parolaNoua)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(parolaNoua))
            return false;

        var utilizator = await _utilizatorRepo.GetByResetTokenAsync(token);

        if (utilizator == null
            || utilizator.ResetTokenExpires == null
            || utilizator.ResetTokenExpires < DateTime.UtcNow)
        {
            return false;
        }

        utilizator.ParolaHash = _hasher.Hash(parolaNoua);
        utilizator.ResetToken = null;
        utilizator.ResetTokenExpires = null;

        _utilizatorRepo.Update(utilizator);
        await _utilizatorRepo.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SchimbaParolaAsync(int utilizatorId, string parolaCurenta, string parolaNoua)
    {
        var utilizator = await _utilizatorRepo.GetByIdAsync(utilizatorId);
        if (utilizator == null) return false;

        if (!_hasher.Verify(parolaCurenta, utilizator.ParolaHash))
            return false;

        utilizator.ParolaHash = _hasher.Hash(parolaNoua);
        _utilizatorRepo.Update(utilizator);
        await _utilizatorRepo.SaveChangesAsync();
        return true;
    }

    private static string GenereazaTokenSecurizat()
    {
        // 256 biți de entropie — practic imposibil de ghicit
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(bytes);
    }
}
