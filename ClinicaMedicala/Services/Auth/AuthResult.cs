using ClinicaMedicala.Models;

namespace ClinicaMedicala.Services.Auth;

// Rezultat tipizat pentru operațiunile de autentificare — evită aruncarea de excepții
// pentru fluxuri normale (login eșuat etc.)
public class AuthResult
{
    public bool Succes { get; private set; }
    public string? Eroare { get; private set; }
    public Utilizator? Utilizator { get; private set; }

    public static AuthResult Ok(Utilizator utilizator) => new()
    {
        Succes = true,
        Utilizator = utilizator
    };

    public static AuthResult Esec(string mesaj) => new()
    {
        Succes = false,
        Eroare = mesaj
    };
}
