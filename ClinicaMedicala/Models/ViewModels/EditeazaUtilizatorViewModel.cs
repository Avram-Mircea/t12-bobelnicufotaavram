using System.ComponentModel.DataAnnotations;

namespace ClinicaMedicala.Models.ViewModels;

// Editare cont de către admin.
// Câmpurile imuabile (Email, Rol, CNP, DataNastere) sunt afișate readonly.
public class EditeazaUtilizatorViewModel
{
    public int Id { get; set; }

    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;

    // ── Comune ───────────────────────────────────────────────────────────────
    [Required, MaxLength(100)]
    public string Nume { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Prenume { get; set; } = null!;

    [Required, MaxLength(15)]
    [Phone(ErrorMessage = "Număr de telefon invalid.")]
    public string Telefon { get; set; } = null!;

    [Required, MaxLength(250)]
    public string Adresa { get; set; } = null!;

    // ── Medic ────────────────────────────────────────────────────────────────
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

    // ── Asistent ─────────────────────────────────────────────────────────────
    [MaxLength(100)]
    public string? Departament { get; set; }

    public Tura? Tura { get; set; }

    // ── Pacient (limitat — CNP/DataNastere/GrupaSanguina nu se schimbă) ─────
    [MaxLength(13), MinLength(13)]
    [Display(Name = "CNP")]
    public string? CNP { get; set; }   // readonly în view

    [Display(Name = "Data nașterii")]
    public DateTime? DataNastere { get; set; }   // readonly în view

    [Display(Name = "Grupa sanguină")]
    public GrupaSanguina? GrupaSanguina { get; set; }   // readonly în view

    [Display(Name = "Asigurat CNAS")]
    public bool? AsiguratCNAS { get; set; }

    [MaxLength(500)]
    [Display(Name = "Alergii cunoscute")]
    public string? AlergiiCunoscute { get; set; }

    [MaxLength(150)]
    [Display(Name = "Nume contact urgență")]
    public string? ContactUrgentaNume { get; set; }

    [MaxLength(15)]
    [Phone]
    [Display(Name = "Telefon contact urgență")]
    public string? ContactUrgentaTelefon { get; set; }
}
