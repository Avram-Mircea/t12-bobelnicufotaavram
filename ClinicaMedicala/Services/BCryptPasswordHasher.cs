namespace ClinicaMedicala.Services;

public class BCryptPasswordHasher : IPasswordHasher
{
    // Work factor 12 — echilibru între securitate și performanță (≈250ms pe hash)
    private const int WorkFactor = 12;

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
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash invalid/corupt în DB
            return false;
        }
    }
}
