using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class EditeazaResursaViewModel
{
    public int Id { get; set; }

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
    [Display(Name = "Locație")]
    public string? Locatie { get; set; }

    [Required]
    [Display(Name = "Stare")]
    public StareResursa Stare { get; set; }

    [Display(Name = "Specializări permise")]
    public List<int> SpecializareIds { get; set; } = new();

    public List<Specializare> SpecializariDisponibile { get; set; } = new();

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data ultimei revizii")]
    public DateTime DataUltimaRevizie { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Data scadenței următoarei revizii")]
    public DateTime DataScadentaRevizie { get; set; }
}
