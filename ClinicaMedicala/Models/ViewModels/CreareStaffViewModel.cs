using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

// Folosit de admin pentru a crea conturi pentru Medici, Asistenți, alți Administratori
// Pacienții se înregistrează singuri prin RegisterPacientViewModel
public class CreareStaffViewModel
{
    [Required, MaxLength(100)]
    public string Nume { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Prenume { get; set; } = null!;

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; } = null!;

    [Required, MaxLength(15)]
    [Phone]
    public string Telefon { get; set; } = null!;

    [Required, MaxLength(250)]
    public string Adresa { get; set; } = null!;

    [Required]
    [Display(Name = "Rol")]
    public Rol Rol { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Parola trebuie să aibă minim 8 caractere.")]
    [DataType(DataType.Password)]
    public string Parola { get; set; } = null!;

    // ── Câmpuri specifice MEDIC ─────────────────────────────────────────────
    [MaxLength(100)]
    public string? Specializare { get; set; }

    [MaxLength(6)]
    [Display(Name = "Cod parafă")]
    public string? CodParafa { get; set; }

    [Display(Name = "Grad profesional")]
    public GradProfesional? GradProfesional { get; set; }

    [Display(Name = "Cost consultație")]
    [Range(0, 100000)]
    public decimal? CostConsultatie { get; set; }

    [MaxLength(50)]
    [Display(Name = "Număr contract CAS")]
    public string? NumarContractCAS { get; set; }

    // ── Câmpuri specifice ASISTENT ───────────────────────────────────────────
    [MaxLength(100)]
    public string? Departament { get; set; }

    public Tura? Tura { get; set; }
}
