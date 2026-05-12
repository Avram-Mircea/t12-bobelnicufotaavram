using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class CreareResursaViewModel
{
    [Required(ErrorMessage = "Denumirea este obligatorie.")]
    [MaxLength(150)]
    [Display(Name = "Denumire")]
    public string Denumire { get; set; } = null!;

    [Required]
    [Display(Name = "Tip resursă")]
    public TipResursa Tip { get; set; }

    [Required(ErrorMessage = "Numărul de inventar este obligatoriu.")]
    [MaxLength(50)]
    [Display(Name = "Număr inventar")]
    public string NumarInventar { get; set; } = null!;

    [MaxLength(100)]
    [Display(Name = "Locație (ex: „Etaj 2, Camera 204”)")]
    public string? Locatie { get; set; }

    // Pentru task 4 — pentru acum simplu input text liber
    [MaxLength(100)]
    [Display(Name = "Specializare permisă (opțional)")]
    public string? SpecializarePermisa { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data ultimei revizii")]
    public DateTime DataUltimaRevizie { get; set; } = DateTime.UtcNow.Date;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data scadenței următoarei revizii")]
    public DateTime DataScadentaRevizie { get; set; } = DateTime.UtcNow.Date.AddYears(1);
}
