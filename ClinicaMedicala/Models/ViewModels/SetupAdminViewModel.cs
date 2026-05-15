using System.ComponentModel.DataAnnotations;
using ClinicaMedicala.Models.Validation;

namespace ClinicaMedicala.Models.ViewModels;

// Primul utilizator din sistem își creează singur contul de administrator.
// Folosit doar la prima rulare, când baza de date nu conține niciun cont.
public class SetupAdminViewModel
{
    [Required, MaxLength(100)]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = null!;

    [Required, MaxLength(100)]
    [Display(Name = "Prenume")]
    public string Prenume { get; set; } = null!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(15)]
    [Phone]
    public string Telefon { get; set; } = null!;

    [Required, MaxLength(250)]
    [Display(Name = "Adresă")]
    public string Adresa { get; set; } = null!;

    [Required]
    [StrongPassword]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă")]
    public string Parola { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Parola), ErrorMessage = "Parolele nu coincid.")]
    [Display(Name = "Confirmă parola")]
    public string ConfirmaParola { get; set; } = null!;
}
