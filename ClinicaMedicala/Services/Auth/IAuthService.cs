namespace ClinicaMedicala.Services.Auth;

// Autentificare + jurnalizare (REQ-07)
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string parola, string? adresaIp, string? userAgent);
    Task LogAutentificareAsync(int utilizatorId, bool succes, string? adresaIp, string? userAgent);
}
