using System.ComponentModel.DataAnnotations;
using ClinicaMedicala.Models.Validation;

namespace ClinicaMedicala.Models.ViewModels;

// Pacientul se înregistrează singur (REQ-02); staff-ul e creat doar de admin.
public class RegisterPacientViewModel
{
    [Required, MaxLength(100)]
    [Display(Name = "Nume")]
    public string Nume { get; set; } = null!;

    [Required, MaxLength(100)]
    [Display(Name = "Prenume")]
    public string Prenume { get; set; } = null!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required]
    [StringLength(13, MinimumLength = 13, ErrorMessage = "CNP-ul trebuie să aibă exact 13 cifre.")]
    [RegularExpression(@"^\d{13}$", ErrorMessage = "CNP-ul trebuie să conțină doar cifre.")]
    public string CNP { get; set; } = null!;

    [Required, DataType(DataType.Date)]
    [Display(Name = "Data nașterii")]
    public DateTime DataNastere { get; set; }

    [Required, MaxLength(15)]
    [Phone]
    public string Telefon { get; set; } = null!;

    [Required, MaxLength(250)]
    [Display(Name = "Adresă")]
    public string Adresa { get; set; } = null!;

    [Required]
    [Display(Name = "Grupa sanguină")]
    public GrupaSanguina GrupaSanguina { get; set; }

    [Required, MaxLength(150)]
    [Display(Name = "Nume contact urgență")]
    public string ContactUrgentaNume { get; set; } = null!;

    [Required, MaxLength(15)]
    [Phone]
    [Display(Name = "Telefon contact urgență")]
    public string ContactUrgentaTelefon { get; set; } = null!;

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
