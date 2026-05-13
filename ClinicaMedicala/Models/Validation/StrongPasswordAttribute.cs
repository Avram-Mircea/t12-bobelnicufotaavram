using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.Validation;

// Validare parolă: minim 8 caractere + cel puțin un simbol (caracter non-alfanumeric).
// Aceeași logică e oglindită în wwwroot/js/password-strength.js pentru feedback live.
public class StrongPasswordAttribute : ValidationAttribute
{
    public const int LungimeMinima = 8;
    public const int LungimeMaxima = 100;

    public StrongPasswordAttribute()
    {
        ErrorMessage = "Parola trebuie să aibă minim 8 caractere și cel puțin un simbol (ex: ! @ # $ % & *).";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string s) return false;
        if (s.Length < LungimeMinima || s.Length > LungimeMaxima) return false;
        if (!s.Any(c => !char.IsLetterOrDigit(c))) return false;
        return true;
    }
}
