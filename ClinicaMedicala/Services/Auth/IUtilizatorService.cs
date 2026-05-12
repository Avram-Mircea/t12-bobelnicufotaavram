using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Auth;

// REQ-02: management conturi utilizatori
public interface IUtilizatorService
{
    Task<IEnumerable<Utilizator>> GetAllAsync();
    Task<Utilizator?> GetByIdAsync(int id);
    Task<Utilizator?> GetByEmailAsync(string email);
    Task<IEnumerable<Utilizator>> GetByRolAsync(Rol rol);

    // Creează cont cu parolă plain — serviciul o hash-uiește.
    // Folosit de admin pentru a crea staff și de flow-ul de înregistrare pacient.
    Task<Utilizator> CreeazaAsync(Utilizator utilizator, string parolaPlain);

    Task ActualizeazaAsync(Utilizator utilizator);

    // Soft-delete: dezactivare cont (REQ-02), nu ștergere fizică
    Task<bool> DezactiveazaAsync(int id);
    Task<bool> ReactiveazaAsync(int id);

    // Resetare parolă de către admin — fără token, fără parola curentă.
    // Folosit când un membru de staff și-a uitat parola și sună la admin.
    Task<bool> ReseteazaParolaCaAdminAsync(int id, string parolaNoua);
}
