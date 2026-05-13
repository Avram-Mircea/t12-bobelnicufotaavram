using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class TestValidatorViewModel
{
    [Required(ErrorMessage = "Selectează un medic.")]
    [Display(Name = "Medic")]
    public int? MedicId { get; set; }

    [Required]
    [Display(Name = "Tip programare")]
    public TipProgramare TipProgramare { get; set; }

    [Display(Name = "Asistent (opțional)")]
    public int? AsistentId { get; set; }

    [Display(Name = "Resurse")]
    public List<int> ResursaIds { get; set; } = new();

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Data început")]
    public DateTime DataStart { get; set; } = DateTime.Now.Date.AddDays(1).AddHours(9);

    [Required]
    [DataType(DataType.DateTime)]
    [Display(Name = "Data sfârșit")]
    public DateTime DataEnd { get; set; } = DateTime.Now.Date.AddDays(1).AddHours(10);

    // Populate pentru dropdown-uri
    public List<Medic> Medici { get; set; } = new();
    public List<Asistent> Asistenti { get; set; } = new();
    public List<Resursa> Resurse { get; set; } = new();
}
