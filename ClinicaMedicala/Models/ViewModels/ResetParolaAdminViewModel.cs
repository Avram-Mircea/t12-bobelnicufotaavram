using System.ComponentModel.DataAnnotations;
using ClinicaMedicala.Models.Validation;

namespace ClinicaMedicala.Models.ViewModels;

// Folosit de admin pentru a reseta parola unui utilizator
public class ResetParolaAdminViewModel
{
    public int Id { get; set; }

    [Display(Name = "Utilizator")]
    public string NumeComplet { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required]
    [StrongPassword]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă nouă")]
    public string ParolaNoua { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(ParolaNoua), ErrorMessage = "Parolele nu coincid.")]
    [Display(Name = "Confirmă parola")]
    public string ConfirmaParola { get; set; } = null!;
}
