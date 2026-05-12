using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = null!;

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie să aibă minim 8 caractere.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă nouă")]
    public string ParolaNoua { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(ParolaNoua), ErrorMessage = "Parolele nu coincid.")]
    [Display(Name = "Confirmă parola")]
    public string ConfirmaParola { get; set; } = null!;
}
