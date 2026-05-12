using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaMedicala.Models;

public class Programare
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataStart { get; set; }

    [Required]
    public DateTime DataEnd { get; set; }

    [Required]
    [MaxLength(500)]
    public string MotivVizita { get; set; } = null!;

    [Required]
    public TipProgramare TipProgramare { get; set; }

    [Required]
    public StatusProgramare Status { get; set; } = StatusProgramare.Programat;

    // Completat la anulare — util pentru statistici și audit
    [MaxLength(500)]
    public string? MotivAnulare { get; set; }

    // Setat true după trimiterea email/SMS de confirmare
    public bool NotificareTrimisa { get; set; } = false;

    // Canalul prin care s-a trimis notificarea
    public CanalNotificare? CanalNotificare { get; set; }

    [Required]
    public DateTime DataCreare { get; set; } = DateTime.UtcNow;

    // FK Pacient
    public int PacientId { get; set; }

    [ForeignKey(nameof(PacientId))]
    public Pacient Pacient { get; set; } = null!;

    // FK Medic
    public int MedicId { get; set; }

    [ForeignKey(nameof(MedicId))]
    public Medic Medic { get; set; } = null!;

    // FK Asistent — cel care a confirmat/gestionat programarea
    public int? AsistentId { get; set; }

    [ForeignKey(nameof(AsistentId))]
    public Asistent? Asistent { get; set; }

    // FK Resursa — cabineta/aparatul alocat (opțional)
    public int? ResursaId { get; set; }

    [ForeignKey(nameof(ResursaId))]
    public Resursa? Resursa { get; set; }

    // Consultația rezultată din această programare
    public Consultatie? Consultatie { get; set; }
}
