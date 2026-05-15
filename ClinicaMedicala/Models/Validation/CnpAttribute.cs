using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.Validation;

// Validare CNP cu mesaje contextuale: în funcție de tipul greșelii (lungime
// vs. caractere nepermise) afișăm un mesaj diferit, ca utilizatorul să știe
// exact ce să corecteze.
public class CnpAttribute : ValidationAttribute
{
    public const int LungimeCnp = 13;

    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        if (value is not string s || string.IsNullOrWhiteSpace(s))
        {
            return new ValidationResult("CNP-ul este obligatoriu.");
        }

        // Eliminăm spațiile, ca utilizatorul să poată tasta cu sau fără ele.
        s = s.Trim();

        // 1) Conține alt caracter în afară de cifre? — prioritizăm acest mesaj
        //    pentru că e cauza cea mai des întâlnită (litere lipite de cifre).
        if (s.Any(c => !char.IsDigit(c)))
        {
            return new ValidationResult("CNP-ul trebuie să conțină doar cifre (fără litere sau simboluri).");
        }

        // 2) Are exact 13 cifre?
        if (s.Length < LungimeCnp)
        {
            return new ValidationResult($"CNP necesită 13 cifre — ai introdus {s.Length}.");
        }

        if (s.Length > LungimeCnp)
        {
            return new ValidationResult($"CNP are doar 13 cifre — ai introdus {s.Length}.");
        }

        return ValidationResult.Success;
    }
}
