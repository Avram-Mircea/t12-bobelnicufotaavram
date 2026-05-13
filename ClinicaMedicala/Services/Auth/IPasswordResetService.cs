namespace ClinicaMedicala.Services.Auth;

// REQ-06: resetare parolă prin token securizat trimis pe email
public interface IPasswordResetService
{
    Task<string?> SolicitaResetAsync(string email);
    Task<bool> ReseteazaParolaAsync(string token, string parolaNoua);
    Task<bool> SchimbaParolaAsync(int utilizatorId, string parolaCurenta, string parolaNoua);
}
