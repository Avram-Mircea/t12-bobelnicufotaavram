namespace ClinicaMedicala.Services;

// Abstracție peste algoritmul de hashing.
// Permite înlocuirea BCrypt cu alt algoritm fără a schimba consumatorii.
public interface IPasswordHasher
{
    string Hash(string parolaPlainText);
    bool Verify(string parolaPlainText, string hash);
}
