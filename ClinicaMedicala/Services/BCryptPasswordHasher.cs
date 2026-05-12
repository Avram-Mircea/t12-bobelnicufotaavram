using Microsoft.Extensions.Logging;

namespace ClinicaMedicala.Services;

public class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    private readonly ILogger<BCryptPasswordHasher>? _logger;

    public BCryptPasswordHasher(ILogger<BCryptPasswordHasher>? logger = null)
    {
        _logger = logger;
    }

    public string Hash(string parolaPlainText)
    {
        if (string.IsNullOrWhiteSpace(parolaPlainText))
            throw new ArgumentException("Parola nu poate fi goală.", nameof(parolaPlainText));

        return BCrypt.Net.BCrypt.HashPassword(parolaPlainText, WorkFactor);
    }

    public bool Verify(string parolaPlainText, string hash)
    {
        if (string.IsNullOrWhiteSpace(parolaPlainText) || string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(parolaPlainText, hash);
        }
        catch (Exception ex)
        {
            // Hash corupt sau format necunoscut — tratăm ca verificare eșuată
            _logger?.LogError(ex, "BCrypt.Verify a aruncat {Type}", ex.GetType().Name);
            return false;
        }
    }
}
