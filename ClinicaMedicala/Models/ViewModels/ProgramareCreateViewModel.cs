using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

public class ProgramareCreateViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Selectați data și ora de început.")]
    [Display(Name = "Data și ora de început")]
    public DateTime DataStart { get; set; }

    [Required(ErrorMessage = "Selectați data și ora de sfârșit.")]
    [Display(Name = "Data și ora de sfârșit")]
    public DateTime DataEnd { get; set; }

    [Required(ErrorMessage = "Motivul vizitei este obligatoriu.")]
    [StringLength(500)]
    [Display(Name = "Motivul vizitei")]
    public string MotivVizita { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tip programare")]
    public TipProgramare TipProgramare { get; set; }

    [Required(ErrorMessage = "Selectați un pacient.")]
    [Display(Name = "Pacient")]
    public int PacientId { get; set; }

    [Required(ErrorMessage = "Selectați un medic.")]
    [Display(Name = "Medic")]
    public int MedicId { get; set; }

    [Display(Name = "Asistent (opțional)")]
    public int? AsistentId { get; set; }

    [Display(Name = "Sală / Resursă (opțional)")]
    public int? ResursaId { get; set; }
}
