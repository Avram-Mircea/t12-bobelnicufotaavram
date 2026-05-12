using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email-ul este obligatoriu.")]
    [EmailAddress(ErrorMessage = "Format email invalid.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Parola este obligatorie.")]
    [DataType(DataType.Password)]
    [Display(Name = "Parolă")]
    public string Parola { get; set; } = null!;

    [Display(Name = "Ține-mă minte")]
    public bool TineMaMinte { get; set; }

    public string? ReturnUrl { get; set; }
}
