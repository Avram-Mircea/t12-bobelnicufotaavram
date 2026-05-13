using System.ComponentModel.DataAnnotations;
using ClinicaMedicala.Models.Validation;

namespace ClinicaMedicala.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = null!;

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
