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

    // REQ-13: lista de ID-uri ale specializărilor care pot folosi această resursă
    [Display(Name = "Specializări permise")]
    public List<int> SpecializareIds { get; set; } = new();

    // Populat de controller pentru dropdown (toate specializările active)
    public List<Specializare> SpecializariDisponibile { get; set; } = new();
}
